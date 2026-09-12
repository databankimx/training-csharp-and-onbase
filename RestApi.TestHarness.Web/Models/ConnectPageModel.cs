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

namespace RestApi.TestHarness.Web.Models
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness.Web's own
     * ConnectPageModel. No SessionId/CurrentUserDisplayName, no
     * ReconnectSessionIdInput, no IsServerAvailable/ServerStatusMessage: all confirmed
     * gaps, see RestApi.00's own ServiceLocation Training Notes (no client-visible
     * session identifier) and RestApi.TestHarness's own ConnectionViewModel Training
     * Notes (no documented health-check endpoint to ping). ApiServerUrl/FormsApiUrl
     * replace ServicePath/DataSource.
     *
     * Username is new here, and genuinely different in kind from everything else on this
     * model: every other property is a READ-ONLY summary of already-configured/connected
     * state, but Username is the value the Connect form's own username field submits
     * back (Password is a plain &lt;input type="password"&gt; on the view, never echoed
     * back into this model at all, consistent with never displaying a submitted
     * password back to the browser). This reflects the confirmed requirement that every
     * user of this app authenticates with their own credentials, entered here, not a
     * shared, admin-configured identity the way ApiServerUrl/AuthenticationMode/etc.
     * still are.
     */
    #endregion

    /// <summary>
    /// View-facing data for the Connect page: a snapshot of the current session's
    /// connection state, plus the Username input field for the Connect form itself.
    /// </summary>
    public class ConnectPageModel
    {
        #region Properties
        /// <summary>
        /// Whether the current session is currently connected.
        /// </summary>
        public bool IsConnected { get; set; }

        /// <summary>
        /// A read-only summary of the currently-configured API Server URL.
        /// </summary>
        public string ApiServerUrl { get; set; }

        /// <summary>
        /// A read-only summary of the currently-configured Forms API URL.
        /// </summary>
        public string FormsApiUrl { get; set; }

        /// <summary>
        /// A read-only summary of the currently-configured Authentication Mode.
        /// </summary>
        public string AuthenticationMode { get; set; }

        /// <summary>
        /// Whether Keep Alive is currently enabled.
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// The Connect form's own username field. See this class's own Training Notes
        /// for why this (unlike every other property here) is user input, not a
        /// read-only summary of already-configured state.
        /// </summary>
        public string Username { get; set; }
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
