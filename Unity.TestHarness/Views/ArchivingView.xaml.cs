#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
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
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Unity.TestHarness.ViewModels;
#endregion

namespace Unity.TestHarness.Views
{
    #region Training Notes
    /*
     * *Migration Note: drag/drop is a genuine VIEW concern (a view model has no way to
     * receive a WPF DragEventArgs, and shouldn't need to), handled here directly rather
     * than routed through a command: each drop zone's Drop handler reads the dropped file
     * paths and adds them straight to the matching ObservableCollection<string> on the
     * view model (NewFiles/RevisionFiles/RenditionFiles, all already public), the same
     * way RetrievalView's code-behind directly manipulates its ListBox for auto-scroll.
     *
     * CommandManager.InvalidateRequerySuggested() is called explicitly after a drop:
     * CommandManager.RequerySuggested (what actually drives a Button's visual
     * enabled/disabled state) only fires automatically on standard input events (mouse
     * clicks, keyboard, focus changes), NOT on an arbitrary ObservableCollection mutation
     * like this Add() from code-behind. Without the explicit call, a command whose
     * CanExecute depends on RenditionFiles.Count (e.g. AddRenditionCommand) stays visually
     * stale (looking disabled) after a drag/drop add, even though the underlying data
     * already allows it, until some unrelated input event happens to trigger a requery.
     */
    #endregion

    /// <summary>
    /// Interaction logic for ArchivingView.xaml
    /// </summary>
    public partial class ArchivingView : UserControl
    {
        /// <summary>
        /// Create a new instance of the ArchivingView class
        /// </summary>
        public ArchivingView()
        {
            InitializeComponent();

            NewFilesDropZone.Drop += (_, e) => HandleDrop(e, vm => vm.NewFiles);
            RevisionFilesDropZone.Drop += (_, e) => HandleDrop(e, vm => vm.RevisionFiles);
            RenditionFilesDropZone.Drop += (_, e) => HandleDrop(e, vm => vm.RenditionFiles);
        }

        // Add dropped file paths to the given ViewModel collection, skipping duplicates
        private void HandleDrop(DragEventArgs e, System.Func<ArchivingViewModel, ObservableCollection<string>> selectTarget)
        {
            if (DataContext is not ArchivingViewModel viewModel) return;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var target = selectTarget(viewModel);
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            #pragma warning disable S3267 // No need for LINQ here
            foreach (var path in paths)
            {
                if (!target.Contains(path)) target.Add(path);
            }
            #pragma warning restore S3267

            // See Training Notes above: forces an immediate CanExecute re-evaluation for
            // every command, since a plain collection mutation here doesn't trigger one
            // on its own.
            CommandManager.InvalidateRequerySuggested();
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
