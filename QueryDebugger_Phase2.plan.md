# Query Debugger - Phase 2: Enhancements

This plan outlines the next steps to improve usability and functionality of the Query Debugger, building upon the MVP.

## 1. Query & Connection History
*   **Goal:** Allow users to quickly reuse previous connection strings and queries.
*   **Implementation:**
    *   Create `HistoryService` in `QueryDebugger.Core`.
    *   Save `LastConnectionStrings` (List<string>) and `RecentQueries` (List<HistoryItem>) to `appsettings.json` or a local JSON file.
    *   Add a ComboBox or "Recent" dropdown to the UI for Connection String.

## 2. Enhanced UI with Syntax Highlighting
*   **Goal:** Make SQL easier to read.
*   **Implementation:**
    *   Replace `TextBox` for SQL input with `AvalonEdit` (referencing the package used in the sibling project if available, or installing a new one).
    *   Configure SQL Syntax Highlighting.

## 3. Export Functionality
*   **Goal:** Share analysis results.
*   **Implementation:**
    *   Add "Export Report" button.
    *   Generate a Markdown or Text file summary of the analysis (Success/Fail steps).

## 4. Advanced Analysis (Phase 2 Logic)
*   **Goal:** Better diagnostics for "Why did the join fail?".
*   **Implementation:**
    *   **Foreign Key Null Check:** If a join fails, check if the source column itself was NULL.
    *   **Orphan Check:** If source is not NULL, explicitly state "Value X exists in Table A but not in Table B".

## Todo List
1.  [ ] Install `AvalonEdit` and implement Syntax Highlighting in `MainWindow.xaml`.
2.  [ ] Implement `HistoryService` to save/load recent connections.
3.  [ ] Update `AnalysisService` to include "FK Null vs Orphan" distinction.
4.  [ ] Add "Export" button and logic.

