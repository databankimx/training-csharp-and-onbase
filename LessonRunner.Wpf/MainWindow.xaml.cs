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
/// <remarks>Coordinates theme switching, markdown rendering, output display, argument parsing, and run lifecycle
/// management for snippet and external execution modes. Handles interactive lesson flow, including pause and continue
/// prompts and console input collection.</remarks>
public partial class MainWindow : Window
{
    #region State Fields
    // The root directory of the solution, used to locate chapters and lesson files.
    private readonly string        _solutionRoot;

    // The runner for executing code snippets within the application.
    private readonly SnippetRunner  _snippetRunner  = new();

    // The runner for executing code in an external process.
    private readonly ExternalRunner  _externalRunner = new();

    // Cancellation token source for managing the lifecycle of the current run operation.
    private CancellationTokenSource? _runCts;

    // The list of chapters discovered in the solution, used to populate the chapter selection UI.
    private List<ChapterEntry> _chapters     = [];

    // The list of steps for the currently selected chapter, used to populate the step selection UI.
    private List<LessonStep>   _currentSteps = [];

    // The currently selected lesson step, used to display its content and manage execution.
    private LessonStep?        _selectedStep;

    // Flag indicating whether a pause has been requested during execution, used to control the display of the continue button.
    private bool _pausePending;

    // The gate used to block the runner thread while waiting for user input or continue action.
    private ManualResetEventSlim? _continueGate;

    // The callback to invoke when the user provides input or clicks continue, used to resume execution.
    private Action<string>? _continueResult;

    // Sentinels used to signal special actions in the output stream, such as pausing or clearing the output.
    private const string PauseSentinel = "##LESSON_PAUSE##";
    private const string ClearSentinel  = "##LESSON_CLEAR##";
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <remarks>Initializes the window components, resolves the solution root path, applies the dark editor
    /// theme, and loads chapter data.</remarks>
    public MainWindow()
    {
        InitializeComponent();
        _solutionRoot = FindSolutionRoot();
        ThemeManager.Apply(AppTheme.Dark, SourceEditor, Application.Current.Resources);
        LoadChapters();
    }
    #endregion

