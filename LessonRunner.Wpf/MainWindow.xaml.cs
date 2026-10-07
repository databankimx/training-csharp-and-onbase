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
    private const string PauseSentinel = "##LESSON_PAUSE##";
    private const string ClearSentinel  = "##LESSON_CLEAR##";
    #endregion

    #region Constructor
    public MainWindow()
    {
        InitializeComponent();
        _solutionRoot = FindSolutionRoot();
        ThemeManager.Apply(AppTheme.Dark, SourceEditor, Application.Current.Resources);
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

        SourceEditor.Document.Text = step.SourceCode;

        if (!string.IsNullOrEmpty(step.DefaultArgs))
            ArgsBox.Text = step.DefaultArgs;

        RunButton.IsEnabled = true;
        HideContinueButton();
        ClearOutputDisplay();
    }

    private void ClearStep()
    {
        _selectedStep              = null;
        StepTitleText.Text         = "Select a step from the left panel";
        StepSubtitleText.Text      = string.Empty;
        SourceEditor.Document.Text = string.Empty;
        RunButton.IsEnabled        = false;
        HideContinueButton();
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
        OutputBox.Document.Blocks.Clear();
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
        var brush = foreground ?? (Brush)FindResource("ForegroundBrush");
        var para  = new Paragraph(new Run(line))
        {
            Margin     = new Thickness(0),
            Foreground = brush,
        };
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

            var result = await runner.RunAsync(
                _selectedStep,
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
    private static List<ChapterGroup> DiscoverChapterGroups(string solutionRoot)
    {
        var entries = Directory
            .EnumerateDirectories(solutionRoot)
            .Where(d => Directory.Exists(Path.Combine(d, "Lessons")))
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
            var key            = ExtractChapterKey(folder);
            var isSupplemental = folder.Contains("Supplemental",  StringComparison.OrdinalIgnoreCase)
                              || folder.Contains("TextbookCode",  StringComparison.OrdinalIgnoreCase);

            if (!grouped.TryGetValue(key, out var bucket))
                bucket = grouped[key] = (null, new List<ChapterEntry>());

            if (isSupplemental)
                bucket.Supplementals.Add(entry);
            else
                grouped[key] = (entry, bucket.Supplementals);
        }

        return [.. grouped.Values
            .Where(b => b.Main is not null)
            .Select(b => new ChapterGroup(b.Main!.Name, b.Main, b.Supplementals))];
    }

    // Extracts a normalised chapter key such as "ch05" from a folder name.
    private static string ExtractChapterKey(string folderName)
    {
        var match = Regex.Match(folderName, @"Ch\d+",
            RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(100));
        return match.Success ? match.Value.ToLowerInvariant() : folderName.ToLowerInvariant();
    }

    // Formats a folder name into a short readable display name.
    // "CSharp.Ch05.ImplementingClassHierarchies" -> "Ch05 · Implementing Class Hierarchies"
    // "CSharp.Ch05.Supplemental.Cloning"         -> "Cloning"
    private static string FormatShortName(string folderName)
    {
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
