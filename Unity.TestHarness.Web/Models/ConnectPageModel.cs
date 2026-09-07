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

namespace Unity.TestHarness.Web.Models
{
    /// <summary>
    /// View-facing data for the Connect page, a serializable snapshot of
    /// <see cref="Infrastructure.SessionConnectionManager"/>'s current state (a plain
    /// DTO here plays the same role ConnectionViewModel's bound properties play in
    /// Unity.TestHarness, the WPF version).
    /// </summary>
    public class ConnectPageModel
    {
        #region Properties
        /// <summary>
        /// Whether the current session is currently connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// The connected session's ID, or <see langword="null"/> if not connected.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// The connected user's display name, or <see langword="null"/> if not connected.
        /// </summary>
        public string CurrentUserDisplayName { get; set; }

        /// <summary>
        /// A read-only summary of the currently-configured Service Path.
        /// </summary>
        public string ServicePath { get; set; }

        /// <summary>
        /// A read-only summary of the currently-configured Data Source.
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// A read-only summary of the currently-configured Authentication Mode.
        /// </summary>
        public string AuthenticationMode { get; set; }

        /// <summary>
        /// Whether Keep Alive is currently enabled.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// The last Session ID entered for reconnecting.
        /// </summary>
        public string ReconnectSessionIdInput { get; set; }

        /// <summary>
        /// Whether the Application Server was reachable the last time it was checked, or
        /// <see langword="null"/> if it hasn't been checked yet this session.
        /// </summary>
        public bool? IsServerAvailable { get; set; }

        /// <summary>
        /// A short human-readable description of the last <see cref="IsServerAvailable"/> check.
        /// </summary>
        public string ServerStatusMessage { get; set; }
        #endregion
    }
}
