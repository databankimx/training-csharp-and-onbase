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
using System.Text.Json.Serialization;
#endregion

namespace RestApi.TestHarness.Web.Models
{
    #region Training Notes
    /*
     * *Migration Note: a close port of Unity.TestHarness.Web's own LogEntry, with one
     * real difference: this app's own SessionLogService (see its own Training Notes)
     * stores entries in ASP.NET Core's ISession as JSON (System.Text.Json), not as a
     * live object reference the way classic ASP.NET's InProc session mode allowed.
     * That means this class needs to round-trip through JSON correctly, including
     * Timestamp: the original two-parameter constructor alone would reset Timestamp to
     * "now" on every deserialization (losing when the entry was actually logged), so a
     * second, JsonConstructor-annotated constructor taking all three values explicitly
     * is what System.Text.Json actually uses when reading entries back out of session
     * storage; the original two-parameter constructor remains what callers use to
     * create a brand new entry.
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
    /// A single entry in the harness's shared output log, held per-session (see
    /// <see cref="Infrastructure.SessionLogService"/>).
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
        /// Create a new instance of the LogEntry class, timestamped now.
        /// </summary>
        /// <param name="severity">The entry's severity.</param>
        /// <param name="message">The log message.</param>
        public LogEntry(LogSeverity severity, string message) : this(DateTime.Now, severity, message) { }

        /// <summary>
        /// Create a new instance of the LogEntry class with an explicit timestamp. Used
        /// by System.Text.Json when deserializing entries back out of session storage,
        /// so the original Timestamp survives the round trip (see this class's own
        /// Training Notes).
        /// </summary>
        /// <param name="timestamp">When the entry was logged.</param>
        /// <param name="severity">The entry's severity.</param>
        /// <param name="message">The log message.</param>
        [JsonConstructor]
        public LogEntry(DateTime timestamp, LogSeverity severity, string message)
        {
            Timestamp = timestamp;
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
