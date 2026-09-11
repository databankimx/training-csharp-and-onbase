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
#endregion

namespace RestApi.TestHarness.Models
{
    #region Training Notes
    /*
     * *Migration Note: identical to Unity.TestHarness's own LogEntry, purely generic UI
     * infrastructure with no Unity API/REST API dependency at all.
     */
    #endregion

    /// <summary>
    /// The severity of a <see cref="LogEntry"/>, used to color its display in the output panel.
    /// </summary>
    public enum LogSeverity
    {
        /// <summary>
        /// General informational message.
        /// </summary>
        Info,

        /// <summary>
        /// An operation completed successfully.
        /// </summary>
        Success,

        /// <summary>
        /// An operation failed, or an exception was caught.
        /// </summary>
        Error
    }

    /// <summary>
    /// A single entry in the harness's shared output log.
    /// </summary>
    /// <remarks>
    /// Create a new instance of the LogEntry class
    /// </remarks>
    /// <param name="severity">The entry's severity.</param>
    /// <param name="message">The log message.</param>
    public class LogEntry(LogSeverity severity, string message)
    {
        #region Properties
        /// <summary>
        /// When this entry was logged.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>
        /// The entry's severity.
        /// </summary>
        public LogSeverity Severity { get; } = severity;

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; } = message;

        /// <summary>
        /// The entry formatted as a single display line: "[HH:mm:ss] Message".
        /// </summary>
        public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Message}";
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
