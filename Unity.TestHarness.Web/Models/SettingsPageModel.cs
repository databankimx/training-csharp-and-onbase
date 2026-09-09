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
    /// View-facing data for the Settings page, mirroring
    /// Unity.TestHarness (the WPF version)'s own SettingsViewModel fields.
    /// </summary>
    public class SettingsPageModel
    {
        /// <summary>The Unity Integration application GUID.</summary>
        public string ApplicationId { get; set; }

        /// <summary>The URL to the Application Server's Service.asmx.</summary>
        public string ServicePath { get; set; }

        /// <summary>The OnBase data source name.</summary>
        public string DataSource { get; set; }

        /// <summary>The license type to connect with, as a string (matches Hyland.Unity.LicenseType's name).</summary>
        public string LicenseType { get; set; }

        /// <summary>Which of the four Unity API authentication modes to connect with, as a string.</summary>
        public string AuthenticationMode { get; set; }

        /// <summary>The OnBase username.</summary>
        public string Username { get; set; }

        /// <summary>The OnBase password.</summary>
        public string Password { get; set; }

        /// <summary>A pre-obtained Hyland IdP access token.</summary>
        public string AccessToken { get; set; }

        /// <summary>The Single Sign-On license token.</summary>
        public string LicenseToken { get; set; }

        /// <summary>An existing OnBase session ID to reconnect to first.</summary>
        public string SessionId { get; set; }

        /// <summary>When true, the connected session's IsDisconnectEnabled is set to false.</summary>
        public bool KeepAlive { get; set; }

        /// <summary>When true, a failed reconnect to SessionId falls back to a new session.</summary>
        public bool AllowSessionFailover { get; set; }

        /// <summary>The DocPop ASPX page's base URL.</summary>
        public string DocPopBaseUrl { get; set; }

        /// <summary>The DocPop checksum seed.</summary>
        public string DocPopChecksumSeed { get; set; }

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
