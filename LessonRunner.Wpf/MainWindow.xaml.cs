#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using LessonRunner.Core.Execution;
using LessonRunner.Core.Models;
using LessonRunner.Core.Parsing;
#endregion

namespace LessonRunner.Wpf;

/// <summary>
/// Provides the main WPF window for the developer training app, including chapter and step navigation, lesson content
/// display, and code execution.
/// </summary>
public partial class MainWindow : Window
{
    #region State Fields
    private readonly string          _solutionRoot;
    private readonly SnippetRunner   _snippetRunner  = new();
    private readonly ExternalRunner  _externalRunner = new();
    private CancellationTokenSource? _runCts;
    private List<LessonStep>         _currentSteps = [];
    private LessonStep?              _selectedStep;
    private bool                     _pausePending;
    private ManualResetEventSlim?    _continueGate;
    private Action<string>?          _continueResult;
    private bool                     _suppressDirty;   // true while DisplayStep is loading source
    private const string PauseSentinel = "##LESSON_PAUSE##";
    private const string ClearSentinel  = "##LESSON_CLEAR##";
    #endregion

    #region Constructor
    public MainWindow()
    {
        InitializeComponent();
        _solutionRoot = FindSolutionRoot();
        ThemeManager.Apply(AppTheme.Dark, SourceEditor, Application.Current.Resources);
        SourceEditor.Document.TextChanged += SourceEditor_TextChanged;
        SourceEditor.PreviewKeyDown        += SourceEditor_PreviewKeyDown;
        LoadChapters();
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        _lessonWindow?.CloseWithoutDocking();
        _lessonWindow = null;
        base.OnClosing(e);
    }
    #endregion

    #region Lesson Pop-Out
    private LessonWindow? _lessonWindow;

    private void PopOutLesson_Click(object sender, RoutedEventArgs e)
    {
        if (_lessonWindow is null)
        {
            _lessonWindow = new LessonWindow(this);
            _lessonWindow.Show();

            LessonViewer.Visibility               = Visibility.Collapsed;
            LessonPoppedOutPlaceholder.Visibility  = Visibility.Visible;
            PopOutButton.Content                   = "\u2199 Dock";
            PopOutButton.ToolTip                   = "Re-dock lesson into the main window";
        }
        else
        {
            DockLessonWindow();
        }
    }

    public void DockLessonWindow()
    {
        if (_lessonWindow is null) return;

        _lessonWindow.CloseWithoutDocking();
        _lessonWindow = null;

        LessonViewer.Visibility               = Visibility.Visible;
        LessonPoppedOutPlaceholder.Visibility  = Visibility.Collapsed;
        PopOutButton.Content                   = "\u2197";
        PopOutButton.ToolTip                   = "Pop out lesson into a separate window";

        ContentTabs.SelectedIndex = 1;
    }
    #endregion

