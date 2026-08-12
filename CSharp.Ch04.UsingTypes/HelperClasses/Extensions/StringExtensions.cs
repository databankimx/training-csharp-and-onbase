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
using System;
#endregion

namespace CSharp.Ch04.UsingTypes.HelperClasses.Extensions
{
    public static class StringExtensions
    {
        #region Public Extension Methods
        /// <summary>
        /// Extend string's instance.CompareTo to accept a comparison type
        /// </summary>
        /// <param name="strA">Instance string</param>
        /// <param name="strB">String to compare</param>
        /// <param name="comparisonType">Comparison method</param>
        /// <returns>Ordering int</returns>
        public static int CompareTo(this string strA, string strB, StringComparison comparisonType)
        {
            return string.Compare(strA, strB, comparisonType);
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
