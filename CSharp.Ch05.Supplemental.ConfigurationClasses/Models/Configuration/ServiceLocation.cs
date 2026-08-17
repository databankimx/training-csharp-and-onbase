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
using CSharp.Ch05.Supplemental.ConfigurationClasses.HelperClasses.Extensions;
using Hyland.Unity;
#endregion

namespace CSharp.Ch05.Supplemental.ConfigurationClasses.Models.Configuration
{
    public class ServiceLocation : ConfigurationElement
    {
        #region Private Members
        // Username (decrypted or plain-text)
        private string decryptedUsername;

        // Password (decrypted or plain-text)
        private string decryptedPassword;
        #endregion

        #region Properties
        /// <summary>
        /// The URL to the Service.asmx page of the Application Server
        /// </summary>
        [ConfigurationProperty("servicePath", IsRequired = true)]
        public string ServicePath => (string)this["servicePath"];

        /// <summary>
        /// The data source name (configured at the Application Server) to connect to
        /// </summary>
        [ConfigurationProperty("dataSource", IsRequired = true)]
        public string DataSource => (string)this["dataSource"];

        /// <summary>
        /// The license type to use
        /// </summary>
        [ConfigurationProperty("licenseType", IsRequired = true)]
        public LicenseType LicenseType => (LicenseType)this["licenseType"];

        /// <summary>
        /// A "true" or "false" value indicating of NT authentication is to be used.
        /// </summary>
        [ConfigurationProperty("useNTAuthentication", IsRequired = true)]
        public bool UseNtAuthentication => (bool)this["useNTAuthentication"];

        /// <summary>
        /// The Domain to connect to when using NT Authentication
        /// </summary>
        [ConfigurationProperty("domain", IsRequired = false)]
        public string Domain => (string)this["domain"];

        /// <summary>
        /// The OnBase password that will be used to connect
        /// </summary>
        [ConfigurationProperty("password", IsRequired = false)]
        public string Password => (string)this["password"];

        /// <summary>
        /// The OnBase username that will be used to connect
        /// </summary>
        [ConfigurationProperty("username", IsRequired = false)]
        public string Username => (string)this["username"];

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
        /// When using NT Authentication, indicates whether to login using the account running the application (false) or to login
        /// using an alternate domain user (true)
        /// </summary>
        internal bool AlternateDomainUser { get; private set; }
        #endregion

        #region Serialization Methods
        /// <summary>
        /// Enforce username and password when useNTAuthentication is false
        /// </summary>
        protected override void PostDeserialize()
        {
            base.PostDeserialize();

            // if NT Authentication is false, verify a username and password is configured
            if (!UseNtAuthentication &&
                (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password)))
            {
                throw new ConfigurationErrorsException("UseNTAuthentication value of 'false' requires both a username and password attribute.");
            }

            // if NT Authentication is true, verify domain, username, and password are either all specified or none specified
            if (!UseNtAuthentication ||
                (string.IsNullOrEmpty(Domain) && string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)))
                return;
            if (string.IsNullOrEmpty(Domain) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                throw new ConfigurationErrorsException("UseNTAuthentication value of 'true' requires domain, username, or password either all be specified or none be specified.");
            }

            AlternateDomainUser = true;
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
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
