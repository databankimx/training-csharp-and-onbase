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
#endregion

namespace Unity._00.CommonFunctionality.Models.Configuration
{
    /// <summary>
    /// Defines settings related to DocPop links for the OnBase system
    /// </summary>
    public class DocPopSettings : ConfigurationElement
    {
        #region Properties
        /// <summary>
        /// XML configuration file name
        /// </summary>
        public const string ElementName = "docPop";

        /// <summary>
        /// URL to DocPop ASPX page on the OnBase web server
        /// </summary>
        [ConfigurationProperty("docPopBaseUrl", IsRequired = true)]
        public string DocPopBaseUrl
        {
            get => (string)base["docPopBaseUrl"];
            set => base["docPopBaseUrl"] = value;
        }

        /// <summary>
        /// Seed value for generating checksum hashes - must match value in AppNet web.config
        /// </summary>
        [ConfigurationProperty("docPopChecksumSeed", IsRequired = true)]
        public string DocPopChecksumSeed
        {
            get => (string)base["docPopChecksumSeed"];
            set => base["docPopChecksumSeed"] = value;
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
