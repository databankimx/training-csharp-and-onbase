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
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
using System.Xml.Linq;
using Hyland.Unity;
using Serilog;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._01.ConnectingToOnBase.HelperClasses.OnBase;
using UnityApplication = Hyland.Unity.Application;
#endregion

namespace Unity.TestHarness.Web.Infrastructure
{
    #region Training Notes
    /*
     * *Migration Note: web equivalent of Unity.TestHarness (the WPF version)'s
     * ConnectionViewModel: the live Unity API Application object is held in Session
     * (InProc mode, see Web.config), not as a field on a long-lived view model instance,
     * every controller reads GetCurrentApplication() at the start of each action.
     * InProc is a genuine constraint here, not just a default: it's the only built-in
     * ASP.NET session mode that can hold a non-serializable object reference at all
     * (StateServer/SQLServer both require session content to serialize). This means the
     * connection doesn't survive an app pool recycle and ties this app to a single
     * server without further work; the documented alternative (not implemented in this
     * first pass) is reconnecting by Session ID on every request instead of holding the
     * Application object itself, using ReconnectBySessionId below, the same capability
     * the WPF version's own "Reconnect to Session ID" button exercises.
     *
     * Same two reconnect entry points as the WPF version: Connect() uses whatever's
     * currently configured (ServiceLocation.SessionId first if set, falling back to
     * AuthenticationMode), while ReconnectBySessionId exercises Connect(string) directly,
     * an ad hoc session ID independent of configured Settings.
     *
     * DisconnectOrphanedSession exists for a genuinely different problem: a browser tab
     * closing sends the server NO signal at all (unlike WPF's MainWindow.Closing, which
     * fires reliably), HTTP is stateless. The only thing ASP.NET can act on is session
     * EXPIRATION (Global.asax's Session_End, which only fires under InProc mode, already
     * in use here), so this is a delayed safety net (up to the configured session
     * timeout), not immediate cleanup, and Session_End's own HttpSessionState is passed
     * in explicitly rather than read via HttpContext.Current, which is unreliable inside
     * Session_End specifically, a well-known ASP.NET gotcha. Logs straight to Serilog
     * (not SessionLog), the session's own Output panel is already gone by the time this
     * runs, no one would ever see an entry written there.
     */
    #endregion

    /// <summary>
    /// The harness's connection state, held per-session. Static, not an instance, every
    /// call reads/writes <see cref="HttpContext.Current"/>'s <c>Session</c> directly.
    /// </summary>
    public static class SessionConnectionManager
    {
        #region Private Members
        private const string SessionKey = "TestHarness.CurrentApplication";
        private const string ServerAvailableKey = "TestHarness.IsServerAvailable";
        private const string ServerStatusMessageKey = "TestHarness.ServerStatusMessage";
        #endregion

        #region Public Methods
        /// <summary>
        /// The connected Unity API Application object for the current session, or
        /// <see langword="null"/> if not currently connected.
        /// </summary>
        /// <returns>The current session's connected Application, or <see langword="null"/>.</returns>
        public static UnityApplication GetCurrentApplication()
        {
            return HttpContext.Current?.Session?[SessionKey] as UnityApplication;
        }

        /// <summary>
        /// Whether the current session is currently connected.
        /// </summary>
        /// <returns><see langword="true"/> if connected.</returns>
        public static bool IsConnected()
        {
            return GetCurrentApplication() != null;
        }

        /// <summary>
        /// The connected session's ID, or <see langword="null"/> if not connected.
        /// </summary>
        /// <returns>The connected session's ID, or <see langword="null"/>.</returns>
        public static string GetSessionId()
        {
            return GetCurrentApplication()?.SessionID;
        }

        /// <summary>
        /// The connected user's display name, or <see langword="null"/> if not connected.
        /// </summary>
        /// <returns>The connected user's display name, or <see langword="null"/>.</returns>
        public static string GetCurrentUserDisplayName()
        {
            return GetCurrentApplication()?.CurrentUser?.DisplayName;
        }

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        /// <returns>The currently-configured Service Path.</returns>
        public static string GetServicePath()
        {
            return SessionManagement.ServiceLocation?.ServicePath;
        }

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        /// <returns>The currently-configured Data Source.</returns>
        public static string GetDataSource()
        {
            return SessionManagement.ServiceLocation?.DataSource;
        }

        /// <summary>
        /// A read-only summary of the currently-configured connection settings (see the
        /// Settings page to change them).
        /// </summary>
        /// <returns>The currently-configured Authentication Mode.</returns>
        public static AuthenticationMode GetAuthenticationMode()
        {
            return SessionManagement.ServiceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials;
        }

        /// <summary>
        /// Reads/writes SessionManagement.ServiceLocation.KeepAlive directly, so toggling
        /// it takes effect immediately without a trip to Settings.
        /// </summary>
        /// <returns>The currently-configured KeepAlive value.</returns>
        public static bool GetKeepAlive()
        {
            return SessionManagement.ServiceLocation?.KeepAlive ?? false;
        }

        /// <summary>
        /// Sets SessionManagement.ServiceLocation.KeepAlive directly.
        /// </summary>
        /// <param name="value">The new KeepAlive value.</param>
        public static void SetKeepAlive(bool value)
        {
            if (SessionManagement.ServiceLocation != null) SessionManagement.ServiceLocation.KeepAlive = value;
        }

