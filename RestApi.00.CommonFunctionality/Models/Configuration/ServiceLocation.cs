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
using System.Configuration;
using RestApi._00.CommonFunctionality.HelperClasses.Extensions;
using RestApi._00.CommonFunctionality.Models.Enumerations;
#endregion

namespace RestApi._00.CommonFunctionality.Models.Configuration
{
    #region Training Notes
    /*
     * *Migration Note: adapted from Unity.00.CommonFunctionality's own ServiceLocation,
     * but genuinely reshaped, not just renamed, several Unity API concepts don't carry
     * over:
     *
     * - ApiServerUrl replaces ServicePath: the REST API's base URL shape
     *   ({protocol}://{server}/{product}, per the OpenAPI spec's own servers block) is
     *   represented as a single combined URL here, the same "one string" approach
     *   ServicePath itself took, rather than three separate fields.
     *
     * - LicenseType and ApplicationId (the Unity Integration GUID) are DROPPED entirely,
     *   neither concept appears anywhere in the Document Management API documentation
     *   reviewed for this training set; OnBase licensing for REST API access appears to
     *   be handled server-side, transparent to the client.
     *
     * - SessionId/AllowSessionFailover are DROPPED, and this is a genuine, worth-flagging
     *   feature gap, not an oversight: the REST API's session is carried by the
     *   Cookie.Session.OnBase.Hyland cookie the Document Management API itself issues on
     *   first authenticated request, not a value the client can supply up front to
     *   reconnect to a specific prior session the way Unity API's SessionID
     *   authentication properties allow. There is no documented REST equivalent of
     *   Unity.TestHarness's "Reconnect to Session ID" feature. RestApi.TestHarness's own
     *   Connect page will need to omit or reimagine that specific control accordingly.
     *
     * - KeepAlive is KEPT, but its meaning shifts: Unity API's KeepAlive means "don't
     *   disconnect this session when this connection ends" (IsDisconnectEnabled=false).
     *   Here it means "send an explicit heartbeat call periodically", since a REST API
     *   session's cookie expires after 5 minutes of inactivity regardless (see the
     *   Authentication guide's own Heartbeat section), there is nothing to configure
     *   AT CONNECT TIME the way Unity's IsDisconnectEnabled is; keeping a session alive
     *   is an ongoing runtime behavior, not a one-time connection property. See
     *   RestApi.01.ConnectingToOnBase's SessionManagement for where the actual heartbeat
     *   timer lives.
     *
     * - Secret fields (Password/AccessToken/LicenseToken) use DataProtectionExtensions
     *   (Microsoft.AspNetCore.DataProtection's IDataProtector), not Unity.00's
     *   DPAPI-via-registry (aspnet_setreg.exe) approach, once this track was no longer
     *   tied to net48, there was no reason to keep the registry indirection. See
     *   DataProtectionExtensions' own Training Notes.
     *
     * - FormsApiUrl is NEW, with no Unity API counterpart: the OnBase REST surface is
     *   actually several separate APIs (Document Management, Forms, Admin, Workflow,
     *   ...), each with its own base URL differing only in the {product} path segment
     *   (confirmed against forms-api.json's own servers block: onbase/core vs
     *   onbase/forms, same {server}). Confirmed directly: every REST API is served by
     *   the same server and honors the same Bearer token, so ServiceLocation holds one
     *   set of credentials/AuthenticationMode for all of them, just a second base URL.
     *   See RestApi.01.ConnectingToOnBase's SessionManagement for how the session
     *   itself (specifically, whether its cookie carries over between the two) is
     *   handled defensively, since that part isn't confirmed either way.
     *
     * AccessToken/LicenseToken properties are still present, matching AuthenticationMode's
     * own four members, even though DomainCredentials/AccessToken/SingleSignOn are
     * currently stubbed, so the settings schema (and RestApi.TestHarness's Settings page)
     * has the same shape as the Unity API version, ready for those modes to be filled in
     * later without another schema change.
     */
    #endregion

    public class ServiceLocation : ConfigurationElement
    {
        #region Private Members
        // Username (unprotected or plain-text)
        private string decryptedUsername;

        // Password (unprotected or plain-text)
        private string decryptedPassword;

        // Access Token (unprotected or plain-text)
        private string decryptedAccessToken;

        // License Token (unprotected or plain-text)
        private string decryptedLicenseToken;
        #endregion

        #region Properties
        /// <summary>
        /// Section name to appear in the XML configuration file
        /// </summary>
        public const string ElementName = "serviceLocation";

        /// <summary>
        /// The base URL of the OnBase API Server's Document Management API (e.g.
        /// "https://localhost/apiserver/onbase/core").
        /// </summary>
        [ConfigurationProperty("apiServerUrl", IsRequired = true)]
        public string ApiServerUrl
        {
            get => (string)this["apiServerUrl"];
            set => this["apiServerUrl"] = value;
        }

        /// <summary>
        /// The base URL of the OnBase API Server's Forms API (e.g.
        /// "https://localhost/apiserver/onbase/forms"), used for Unity Form/E-Form
        /// lookups. Optional: only needed by RestApi.02.AccessingTaxonomy's Unity Form
        /// methods.
        /// </summary>
        [ConfigurationProperty("formsApiUrl", IsRequired = false)]
        public string FormsApiUrl
        {
            get => (string)this["formsApiUrl"];
            set => this["formsApiUrl"] = value;
        }

        /// <summary>
        /// Which of AuthenticationMode's four modes to use to establish a NEW session.
        /// See <see cref="AuthenticationMode"/>; only <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>
        /// is currently implemented.
        /// </summary>
        [ConfigurationProperty("authenticationMode", IsRequired = false, DefaultValue = AuthenticationMode.OnBaseCredentials)]
        public AuthenticationMode AuthenticationMode
        {
            get => (AuthenticationMode)this["authenticationMode"];
            set => this["authenticationMode"] = value;
        }

