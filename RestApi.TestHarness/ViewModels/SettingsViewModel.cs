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
using SysConfig = System.Configuration;
using RestApi._00.CommonFunctionality.Models.Configuration;
using RestApi._00.CommonFunctionality.Models.Enumerations;
using RestApi._01.ConnectingToOnBase.HelperClasses.OnBase;
#endregion

namespace RestApi.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: the REST API counterpart to Unity.TestHarness's own
     * SettingsViewModel. Same Apply/Save to Config split (Apply pushes in-memory only,
     * Save to Config additionally writes to disk, matching that this page is confirmed
     * as an admin-style, global setting, see RestApi.00's own ServiceLocation Training
     * Notes).
     *
     * ApiServerUrl/FormsApiUrl replace ServicePath (two REST APIs, one server, different
     * base paths, see RestApi.00's own ServiceLocation). No LicenseType/ApplicationId (no
     * REST equivalent exists, see RestApi.00's own Training Notes). No SessionId/
     * AllowSessionFailover (confirmed gap). No DocPop (confirmed gap, so unlike Unity's
     * own Load()/SaveToConfig(), there's no separate OnBaseSettings.DocPop lookup/write
     * here at all).
     *
     * Apply() still calls ServiceLocation.Validate() explicitly for the same reason
     * Unity's own version does: constructing one directly never runs PostDeserialize().
     */
    #endregion

    /// <summary>
    /// Edits <see cref="SessionManagement.ServiceLocation"/>/<see cref="SessionManagement.IdpSettings"/>,
    /// either applying changes in-memory or saving them back to App.config.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        #region Private Members
        private readonly LogViewModel log;

        private string apiServerUrl;
        private string formsApiUrl;
        private AuthenticationMode authenticationMode;
        private string username;
        private string password;
        private string accessToken;
        private string licenseToken;
        private bool keepAlive;
        private string idpUrl;
        private string idpTenant;
        private string idpClientId;
        private string idpClientSecret;
        private string idpScope;
        private string idpGrantType;
        #endregion

        #region Properties
        /// <summary>
        /// The base URL of the OnBase API Server's Document Management API.
        /// </summary>
        public string ApiServerUrl
        {
            get => apiServerUrl;
            set => SetField(ref apiServerUrl, value);
        }

        /// <summary>
        /// The base URL of the OnBase API Server's Forms API.
        /// </summary>
        public string FormsApiUrl
        {
            get => formsApiUrl;
            set => SetField(ref formsApiUrl, value);
        }

        /// <summary>
        /// Which of the four modes to connect with.
        /// </summary>
        public AuthenticationMode AuthenticationMode
        {
            get => authenticationMode;
            set
            {
                if (!SetField(ref authenticationMode, value)) return;
                OnPropertyChanged(nameof(IsOnBaseCredentialsMode));
                OnPropertyChanged(nameof(IsDomainCredentialsMode));
                OnPropertyChanged(nameof(IsAccessTokenMode));
                OnPropertyChanged(nameof(IsSingleSignOnMode));
            }
        }

        /// <summary>
        /// Every <see cref="Enumerations.AuthenticationMode"/> value, for a dropdown.
        /// </summary>
        public IEnumerable<AuthenticationMode> AuthenticationModes { get; } = (AuthenticationMode[])Enum.GetValues(typeof(AuthenticationMode));

        /// <summary>
        /// Whether <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>.
        /// </summary>
        public bool IsOnBaseCredentialsMode => AuthenticationMode == AuthenticationMode.OnBaseCredentials;

        /// <summary>
        /// Whether <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.DomainCredentials"/>.
        /// </summary>
        public bool IsDomainCredentialsMode => AuthenticationMode == AuthenticationMode.DomainCredentials;

        /// <summary>
        /// Whether <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.AccessToken"/>.
        /// </summary>
        public bool IsAccessTokenMode => AuthenticationMode == AuthenticationMode.AccessToken;

        /// <summary>
        /// Whether <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.SingleSignOn"/>.
        /// </summary>
        public bool IsSingleSignOnMode => AuthenticationMode == AuthenticationMode.SingleSignOn;

        /// <summary>
        /// The OnBase username, required for <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>
        /// (the only currently-implemented mode).
        /// </summary>
        public string Username
        {
            get => username;
            set => SetField(ref username, value);
        }

        /// <summary>
        /// The OnBase password. See <see cref="Username"/>.
        /// </summary>
        public string Password
        {
            get => password;
            set => SetField(ref password, value);
        }

        /// <summary>
        /// A pre-obtained Hyland IdP access token, used by (stubbed)
        /// <see cref="Enumerations.AuthenticationMode.AccessToken"/>.
        /// </summary>
        public string AccessToken
        {
            get => accessToken;
            set => SetField(ref accessToken, value);
        }

        /// <summary>
        /// The Single Sign-On license token, used by (stubbed)
        /// <see cref="Enumerations.AuthenticationMode.SingleSignOn"/>.
        /// </summary>
        public string LicenseToken
        {
            get => licenseToken;
            set => SetField(ref licenseToken, value);
        }

        /// <summary>
        /// When true, an explicit heartbeat call is sent periodically to keep the session
        /// alive during idle periods.
        /// </summary>
        public bool KeepAlive
        {
            get => keepAlive;
            set => SetField(ref keepAlive, value);
        }

        /// <summary>
        /// The Hyland IdP token endpoint URL.
        /// </summary>
        public string IdpUrl
        {
            get => idpUrl;
            set => SetField(ref idpUrl, value);
        }

        /// <summary>
        /// The Hyland IdP tenant.
        /// </summary>
        public string IdpTenant
        {
            get => idpTenant;
            set => SetField(ref idpTenant, value);
        }

        /// <summary>
        /// The Hyland IdP client ID.
        /// </summary>
        public string IdpClientId
        {
            get => idpClientId;
            set => SetField(ref idpClientId, value);
        }

        /// <summary>
        /// The Hyland IdP client secret.
        /// </summary>
        public string IdpClientSecret
        {
            get => idpClientSecret;
            set => SetField(ref idpClientSecret, value);
        }

        /// <summary>
        /// The scope requested from the Hyland IdP.
        /// </summary>
        public string IdpScope
        {
            get => idpScope;
            set => SetField(ref idpScope, value);
        }

        /// <summary>
        /// The OAuth2 grant type used against the Hyland IdP.
        /// </summary>
        public string IdpGrantType
        {
            get => idpGrantType;
            set => SetField(ref idpGrantType, value);
        }
        #endregion

        #region Commands
        /// <summary>
        /// Reverts every field on this page to whatever's currently applied.
        /// </summary>
        public RelayCommand ReloadCommand { get; }

        /// <summary>
        /// Applies the edited values in-memory, for this run only.
        /// </summary>
        public RelayCommand ApplyCommand { get; }

        /// <summary>
        /// Applies the edited values (same as <see cref="ApplyCommand"/>) AND writes them
        /// back to App.config on disk, so they persist across runs.
        /// </summary>
        public RelayCommand SaveToConfigCommand { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the SettingsViewModel class
        /// </summary>
        /// <param name="log">The shared output log.</param>
        public SettingsViewModel(LogViewModel log)
        {
            this.log = log;

            ReloadCommand = new RelayCommand(_ => Load());
            ApplyCommand = new RelayCommand(_ => Apply());
            SaveToConfigCommand = new RelayCommand(_ => SaveToConfig());

            Load();
        }
        #endregion

        #region Private Methods
        // Populate every field from SessionManagement's current ServiceLocation/IdpSettings
        private void Load()
        {
            try
            {
                var serviceLocation = SessionManagement.ServiceLocation;
                var idpSettings = SessionManagement.IdpSettings;

                ApiServerUrl = serviceLocation?.ApiServerUrl;
                FormsApiUrl = serviceLocation?.FormsApiUrl;
                AuthenticationMode = serviceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials;
                Username = serviceLocation?.Username;
                Password = serviceLocation?.Password;
                AccessToken = serviceLocation?.AccessToken;
                LicenseToken = serviceLocation?.LicenseToken;
                KeepAlive = serviceLocation?.KeepAlive ?? false;

                IdpUrl = idpSettings?.IdpUrl;
                IdpTenant = idpSettings?.IdpTenant;
                IdpClientId = idpSettings?.IdpClientId;
                IdpClientSecret = idpSettings?.IdpClientSecret;
                IdpScope = idpSettings?.IdpScope ?? "evolution";
                IdpGrantType = idpSettings?.IdpGrantType ?? "password";

                log.Info("Settings reloaded from current configuration.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Push edited values into SessionManagement.ServiceLocation/IdpSettings (in-memory only)
        private void Apply()
        {
            try
            {
                var serviceLocation = new ServiceLocation
                {
                    ApiServerUrl = ApiServerUrl,
                    FormsApiUrl = FormsApiUrl,
                    AuthenticationMode = AuthenticationMode,
                    Username = Username,
                    Password = Password,
                    AccessToken = AccessToken,
                    LicenseToken = LicenseToken,
                    KeepAlive = KeepAlive
                };

                // Constructing a ServiceLocation directly never runs PostDeserialize(),
                // so this is called explicitly to get the same "AuthenticationMode 'X'
                // requires Y" validation App.config loading gets for free.
                serviceLocation.Validate();

                var idpSettings = new IdpSettings
                {
                    IdpUrl = IdpUrl,
                    IdpTenant = IdpTenant,
                    IdpClientId = IdpClientId,
                    IdpClientSecret = IdpClientSecret,
                    IdpScope = IdpScope,
                    IdpGrantType = IdpGrantType
                };

                SessionManagement.ServiceLocation = serviceLocation;
                SessionManagement.IdpSettings = idpSettings;

                log.Success("Settings applied for this session.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        // Apply, then write the same values back to App.config on disk
        private void SaveToConfig()
        {
            try
            {
                Apply();

                var config = SysConfig.ConfigurationManager.OpenExeConfiguration(SysConfig.ConfigurationUserLevel.None);
                var section = (RestApiSettings)config.GetSection(RestApiSettings.SectionName);

                section.ServiceLocation.ApiServerUrl = ApiServerUrl;
                section.ServiceLocation.FormsApiUrl = FormsApiUrl;
                section.ServiceLocation.AuthenticationMode = AuthenticationMode;
                section.ServiceLocation.Username = Username;
                section.ServiceLocation.Password = Password;
                section.ServiceLocation.AccessToken = AccessToken;
                section.ServiceLocation.LicenseToken = LicenseToken;

                section.IdpSettings.IdpUrl = IdpUrl;
                section.IdpSettings.IdpTenant = IdpTenant;
                section.IdpSettings.IdpClientId = IdpClientId;
                section.IdpSettings.IdpClientSecret = IdpClientSecret;
                section.IdpSettings.IdpScope = IdpScope;
                section.IdpSettings.IdpGrantType = IdpGrantType;

                config.Save(SysConfig.ConfigurationSaveMode.Modified);
                SysConfig.ConfigurationManager.RefreshSection(RestApiSettings.SectionName);

                log.Success("Settings saved to App.config.");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
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