        /// <summary>
        /// Whether the Application Server was reachable the last time <see cref="TestServer"/>
        /// was called this session, or <see langword="null"/> if it hasn't been checked yet.
        /// </summary>
        /// <returns>The last known server availability, or <see langword="null"/>.</returns>
        public static bool? GetIsServerAvailable()
        {
            return HttpContext.Current?.Session?[ServerAvailableKey] as bool?;
        }

        /// <summary>
        /// A short human-readable description of the last <see cref="TestServer"/> check,
        /// or <see langword="null"/> if it hasn't been checked yet.
        /// </summary>
        /// <returns>The last server status message, or <see langword="null"/>.</returns>
        public static string GetServerStatusMessage()
        {
            return HttpContext.Current?.Session?[ServerStatusMessageKey] as string;
        }

        /// <summary>
        /// Connects using whatever's currently configured in Settings, storing the
        /// resulting Application in the current session.
        /// </summary>
        /// <returns><see langword="true"/> if the connection succeeded.</returns>
        public static bool Connect()
        {
            try
            {
                var app = SessionManagement.Connect();
                SetCurrentApplication(app);
                SessionLog.Success($"Connected (Session ID: {app.SessionID}, User: {app.CurrentUser?.DisplayName}).");
                return true;
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Reconnects to an ad hoc session ID directly, independent of whatever Settings
        /// currently holds, storing the resulting Application in the current session.
        /// </summary>
        /// <param name="sessionId">The Session ID to reconnect to.</param>
        /// <returns><see langword="true"/> if the reconnect succeeded.</returns>
        public static bool ReconnectBySessionId(string sessionId)
        {
            try
            {
                var app = sessionId.Connect();
                SetCurrentApplication(app);
                SessionLog.Success($"Reconnected (Session ID: {app.SessionID}, User: {app.CurrentUser?.DisplayName}).");
                return true;
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                return false;
            }
        }

        /// <summary>
        /// Disconnects the current session's Application, if connected.
        /// </summary>
        public static void Disconnect()
        {
            try
            {
                var app = GetCurrentApplication();
                if (app == null) return;

                app.Disconnect();
                SetCurrentApplication(null);
                SessionLog.Success("Disconnected.");
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
            }
        }

        /// <summary>
        /// Disconnects an orphaned Application on session expiration (Global.asax's
        /// Session_End), for a session whose browser tab was closed without a manual
        /// Disconnect. A delayed safety net (fires on session timeout, not immediately on
        /// tab close, HTTP gives the server no way to know that happened), not a
        /// replacement for the ordinary <see cref="Disconnect"/> action.
        /// </summary>
        /// <param name="endingSession">The ending session, passed directly from
        /// Session_End (not read via HttpContext.Current, which is unreliable there).</param>
        public static void DisconnectOrphanedSession(HttpSessionState endingSession)
        {
            try
            {
                var app = endingSession?[SessionKey] as UnityApplication;
                if (app == null) return;

                app.Disconnect();
                Log.Information("Disconnected an orphaned connection on session expiration (Session ID no longer tracked; likely a closed browser tab).");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error disconnecting an orphaned connection on session expiration");
            }
        }

        /// <summary>
        /// Calls the Application Server's Ping operation directly to check whether it's
        /// reachable, independent of (and without requiring) an OnBase session, entirely
        /// bypassing the Unity API (a plain HTTP call, not SessionManagement).
        /// </summary>
        /// <returns>A tuple of whether the server is available, and a short status message.</returns>
        public static async Task<(bool IsAvailable, string StatusMessage)> TestServer()
        {
            var servicePath = GetServicePath();

            if (string.IsNullOrEmpty(servicePath))
            {
                SessionLog.Error("Cannot test server: no Service Path configured.");
                SetServerStatus(false, "No Service Path configured.");
                return (false, "No Service Path configured.");
            }

            try
            {
                var pingUrl = $"{servicePath}/Ping";

                using (var client = new HttpClient())
                {
                    var response = await client.GetStringAsync(pingUrl);
                    var value = XDocument.Parse(response).Root?.Value;

                    if (string.Equals(value, "INITIALIZED", StringComparison.OrdinalIgnoreCase))
                    {
                        SessionLog.Success($"Ping succeeded at [{pingUrl}]: {value}");
                        SetServerStatus(true, $"Server available ({value}).");
                        return (true, $"Server available ({value}).");
                    }

                    SessionLog.Error($"Ping at [{pingUrl}] returned unexpected value: {value}");
                    SetServerStatus(false, $"Server responded, but not initialized: {value}");
                    return (false, $"Server responded, but not initialized: {value}");
                }
            }
            catch (Exception ex)
            {
                SessionLog.Error(ex);
                SetServerStatus(false, "Server unreachable.");
                return (false, "Server unreachable.");
            }
        }
        #endregion

        #region Private Methods
        // Store the last server-check result
        private static void SetServerStatus(bool isAvailable, string message)
        {
            var session = HttpContext.Current?.Session;
            if (session == null) return;

            session[ServerAvailableKey] = isAvailable;
            session[ServerStatusMessageKey] = message;
        }

        // Store (or clear) the current session's Application
        private static void SetCurrentApplication(UnityApplication app)
        {
            var session = HttpContext.Current?.Session;
            if (session == null) return;

            if (app == null) session.Remove(SessionKey);
            else session[SessionKey] = app;
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
