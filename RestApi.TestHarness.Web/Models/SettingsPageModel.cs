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
     * SettingsPageModel. No ApplicationId/LicenseType/SessionId/AllowSessionFailover, no
     * DocPop fields at all: all confirmed gaps, see RestApi.00's own ServiceLocation
     * Training Notes and RestApi.TestHarness's own SettingsViewModel Training Notes.
     * ApiServerUrl/FormsApiUrl replace ServicePath/DataSource.
     *
     * See SettingsController's own Training Notes for a genuine behavioral difference
     * this model doesn't show on its own: Apply here only affects the CURRENT user's own
     * session, not every user of the app simultaneously the way Unity.TestHarness.Web's
     * own Apply (backed by a truly static SessionManagement) did.
     */
    #endregion

    /// <summary>
    /// View-facing data for the Settings page.
    /// </summary>
    public class SettingsPageModel
    {
        /// <summary>The base URL of the OnBase API Server's Document Management API.</summary>
        public string ApiServerUrl { get; set; }

        /// <summary>The base URL of the OnBase API Server's Forms API.</summary>
        public string FormsApiUrl { get; set; }

        /// <summary>Which of the four modes to connect with, as a string.</summary>
        public string AuthenticationMode { get; set; }

        /// <summary>The OnBase username.</summary>
        public string Username { get; set; }

        /// <summary>The OnBase password.</summary>
        public string Password { get; set; }

        /// <summary>A pre-obtained Hyland IdP access token, used by (stubbed) AccessToken mode.</summary>
        public string AccessToken { get; set; }

        /// <summary>The Single Sign-On license token, used by (stubbed) SingleSignOn mode.</summary>
        public string LicenseToken { get; set; }

        /// <summary>When true, an explicit heartbeat call is sent periodically to keep the session alive during idle periods.</summary>
        public bool KeepAlive { get; set; }

        /// <summary>The Hyland IdP token endpoint URL.</summary>
        public string IdpUrl { get; set; }

        /// <summary>The Hyland IdP tenant.</summary>
        public string IdpTenant { get; set; }

        /// <summary>The Hyland IdP client ID.</summary>
        public string IdpClientId { get; set; }

        /// <summary>The Hyland IdP client secret.</summary>
        public string IdpClientSecret { get; set; }

        /// <summary>The scope requested from the Hyland IdP.</summary>
        public string IdpScope { get; set; } = "evolution";

        /// <summary>The OAuth2 grant type used against the Hyland IdP.</summary>
        public string IdpGrantType { get; set; } = "password";
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
