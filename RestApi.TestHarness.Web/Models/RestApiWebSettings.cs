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
     * *Migration Note: the appsettings.json-bound counterpart to RestApi.00's own
     * ServiceLocation/IdpSettings, but deliberately NOT the same type: this class holds
     * only the shared, admin-configured INFRASTRUCTURE settings (server/IdP URLs, IdP
     * client credentials, grant type), the same values every user of this app connects
     * through. It deliberately has no Username/Password: those are per-user (entered on
     * this app's own Connect page, see ConnectController), not something shared,
     * confirmed configuration ("every user should require their own IdP token, not a
     * shared one"). SessionManagementStore combines an instance of this (via IOptions)
     * with each user's own submitted credentials to build their own RestApi.00.ServiceLocation
     * at connect time.
     *
     * Bound from appsettings.json's own "RestApi" section via the IOptions pattern
     * (Program.cs: builder.Services.Configure&lt;RestApiWebSettings&gt;(builder.Configuration.GetSection("RestApi"))),
     * not System.Configuration/a custom XML config section the way RestApi.TestHarness's
     * own App.config is, a deliberate divergence, confirmed acceptable, part of this
     * project's "modernize across the board" scope. See LectureNotes.md.
     */
    #endregion

    /// <summary>
    /// The shared, admin-configured connection infrastructure settings for this app,
    /// bound from <c>appsettings.json</c>'s own <c>RestApi</c> section. Does NOT include
    /// per-user credentials, see this class's own Training Notes.
    /// </summary>
    public class RestApiWebSettings
    {
        /// <summary>
        /// The base URL of the OnBase API Server's Document Management API.
        /// </summary>
        public string ApiServerUrl { get; set; }

        /// <summary>
        /// The base URL of the OnBase API Server's Forms API.
        /// </summary>
        public string FormsApiUrl { get; set; }

        /// <summary>
        /// When true, an explicit heartbeat call is sent periodically to keep each user's
        /// session alive during idle periods.
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// The Hyland IdP token endpoint URL.
        /// </summary>
        public string IdpUrl { get; set; }

        /// <summary>
        /// The Hyland IdP tenant.
        /// </summary>
        public string IdpTenant { get; set; }

        /// <summary>
        /// The Hyland IdP client ID.
        /// </summary>
        public string IdpClientId { get; set; }

        /// <summary>
        /// The Hyland IdP client secret.
        /// </summary>
        public string IdpClientSecret { get; set; }

        /// <summary>
        /// The scope requested from the Hyland IdP.
        /// </summary>
        public string IdpScope { get; set; } = "evolution";

        /// <summary>
        /// The OAuth2 grant type used against the Hyland IdP. Only "password" is
        /// currently implemented (see RestApi.00's own AuthenticationMode).
        /// </summary>
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
