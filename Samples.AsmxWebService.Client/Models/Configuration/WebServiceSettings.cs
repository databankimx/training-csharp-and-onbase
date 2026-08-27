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

#region Directives
using System.Configuration;
#endregion

namespace Samples.AsmxWebService.Client.Models.Configuration
{
    /// <summary>
    /// Web service configuration file settings
    /// </summary>
    public class WebServiceSettings : ConfigurationSection
    {
        #region Constants
        /// <summary>
        /// Section name in XML Configuration File
        /// </summary>
        public const string SectionName = "webServiceSettings";
        #endregion

        #region Properties
        /// <summary>
        /// URL to WCF Service
        /// </summary>
        [ConfigurationProperty("webServiceUrl", IsRequired = true)]
        public string WebServiceUrl
        {
            get => (string)base["webServiceUrl"];
            set => base["webServiceUrl"] = value;
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
