using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using QueryDebugger.Core.Analysis;
using QueryDebugger.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Xml;
using QueryDebugger.Core.Models;

namespace QueryDebugger.UI
{
    public partial class MainWindow : Window
    {
        private readonly AnalysisService _analysisService;
        private readonly HistoryService _historyService;
        private List<StepResult> _lastResults;

        public MainWindow()
        {
            InitializeComponent();
            _analysisService = new AnalysisService();
            _historyService = new HistoryService();
            LoadHighlighting();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var history = await _historyService.LoadHistoryAsync();
            if (history != null)
            {
                CmbConnectionString.ItemsSource = history.RecentConnectionStrings;
                if (history.RecentConnectionStrings.Count > 0)
                {
                    CmbConnectionString.Text = history.RecentConnectionStrings[0];
                }
                else
                {
                     CmbConnectionString.Text = @"Server=(localdb)\mssqllocaldb;Database=QueryDebugDb;Trusted_Connection=True;";
                }

                if (!string.IsNullOrEmpty(history.LastQuery))
                {
                    TxtSqlQuery.Text = history.LastQuery;
                }
            }
        }

        private void LoadHighlighting()
        {
            try
            {
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("QueryDebugger.UI.Resources.SQL.xshd"))
                {
                    if (stream != null)
                    {
                        using (var reader = new XmlTextReader(stream))
                        {
                            TxtSqlQuery.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Fallback or ignore
            }
        }

        private async void BtnAnalyze_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BtnAnalyze.IsEnabled = false;
                TxtStatus.Text = "Analyzing...";
                LstResults.ItemsSource = null;
                _lastResults = null;

                var connectionString = CmbConnectionString.Text;
                var sql = TxtSqlQuery.Text;
                var keyColumn = TxtKeyColumn.Text;
                var keyValue = TxtKeyValue.Text;

                if (string.IsNullOrWhiteSpace(connectionString) || string.IsNullOrWhiteSpace(sql))
                {
                    MessageBox.Show("Please provide Connection String and SQL Query.", "Missing Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save History
                var history = await _historyService.LoadHistoryAsync() ?? new HistoryData();
                if (!history.RecentConnectionStrings.Contains(connectionString))
                {
                    history.RecentConnectionStrings.Insert(0, connectionString);
                    if (history.RecentConnectionStrings.Count > 5) history.RecentConnectionStrings.RemoveAt(5);
                }
                history.LastQuery = sql;
                await _historyService.SaveHistoryAsync(history);
                
                // Update UI Combo
                CmbConnectionString.ItemsSource = null; // Refresh hack
                CmbConnectionString.ItemsSource = history.RecentConnectionStrings;
                CmbConnectionString.Text = connectionString;

                var result = await _analysisService.AnalyzeAsync(connectionString, sql, keyColumn, keyValue);

                _lastResults = result.Steps;
                LstResults.ItemsSource = result.Steps;
                TxtStatus.Text = result.IsSuccess ? "Analysis Complete: Record Found" : "Analysis Complete: Record Missing";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Analysis Error", MessageBoxButton.OK, MessageBoxImage.Error);
                TxtStatus.Text = "Error occurred.";
            }
            finally
            {
                BtnAnalyze.IsEnabled = true;
            }
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (_lastResults == null || _lastResults.Count == 0)
            {
                MessageBox.Show("No results to export. Run analysis first.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Query Analysis Report - {DateTime.Now}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Key Column: {TxtKeyColumn.Text}, Value: {TxtKeyValue.Text}");
            sb.AppendLine();

            foreach (var step in _lastResults)
            {
                sb.AppendLine($"[{step.StepName}] - {(step.Passed ? "PASSED" : "FAILED")}");
                sb.AppendLine($"Desc: {step.Description}");
                if (!step.Passed) sb.AppendLine($"ERROR: {step.ErrorMessage}");
                sb.AppendLine($"Query: {step.GeneratedQuery}");
                sb.AppendLine();
            }

            try
            {
                var path = "AnalysisReport.txt";
                File.WriteAllText(path, sb.ToString());
                System.Diagnostics.Process.Start("notepad.exe", path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to export: {ex.Message}");
            }
        }
    }
}
