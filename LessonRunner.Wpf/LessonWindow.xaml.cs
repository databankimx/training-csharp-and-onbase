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
using System.ComponentModel;
using System.Windows;
#endregion

namespace LessonRunner.Wpf;

/// <summary>
/// Represents a secondary window that displays lesson content and chapter title beside the main window, stays
/// synchronized with lesson view model updates, and supports docking back to the owner window when closed.
/// </summary>
/// <remarks>Subscribes to lesson view model property changes while open and unsubscribes during closing to avoid
/// stale event handlers. A standard close action is redirected to docking behavior unless explicitly closed without
/// docking.</remarks>
public partial class LessonWindow : Window
{
    #region Fields
    // Reference to the main window that owns this lesson window, used for positioning and docking behavior.
    private readonly MainWindow _owner;

    // Flag to indicate whether the window is being closed without docking, preventing the default docking behavior on close.
    private bool _suppressClose;
    #endregion

    #region Constructor
    /// <summary>
    /// Initializes a new instance of the <see cref="LessonWindow"/> class.
    /// </summary>
    /// <param name="owner">The main window that owns the lesson window.</param>
    public LessonWindow(MainWindow owner)
    {
        InitializeComponent();
        _owner = owner;

        PositionBesideOwner();

        var vm = LessonViewModel.Instance;
        vm.PropertyChanged += ViewModel_PropertyChanged;

        UpdateContent(vm.Markdown);
        UpdateTitle(vm.ChapterName);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Closes the window while suppressing docking behavior during the close operation.
    /// </summary>
    public void CloseWithoutDocking()
    {
        _suppressClose = true;
        Close();
    }
    #endregion

    #region Private Methods
    // Positions the lesson window beside the owner window, adjusting its position to ensure it remains within the screen bounds.
    private void PositionBesideOwner()
    {
        var screen = SystemParameters.WorkArea;
        double desiredLeft = _owner.Left + _owner.Width + 8;

        if (desiredLeft + Width > screen.Right)
            desiredLeft = _owner.Left - Width - 8;

        Left = Math.Max(screen.Left, desiredLeft);
        Top  = _owner.Top;
    }

    // Updates the lesson content displayed in the window based on the provided markdown string.
    private void UpdateContent(string markdown)
    {
        LessonViewer.Document = string.IsNullOrEmpty(markdown)
            ? null
            : MarkdownRenderer.Render(markdown);
    }

    // Updates the window title and chapter name text based on the provided chapter name, formatting it appropriately.
    private void UpdateTitle(string chapterName)
    {
        Title = string.IsNullOrEmpty(chapterName)
            ? "Lesson"
            : $"Lesson  \u2013  {chapterName}";
        ChapterNameText.Text = chapterName.ToUpperInvariant();
    }
    #endregion

    #region Event Handlers
    // Handles property changes in the lesson view model, updating the content or title of the window as necessary.
    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var vm = LessonViewModel.Instance;
        if (e.PropertyName == nameof(LessonViewModel.Markdown))
            UpdateContent(vm.Markdown);
        if (e.PropertyName == nameof(LessonViewModel.ChapterName))
            UpdateTitle(vm.ChapterName);
    }

    // Handles the click event of the dock button, triggering the docking behavior in the owner window.
    private void DockButton_Click(object sender, RoutedEventArgs e)
    {
        _owner.DockLessonWindow();
    }

    // Handles the closing event of the lesson window, unsubscribing from view model property changes and
    // preventing the default docking behavior if the window is being closed without docking.
    private void LessonWindow_Closing(object? sender, CancelEventArgs e)
    {
        LessonViewModel.Instance.PropertyChanged -= ViewModel_PropertyChanged;

        if (!_suppressClose)
        {
            // Only attempt to re-dock if the owner window is still open.
            // If the owner is closing or already closed, just let this window close too.
            if (_owner.IsLoaded && _owner.IsVisible)
            {
                e.Cancel = true;
                _owner.DockLessonWindow();
            }
        }
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
