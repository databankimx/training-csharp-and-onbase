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
using Hyland.Unity;
using Unity._00.CommonFunctionality.HelperClasses.Extensions;
using Unity._00.CommonFunctionality.Models.Enumerations;
#endregion

namespace Unity._00.CommonFunctionality.Models.Configuration
{
    #region Training Notes
    /*
     * *Migration Note: UseNTAuthentication (a bool) has been REMOVED entirely, replaced
     * with the AuthenticationMode enum (see Models.Enumerations.AuthenticationMode for
     * the full reasoning). This class now has a property (and, where needed, validation)
     * for each of AuthenticationMode's four members: Username/Password
     * (OnBaseCredentials), AccessToken (AccessToken), and LicenseToken (SingleSignOn).
     * DomainCredentials needs no property of its own at all: CreateDomainAuthenticationProperties
     * only accepts (url, datasource), confirmed against the actual Unity API, NT
     * authentication is purely the Windows identity the process is already running as,
     * there is no Unity API mechanism for an alternate domain user (that would require
     * OS-level impersonation performed before Connect() is called, a separate concern
     * entirely, not something this configuration schema attempts to support). An earlier
     * version of this class had a Domain property and an AlternateDomainUser flag for
     * exactly that scenario; both were removed once this was confirmed.
     *
     * SessionId is DELIBERATELY NOT tied to AuthenticationMode at all (an earlier version
     * had it as a fifth enum member; see AuthenticationMode's own Training Notes for why
     * that made failover impossible to express correctly). It's an independent, optional
     * property here: when set, Unity.01.ConnectingToOnBase's Connect() always attempts a
     * reconnect to it FIRST, regardless of AuthenticationMode, and only falls back to
     * establishing a brand new session (using whatever AuthenticationMode IS configured)
     * if that reconnect fails and AllowSessionFailover is true.
     *
     * DecryptedAccessToken/DecryptedLicenseToken reuse the SAME DPAPI/registry-encryption
     * mechanism as DecryptedUsername/DecryptedPassword, every one of these is a secret
     * and deserves the same protection.
     *
     * Validate() was split OUT of PostDeserialize() (which now just calls it) once it
     * became clear code that builds a ServiceLocation manually, rather than loading one
     * from App.config, never triggers PostDeserialize at all, that method only runs
     * during actual XML deserialization. Any caller constructing one directly (a
     * settings UI, a test) should call Validate() itself to get the same "requires X"
     * errors App.config loading gets for free.
     */
    #endregion

    public class ServiceLocation : ConfigurationElement
    {
        #region Private Members
        // Username (decrypted or plain-text)
        private string decryptedUsername;

        // Password (decrypted or plain-text)
        private string decryptedPassword;

        // Access Token (decrypted or plain-text)
        private string decryptedAccessToken;

        // License Token (decrypted or plain-text)
        private string decryptedLicenseToken;
        #endregion

        #region Properties
        /// <summary>
        /// Section name to appear in the XML configuration file
        /// </summary>
        public const string ElementName = "serviceLocation";

        /// <summary>
        /// Application GUID (used in Unity Integrations configuration in OnBase Studio)
        /// </summary>
        [ConfigurationProperty("applicationId", IsRequired = true)]
        public string ApplicationId
        {
            get => (string)base["applicationId"];
            set => base["applicationId"] = value;
        }

        /// <summary>
        /// The URL to the Service.asmx page of the Application Server
        /// </summary>
        [ConfigurationProperty("servicePath", IsRequired = true)]
        public string ServicePath
        {
            get => (string)this["servicePath"];
            set => this["servicePath"] = value;
        }

        /// <summary>
        /// The data source name (configured at the Application Server) to connect to
        /// </summary>
        [ConfigurationProperty("dataSource", IsRequired = true)]
        public string DataSource
        {
            get => (string)this["dataSource"];
            set => this["dataSource"] = value;
        }

        /// <summary>
        /// The license type to use
        /// </summary>
        [ConfigurationProperty("licenseType", IsRequired = true)]
        public LicenseType LicenseType
        {
            get => (LicenseType)this["licenseType"];
            set => this["licenseType"] = value;
        }

        /// <summary>
        /// Which of AuthenticationMode's four modes to use to establish a NEW session.
        /// See <see cref="AuthenticationMode"/>, and <see cref="SessionId"/> for the fifth,
        /// independent, reconnect-first option.
        /// </summary>
        [ConfigurationProperty("authenticationMode", IsRequired = false, DefaultValue = AuthenticationMode.OnBaseCredentials)]
        public AuthenticationMode AuthenticationMode
        {
            get => (AuthenticationMode)this["authenticationMode"];
            set => this["authenticationMode"] = value;
        }

