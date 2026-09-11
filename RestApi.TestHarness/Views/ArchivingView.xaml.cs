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
using RestApi.TestHarness.ViewModels;
#endregion

namespace RestApi.TestHarness.Views
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own ArchivingView.xaml.cs,
     * purely generic drag/drop wiring with no Unity API/REST API dependency at all.
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

            foreach (var path in paths)
            {
                if (!target.Contains(path)) target.Add(path);
            }

            // See that class's own Training Notes: forces an immediate CanExecute
            // re-evaluation for every command, since a plain collection mutation here
            // doesn't trigger one on its own.
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
