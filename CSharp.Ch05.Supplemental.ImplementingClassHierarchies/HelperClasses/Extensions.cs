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
using CSharp.Ch05.Supplemental.ImplementingClassHierarchies.Models.Objects;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch05.Supplemental.ImplementingClassHierarchies.HelperClasses
{
    /// <summary>
    /// Extension methods
    /// </summary>
    public static class Extensions
    {
        #region Public Extension Methods
        /// <summary>
        /// Get a Person's initials
        /// </summary>
        /// <typeparam name="T">Any type derived from Person</typeparam>
        /// <param name="t">Person (or descendant) instance</param>
        /// <returns>Initials, uppercased</returns>
        public static string Initials<T>(this T t) where T : Person
        {
            var person = t as Person;
            if (string.IsNullOrEmpty(person.FirstName) || string.IsNullOrEmpty(person.LastName))
                throw new DatabankException("Unable to produce initials. One or more required name(s) blank!");
            return $"{person.FirstName.Substring(0, 1)}{(string.IsNullOrEmpty(person.MiddleName) ? "" : person.MiddleName.Substring(0, 1))}{person.LastName.Substring(0, 1)}".ToUpper();
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
