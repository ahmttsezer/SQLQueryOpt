using Microsoft.SqlServer.TransactSql.ScriptDom;
using QueryDebugger.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace QueryDebugger.Core.Parsing
{
    public class QueryVisitor : TSqlFragmentVisitor
    {
        public ParsedQuery Result { get; private set; } = new ParsedQuery();

        public override void ExplicitVisit(QuerySpecification node)
        {
            // Handle FROM clause
            if (node.FromClause != null)
            {
                foreach (var tableRef in node.FromClause.TableReferences)
                {
                    ProcessTableReference(tableRef);
                }
            }

            // Handle WHERE clause
            if (node.WhereClause != null)
            {
                ProcessBooleanExpression(node.WhereClause.SearchCondition);
            }

            base.ExplicitVisit(node);
        }

        private void ProcessTableReference(TableReference tableRef)
        {
            if (tableRef is NamedTableReference namedTable)
            {
                // Main table (usually the first one if not a join)
                if (Result.MainTable == null)
                {
                    Result.MainTable = new TableNode
                    {
                        Name = namedTable.SchemaObject.BaseIdentifier.Value,
                        Schema = namedTable.SchemaObject.SchemaIdentifier?.Value ?? "dbo",
                        Alias = namedTable.Alias?.Value
                    };
                }
            }
            else if (tableRef is QualifiedJoin join)
            {
                // Recursive call for the left side if it's also a join or the main table
                ProcessTableReference(join.FirstTableReference);

                // The right side is the joined table
                var joinedTableRef = join.SecondTableReference as NamedTableReference;
                if (joinedTableRef != null)
                {
                    var joinNode = new JoinNode
                    {
                        Table = new TableNode
                        {
                            Name = joinedTableRef.SchemaObject.BaseIdentifier.Value,
                            Schema = joinedTableRef.SchemaObject.SchemaIdentifier?.Value ?? "dbo",
                            Alias = joinedTableRef.Alias?.Value
                        },
                        JoinType = join.QualifiedJoinType.ToString(),
                        Condition = ScriptDomUtils.GetScript(join.SearchCondition)
                    };
                    Result.Joins.Add(joinNode);
                }
            }
        }

        private void ProcessBooleanExpression(BooleanExpression expr)
        {
            if (expr is BooleanBinaryExpression binaryExpr && binaryExpr.BinaryExpressionType == BooleanBinaryExpressionType.And)
            {
                // Recursively split AND conditions
                ProcessBooleanExpression(binaryExpr.FirstExpression);
                ProcessBooleanExpression(binaryExpr.SecondExpression);
            }
            else
            {
                // Leaf condition (or OR condition, which we treat as a single block for now)
                Result.Filters.Add(new FilterNode
                {
                    Condition = ScriptDomUtils.GetScript(expr)
                });
            }
        }
    }

    public static class ScriptDomUtils
    {
        public static string GetScript(TSqlFragment fragment)
        {
            if (fragment == null) return string.Empty;
            
            // This is a simplified way to get text. 
            // In a real app, we might need to use the original script + tokens to extract exact text,
            // or use a SqlScriptGenerator to regenerate it.
            // For now, let's assume we can't easily get the original text without passing it in,
            // so we'll use a generator for standardization.
            
            var generator = new Sql160ScriptGenerator();
            generator.GenerateScript(fragment, out var script);
            return script;
        }
    }
}

