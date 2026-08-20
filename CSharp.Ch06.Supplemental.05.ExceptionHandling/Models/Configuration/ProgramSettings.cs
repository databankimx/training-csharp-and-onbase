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

#pragma warning disable S125 // Allow commented code in training projects
namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Configuration
{
    /// <summary>
    /// Configuration file settings for program
    /// </summary>
    public class ProgramSettings : ConfigurationSection
    {
        #region Constants
        /// <summary>
        /// Section name in XML config file
        /// </summary>
        public const string SectionName = "programSettings";
        #endregion

        #region Properties
        /// <summary>
        /// When true, trace logging will be written
        /// </summary>
        [ConfigurationProperty("debugMode", IsRequired = true)]
        public bool DebugMode
        {
            get => (bool)base["debugMode"];
            set => base["debugMode"] = value;

            // Alternate syntax (may be easier to understand)
            //get
            //{
            //    return (bool)base["debugMode"];
            //}
            //set
            //{
            //    base["debugMode"] = value;
            //}
        }

        /// <summary>
        /// When true, the program window will wait for user interaction before closing
        /// </summary>
        [ConfigurationProperty("interactive", IsRequired = true)]
        public bool Interactive
        {
            get => (bool)base["interactive"];
            set => base["interactive"] = value;
        }
        #endregion
    }
}
#pragma warning restore S125

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
