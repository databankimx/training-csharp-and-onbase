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

namespace Samples.WcfService.Models.Configuration
{
    /// <summary>
    /// Defines the operating settings for thew web service
    /// </summary>
    public class ServiceSettings : ConfigurationSection
    {
        #region Properties
        /// <summary>
        /// Name of section in web.config XML
        /// </summary>
        public const string SectionName = "serviceSettings";

        /// <summary>
        /// When true, trace logging will be enabled (otherwise only errors are logged)
        /// </summary>
        [ConfigurationProperty("debugMode", IsRequired = true)]
        public bool DebugMode
        {
            get => (bool)base["debugMode"];
            set => base["debugMode"] = value;
        }

        /// <summary>
        /// Connection settings for the database
        /// </summary>
        [ConfigurationProperty("database", IsRequired = true)]
        public DatabaseSettings Database
        {
            get => (DatabaseSettings)base["database"];
            set => base["database"] = value;
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// Allow the class properties to be editable
        /// </summary>
        /// <returns>False (not read-only)</returns>
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
