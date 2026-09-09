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
using System.Collections.Generic;
using System.Web;
using Serilog;
using Unity.TestHarness.Web.Models;
#endregion

namespace Unity.TestHarness.Web.Infrastructure
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s
     * LogViewModel: a shared output log, but held per-SESSION (List&lt;LogEntry&gt; in
     * Session state) rather than as a bound ObservableCollection, since there's no
     * single long-lived view model instance across requests the way there is in a
     * desktop app. Every entry ALSO goes through Serilog (file sinks only, see
     * Global.asax.cs), same dual-logging pattern as the WPF version. Static methods
     * (not an instance a controller holds) since there's no per-controller lifetime to
     * tie this to that would outlive a single request the way a WPF view model does,
     * every call reads/writes HttpContext.Current.Session directly.
     */
    #endregion

    /// <summary>
    /// The harness's shared output log, held per-session. Static, not an instance, every
    /// call reads/writes <see cref="HttpContext.Current"/>'s <c>Session</c> directly.
    /// </summary>
    public static class SessionLog
    {
        #region Private Members
        private const string SessionKey = "TestHarness.LogEntries";
        #endregion

        #region Public Methods
        /// <summary>
        /// This session's log entries, oldest first. Never null; an empty list is
        /// returned (and stored) the first time this is called for a session.
        /// </summary>
        /// <returns>The current session's log entries.</returns>
        public static List<LogEntry> GetEntries()
        {
            var session = HttpContext.Current?.Session;
            if (session == null) return [];

            if (session[SessionKey] is not List<LogEntry> entries)
            {
                entries = [];
                session[SessionKey] = entries;
            }

            return entries;
        }

        /// <summary>
        /// Clears this session's log entries.
        /// </summary>
        public static void Clear()
        {
            var session = HttpContext.Current?.Session;
            session?[SessionKey] = new List<LogEntry>();
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Info(string message)
        {
            Log.Information("{Message}", message);
            AddEntry(LogSeverity.Info, message);
        }

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Success(string message)
        {
            // Serilog has no dedicated "success" level; Information is the closest fit.
            Log.Information("{Message}", message);
            AddEntry(LogSeverity.Success, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public static void Error(string message)
        {
            Log.Error("{Message}", message);
            AddEntry(LogSeverity.Error, message);
        }

        /// <summary>
        /// Logs an exception, including the full <see cref="Exception.InnerException"/> chain.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public static void Error(Exception ex)
        {
            Log.Error(ex, "Exception occurred in Unity Test Harness Web");

            var current = ex;
            while (current != null)
            {
                AddEntry(LogSeverity.Error, $"{current.GetType().Name}: {current.Message}");
                current = current.InnerException;
            }
        }
        #endregion

        #region Private Methods
        // Add an entry to the session's log
        private static void AddEntry(LogSeverity severity, string message)
        {
            GetEntries().Add(new LogEntry(severity, message));
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
