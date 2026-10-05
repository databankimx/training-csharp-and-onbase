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
using System.Runtime.CompilerServices;
#endregion

namespace LessonRunner.Wpf;

/// <summary>
/// Shared state for the lesson pane. Both MainWindow and LessonWindow
/// observe this so a chapter change in MainWindow propagates to whichever
/// window is currently showing the lesson content.
/// Stores the raw markdown text rather than a rendered FlowDocument because
/// a FlowDocument can only belong to one FlowDocumentScrollViewer at a time.
/// Each viewer renders its own independent document from the same source.
/// </summary>
public sealed class LessonViewModel : INotifyPropertyChanged
{
    #region Singleton
    /// <summary>
    /// Gets the singleton instance of <see cref="LessonViewModel"/>.
    /// </summary>
    /// <remarks>Initialized once when the type is first accessed and shared for the application
    /// lifetime.</remarks>
    public static LessonViewModel Instance { get; } = new();

    // Private constructor to prevent external instantiation.
    private LessonViewModel() { }
    #endregion

    #region Properties
    // Markdown content for the current chapter's Lesson.md file.
    private string _markdown = string.Empty;

    // Display name of the current chapter, shown in the pop-out window title.
    private string _chapterName = string.Empty;

    /// <summary>
    /// Raw markdown text for the current chapter's Lesson.md.
    /// </summary>
    public string Markdown
    {
        get => _markdown;
        set { _markdown = value; OnPropertyChanged(); }
    }

    /// <summary>
    /// Display name of the current chapter, shown in the pop-out window title.
    /// </summary>
    public string ChapterName
    {
        get => _chapterName;
        set { _chapterName = value; OnPropertyChanged(); }
    }
    #endregion

    #region INotifyPropertyChanged
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    // Raises the PropertyChanged event for the specified property name.
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