    #region Theme Switching
    private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(AppTheme.Light, SourceEditor, Application.Current.Resources);
        RefreshLessonAfterThemeChange();
    }

    private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(AppTheme.Dark, SourceEditor, Application.Current.Resources);
        RefreshLessonAfterThemeChange();
    }

    private void RefreshLessonAfterThemeChange()
    {
        var folder = ChapterTree.SelectedItem switch
        {
            ChapterEntry  e => e.ProjectFolder,
            ChapterGroup  g => g.Main.ProjectFolder,
            _               => null
        };
        if (folder is not null)
            LoadLessonMd(folder);
    }
    #endregion

    #region Chapter and Step Navigation
    private void LoadChapters()
    {
        ChapterTree.ItemsSource = DiscoverChapterGroups(_solutionRoot);
    }

    private void SelectChapter(ChapterEntry chapter)
    {
        var lessonsDir = Path.Combine(_solutionRoot, chapter.ProjectFolder, "Lessons");
        _currentSteps  = [.. LessonStepParser.ParseDirectory(lessonsDir)];
        StepList.ItemsSource = _currentSteps;

        if (_currentSteps.Count > 0)
            StepList.SelectedIndex = 0;

        LoadLessonMd(chapter.ProjectFolder);
    }

    private void ChapterTree_SelectedItemChanged(object sender,
        System.Windows.RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is ChapterEntry chapter)
            SelectChapter(chapter);
        else if (e.NewValue is ChapterGroup group)
            SelectChapter(group.Main);
    }

    // Fires when the user clicks the main chapter label in a group header.
    // Marks the TreeViewItem as selected so SelectedItemChanged fires naturally
    // rather than calling SelectChapter directly (which would leave SelectedItem null).
    private void ChapterHeader_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBlock tb)
        {
            var item = FindAncestor<System.Windows.Controls.TreeViewItem>(tb);
            if (item is not null)
                item.IsSelected = true;
        }
    }

    private void StepList_SelectionChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (StepList.SelectedItem is not LessonStep step)
        {
            ClearStep();
            return;
        }
        _selectedStep = step;
        DisplayStep(step);
    }
    #endregion

    #region Display Helpers
    private void DisplayStep(LessonStep step)
    {
        StepTitleText.Text    = step.Title;
        StepSubtitleText.Text =
            $"Step {step.Index}  \u00b7  Chapter {step.Chapter}  \u00b7  {step.TargetFramework}";

        _suppressDirty = true;
        SourceEditor.Document.Text = step.SourceCode ?? string.Empty;
        _suppressDirty = false;
        ClearDirty();

        if (!string.IsNullOrEmpty(step.DefaultArgs))
            ArgsBox.Text = step.DefaultArgs;

        HideContinueButton();
        HideVisualizationButton();
        ClearOutputDisplay();

        if (step.LaunchMode == LaunchMode.Browser)
        {
            // Browser steps have no code to run in-process - the button opens the page directly.
            RunButton.IsEnabled = false;
            RunButton.Visibility = Visibility.Collapsed;
            var resolved = ResolveBrowserUrl(step);
            ShowVisualizationButton(resolved);
        }
        else
        {
            RunButton.IsEnabled  = true;
            RunButton.Visibility = Visibility.Visible;
        }
    }

    private void ClearStep()
    {
        _selectedStep         = null;
        StepTitleText.Text    = "Select a step from the left panel";
        StepSubtitleText.Text = string.Empty;
        _suppressDirty = true;
        SourceEditor.Document.Text = string.Empty;
        _suppressDirty = false;
        ClearDirty();
        RunButton.IsEnabled  = false;
        RunButton.Visibility = Visibility.Visible;
        HideContinueButton();
        HideVisualizationButton();
        ClearOutputDisplay();
    }

    private void LoadLessonMd(string projectFolder)
    {
        var mdPath = Path.Combine(_solutionRoot, projectFolder, "Lesson.md");
        var markdown = File.Exists(mdPath)
            ? File.ReadAllText(mdPath)
            : "*No Lesson.md found for this chapter.*";

        var chapterEntry = ChapterTree.SelectedItem as ChapterEntry;
        LessonViewModel.Instance.ChapterName = chapterEntry?.Name ?? string.Empty;
        LessonViewModel.Instance.Markdown     = markdown;

        LessonViewer.Document = MarkdownRenderer.Render(markdown);
    }
    #endregion

    #region Output Helpers
    private void ClearOutputDisplay()
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(0),
        };
        // Bind the document's foreground to the RichTextBox so inherited colour
        // flows correctly into paragraphs that don't set an explicit foreground.
        doc.SetResourceReference(FlowDocument.ForegroundProperty, "ForegroundBrush");
        OutputBox.Document = doc;
    }

    private void ClearOutput_Click(object sender, RoutedEventArgs e)
        => ClearOutputDisplay();

    private void CopyOutput_Click(object sender, RoutedEventArgs e)
    {
        var text = new System.Windows.Documents.TextRange(
            OutputBox.Document.ContentStart,
            OutputBox.Document.ContentEnd).Text;
        if (!string.IsNullOrEmpty(text.Trim()))
        {
            Clipboard.SetText(text);
            ShowToast(OutputToastBorder, OutputToastText, "Output copied", ref _outputToastSb);
        }
    }

    private void CopyCode_Click(object sender, RoutedEventArgs e)
    {
        if (ContentTabs.SelectedIndex == 0)
        {
            var text = SourceEditor.Document.Text;
            if (!string.IsNullOrEmpty(text))
            {
                Clipboard.SetText(text);
                ShowToast(CodeToastBorder, CodeToastText, "Code copied", ref _codeToastSb);
            }
        }
        else
        {
            if (ChapterTree.SelectedItem is ChapterEntry chapter)
            {
                var mdPath = Path.Combine(_solutionRoot, chapter.ProjectFolder, "Lesson.md");
                if (File.Exists(mdPath))
                {
                    Clipboard.SetText(File.ReadAllText(mdPath));
                    ShowToast(CodeToastBorder, CodeToastText, "Markdown copied", ref _codeToastSb);
                }
            }
        }
    }

    private System.Windows.Media.Animation.Storyboard? _outputToastSb;
    private System.Windows.Media.Animation.Storyboard? _codeToastSb;

    private static void ShowToast(
        FrameworkElement border,
        System.Windows.Controls.TextBlock label,
        string message,
        ref System.Windows.Media.Animation.Storyboard? existing)
    {
        label.Text = message;
        existing?.Stop(border);

        var sb = new System.Windows.Media.Animation.Storyboard();

        void AddAnim(double from, double to, double beginMs, double durationMs)
        {
            var a = new System.Windows.Media.Animation.DoubleAnimation
            {
                From      = from,
                To        = to,
                Duration  = TimeSpan.FromMilliseconds(durationMs),
                BeginTime = TimeSpan.FromMilliseconds(beginMs),
            };
            System.Windows.Media.Animation.Storyboard.SetTarget(a, border);
            System.Windows.Media.Animation.Storyboard.SetTargetProperty(
                a, new PropertyPath(UIElement.OpacityProperty));
            sb.Children.Add(a);
        }

        AddAnim(0, 1,  0,    120);
        AddAnim(1, 1,  120,  1400);
        AddAnim(1, 0,  1520, 400);

        existing = sb;
        sb.Begin(border);
    }

    private void AppendOutputLine(string line, Brush? foreground = null)
    {
        var para = new Paragraph(new Run(line))
        {
            Margin = new Thickness(0),
        };

        // Only set an explicit foreground when overriding the default (errors, muted).
        // For normal output, leave it unset so it inherits from OutputBox.Foreground,
        // which is bound to ForegroundBrush via DynamicResource and updates with the theme.
        if (foreground is not null)
            para.Foreground = foreground;

        OutputBox.Document.Blocks.Add(para);
        OutputBox.ScrollToEnd();
    }

    private string GetLastOutputLine()
    {
        if (OutputBox.Document.Blocks.LastBlock is not Paragraph last)
            return string.Empty;
        return new TextRange(last.ContentStart, last.ContentEnd).Text.Trim();
    }

    private void DisplayResult(ExecutionResult result)
    {
        var errorBrush = (Brush)FindResource("ErrorBrush");
        var mutedBrush = (Brush)FindResource("MutedBrush");

        if (!result.Success)
        {
            AppendOutputLine(string.Empty);
            AppendOutputLine(
                "\u2500\u2500 Errors \u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500\u2500",
                errorBrush);
            foreach (var errLine in result.Error.Split('\n'))
                AppendOutputLine(errLine.TrimEnd('\r'), errorBrush);
        }

        AppendOutputLine(string.Empty);
        AppendOutputLine(
            $"\u2500\u2500 Finished in {result.Elapsed.TotalMilliseconds:F0} ms \u2500\u2500",
            mutedBrush);

        // Show the visualization button after a successful run if this step has one.
        if (result.Success)
        {
            var hasViz = !string.IsNullOrEmpty(_selectedStep?.VisualizationAlgorithm)
                      || !string.IsNullOrEmpty(_selectedStep?.BrowserUrl);
            if (hasViz)
            {
                var url = ResolveVisualizationUrl(_selectedStep!);
                if (!string.IsNullOrEmpty(url))
                    ShowVisualizationButton(url);
            }
        }
    }
    #endregion

    #region Visualization Button
    private void ShowVisualizationButton(string url)
    {
        VisualizationButton.Tag       = url;
        VisualizationButton.Visibility = Visibility.Visible;
        VisualizationButton.IsEnabled  = true;
    }

    private void HideVisualizationButton()
    {
        VisualizationButton.Visibility = Visibility.Collapsed;
        VisualizationButton.Tag        = null;
    }

    private void VisualizationButton_Click(object sender, RoutedEventArgs e)
    {
        if (VisualizationButton.Tag is string url && !string.IsNullOrEmpty(url))
        {
            // Write current-algorithm.js if this is a visualization player step.
            if (_selectedStep?.VisualizationAlgorithm is { Length: > 0 } algo)
                WriteCurrentAlgorithmJs(url, algo);

            OpenInBrowser(url);
        }
    }

    private static void WriteCurrentAlgorithmJs(string playerHtmlPath, string algorithmName)
    {
        // current-algorithm.js lives in the same directory as player.html.
        var dir     = Path.GetDirectoryName(playerHtmlPath) ?? string.Empty;
        var jsPath  = Path.Combine(dir, "current-algorithm.js");
        var escaped = algorithmName.Replace("\\", "\\\\").Replace("\"", "\\\"");
        File.WriteAllText(jsPath, $"window.CURRENT_ALGORITHM = \"{escaped}\";");
    }
    #endregion

    #region Keyboard Commands
    public static readonly RoutedCommand RunCommand      = new(nameof(RunCommand),      typeof(MainWindow));
    public static readonly RoutedCommand EscapeCommand   = new(nameof(EscapeCommand),   typeof(MainWindow));
    public static readonly RoutedCommand ContinueCommand = new(nameof(ContinueCommand), typeof(MainWindow));

    private void RunCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = RunButton.IsEnabled && RunButton.Visibility == Visibility.Visible;

    private void RunCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        => RunButton_Click(sender, new RoutedEventArgs());

    private void EscapeCommand_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        // Priority 1: cancel an active run
        if (_runCts is { IsCancellationRequested: false })
        {
            _runCts.Cancel();
            return;
        }
        // Priority 2: reset dirty editor to original
        if (ResetBar.Visibility == Visibility.Visible)
            ResetButton_Click(sender, new RoutedEventArgs());
    }

    private void ContinueCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        => e.CanExecute = ContinueButton.Visibility == Visibility.Visible;

    private void ContinueCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        => ContinueButton_Click(sender, new RoutedEventArgs());

    // AvalonEdit captures key events before window-level bindings fire.
    // PreviewKeyDown intercepts F5 and Escape while the editor has focus.
    private void SourceEditor_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        switch (e.Key)
        {
            case System.Windows.Input.Key.F5:
                if (RunButton.IsEnabled && RunButton.Visibility == Visibility.Visible)
                {
                    RunButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                break;

            case System.Windows.Input.Key.Escape:
                if (_runCts is { IsCancellationRequested: false })
                {
                    _runCts.Cancel();
                    e.Handled = true;
                }
                else if (ResetBar.Visibility == Visibility.Visible)
                {
                    ResetButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                break;

            case System.Windows.Input.Key.Enter
                when e.KeyboardDevice.Modifiers == (System.Windows.Input.ModifierKeys.Control
                                                  | System.Windows.Input.ModifierKeys.Shift):
                if (ContinueButton.Visibility == Visibility.Visible)
                {
                    ContinueButton_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                break;
        }
    }
    #endregion

    #region Dirty State
    private void SourceEditor_TextChanged(object? sender, EventArgs e)
    {
        if (_suppressDirty || _selectedStep is null) return;

        // Compare editor content to original step source to determine dirty state.
        // Use ordinal comparison - we care about exact character-level equality.
        bool isDirty = SourceEditor.Document.Text != (_selectedStep.SourceCode ?? string.Empty);
        if (isDirty)
            SetDirty();
        else
            ClearDirty();
    }

    private void SetDirty()
    {
        DirtyIndicator.Visibility = Visibility.Visible;
        ResetBar.Visibility       = Visibility.Visible;
    }

    private void ClearDirty()
    {
        DirtyIndicator.Visibility = Visibility.Collapsed;
        ResetBar.Visibility       = Visibility.Collapsed;
    }

    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedStep is null) return;
        _suppressDirty = true;
        SourceEditor.Document.Text = _selectedStep.SourceCode ?? string.Empty;
        _suppressDirty = false;
        ClearDirty();
    }
    #endregion

    #region Pause / Continue
    private void ShowContinueButton()
    {
        ContinueButton.Visibility = Visibility.Visible;
        ContinueButton.IsEnabled  = true;
    }

    private void HideContinueButton()
    {
        ContinueButton.Visibility = Visibility.Collapsed;
        ContinueButton.IsEnabled  = false;
    }

    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        HideContinueButton();
        _continueResult?.Invoke(string.Empty);
        _continueGate?.Set();
        _continueGate   = null;
        _continueResult = null;
    }
    #endregion

    #region Run
    private async void RunButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedStep is null) return;
        if (_selectedStep.LaunchMode == LaunchMode.Browser) return;

        _runCts?.Cancel();
        _runCts = new CancellationTokenSource();
        var token = _runCts.Token;

        RunButton.IsEnabled = false;
        RunButton.Content   = "Running\u2026";
        HideContinueButton();
        ClearOutputDisplay();

        try
        {
            var args = ParseArgs(ArgsBox.Text);
            var hwnd = new WindowInteropHelper(this).Handle;

            ILessonRunner runner = _selectedStep.LaunchMode == LaunchMode.External
                ? _externalRunner
                : _snippetRunner;

            if (_selectedStep.LaunchMode == LaunchMode.External)
                AppendOutputLine("[Launching as external process...]",
                    (Brush)FindResource("MutedBrush"));

            // Use whatever is currently in the editor - the user may have modified it.
            var stepToRun = SourceEditor.Document.Text != (_selectedStep.SourceCode ?? string.Empty)
                ? _selectedStep with { SourceCode = SourceEditor.Document.Text }
                : _selectedStep;

            var result = await runner.RunAsync(
                stepToRun,
                onOutputLine: line => Dispatcher.Invoke(() =>
                {
                    if (line == PauseSentinel)
                    {
                        _pausePending = true;
                        AppendOutputLine("\u2015\u2015 Click Continue to proceed \u2015\u2015",
                            (Brush)FindResource("MutedBrush"));
                    }
                    else if (line == ClearSentinel)
                    {
                        ClearOutputDisplay();
                    }
                    else
                    {
                        AppendOutputLine(line);
                    }
                }),
                onInputRequired: prompt =>
                {
                    var gate        = new System.Threading.ManualResetEventSlim(false);
                    var inputResult = string.Empty;

                    Dispatcher.InvokeAsync(() =>
                    {
                        if (_pausePending)
                        {
                            _pausePending = false;
                            ShowContinueButton();
                            _continueGate   = gate;
                            _continueResult = s => inputResult = s;
                        }
                        else
                        {
                            var lastLine = GetLastOutputLine();
                            var dialog   = new ConsoleInputDialog(lastLine, this, InputDialogMode.ReadLine);
                            if (dialog.ShowDialog() == true)
                            {
                                inputResult = dialog.Value;
                                AppendOutputLine(dialog.Value);
                            }
                            gate.Set();
                        }
                    });

                    gate.Wait(token);
                    return inputResult;
                },
                args: args,
                ownerHwnd: hwnd,
                cancellationToken: token);

            if (!token.IsCancellationRequested)
                DisplayResult(result);
        }
        catch (OperationCanceledException)
        {
            AppendOutputLine("[Run cancelled]");
        }
        catch (Exception ex)
        {
            AppendOutputLine($"[Runner error] {ex.Message}");
        }
        finally
        {
            RunButton.Content   = "\u25b6  Run";
            RunButton.IsEnabled = _selectedStep is not null;
            HideContinueButton();
        }
    }
    #endregion

    #region Helper Functions
    private static void OpenInBrowser(string url)
    {
        try
        {
            System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not open browser:\n{ex.Message}",
                "Browser Launch Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    // Resolves the URL for a browser-mode step (BrowserUrl field).
    private static string ResolveBrowserUrl(LessonStep step)
    {
        var url = step.BrowserUrl;
        if (string.IsNullOrWhiteSpace(url)) return string.Empty;

        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase)
         || url.StartsWith("file:", StringComparison.OrdinalIgnoreCase)
         || Path.IsPathRooted(url))
            return url;

        var stepDir = Path.GetDirectoryName(step.SourceFile) ?? string.Empty;
        return Path.GetFullPath(Path.Combine(stepDir, url));
    }

    // Resolves the visualization player URL for a step with a VisualizationAlgorithm.
    // Uses BrowserUrl if explicitly set; otherwise locates player.html by walking up from
    // the step file to find the Visualizations project alongside it.
    private static string ResolveVisualizationUrl(LessonStep step)
    {
        if (!string.IsNullOrWhiteSpace(step.BrowserUrl))
            return ResolveBrowserUrl(step);

        // Walk up from the step's Lessons/ directory to the solution root, then find the
        // Visualizations project alongside the other supplemental projects.
        var stepDir    = Path.GetDirectoryName(step.SourceFile) ?? string.Empty;
        var projectDir = Path.GetDirectoryName(stepDir) ?? string.Empty;  // e.g. .../Algorithms.Sort
        var solutionDir = Path.GetDirectoryName(projectDir) ?? string.Empty;
        var playerPath = Path.Combine(solutionDir,
            "CSharp.Supplemental.Algorithms.Visualizations", "player.html");

        return File.Exists(playerPath) ? playerPath : string.Empty;
    }

    private static string[] ParseArgs(string argsText)
    {
        if (string.IsNullOrWhiteSpace(argsText))
            return [];

        var args      = new List<string>();
        var current   = new System.Text.StringBuilder();
        bool inQuotes = false;

        foreach (char c in argsText)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (c == ' ' && !inQuotes)
            {
                if (current.Length > 0) { args.Add(current.ToString()); current.Clear(); }
            }
            else
                current.Append(c);
        }

        if (current.Length > 0) args.Add(current.ToString());
        return [.. args];
    }

    // Groups chapter projects by chapter number (e.g. "ch05").
    // Main projects form the group header; supplementals appear as children.
    // Standalone supplemental projects (no Ch## in the name) appear as their own top-level entries.
    private static List<ChapterGroup> DiscoverChapterGroups(string solutionRoot)
    {
        var entries = Directory
            .EnumerateDirectories(solutionRoot)
            .Where(d => Directory.Exists(Path.Combine(d, "Lessons")))
            .Where(d =>
            {
                // For the FactoryPattern series, only the .01 project hosts all three steps.
                // Exclude .02 and .03 from chapter discovery - their steps live in .01/Lessons/.
                var name = Path.GetFileName(d)!;
                if (name.Contains("FactoryPattern", StringComparison.OrdinalIgnoreCase)
                 && !name.Contains(".01.", StringComparison.OrdinalIgnoreCase))
                    return false;
                // TrieExamples has its one step hosted under DataStructureFundamentals.
                if (name.Equals("CSharp.Supplemental.TrieExamples", StringComparison.OrdinalIgnoreCase))
                    return false;
                return true;
            })
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
            .Select(d =>
            {
                var folder = Path.GetFileName(d)!;
                return (folder, entry: new ChapterEntry(FormatShortName(folder), folder));
            })
            .ToList();

        var grouped = new Dictionary<string, (ChapterEntry? Main, List<ChapterEntry> Supplementals)>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var (folder, entry) in entries)
        {
            var chapterMatch   = Regex.Match(folder, @"Ch\d+",
                                     RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
            var isSupplemental = folder.Contains("Supplemental",  StringComparison.OrdinalIgnoreCase)
                              || folder.Contains("TextbookCode",  StringComparison.OrdinalIgnoreCase);

            // A folder that is supplemental but has no chapter number is a standalone project
            // (e.g. CSharp.Supplemental.Algorithms.Sort). Treat it as its own main entry,
            // unless it shares a key with an already-registered standalone group (factory pattern).
            var isStandalone = isSupplemental && !chapterMatch.Success;

            var key = chapterMatch.Success
                ? chapterMatch.Value.ToLowerInvariant()
                : StandaloneGroupKey(folder);

            if (!grouped.TryGetValue(key, out var bucket))
                bucket = grouped[key] = (null, new List<ChapterEntry>());

            if (isStandalone || !isSupplemental)
                grouped[key] = (entry, bucket.Supplementals);
            else
                bucket.Supplementals.Add(entry);
        }

        return [.. grouped.Values
            .Where(b => b.Main is not null)
            .OrderBy(b => StandaloneSortOrder(b.Main!.ProjectFolder))
            .Select(b => new ChapterGroup(b.Main!.Name, b.Main, b.Supplementals))];
    }

    // Maps a standalone supplemental folder to a shared group key.
    private static string StandaloneGroupKey(string folder) => folder.ToLowerInvariant();

    // Explicit ordering for standalone supplemental groups; chapter groups sort by their
    // Ch## key and always appear before supplementals (prefix "s." sorts after digits).
    private static string StandaloneSortOrder(string projectFolder)
    {
        // Chapter projects: sort by their Ch## number naturally
        var m = Regex.Match(projectFolder, @"Ch(\d+)",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        if (m.Success) return $"a.{int.Parse(m.Groups[1].Value):D4}";

        // Standalone supplementals: explicit order
        if (projectFolder.Contains("BigOConcepts",              StringComparison.OrdinalIgnoreCase)) return "s.0100";
        if (projectFolder.Contains("DataStructureFundamentals", StringComparison.OrdinalIgnoreCase)) return "s.0200";
        if (projectFolder.Contains("Algorithms.Search",        StringComparison.OrdinalIgnoreCase)) return "s.0300";
        if (projectFolder.Contains("Algorithms.Sort",          StringComparison.OrdinalIgnoreCase)) return "s.0400";
        if (projectFolder.Contains("ReducingComplexity",       StringComparison.OrdinalIgnoreCase)) return "s.0500";
        if (projectFolder.Contains("Algorithms.Recursion",     StringComparison.OrdinalIgnoreCase)) return "s.0600";
        if (projectFolder.Contains("BitwiseOperations",        StringComparison.OrdinalIgnoreCase)) return "s.0700";
        if (projectFolder.Contains("StringPerformance",        StringComparison.OrdinalIgnoreCase)) return "s.0800";
        if (projectFolder.Contains("FactoryPattern",           StringComparison.OrdinalIgnoreCase)) return "s.0900";

        return $"s.9999.{projectFolder}";
    }

    // Formats a folder name into a short readable display name.
    // "CSharp.Ch05.ImplementingClassHierarchies" -> "Ch05 · Implementing Class Hierarchies"
    // "CSharp.Ch05.Supplemental.Cloning"         -> "Cloning"
    private static string FormatShortName(string folderName)
    {
        // Special cases where the auto-generated name would be misleading
        if (folderName.Equals("CSharp.Supplemental.FactoryPattern.01.NoFactory",
                StringComparison.OrdinalIgnoreCase))
            return "Factory Patterns";
        var name = folderName.StartsWith("CSharp.", StringComparison.OrdinalIgnoreCase)
            ? folderName["CSharp.".Length..]
            : folderName;

        return string.Join(" \u00b7 ", name.Split('.')
            .Where(p => !p.Equals("Supplemental", StringComparison.OrdinalIgnoreCase)
                     && !p.Equals("TextbookCode",  StringComparison.OrdinalIgnoreCase))
            .Select(p => Regex.Replace(p, @"(?<=[a-z])(?=[A-Z])", " ",
                         RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))));
    }

    private const string SolutionFileName = "DataBank.DeveloperTraining.sln";

    // Walks the visual tree upward from a starting element to find the nearest ancestor of type T.
    private static T? FindAncestor<T>(System.Windows.DependencyObject start)
        where T : System.Windows.DependencyObject
    {
        var current = System.Windows.Media.VisualTreeHelper.GetParent(start);
        while (current is not null)
        {
            if (current is T match) return match;
            current = System.Windows.Media.VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, SolutionFileName)))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new InvalidOperationException(
            $"Could not locate {SolutionFileName} above {AppContext.BaseDirectory}");
    }
    #endregion

    #region Nested Types
    // These are file-level types so XAML DataTemplate can reference them via local: namespace.
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion

// File-level types -- must be internal (not private nested) so XAML
// DataTemplate can reference them via the local: namespace prefix.
internal sealed record ChapterEntry(string Name, string ProjectFolder);

internal sealed record ChapterGroup(
    string ChapterLabel,
    ChapterEntry Main,
    List<ChapterEntry> Supplementals);
