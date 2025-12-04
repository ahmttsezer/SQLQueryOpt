using Dapper;
using Microsoft.Data.SqlClient;
using QueryDebugger.Core.Models;
using QueryDebugger.Core.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace QueryDebugger.Core.Analysis
{
    public class AnalysisService
    {
        private readonly SqlParserService _parser;

        public AnalysisService()
        {
            _parser = new SqlParserService();
        }

        public async Task<AnalysisResult> AnalyzeAsync(string connectionString, string sql, string targetKeyColumn, string targetKeyValue)
        {
            var result = new AnalysisResult();
            var parsedQuery = _parser.Parse(sql);

            if (parsedQuery.MainTable == null)
            {
                result.Steps.Add(new StepResult { StepName = "Parse", Description = "Parsing SQL", Passed = false, ErrorMessage = "Could not identify main table." });
                return result;
            }

            using var connection = new SqlConnection(connectionString);
            
            // Step 1: Check Base Table Existence
            var baseAlias = parsedQuery.MainTable.Alias ?? parsedQuery.MainTable.Name;
            var baseCheckSql = $"SELECT COUNT(1) FROM {parsedQuery.MainTable.Schema}.{parsedQuery.MainTable.Name} AS {baseAlias} WHERE {baseAlias}.{targetKeyColumn} = @Id";
            
            var baseStep = new StepResult 
            { 
                StepName = "Base Table Check", 
                Description = $"Checking existence in {parsedQuery.MainTable.Name}",
                GeneratedQuery = baseCheckSql 
            };

            try
            {
                var count = await connection.ExecuteScalarAsync<int>(baseCheckSql, new { Id = targetKeyValue });
                baseStep.RecordCount = count;
                baseStep.Passed = count > 0;
                
                if (count == 0)
                {
                    baseStep.ErrorMessage = "Record not found in base table.";
                    result.Steps.Add(baseStep);
                    return result; // Stop if base record doesn't exist
                }
            }
            catch (Exception ex)
            {
                baseStep.Passed = false;
                baseStep.ErrorMessage = ex.Message;
                result.Steps.Add(baseStep);
                return result;
            }
            
            result.Steps.Add(baseStep);

            // Track aliases available for binding
            var availableAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { baseAlias };

            // Step 2: Check Joins Incrementally
            var currentFromClause = new StringBuilder();
            currentFromClause.Append($"FROM {parsedQuery.MainTable.Schema}.{parsedQuery.MainTable.Name} AS {baseAlias} ");

            foreach (var join in parsedQuery.Joins)
            {
                // Add this join to the pile
                var joinAlias = join.Table.Alias ?? join.Table.Name;
                var joinClause = $"{join.JoinType.ToUpper()} JOIN {join.Table.Schema}.{join.Table.Name} AS {joinAlias} ON {join.Condition} ";
                var testSql = $"SELECT COUNT(1) {currentFromClause} {joinClause} WHERE {baseAlias}.{targetKeyColumn} = @Id";

                var joinStep = new StepResult
                {
                    StepName = $"Join Check: {join.Table.Name}",
                    Description = $"Checking join with {join.Table.Name}",
                    GeneratedQuery = testSql
                };

                try
                {
                    var count = await connection.ExecuteScalarAsync<int>(testSql, new { Id = targetKeyValue });
                    joinStep.RecordCount = count;
                    joinStep.Passed = count > 0;

                    if (count == 0)
                    {
                        // Join Failed. Analyze WHY.
                        // Try to find the foreign key column in the condition that belongs to an available alias.
                        var reason = "Join failed. No matching records found.";
                        
                        var fkCheck = await AnalyzeJoinFailureAsync(connection, currentFromClause.ToString(), baseAlias, targetKeyColumn, targetKeyValue, join.Condition, availableAliases);
                        if (!string.IsNullOrEmpty(fkCheck))
                        {
                            reason = fkCheck;
                        }

                        joinStep.ErrorMessage = reason;
                        
                        if (join.JoinType.Contains("Inner", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Steps.Add(joinStep);
                            return result;
                        }
                    }
                }
                catch (Exception ex)
                {
                    joinStep.Passed = false;
                    joinStep.ErrorMessage = ex.Message;
                    result.Steps.Add(joinStep);
                    return result;
                }

                result.Steps.Add(joinStep);
                currentFromClause.Append(joinClause);
                availableAliases.Add(joinAlias);
            }

            // Step 3: Check Filters
            foreach (var filter in parsedQuery.Filters)
            {
                var testSql = $"SELECT COUNT(1) {currentFromClause} WHERE {baseAlias}.{targetKeyColumn} = @Id AND ({filter.Condition})";

                var filterStep = new StepResult
                {
                    StepName = "Filter Check",
                    Description = $"Checking filter: {filter.Condition}",
                    GeneratedQuery = testSql
                };

                try
                {
                    var count = await connection.ExecuteScalarAsync<int>(testSql, new { Id = targetKeyValue });
                    filterStep.RecordCount = count;
                    filterStep.Passed = count > 0;

                    if (count == 0)
                    {
                        filterStep.ErrorMessage = "Filter criteria not met.";
                        result.Steps.Add(filterStep);
                        return result; 
                    }
                }
                catch (Exception ex)
                {
                    filterStep.Passed = false;
                    filterStep.ErrorMessage = ex.Message;
                    result.Steps.Add(filterStep);
                    return result;
                }

                result.Steps.Add(filterStep);
            }

            result.IsSuccess = true;
            return result;
        }

        private async Task<string> AnalyzeJoinFailureAsync(SqlConnection connection, string fromClause, string baseAlias, string keyCol, string keyValue, string condition, HashSet<string> availableAliases)
        {
            // Regex to find "Alias.Column"
            var matches = Regex.Matches(condition, @"([a-zA-Z0-9_]+)\.([a-zA-Z0-9_]+)");
            
            foreach (Match match in matches)
            {
                var alias = match.Groups[1].Value;
                var column = match.Groups[2].Value;

                if (availableAliases.Contains(alias))
                {
                    // This is a source column! Check its value.
                    var sql = $"SELECT {alias}.{column} {fromClause} WHERE {baseAlias}.{keyCol} = @Id";
                    try
                    {
                        var value = await connection.ExecuteScalarAsync<object>(sql, new { Id = keyValue });
                        
                        if (value == null || value == DBNull.Value)
                        {
                            return $"Join failed because source column '{alias}.{column}' is NULL.";
                        }
                        else
                        {
                            return $"Join failed. Source value for '{alias}.{column}' is '{value}', but no match found in target table.";
                        }
                    }
                    catch
                    {
                        // Ignore errors during heuristic check
                    }
                }
            }

            return null;
        }
    }
}
