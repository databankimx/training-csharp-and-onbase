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
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RestApi.TestHarness.Web.Models;
#endregion

namespace RestApi.TestHarness.Web.Infrastructure
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of RestApi.TestHarness (the WPF version)'s own
     * LogViewModel, and the ASP.NET Core counterpart to Unity.TestHarness.Web's own
     * (static) SessionLog. Genuinely reshaped in two ways, per this project's own
     * "modernize across the board" scope:
     *
     * - An ordinary, DI-registered (scoped) service taking IHttpContextAccessor in its
     *   constructor, not a static class reading HttpContext.Current the way
     *   Unity.TestHarness.Web's own SessionLog does (ASP.NET Core has no
     *   HttpContext.Current at all, IHttpContextAccessor is the idiomatic replacement,
     *   and DI-first is this whole project's own confirmed direction).
     *
     * - Entries are stored as a JSON string in ASP.NET Core's own ISession (via
     *   Session.SetString/GetString), not a live List&lt;LogEntry&gt; object reference:
     *   unlike SessionManagement (a genuinely non-serializable HttpClient holder, see
     *   SessionManagementStore's own Training Notes), a List&lt;LogEntry&gt; IS plain
     *   serializable data (timestamps/strings/an enum), so it can live directly in
     *   ASP.NET Core's session state after all, no separate server-memory store needed
     *   for this one. See LogEntry's own Training Notes for the JsonConstructor needed
     *   to round-trip Timestamp correctly.
     *
     * Every entry still also goes through Serilog (via ILogger&lt;T&gt;, DI-injected
     * rather than the static Serilog.Log class Unity.TestHarness.Web's own SessionLog
     * used directly), same dual-logging intent, just via the ASP.NET Core-idiomatic
     * logging abstraction instead of calling Serilog's own API directly.
     */
    #endregion

    /// <summary>
    /// The harness's shared output log, held per-session (as JSON in ASP.NET Core's own
    /// <see cref="ISession"/>, see this class's own Training Notes for why that's
    /// possible here but not for <see cref="SessionManagement"/>).
    /// </summary>
    /// <remarks>
    /// Create a new instance of the SessionLogService class
    /// </remarks>
    /// <param name="httpContextAccessor">Provides access to the current request's HttpContext/Session.</param>
    /// <param name="logger">Logger every entry is also written to.</param>
    public class SessionLogService(IHttpContextAccessor httpContextAccessor, ILogger<SessionLogService> logger)
    {
        #region Constants
        private const string SessionKey = "TestHarness.LogEntries";
        #endregion

        #region Public Methods
        /// <summary>
        /// This session's log entries, oldest first. Never null; an empty list is
        /// returned if none have been logged yet this session.
        /// </summary>
        /// <returns>The current session's log entries.</returns>
        public List<LogEntry> GetEntries()
        {
            var session = httpContextAccessor.HttpContext?.Session;
            var json = session?.GetString(SessionKey);

            if (string.IsNullOrEmpty(json)) return [];

            try
            {
                return JsonSerializer.Deserialize<List<LogEntry>>(json) ?? [];
            }
            catch (JsonException)
            {
                // Corrupt/unexpected session content, treat it as an empty log rather
                // than throwing on every subsequent request this session.
                return [];
            }
        }

        /// <summary>
        /// Clears this session's log entries.
        /// </summary>
        public void Clear()
        {
            httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }

        /// <summary>
        /// Logs an informational message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Info(string message)
        {
            logger.LogInformation("{Message}", message);
            AddEntry(LogSeverity.Info, message);
        }

        /// <summary>
        /// Logs a success message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Success(string message)
        {
            // ILogger has no dedicated "success" level; Information is the closest fit.
            logger.LogInformation("{Message}", message);
            AddEntry(LogSeverity.Success, message);
        }

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        public void Error(string message)
        {
            logger.LogError("{Message}", message);
            AddEntry(LogSeverity.Error, message);
        }

        /// <summary>
        /// Logs an exception, including the full <see cref="Exception.InnerException"/> chain.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        public void Error(Exception ex)
        {
            logger.LogError(ex, "Exception occurred in RestApi Test Harness Web");

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
        private void AddEntry(LogSeverity severity, string message)
        {
            var entries = GetEntries();
            entries.Add(new LogEntry(severity, message));
            SaveEntries(entries);
        }

        // Serialize and save the given entries back to session state
        private void SaveEntries(List<LogEntry> entries)
        {
            var session = httpContextAccessor.HttpContext?.Session;
            session?.SetString(SessionKey, JsonSerializer.Serialize(entries));
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
