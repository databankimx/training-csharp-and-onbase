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

namespace OnBase.Preprocessor.Models.Configuration
{
    /// <summary>
    /// Defines an explicit character replacement for the preprocessor
    /// </summary>
    public class CharacterElement : ConfigurationElement
    {
        #region Properties
        /// <summary>
        /// Character to be removed from the output file
        /// </summary>
        [ConfigurationProperty("original", IsRequired = true)]
        public char Original
        {
            get => (char)base["original"];
            set => base["original"] = value;
        }

        /// <summary>
        /// Character to replace the removed character
        /// </summary>
        [ConfigurationProperty("replacement", IsRequired = true)]
        public char Replacement
        {
            get => (char)base["replacement"];
            set => base["replacement"] = value;
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
