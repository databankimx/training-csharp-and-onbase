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
using Hyland.Unity;
#endregion

namespace Unity._03.DocumentRetrieval.Models.Objects
{
    /// <summary>
    /// Defines a keyword for integration retrieval
    /// </summary>
    public class KeywordInfo
    {
        #region Properties
        /// <summary>
        /// Keyword type ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Keyword type name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Keyword value
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Keyword data type
        /// </summary>
        public KeywordDataType Type { get; set; }

        /// <summary>
        /// Keyword data length
        /// </summary>
        public long Length { get; set; }
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