        /// <summary>
        /// The OnBase password that will be used to connect. Required when
        /// <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>;
        /// also read (via <see cref="DecryptedPassword"/>) when <see cref="AuthenticationMode"/>
        /// is <see cref="Enumerations.AuthenticationMode.AccessToken"/> and no
        /// <see cref="AccessToken"/> was supplied directly, in which case it's traded for a
        /// token via the Hyland IdP's "password" grant type (see
        /// Unity.01.ConnectingToOnBase's IdpAuthentication).
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password
        {
            get => (string)this["password"];
            set => this["password"] = value;
        }

        /// <summary>
        /// The OnBase username that will be used to connect. Required when
        /// <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.OnBaseCredentials"/>;
        /// also read (via <see cref="DecryptedUsername"/>) for the same IdP "password"
        /// grant scenario described on <see cref="Password"/>.
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false)]
        public string Username
        {
            get => (string)this["username"];
            set => this["username"] = value;
        }

        /// <summary>
        /// The Hyland Identity Provider (IdP) access token that will be used to connect,
        /// when <see cref="AuthenticationMode"/> is <see cref="Enumerations.AuthenticationMode.AccessToken"/>.
        /// Optional: when left blank in this mode, a token is instead obtained from the
        /// IdP at connect time (see <see cref="IdpSettings"/> and
        /// Unity.01.ConnectingToOnBase's IdpAuthentication).
        /// </summary>
        [ConfigurationProperty("accessToken", IsRequired = false)]
        public string AccessToken
        {
            get => (string)this["accessToken"];
            set => this["accessToken"] = value;
        }

        /// <summary>
        /// The Session ID of an already-established OnBase session to reconnect to.
        /// Optional, and independent of <see cref="AuthenticationMode"/>: when set,
        /// Connect() always attempts a reconnect to this session FIRST, regardless of
        /// which AuthenticationMode is configured, falling back to establishing a brand
        /// new session (via AuthenticationMode) only if that reconnect fails and
        /// <see cref="AllowSessionFailover"/> is true. Reconnecting only succeeds if the
        /// session's own IsDisconnectEnabled value is false (see <see cref="KeepAlive"/>).
        /// </summary>
        [ConfigurationProperty("sessionId", IsRequired = false)]
        public string SessionId
        {
            get => (string)this["sessionId"];
            set => this["sessionId"] = value;
        }

        /// <summary>
        /// The separately-issued Single Sign-On license token, required when
        /// <see cref="AuthenticationMode"/> is
        /// <see cref="Enumerations.AuthenticationMode.SingleSignOn"/>.
        /// </summary>
        [ConfigurationProperty("licenseToken", IsRequired = false)]
        public string LicenseToken
        {
            get => (string)this["licenseToken"];
            set => this["licenseToken"] = value;
        }

        /// <summary>
        /// Decrypted Username
        /// </summary>
        public string DecryptedUsername
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedUsername))
                {
                    return decryptedUsername;
                }

                decryptedUsername = Username.IsEncrypted()
                    ? Username.DecryptRegistryKey()
                    : Username;

                return decryptedUsername;
            }
        }

        /// <summary>
        /// Decrypted Password
        /// </summary>
        public string DecryptedPassword
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedPassword))
                {
                    return decryptedPassword;
                }

                decryptedPassword = Password.IsEncrypted()
                    ? Password.DecryptRegistryKey()
                    : Password;

                return decryptedPassword;
            }
        }

        /// <summary>
        /// Decrypted Access Token
        /// </summary>
        public string DecryptedAccessToken
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedAccessToken))
                {
                    return decryptedAccessToken;
                }

                decryptedAccessToken = AccessToken.IsEncrypted()
                    ? AccessToken.DecryptRegistryKey()
                    : AccessToken;

                return decryptedAccessToken;
            }
        }

        /// <summary>
        /// Decrypted License Token
        /// </summary>
        public string DecryptedLicenseToken
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedLicenseToken))
                {
                    return decryptedLicenseToken;
                }

                decryptedLicenseToken = LicenseToken.IsEncrypted()
                    ? LicenseToken.DecryptRegistryKey()
                    : LicenseToken;

                return decryptedLicenseToken;
            }
        }

        /// <summary>
        /// When true, the connected session's IsDisconnectEnabled value will be set to false
        /// </summary>
        public bool KeepAlive { get; set; }

        /// <summary>
        /// If true, when reconnecting a session fails, the system will connect a new session instead of throwing an exception
        /// </summary>
        public bool AllowSessionFailover { get; set; }
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
                    // No fields to validate: NT authentication uses the Windows identity
                    // the process is already running as, the Unity API takes no
                    // credentials of any kind for this mode. See Training Notes above.
                    break;

                case AuthenticationMode.AccessToken:
                    // No hard requirement here: AccessToken can legitimately be blank in
                    // this mode, in which case a token is obtained from the IdP at
                    // connect time instead (using Username/DecryptedPassword and
                    // IdpSettings). See Training Notes above.
                    break;

                case AuthenticationMode.SingleSignOn:
                    if (string.IsNullOrEmpty(LicenseToken))
                    {
                        throw new ConfigurationErrorsException("AuthenticationMode 'SingleSignOn' requires a licenseToken attribute.");
                    }
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
