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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch07.Supplemental._03.TaskParallelLibrary
{
    /// <summary>
    /// Form to demonstrate using a task to update the UI
    /// </summary>
    public partial class ParentForm : Form
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of the ParentForm class
        /// </summary>
        public ParentForm()
        {
            InitializeComponent();
        }
        #endregion

        #region Form Event Handlers
        // Run task when BtnCannot is clicked
        private void BtnCannot_Click(object sender, EventArgs e)
        {
            // Because this is not executed by the UI thread, it will throw an exception when attempting to update the UI
            Task.Factory.StartNew(() => UpdateLabel("BtnCannot"));
        }

        // Run task when BtnCan is clicked
        private void BtnCan_Click(object sender, EventArgs e)
        {
            // Here, we ensure that the UI thread executes the Task, so it can update the UI
            Task.Factory.StartNew(() => UpdateLabel("BtnCan"), CancellationToken.None, TaskCreationOptions.None,
                TaskScheduler.FromCurrentSynchronizationContext());
        }
        #endregion

        #region Helper Functions
        // Update the "source" label on the form
        private void UpdateLabel(string message)
        {
            try
            {
                LblSource.Text = message;
            }
            catch (Exception ex)
            {
                string nl = Environment.NewLine;
                while (ex != null)
                {
                    MessageBox.Show($@"{ex.GetType().Name}: {ex.Message}{nl}{nl}Stack Trace:{nl}{ex.StackTrace}",
                        @"Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ex = ex.InnerException;
                }
            }
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