    #region Theme Switching
    // Handles the Checked event of the ThemeToggle control to switch to light theme.
    private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(AppTheme.Light, SourceEditor, Application.Current.Resources);
        if (ChapterList.SelectedItem is ChapterEntry chapter)
            LoadLessonMd(chapter.ProjectFolder);
    }

    // Handles the Unchecked event of the ThemeToggle control to switch to dark theme.
    private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        ThemeManager.Apply(AppTheme.Dark, SourceEditor, Application.Current.Resources);
        if (ChapterList.SelectedItem is ChapterEntry chapter)
            LoadLessonMd(chapter.ProjectFolder);
    }
    #endregion

    #region Chapter and Step Navigation
    // Loads the list of chapters from the solution root and binds it to the ChapterList UI control.
    private void LoadChapters()
    {
        _chapters = DiscoverChapters(_solutionRoot);
        ChapterList.ItemsSource = _chapters;
    }

    // Handles the SelectionChanged event of the ChapterList control to load the steps for the selected chapter.
    private void ChapterList_SelectionChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ChapterList.SelectedItem is not ChapterEntry chapter) return;

        var lessonsDir = Path.Combine(_solutionRoot, chapter.ProjectFolder, "Lessons");
        _currentSteps  = [.. LessonStepParser.ParseDirectory(lessonsDir)];
        StepList.ItemsSource = _currentSteps;

        if (_currentSteps.Count > 0)
            StepList.SelectedIndex = 0;

        LoadLessonMd(chapter.ProjectFolder);
    }

    // Handles the SelectionChanged event of the StepList control to display the selected lesson step.
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
    // Displays the details of the selected lesson step in the UI, including title, subtitle, source code, and default arguments.
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

    // Clears the current lesson step display, resetting the title, subtitle, source code, and output display.
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

    // Loads the Lesson.md file for the specified project folder and renders it in the LessonViewer control.
    private void LoadLessonMd(string projectFolder)
    {
        var mdPath = Path.Combine(_solutionRoot, projectFolder, "Lesson.md");
        var markdown = File.Exists(mdPath)
            ? File.ReadAllText(mdPath)
            : "*No Lesson.md found for this chapter.*";
        LessonViewer.Document = MarkdownRenderer.Render(markdown);
    }
    #endregion

    #region Output Helpers
    // Clears the output display by removing all blocks from the OutputBox document.
    private void ClearOutputDisplay()
    {
        OutputBox.Document.Blocks.Clear();
    }

    // Handles the Click event of the ClearOutput button to clear the output display.
    private void ClearOutput_Click(object sender, RoutedEventArgs e)
        => ClearOutputDisplay();

    // Handles the Click event of the CopyOutput button to copy the output text to the clipboard and show a toast notification.
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

    // Handles the Click event of the CopyCode button to copy either the source code or the lesson markdown to the clipboard, depending on the selected tab, and show a toast notification.
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
            if (ChapterList.SelectedItem is ChapterEntry chapter)
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

    // Storyboards for managing toast notifications for output and code copy actions.
    private System.Windows.Media.Animation.Storyboard? _outputToastSb;
    private System.Windows.Media.Animation.Storyboard? _codeToastSb;

    // Displays a toast notification with the specified message, fading in and out over time, and stops any existing toast animation.
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

        AddAnim(0, 1,  0,    120);   // fade in
        AddAnim(1, 1,  120,  1400);  // hold
        AddAnim(1, 0,  1520, 400);   // fade out

        existing = sb;
        sb.Begin(border);
    }

    // Appends a line of text to the output display, optionally with a specified foreground color.
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

    // Retrieves the last line of text from the output display, trimming any whitespace.
    private string GetLastOutputLine()
    {
        if (OutputBox.Document.Blocks.LastBlock is not Paragraph last)
            return string.Empty;
        return new TextRange(last.ContentStart, last.ContentEnd).Text.Trim();
    }

    //Writes execution output to the display, including formatted error lines when execution fails and a completion
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
    // Shows the Continue button in the UI, making it visible and enabled for user interaction.
    private void ShowContinueButton()
    {
        ContinueButton.Visibility = Visibility.Visible;
        ContinueButton.IsEnabled  = true;
    }

    // Hides the Continue button in the UI, making it collapsed and disabled to prevent user interaction.
    private void HideContinueButton()
    {
        ContinueButton.Visibility = Visibility.Collapsed;
        ContinueButton.IsEnabled  = false;
    }

    // Handles the Click event of the Continue button, signaling the runner thread to continue execution after a pause.
    private void ContinueButton_Click(object sender, RoutedEventArgs e)
    {
        HideContinueButton();
        _continueResult?.Invoke(string.Empty);
        _continueGate?.Set();
        _continueGate  = null;
        _continueResult = null;
    }
    #endregion

    #region Run
    // Handles the Click event of the Run button, initiating the execution of the selected step.
    #pragma warning disable S3776 // Not excessively complex - complexity is acceptable here due to the nature of the run logic.
    private async void RunButton_Click(object sender, RoutedEventArgs e)
    #pragma warning restore S3776
    {
        if (_selectedStep is null) return;

        #pragma warning disable S6966 // Switch to async in future - this is a fire-and-forget event handler
        _runCts?.Cancel();
        #pragma warning restore S6966

        #pragma warning disable S2930 // CancellationTokenSource is disposed in the RunButton_Click event handler
        _runCts = new CancellationTokenSource();
        #pragma warning restore S2930
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
                    // This callback runs on the runner thread (a Task.Run thread pool thread).
                    // We must NOT block the UI thread here -- doing so while also waiting
                    // for a UI interaction (Continue click) causes a deadlock.
                    //
                    // Strategy: post UI work with InvokeAsync (fire and forget from the
                    // runner thread's perspective), then block the runner thread on a
                    // ManualResetEventSlim until the UI signals completion.
                    var gate   = new System.Threading.ManualResetEventSlim(false);
                    var inputResult = string.Empty;

                    Dispatcher.InvokeAsync(() =>
                    {
                        if (_pausePending)
                        {
                            _pausePending = false;
                            ShowContinueButton();
                            // ContinueButton_Click will set the result and release the gate.
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
                            gate.Set(); // release immediately after dialog closes
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
    // Parses a string of command-line arguments into an array of individual arguments, handling quoted strings and whitespace.
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

    // Discovers chapters in the solution root by enumerating directories that contain a "Lessons" subdirectory, formatting their names, and returning a list of ChapterEntry objects.
    private static List<ChapterEntry> DiscoverChapters(string solutionRoot)
    {
        return [.. Directory
            .EnumerateDirectories(solutionRoot)
            .Where(d => Directory.Exists(Path.Combine(d, "Lessons")))
            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
            .Select(d =>
            {
                var folder = Path.GetFileName(d);
                return new ChapterEntry(FormatChapterName(folder), folder);
            })];
    }
    
    // Formats a chapter folder name into a more readable chapter name by removing a "CSharp." prefix and inserting spaces before capital letters.
    private static string FormatChapterName(string folderName)
    {
        var name = folderName.StartsWith("CSharp.", StringComparison.OrdinalIgnoreCase)
            ? folderName["CSharp.".Length..]
            : folderName;

        return string.Join(" \u00b7 ", name.Split('.').Select(p =>
            Regex.Replace(p, @"(?<=[a-z])(?=[A-Z])", " ", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100))));
    }

    // The name of the solution file used to locate the solution root directory.
    // Future: Move this to a config file or environment variable for flexibility.
    private const string SolutionFileName = "DataBank.DeveloperTraining.sln";

    // Finds the root directory of the solution by traversing up from the application's base directory until it finds the solution file.
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
    // Represents a chapter entry with a display name and the corresponding project folder name.
    private sealed record ChapterEntry(string Name, string ProjectFolder);
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
