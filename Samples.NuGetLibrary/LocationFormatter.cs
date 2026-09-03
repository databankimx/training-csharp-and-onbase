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
using System.Collections.Generic;
using System.Linq;
using System.Text;
#endregion

namespace Samples.NuGetLibrary
{
    /// <summary>
    /// Formats <see cref="Location"/> values for display, kept consistent across every
    /// project in this training set that shows a location lookup result to a user.
    /// </summary>
    public static class LocationFormatter
    {
        #region Public Methods
        /// <summary>
        /// Formats <paramref name="location"/> as a single line: "City, County County, State ZipCode".
        /// </summary>
        /// <param name="location">The location to format.</param>
        /// <returns>The formatted string.</returns>
        public static string ToDisplayString(Location location)
        {
            // *Migration Note: ArgumentNullException.ThrowIfNull() is a .NET 6+-only static
            //   helper, this project multi-targets net48 too, so the classic null-check
            //   pattern is used instead, it's the one form that compiles on both targets.
            if (location == null) throw new ArgumentNullException(nameof(location));
            return $"{location.City}, {location.County} County, {location.State} {location.ZipCode}";
        }

        /// <summary>
        /// Formats a sequence of <see cref="Location"/> values as one line per location,
        /// or a single "No results found" line if <paramref name="locations"/> is empty.
        /// </summary>
        /// <param name="locations">The locations to format.</param>
        /// <returns>The formatted, multi-line string.</returns>
        public static string ToDisplayString(IEnumerable<Location> locations)
        {
            if (locations == null) throw new ArgumentNullException(nameof(locations));

            var list = locations.ToList();
            if (list.Count == 0) return "No results found.";

            var builder = new StringBuilder();
            foreach (var location in list) builder.AppendLine(ToDisplayString(location));
            return builder.ToString().TrimEnd();
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
