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
using Hyland.Unity;
using Unity._00.CommonFunctionality.Models.Configuration;
using Unity._00.CommonFunctionality.Models.Enumerations;
using Unity._01.ConnectingToOnBase.HelperClasses.OnBase;
#endregion

namespace Unity.TestHarness.ViewModels
{
    #region Training Notes
    /*
     * *Migration Note: "Apply" and "Save to App.config" are deliberately separate
     * actions (per an earlier discussion): Apply pushes the edited values into
     * SessionManagement.ServiceLocation/IdpSettings immediately, in-memory, so Connect()
     * picks them up right away, no disk write, resets to whatever App.config held the
     * next time the harness starts. Save to App.config additionally writes those SAME
     * values back to the config file on disk, via ConfigurationManager, so they persist
     * across runs, this is the one genuinely "are you sure" action on this page, plain
     * secrets (Password, AccessToken, etc.) get written back in PLAIN TEXT unless you've
     * manually set them to a "registry:HKLM\...,value" reference yourself first, this
     * page does not apply DPAPI/registry encryption on your behalf.
     *
     * Secret fields (Password, AccessToken, LicenseToken, IdpClientSecret) are edited via
     * real PasswordBox controls in the View, concealed on screen like any other password
     * field, bridged to these plain string properties through PasswordBoxBehavior (an
     * attached property, since PasswordBox.Password itself isn't bindable, a deliberate
     * WPF security decision). This project is meant as a template other developers build
     * real desktop apps from, so the concealment is real, not a diagnostic-tool
     * simplification.
     *
     * Apply() calls ServiceLocation.Validate() explicitly, since constructing a
     * ServiceLocation manually (rather than loading one from App.config) never triggers
     * PostDeserialize(), which is what normally runs that validation. Without this call,
     * a misconfigured mode (e.g., OnBaseCredentials with no Username/Password) would only
     * fail later, at the actual Unity API call in SessionManagement.Connect(), with a
     * far less specific error.
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

        private string applicationId;
        private string servicePath;
        private string dataSource;
        private LicenseType licenseType;
        private AuthenticationMode authenticationMode;
        private string username;
        private string password;
        private string accessToken;
        private string licenseToken;
        private string sessionId;
        private bool keepAlive;
        private bool allowSessionFailover;
        private string docPopBaseUrl;
        private string docPopChecksumSeed;
        private string idpUrl;
        private string idpTenant;
        private string idpClientId;
        private string idpClientSecret;
        private string idpScope;
        private string idpGrantType;
        #endregion

        #region Properties
        /// <summary>
        /// The Unity Integration application GUID.
        /// </summary>
        public string ApplicationId
        {
            get => applicationId;
            set => SetField(ref applicationId, value);
        }

        /// <summary>
        /// The URL to the Application Server's Service.asmx.
        /// </summary>
        public string ServicePath
        {
            get => servicePath;
            set => SetField(ref servicePath, value);
        }

        /// <summary>
        /// The OnBase data source name.
        /// </summary>
        public string DataSource
        {
            get => dataSource;
            set => SetField(ref dataSource, value);
        }

        /// <summary>
        /// The license type to connect with.
        /// </summary>
        public LicenseType LicenseType
        {
            get => licenseType;
            set => SetField(ref licenseType, value);
        }

        /// <summary>
        /// Every <see cref="Hyland.Unity.LicenseType"/> value, for a dropdown.
        /// </summary>
        public IEnumerable<LicenseType> LicenseTypes { get; } = new[]
        {
            LicenseType.Default,
            LicenseType.QueryMetering,
            LicenseType.EnterpriseCoreAPI
        };

        /// <summary>
        /// Which of the four Unity API authentication modes to connect with.
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
        /// The OnBase username, used by <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>
        /// and (as the "password" grant identity) by <see cref="Enumerations.AuthenticationMode.AccessToken"/>.
        /// </summary>
        public string Username
        {
            get => username;
            set => SetField(ref username, value);
        }

        /// <summary>
        /// The OnBase password. See <see cref="Username"/> for which modes use it.
        /// </summary>
        public string Password
        {
            get => password;
            set => SetField(ref password, value);
        }

        /// <summary>
        /// A pre-obtained Hyland IdP access token. Leave blank to have one obtained
        /// automatically via <see cref="IdpUrl"/>/<see cref="Username"/>/<see cref="Password"/>.
        /// </summary>
        public string AccessToken
        {
            get => accessToken;
            set => SetField(ref accessToken, value);
        }

        /// <summary>
        /// The Single Sign-On license token.
        /// </summary>
        public string LicenseToken
        {
            get => licenseToken;
            set => SetField(ref licenseToken, value);
        }

        /// <summary>
        /// An existing OnBase session ID to reconnect to first, before falling back to
        /// <see cref="AuthenticationMode"/>. Independent of AuthenticationMode; leave
        /// blank to always establish a new session.
        /// </summary>
        public string SessionId
        {
            get => sessionId;
            set => SetField(ref sessionId, value);
        }

        /// <summary>
        /// When true, the connected session's IsDisconnectEnabled is set to false
        /// (required to later reconnect to it via SessionId).
        /// </summary>
        public bool KeepAlive
        {
            get => keepAlive;
            set => SetField(ref keepAlive, value);
        }

        /// <summary>
        /// When true, a failed reconnect to SessionId falls back to establishing a new
        /// session instead of throwing.
        /// </summary>
        public bool AllowSessionFailover
        {
            get => allowSessionFailover;
            set => SetField(ref allowSessionFailover, value);
        }

        /// <summary>
        /// The DocPop ASPX page's base URL.
        /// </summary>
        public string DocPopBaseUrl
        {
            get => docPopBaseUrl;
            set => SetField(ref docPopBaseUrl, value);
        }

        /// <summary>
        /// The DocPop checksum seed.
        /// </summary>
        public string DocPopChecksumSeed
        {
            get => docPopChecksumSeed;
            set => SetField(ref docPopChecksumSeed, value);
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
        /// Reverts every field on this page to whatever's currently applied in
        /// <see cref="SessionManagement"/> (discarding unsaved edits).
        /// </summary>
        public RelayCommand ReloadCommand { get; }

        /// <summary>
        /// Applies the edited values to <see cref="SessionManagement.ServiceLocation"/>/
        /// <see cref="SessionManagement.IdpSettings"/> in-memory, for this run only.
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
        // Populate every field from SessionManagement's current ServiceLocation/IdpSettings,
        // and DocPop directly from App.config (SessionManagement doesn't expose DocPop at
        // all, only Unity.03.DocumentRetrieval reads it, so there's nowhere else to load
        // it from)
        private void Load()
        {
            try
            {
                var serviceLocation = SessionManagement.ServiceLocation;
                var idpSettings = SessionManagement.IdpSettings;
                var onBaseSettings = (OnBaseSettings)SysConfig.ConfigurationManager.GetSection(OnBaseSettings.SectionName);

                ApplicationId = serviceLocation?.ApplicationId;
                ServicePath = serviceLocation?.ServicePath;
                DataSource = serviceLocation?.DataSource;
                LicenseType = serviceLocation?.LicenseType ?? LicenseType.Default;
                AuthenticationMode = serviceLocation?.AuthenticationMode ?? AuthenticationMode.OnBaseCredentials;
                Username = serviceLocation?.Username;
                Password = serviceLocation?.Password;
                AccessToken = serviceLocation?.AccessToken;
                LicenseToken = serviceLocation?.LicenseToken;
                SessionId = serviceLocation?.SessionId;
                KeepAlive = serviceLocation?.KeepAlive ?? false;
                AllowSessionFailover = serviceLocation?.AllowSessionFailover ?? false;

                DocPopBaseUrl = onBaseSettings?.DocPop?.DocPopBaseUrl;
                DocPopChecksumSeed = onBaseSettings?.DocPop?.DocPopChecksumSeed;

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
                    ApplicationId = ApplicationId,
                    ServicePath = ServicePath,
                    DataSource = DataSource,
                    LicenseType = LicenseType,
                    AuthenticationMode = AuthenticationMode,
                    Username = Username,
                    Password = Password,
                    AccessToken = AccessToken,
                    LicenseToken = LicenseToken,
                    SessionId = SessionId,
                    KeepAlive = KeepAlive,
                    AllowSessionFailover = AllowSessionFailover
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
                var section = (OnBaseSettings)config.GetSection(OnBaseSettings.SectionName);

                section.ServiceLocation.ApplicationId = ApplicationId;
                section.ServiceLocation.ServicePath = ServicePath;
                section.ServiceLocation.DataSource = DataSource;
                section.ServiceLocation.LicenseType = LicenseType;
                section.ServiceLocation.AuthenticationMode = AuthenticationMode;
                section.ServiceLocation.Username = Username;
                section.ServiceLocation.Password = Password;
                section.ServiceLocation.AccessToken = AccessToken;
                section.ServiceLocation.LicenseToken = LicenseToken;
                section.ServiceLocation.SessionId = SessionId;

                section.DocPop.DocPopBaseUrl = DocPopBaseUrl;
                section.DocPop.DocPopChecksumSeed = DocPopChecksumSeed;

                section.IdpSettings.IdpUrl = IdpUrl;
                section.IdpSettings.IdpTenant = IdpTenant;
                section.IdpSettings.IdpClientId = IdpClientId;
                section.IdpSettings.IdpClientSecret = IdpClientSecret;
                section.IdpSettings.IdpScope = IdpScope;
                section.IdpSettings.IdpGrantType = IdpGrantType;

                config.Save(SysConfig.ConfigurationSaveMode.Modified);
                SysConfig.ConfigurationManager.RefreshSection(OnBaseSettings.SectionName);

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
