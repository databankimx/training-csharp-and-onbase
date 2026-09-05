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

namespace Unity.TestHarness.Models
{
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
    public class LogEntry
    {
        #region Properties
        /// <summary>
        /// When this entry was logged.
        /// </summary>
        public DateTime Timestamp { get; }

        /// <summary>
        /// The entry's severity.
        /// </summary>
        public LogSeverity Severity { get; }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// The entry formatted as a single display line: "[HH:mm:ss] Message".
        /// </summary>
        public string DisplayText => $"[{Timestamp:HH:mm:ss}] {Message}";
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the LogEntry class
        /// </summary>
        /// <param name="severity">The entry's severity.</param>
        /// <param name="message">The log message.</param>
        public LogEntry(LogSeverity severity, string message)
        {
            Timestamp = DateTime.Now;
            Severity = severity;
            Message = message;
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
