# LessonRunner.Wpf

The WPF application that hosts the developer training experience. Depends on `LessonRunner.Core` for parsing and execution; everything in this project is UI and presentation.

Targets **net10.0-windows**. The core library also targets net10 - neither project uses net48 even though the lesson steps themselves are written as net48 code (see `LessonRunner.Core/README.md` for why that works).

---

## Project structure

```
LessonRunner.Wpf/
├── App.xaml / App.xaml.cs          Application entry point, global styles and brush resources
├── MainWindow.xaml / .cs           Primary window: navigation, output, source/lesson tabs
├── LessonWindow.xaml / .cs         Pop-out lesson window (secondary, optional)
├── LessonViewModel.cs              Shared state between MainWindow and LessonWindow
├── MarkdownRenderer.cs             Converts Markdig AST to WPF FlowDocument
├── ThemeManager.cs                 Applies dark/light theme to app resources and MarkdownRenderer
├── ConsoleInputDialog.xaml / .cs   Modal dialog for Console.ReadLine() and Pause() prompts
├── NativeMethods.cs                P/Invoke for multi-monitor positioning
├── KeyboardRecovery.cs             Restores keyboard state after external process launches
└── Highlighting/
    ├── CSharp-Dark.xshd            AvalonEdit syntax highlighting - dark theme
    └── CSharp-Light.xshd           AvalonEdit syntax highlighting - light theme
```

---

## Chapter and step discovery

`MainWindow` discovers chapters by walking the solution root directory at startup. The solution root is found by walking up from `AppContext.BaseDirectory` until a directory containing `DataBank.DeveloperTraining.sln` is found.

A project directory is included in the chapter list if and only if it contains a `Lessons/` subdirectory. Projects without `Lessons/` are invisible to the runner (they exist for Visual Studio reference only).

