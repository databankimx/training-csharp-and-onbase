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
using System;
#endregion

namespace Samples.WcfService.HelperClasses
{
    /// <summary>
    /// Date- and time-stamping methods
    /// </summary>
    public static class Stamp
    {
        #region Public Methods
        /// <summary>
        /// Generate date stamp
        /// </summary>
        /// <param name="format">(optional) .NET DateTime format string</param>
        /// <returns>Formatted date stamp</returns>
        public static string Date(string format = null)
        {
            return $"{DateTime.Now.ToString(string.IsNullOrEmpty(format) ? "d" : format)}";
        }

        /// <summary>
        /// Generate time stamp
        /// </summary>
        /// <param name="format">(optional) .NET DateTime format string</param>
        /// <returns>Formatted tim stamp</returns>
        public static string Time(string format = null)
        {
            return $"{DateTime.Now.ToString(string.IsNullOrEmpty(format) ? "T" : format)}";
        }

        /// <summary>
        /// Generate date/time stamp
        /// </summary>
        /// <param name="format">(optional) .NET DateTime format string</param>
        /// <returns>Formatted date/time stamp</returns>
        public static string DateAndTime(string format = null)
        {
            return $"{DateTime.Now.ToString(string.IsNullOrEmpty(format) ? "u" : format)}";
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
