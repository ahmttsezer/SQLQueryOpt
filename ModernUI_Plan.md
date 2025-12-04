# Modern UI/UX Overhaul Plan for LinqSqlAnalyzer

This plan focuses on transforming the WinForms application from a standard utility into a professional-grade "SQL Intelligence Suite".

## Phase 1: Modern UI Framework (Custom Controls)
We will avoid external heavy UI libraries to keep the project lightweight, instead creating a tailored "Theme System".

*   **Color Palette:** VS Code Dark Theme inspired (`#1E1E1E`, `#2D2D30`, `#007ACC`, `#3E3E42`).
*   **ModernButton:** Override standard button. Remove borders, add flat style, custom hover colors, rounded corners.
*   **ModernTextBox:** Remove 3D borders, add bottom-border only (Material design style) or flat single border.
*   **CardPanel:** A container control with a slight drop shadow and rounded corners to display analysis steps.

## Phase 2: Layout Restructuring (Dashboard)
*   **Remove:** Standard `TabControl` headers.
*   **Add:** Left Sidebar (Navigation Rail) with Icons (using Unicode chars or drawn graphics to avoid image dependencies for now).
*   **Sections:**
    1.  **Editor:** SQL/LINQ Input area (Maximized space).
    2.  **Analysis:** Split view (Inputs on left/top, Visual Cards on right/bottom).
    3.  **History:** List of recent queries.
    4.  **Schema:** (Future) Table list.

## Phase 3: Advanced Editor (Syntax Highlighting)
*   **Integration:** Host the WPF `AvalonEdit` control inside WinForms using `ElementHost`. This gives us professional syntax highlighting without rewriting a WinForms editor from scratch.
*   **Features:** Line numbers, SQL coloring, bracket matching.

## Phase 4: Visual Analysis Results
*   Replace `RichTextBox` text dump with a `FlowLayoutPanel` containing dynamic "Result Cards".
*   **Success Card:** Green accent, Check icon, Step description.
*   **Error Card:** Red accent, detailed error message, "Fix Suggestion" button.
*   **Info Card:** For metadata (record counts, execution time).

## Todo List
1.  [ ] Create `ThemeColors` and `ModernControls` (Button, Panel) classes.
2.  [ ] Implement `ElementHost` integration for `AvalonEdit` in WinForms.
3.  [ ] Redesign `MainForm` layout (Sidebar + Dashboard).
4.  [ ] Implement `ResultCard` control for visualizing analysis steps.
5.  [ ] Wire up the `IntegratedAnalysisService` to the new UI.

