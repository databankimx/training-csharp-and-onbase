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
using System.Text.RegularExpressions;
#endregion

namespace Samples.NuGetLibrary
{
    /// <summary>
    /// Validates and normalizes ZIP code input, the SAME validation logic every one of
    /// the Samples.* projects in this training set would otherwise need to reimplement
    /// (or, worse, implement slightly differently) on its own.
    /// </summary>
    public static class ZipCodeValidator
    {
        #region Fields
        private static readonly Regex ZipCodePattern = new(@"^\d{5}$", RegexOptions.Compiled);
        #endregion

        #region Public Methods
        /// <summary>
        /// Determines whether <paramref name="zipCode"/> is a valid 5-digit US ZIP code,
        /// after trimming leading/trailing whitespace.
        /// </summary>
        /// <param name="zipCode">The value to validate.</param>
        /// <returns><see langword="true"/> if <paramref name="zipCode"/> is a valid 5-digit ZIP code; otherwise, <see langword="false"/>.</returns>
        public static bool IsValid(string? zipCode)
        {
            if (string.IsNullOrWhiteSpace(zipCode)) return false;
            return ZipCodePattern.IsMatch(zipCode.Trim());
        }

        /// <summary>
        /// Trims and returns <paramref name="zipCode"/> if it's a valid 5-digit ZIP code;
        /// otherwise returns <see langword="null"/>.
        /// </summary>
        /// <param name="zipCode">The value to normalize.</param>
        /// <returns>The normalized ZIP code, or <see langword="null"/> if <paramref name="zipCode"/> is not valid.</returns>
        public static string? Normalize(string? zipCode)
        {
            if (!IsValid(zipCode)) return null;
            return zipCode!.Trim();
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
