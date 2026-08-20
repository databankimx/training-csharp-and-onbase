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

#region Directives
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch07.Supplemental._02.UnblockingTheUI
{
    /// <summary>
    /// Illustration of UI blocking and unblocking
    /// </summary>
    public partial class UiUnblockingForm : Form
    {
        #region Constants
        // Number of seconds to sleep to simulate long-running process
        private const int SecondsToSleep = 15;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the UiUnblockingForm class
        /// </summary>
        public UiUnblockingForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Event Handlers
        // When the blocking process button is clicked, process in the UI thread
        private void BtnBlock_Click(object sender, EventArgs e)
        {
            Nap();
            MessageBox.Show(@"BLOCKED - All Done!", @"Work Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        // When the non-blocking process button is clicked, process in a background thread
        private void BtnUnblock_Click(object sender, EventArgs e)
        {
            /*
             * Section Notes:
             *
             * A BackgroundWorker abstracts the thread creation and operation process to create a single background thread
             */

            // Create the background thread
            var worker = new BackgroundWorker();

            // Assign delegate event handlers to the start & end work events
            worker.DoWork += OnDoWork;
            worker.RunWorkerCompleted += AfterDoWork;

            // Run the background worker
            if (!worker.IsBusy) worker.RunWorkerAsync();
        }
        #endregion

        #region BackgroundWorker Event Handlers
        // Work handler for background worker
        private static void OnDoWork(object sender, DoWorkEventArgs e)
        {
            Nap();
        }

        // Work completed handler for background worker
        private static void AfterDoWork(object sender, RunWorkerCompletedEventArgs e)
        {
            MessageBox.Show(@"UNBLOCKED - All Done!", @"Work Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        #endregion

        #region Helper Functions
        private static void Nap()
        {
            Thread.Sleep(1000 * SecondsToSleep);
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
