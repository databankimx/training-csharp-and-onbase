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
#endregion

namespace RestApi._00.CommonFunctionality.Models.Configuration
{
    #region Training Notes
    /*
     * *Migration Note: a close port of Unity.00.CommonFunctionality's own IdpSettings,
     * with one real difference in emphasis rather than shape: on the Unity API side, IdP
     * settings are only relevant for ONE of four AuthenticationModes (AccessToken), so
     * every property there stays IsRequired=false at the XML-schema level, with actual
     * "do we have what we need" validation deferred to IdpAuthentication.GetAccessToken()
     * at runtime. Here, EVERY currently-implemented connection (there's only one:
     * OnBaseCredentials, the "password" grant) needs these settings, they're not an
     * edge case. Still left IsRequired=false here too, for the same underlying reason
     * Unity.00 gives: this element has no visibility into ServiceLocation.AuthenticationMode,
     * a sibling element, to know whether it's even relevant for a given config, and
     * RestApi.01.ConnectingToOnBase's IdpAuthentication already does the real
     * "do we have everything" check right where it's needed.
     *
     * IdpClientSecret gets the same DataProtection-based protection as ServiceLocation's
     * Password, via DecryptedIdpClientSecret (see DataProtectionExtensions).
     */
    #endregion

    public class IdpSettings : ConfigurationElement
    {
        #region Private Members
        // IdP Client Secret (decrypted or plain-text)
        private string decryptedIdpClientSecret;
        #endregion

        #region Properties
        /// <summary>
        /// XML configuration file name
        /// </summary>
        public const string ElementName = "idpSettings";

        /// <summary>
        /// The URL to the Hyland Identity Provider's token endpoint (e.g.
        /// "https://MyServer.net/identityprovider/connect/token").
        /// </summary>
        [ConfigurationProperty("idpUrl", IsRequired = false)]
        public string IdpUrl
        {
            get => (string)this["idpUrl"];
            set => this["idpUrl"] = value;
        }

        /// <summary>
        /// The Hyland IdP tenant name.
        /// </summary>
        [ConfigurationProperty("idpTenant", IsRequired = false)]
        public string IdpTenant
        {
            get => (string)this["idpTenant"];
            set => this["idpTenant"] = value;
        }

        /// <summary>
        /// The Hyland IdP client ID.
        /// </summary>
        [ConfigurationProperty("idpClientId", IsRequired = false)]
        public string IdpClientId
        {
            get => (string)this["idpClientId"];
            set => this["idpClientId"] = value;
        }

        /// <summary>
        /// The Hyland IdP client secret.
        /// </summary>
        [ConfigurationProperty("idpClientSecret", IsRequired = false)]
        public string IdpClientSecret
        {
            get => (string)this["idpClientSecret"];
            set => this["idpClientSecret"] = value;
        }

        /// <summary>
        /// The scope requested from the Hyland IdP.
        /// </summary>
        [ConfigurationProperty("idpScope", IsRequired = false, DefaultValue = "evolution")]
        public string IdpScope
        {
            get => (string)this["idpScope"];
            set => this["idpScope"] = value;
        }

        /// <summary>
        /// The OAuth2 grant type to use when requesting a token from the Hyland IdP. Only
        /// "password" is currently implemented, see
        /// RestApi.01.ConnectingToOnBase.HelperClasses.OnBase.IdpAuthentication for the
        /// other (stubbed) grant types.
        /// </summary>
        [ConfigurationProperty("idpGrantType", IsRequired = false, DefaultValue = "password")]
        public string IdpGrantType
        {
            get => (string)this["idpGrantType"];
            set => this["idpGrantType"] = value;
        }

        /// <summary>
        /// Decrypted IdP Client Secret. Plain text is returned as-is; a "protected:"-prefixed
        /// value (see DataProtectionExtensions) is unprotected first.
        /// </summary>
        public string DecryptedIdpClientSecret
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedIdpClientSecret))
                {
                    return decryptedIdpClientSecret;
                }

                decryptedIdpClientSecret = IdpClientSecret.IsProtected()
                    ? IdpClientSecret.Unprotect()
                    : IdpClientSecret;

                return decryptedIdpClientSecret;
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
