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
using Unity._00.CommonFunctionality.HelperClasses.Extensions;
#endregion

namespace Unity._00.CommonFunctionality.Models.Configuration
{
    #region Training Notes
    /*
     * *Migration Note: new in this training set, holds the settings needed to obtain a
     * Hyland Identity Provider (IdP) access token, separate from ServiceLocation, which
     * holds settings needed to CONNECT once a token (or other credentials) is already in
     * hand. This element is intentionally NOT marked IsRequired on OnBaseSettings, and
     * none of its own properties are IsRequired either, a config that never uses
     * AuthenticationMode.AccessToken shouldn't need to fill in placeholder IdP settings
     * it will never use. The actual "do we have everything we need" check happens at
     * runtime, in Unity.01.ConnectingToOnBase's IdpAuthentication.GetAccessToken(), right
     * where it's needed, rather than as static config-schema validation here (which has
     * no visibility into ServiceLocation.AuthenticationMode, a sibling element, to know
     * whether IdP settings are even relevant for a given config).
     *
     * IdpClientSecret gets the SAME DPAPI/registry-encryption treatment as
     * ServiceLocation's Password/AccessToken/LicenseToken, via DecryptedIdpClientSecret,
     * it's exactly as sensitive as any of those.
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
        /// Unity.01.ConnectingToOnBase.HelperClasses.OnBase.IdpAuthentication for the
        /// other (stubbed) grant types.
        /// </summary>
        [ConfigurationProperty("idpGrantType", IsRequired = false, DefaultValue = "password")]
        public string IdpGrantType
        {
            get => (string)this["idpGrantType"];
            set => this["idpGrantType"] = value;
        }

        /// <summary>
        /// Decrypted IdP Client Secret
        /// </summary>
        public string DecryptedIdpClientSecret
        {
            get
            {
                if (!string.IsNullOrEmpty(decryptedIdpClientSecret))
                {
                    return decryptedIdpClientSecret;
                }

                decryptedIdpClientSecret = IdpClientSecret.IsEncrypted()
                    ? IdpClientSecret.DecryptRegistryKey()
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