**Grouping:** Projects are grouped by chapter number, extracted from the folder name by matching `Ch\d+`. Within a group, the main project (the one whose name doesn't contain `Supplemental` or `TextbookCode`) becomes the group header in the TreeView; supplementals and textbook code projects appear as expandable children.

**Display names** are generated from the folder name: `CSharp.Ch05.ImplementingClassHierarchies` becomes `Ch05 · Implementing Class Hierarchies`. The `CSharp.` prefix, `Supplemental`, and `TextbookCode` segments are stripped; remaining segments are split on camel-case boundaries and joined with ` · `.

Step files are parsed by `LessonStepParser.ParseDirectory` (see the Core README). The step list is populated when a chapter is selected and sorted by filename.

---

## The two-pane layout

```
┌─────────────────────────────────────────────────────────┐
│  Left panel (240px, resizable)  │  Right panel           │
│  ─────────────────────────────  │  ──────────────────── │
│  CHAPTERS (TreeView)            │  Title bar + Run       │
│  ─ splitter ─                   │  Args bar              │
│  STEPS (ListBox)                │  Output pane           │
│                                 │  ─ splitter ─          │
│                                 │  Source | Lesson tabs  │
└─────────────────────────────────────────────────────────┘
```

All splitters are live `GridSplitter` elements - the user can resize any of them. Row and column heights are preserved within a session but not across restarts (they're in `.suo`, not persisted separately).

---

## Theming

### Resource dictionary

All theme-sensitive colours live in `App.xaml` as `Color` resources (`BackgroundColor`, `PanelColor`, etc.) with corresponding `SolidColorBrush` resources (`BackgroundBrush`, `PanelBrush`, etc.). All control styles reference these brushes as `DynamicResource`, so replacing a brush instance in the resource dictionary immediately updates every element bound to it.

### ThemeManager

`ThemeManager.Apply(theme, editor, appResources)` is the single entry point for theme changes. It:

1. Calls `SetColor` to replace the `Color` values in the resource dictionary
2. Calls `RefreshBrushes` to replace the `SolidColorBrush` instances with new ones using the updated colours (WPF doesn't auto-update brush instances when their source colour changes)
3. Sets AvalonEdit properties directly (`editor.Background`, `editor.Foreground`, `editor.SyntaxHighlighting`)
4. Sets `MarkdownRenderer` static properties so the next render uses the new colours

`RefreshBrushes` works by iterating over all keys in the resource dictionary that end in `Brush`, finding the corresponding `Color` key, and replacing the brush with a new `SolidColorBrush(color)`. This is why `DynamicResource` is required throughout - WPF resolves `DynamicResource` at runtime against the current dictionary contents, so replacing the brush instance is immediately visible to all bound elements.

**Light theme colours** follow the VS Code light theme model: `PanelColor` is `#F3F3F3` (slightly off-white sidebar) rather than pure white, giving the navigation chrome enough contrast against the `#FFFFFF` content background.

### Syntax highlighting

AvalonEdit uses `.xshd` files (XML Syntax Highlighting Definition) embedded as resources in the project. `CSharp-Dark.xshd` and `CSharp-Light.xshd` are loaded via `ThemeManager.LoadXshd`, which reads the embedded resource by name and passes it to `HighlightingLoader.Load`. Theme switching replaces `editor.SyntaxHighlighting` with the appropriate definition.

---

## Markdown rendering

`MarkdownRenderer` converts a Markdown string into a WPF `FlowDocument`. It is a static class with static properties for colours and fonts; `ThemeManager` sets these before calling `Render`, so each render picks up the current theme.

**Parse pipeline:** Markdig parses the Markdown into an AST using `MarkdownPipelineBuilder().UseAdvancedExtensions()`. The renderer walks the AST and maps each node type to a WPF document element:

| Markdig type | WPF type |
|---|---|
| `HeadingBlock` | `Paragraph` (sized by level, coloured with `HeadingColor`) |
| `FencedCodeBlock` | `Section` > `Paragraph` (monospace, `CodeBgColor`, `CodeFgColor`) |
| `CodeBlock` (indented) | `Paragraph` (same treatment) |
| `ListBlock` | `List` with `ListItem` per item |
| `ThematicBreakBlock` | `BlockUIContainer` with a 1px `Rectangle` |
| `QuoteBlock` | `Section` in `MutedColor` |
| `ParagraphBlock` | `Paragraph` with inline rendering |

Inline types: `LiteralInline` → `Run`; `EmphasisInline` → bold/italic `Span`; `CodeInline` → monospace `Run`; `LinkInline` → underlined `Span`; `LineBreakInline` → `LineBreak`.

**The `FlowDocument` belongs to one viewer at a time.** WPF enforces this - a `FlowDocument` cannot be assigned to two `FlowDocumentScrollViewer` instances simultaneously. This is why `LessonViewModel` stores the raw Markdown string rather than a rendered document; when the pop-out window is open, both it and the main window's (invisible) `LessonViewer` maintain independent `FlowDocument` instances rendered from the same source string.

**Theme switching re-renders the document.** `ThemeManager` updates `MarkdownRenderer`'s static properties; the theme-toggle handler then calls `LoadLessonMd` again, which calls `MarkdownRenderer.Render` and assigns the new document to `LessonViewer`. The `FlowDocument.Background` is set explicitly from `MarkdownRenderer.ProseBackground` (not `Brushes.Transparent`) because the `FlowDocumentScrollViewer`'s internal element tree doesn't consistently propagate background through to the document.

---

## The pop-out lesson window

`LessonWindow` is a secondary `Window` that displays the lesson content beside the main window. It opens when the pop-out button (↗) is clicked and re-docks when closed normally.

**Positioning:** `PositionBesideOwner` places the window to the right of the main window on the same monitor, determined via `NativeMethods.MonitorFromWindow` and `GetMonitorInfo`. Physical pixel coordinates from the Win32 API are converted to WPF device-independent units using the composition target's `TransformFromDevice` matrix, which handles non-100% DPI scaling correctly.

**Synchronisation:** Both windows observe `LessonViewModel.Instance` via `INotifyPropertyChanged`. When the main window loads a new chapter, it sets `LessonViewModel.Instance.Markdown` and `LessonViewModel.Instance.ChapterName`. `LessonWindow` subscribes to `PropertyChanged` and re-renders immediately.

**Close / re-dock mechanics:** Closing the pop-out window normally should re-dock rather than simply close. `LessonWindow_Closing` cancels the close event and calls `Dispatcher.BeginInvoke` to schedule `DockLessonWindow` after the current event unwinds. `BeginInvoke` rather than direct `Invoke` is required because calling `Close()` on the `LessonWindow` from inside its own `Closing` handler would be re-entrant and cause a stack overflow. The `_suppressClose` flag bypasses this logic when `MainWindow` itself closes and calls `CloseWithoutDocking` directly.

---

## Console I/O in the runner

The `SnippetRunner` replaces `Console.In` with a `CallbackReader` before invoking the snippet's `Main()`. Every call to `Console.ReadLine()` inside the snippet invokes `onInputRequired` on the runner thread, which blocks until the WPF layer responds.

`MainWindow.RunAsync` provides the `onInputRequired` callback. The callback dispatches to the UI thread via `Dispatcher.InvokeAsync`, then blocks the runner thread on a `ManualResetEventSlim`. On the UI thread it either:

- Shows the **Continue button** (if `_pausePending` is true, meaning the snippet hit a `Pause()` sentinel)
- Shows the **`ConsoleInputDialog`** (for genuine `Console.ReadLine()` calls)

The runner thread is unblocked when the user clicks Continue or submits input.

**`ConsoleInputDialog`** has two modes, controlled by `InputDialogMode`:

- `Pause` - hides the text input field and shows a Continue button. Used for `GenericFunctions.Pause()`.
- `ReadLine` - shows a text input field and an OK button. Used for genuine input requests.

The prompt text passed to the dialog is the last line of output, extracted from the output pane, so the dialog shows whatever the snippet printed as a prompt (e.g. `"Enter your name:"`).

---

## Adding a new feature to the UI

**New theme colour:** Add a `Color` + `Brush` pair to `App.xaml`, then add the corresponding `SetColor` and `RefreshBrushes` calls (the latter is automatic) in both `ApplyDark` and `ApplyLight` in `ThemeManager`. Reference the brush as `DynamicResource` in XAML.

**New Markdown element type:** Add a case to `MarkdownRenderer.RenderBlock` or `RenderInline`. Markdig's AST node types are in `Markdig.Syntax` and `Markdig.Syntax.Inlines`.

**New frontmatter field affecting UI:** Read it from `LessonStep` in `MainWindow.DisplayStep` or `SelectChapter`. The parser already exposes it; the WPF layer just needs to consume it.

**New assembly needed by snippet steps:** Add it to the preload list in `SnippetRunner.Compile()`. If the error is CS1069 ("type has been forwarded"), also add a `ForceLoad` call. See `LessonRunner.Core/README.md` for the full explanation.
