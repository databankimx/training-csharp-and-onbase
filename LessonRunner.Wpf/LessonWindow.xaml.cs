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
    // Positions the lesson window beside the owner window on whichever monitor that window is on.
    private void PositionBesideOwner()
    {
        var workArea   = GetOwnerWorkArea();
        double desired = _owner.Left + _owner.Width + 8;

        if (desired + Width > workArea.Right)
            desired = _owner.Left - Width - 8;

        Left = Math.Max(workArea.Left, Math.Min(desired, workArea.Right - Width));
        Top  = Math.Max(workArea.Top,  Math.Min(_owner.Top, workArea.Bottom - Height));
    }

    // Returns the work area of the monitor that contains most of the owner window.
    private System.Windows.Rect GetOwnerWorkArea()
    {
        var helper = new System.Windows.Interop.WindowInteropHelper(_owner);
        var hMon   = NativeMethods.MonitorFromWindow(
                         helper.Handle, NativeMethods.MONITOR_DEFAULTTONEAREST);

        var info = new NativeMethods.MONITORINFO { cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf<NativeMethods.MONITORINFO>() };
        if (!NativeMethods.GetMonitorInfo(hMon, ref info))
            return SystemParameters.WorkArea;   // fallback

        // Convert physical pixels to WPF device-independent units.
        var source = System.Windows.PresentationSource.FromVisual(_owner);
        double dpiX = source?.CompositionTarget?.TransformFromDevice.M11 ?? 1.0;
        double dpiY = source?.CompositionTarget?.TransformFromDevice.M22 ?? 1.0;

        var r = info.rcWork;
        return new System.Windows.Rect(
            r.left  * dpiX, r.top    * dpiY,
            (r.right - r.left) * dpiX, (r.bottom - r.top) * dpiY);
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
        // Unsubscribe exactly once regardless of how we end up here.
        LessonViewModel.Instance.PropertyChanged -= ViewModel_PropertyChanged;

        if (!_suppressClose && _owner.IsLoaded && _owner.IsVisible)
        {
            // Cancel this close and let the owner re-dock us cleanly after the
            // current close event fully unwinds.  Using BeginInvoke avoids a
            // re-entrant Close() call while WPF is still processing this one.
            e.Cancel = true;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                () => _owner.DockLessonWindow());
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
