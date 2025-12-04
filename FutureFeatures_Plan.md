# Future Features Implementation Plan

This plan covers the implementation of the requested "Future Features": Schema Browser, Query History, Reporting, and AI Integration.

## Phase 1: Schema Browser (Database Explorer)
*   **Goal:** Allow users to explore tables and columns without leaving the app.
*   **Backend:** `SchemaService` to fetch metadata from SQL Server (`INFORMATION_SCHEMA`).
*   **UI:** `SchemaView` UserControl containing a styled `TreeView`.
*   **Integration:** Add a "Schema" button to the Sidebar to toggle this view.

## Phase 2: Query History
*   **Goal:** Save and restore recent queries.
*   **Backend:** Reuse/Enhance `QueryHistoryService`.
*   **UI:** `HistoryView` UserControl displaying list of past queries.
*   **Interaction:** Clicking a history item loads it into the main Editor.

## Phase 3: One-Click Reporting
*   **Goal:** Export analysis results to HTML.
*   **Backend:** `HtmlReportBuilder` service to generate a styled HTML file from `AnalysisResult`.
*   **UI:** Add an "Export" icon/button to the Results header.

## Phase 4: AI Integration (UI & Mock)
*   **Goal:** Interface for AI-powered optimization suggestions.
*   **Backend:** `AiOptimizationService` (Mock implementation for now).
*   **UI:** "✨ Ask AI" button in the Editor toolbar.
*   **Interaction:** Opens a dialog or side panel with optimization tips.

## Todo List
1.  [ ] **Schema Browser:** Implement `SchemaService` and `SchemaView` control.
2.  [ ] **History:** Implement `HistoryView` and wire up `QueryHistoryService`.
3.  [ ] **Reporting:** Implement `HtmlReportBuilder` and Export button.
4.  [ ] **AI:** Add AI button and Mock Service.
5.  [ ] **MainForm Integration:** Update `MainForm` to switch views based on Sidebar selection.

