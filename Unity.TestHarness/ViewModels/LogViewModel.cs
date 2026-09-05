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
using System;
using System.Collections.ObjectModel;
using Unity.TestHarness.Models;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: this is new, the original 3-form WinForms harness had no shared
     * output log at all, each form's own controls (a DataGridView, a couple of TextBoxes)
     * WERE the output, and errors surfaced as MessageBox popups. A single shared log,
     * visible across every page via MainViewModel, is a much better fit for a DIAGNOSTIC
     * tool specifically: the whole point is seeing what happened and why, across
     * whatever sequence of operations you just tried, not losing that history the moment
     * you switch pages or dismiss a dialog.
     */
    #endregion

    /// <summary>
    /// Holds the harness's shared output log, written to by every page's view model.
    /// </summary>
    public class LogViewModel : ViewModelBase
    {
        #region Properties
        /// <summary>
        /// The log entries, newest last.
        /// </summary>
        public ObservableCollection<LogEntry> Entries { get; } = new ObservableCollection<LogEntry>();
        #endregion

        #region Commands
        /// <summary>
        /// Clears <see cref="Entries"/>.
        /// </summary>
        public RelayCommand ClearCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the LogViewModel class
        /// </summary>
        public LogViewModel()
        {
            ClearCommand = new RelayCommand(_ => Entries.Clear());
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(string message) => Log(LogSeverity.Info, message);

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Success(string message) => Log(LogSeverity.Success, message);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message) => Log(LogSeverity.Error, message);

        /// <summary>
        /// Logs an exception, including the full <see cref="Exception.InnerException"/> chain.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void Error(Exception ex)
        {
            while (ex != null)
            {
                Log(LogSeverity.Error, $"{ex.GetType().Name}: {ex.Message}");
                ex = ex.InnerException;
            }
        }
        #endregion

        #region Private Methods
        // Add an entry to the log
        private void Log(LogSeverity severity, string message)
        {
            Entries.Add(new LogEntry(severity, message));
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