        /// <summary>
        /// The OnBase password that will be used to connect. Required when
        /// <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>,
        /// traded (along with <see cref="Username"/>) for a Hyland IdP access token via
        /// the OAuth2 "password" grant (see RestApi.01.ConnectingToOnBase's
        /// IdpAuthentication).
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get => (string)this["password"];
            set => this["password"] = value;
        }

        /// <summary>
        /// The OnBase username that will be used to connect. Required when
        /// <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>.
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get => (string)this["username"];
            set => this["username"] = value;
        }

        /// <summary>
        /// A pre-obtained Hyland IdP access token, used when <see cref="AuthenticationMode"/>
        /// is <see cref="Enumerations.AuthenticationMode.AccessToken"/>. Stubbed: this
        /// mode is not yet implemented, see <see cref="Enumerations.AuthenticationMode"/>'s
        /// own Training Notes.
        /// </summary>
        [ConfigurationProperty("accessToken", IsRequired = false)]
        public string AccessToken
        {
            get => (string)this["accessToken"];
            set => this["accessToken"] = value;
        }

        /// <summary>
        /// A separately-issued Single Sign-On license token, used when
        /// <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.SingleSignOn"/>.
        /// Stubbed: this mode is not yet implemented, see
        /// <see cref="Enumerations.AuthenticationMode"/>'s own Training Notes. Its exact
        /// meaning would need to be revisited if/when Authorization Code with PKCE is
        /// actually implemented, this property is a placeholder matching the Unity API
        /// version's shape, not a confirmed REST API concept.
        /// </summary>
        [ConfigurationProperty("licenseToken", IsRequired = false)]
        public string LicenseToken
        {
            get => (string)this["licenseToken"];
            set => this["licenseToken"] = value;
        }

        /// <summary>
        /// Unprotected Username. Plain text is returned as-is; a "protected:"-prefixed
        /// value (see DataProtectionExtensions) is unprotected first.
        /// </summary>
        public string DecryptedUsername
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedUsername))
                {
                    return decryptedUsername;
                }

                decryptedUsername = Username.IsProtected()
                    ? Username.Unprotect()
                    : Username;

                return decryptedUsername;
            }
        }

        /// <summary>
        /// Unprotected Password. Plain text is returned as-is; a "protected:"-prefixed
        /// value (see DataProtectionExtensions) is unprotected first.
        /// </summary>
        public string DecryptedPassword
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedPassword))
                {
                    return decryptedPassword;
                }

                decryptedPassword = Password.IsProtected()
                    ? Password.Unprotect()
                    : Password;

                return decryptedPassword;
            }
        }

        /// <summary>
        /// Unprotected Access Token. Plain text is returned as-is; a "protected:"-prefixed
        /// value (see DataProtectionExtensions) is unprotected first.
        /// </summary>
        public string DecryptedAccessToken
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedAccessToken))
                {
                    return decryptedAccessToken;
                }

                decryptedAccessToken = AccessToken.IsProtected()
                    ? AccessToken.Unprotect()
                    : AccessToken;

                return decryptedAccessToken;
            }
        }

        /// <summary>
        /// Unprotected License Token. Plain text is returned as-is; a "protected:"-prefixed
        /// value (see DataProtectionExtensions) is unprotected first.
        /// </summary>
        public string DecryptedLicenseToken
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedLicenseToken))
                {
                    return decryptedLicenseToken;
                }

                decryptedLicenseToken = LicenseToken.IsProtected()
                    ? LicenseToken.Unprotect()
                    : LicenseToken;

                return decryptedLicenseToken;
            }
        }

        /// <summary>
        /// When true, an explicit heartbeat call is sent periodically to keep the OnBase
        /// session's cookie from expiring during idle periods (see
        /// RestApi.01.ConnectingToOnBase's SessionManagement). A different mechanism than
        /// Unity API's own KeepAlive (which controls whether a session CAN be reconnected
        /// to later), but serves the same underlying purpose: keeping a session alive
        /// across idle time.
        /// </summary>
        public bool KeepAlive { get; set; }
        #endregion

        #region Serialization Methods
        /// <summary>
        /// Enforce the required fields for whichever <see cref="AuthenticationMode"/> is configured
        /// </summary>
        protected override void PostDeserialize()
        {
            base.PostDeserialize();

            Validate();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Validates that every field <see cref="AuthenticationMode"/> actually requires is
        /// present, throwing <see cref="ConfigurationErrorsException"/> if not. Called
        /// automatically by <see cref="PostDeserialize"/> when this instance is loaded from
        /// App.config; call this directly for any instance built manually in code (e.g. a
        /// settings UI constructing a new ServiceLocation), which never goes through
        /// PostDeserialize at all.
        /// </summary>
        public void Validate()
        {
            switch (AuthenticationMode)
            {
                case AuthenticationMode.OnBaseCredentials:
                    if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
                    {
                        throw new ConfigurationErrorsException("AuthenticationMode 'OnBaseCredentials' requires both a username and password attribute.");
                    }
                    break;

                case AuthenticationMode.DomainCredentials:
                case AuthenticationMode.AccessToken:
                case AuthenticationMode.SingleSignOn:
                    // No validation performed: these modes are not yet implemented (see
                    // AuthenticationMode's own Training Notes), attempting to connect
                    // with any of them throws NotImplementedException at connect time
                    // instead, in RestApi.01.ConnectingToOnBase's SessionManagement.
                    break;
            }
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// In order to allow the element to be modified at runtime, we need IsReadOnly to return false
        /// </summary>
        /// <returns>Always false</returns>
        public override bool IsReadOnly()
        {
            return false;
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
