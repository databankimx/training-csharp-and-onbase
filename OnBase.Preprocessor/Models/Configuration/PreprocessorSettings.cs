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
using OnBase.Preprocessor.Models.Enumerations;
#endregion

namespace OnBase.Preprocessor.Models.Configuration
{
    /// <summary>
    /// Defines an XML config file section for preprocessor settings
    /// </summary>
    public class PreprocessorSettings : ConfigurationSection
    {
        #region Constants
        /// <summary>
        /// Section name (in XML config file)
        /// </summary>
        public const string SectionName = "preprocessorSettings";
        #endregion

        #region Properties
        /// <summary>
        /// When true, trace logging is enabled
        /// </summary>
        [ConfigurationProperty("debugMode", IsRequired = true)]
        public bool DebugMode
        {
            get => (bool)base["debugMode"];
            set => base["debugMode"] = value;
        }

        /// <summary>
        /// When true, console logging and user interactive wait are enabled
        /// </summary>
        [ConfigurationProperty("interactive", IsRequired = true)]
        public bool Interactive
        {
            get => (bool)base["interactive"];
            set => base["interactive"] = value;
        }

        /// <summary>
        /// Selected character set (Ansi | Ascii | Unicode)
        /// </summary>
        [ConfigurationProperty("charSet", IsRequired = true)]
        public CharacterSet CharSet
        {
            get => (CharacterSet)base["charSet"];
            set => base["charSet"] = value;
        }

        /// <summary>
        /// Character to use in place of invalid characters in output file
        /// </summary>
        [ConfigurationProperty("invalidCharacterSubstitute", IsRequired = true)]
        public string InvalidCharacterSubstitute
        {
            get => (string)base["invalidCharacterSubstitute"];
            set => base["invalidCharacterSubstitute"] = value;
        }

        /// <summary>
        /// When true, for-feed characters will be replaced with newlines
        /// </summary>
        [ConfigurationProperty("replaceFormFeeds", IsRequired = true)]
        public bool ReplaceFormFeeds
        {
            get => (bool)base["replaceFormFeeds"];
            set => base["replaceFormFeeds"] = value;
        }

        /// <summary>
        /// Where Serilog writes this run's log entries. Replaces the old log4net &lt;appender&gt;
        /// file path, which used to live in a separate XML section entirely - keeping every
        /// setting for this tool in the one preprocessorSettings section it already had.
        /// </summary>
        [ConfigurationProperty("logFilePath", IsRequired = true)]
        public string LogFilePath
        {
            get => (string)base["logFilePath"];
            set => base["logFilePath"] = value;
        }

        /// <summary>
        /// List of explicit character replacements to carry out
        /// </summary>
        [ConfigurationProperty("characterReplacements", IsRequired = true)]
        [ConfigurationCollection(typeof(CharacterElement), AddItemName = "character")]
        public CharacterCollection CharacterReplacements
        {
            get => (CharacterCollection)base["characterReplacements"];
            set => base["characterReplacements"] = value;
        }
        #endregion

        #region Parent Class Overrides
        /// <summary>
        /// Allow the class elements to be editable
        /// </summary>
        /// <returns>false (not read-only)</returns>
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
