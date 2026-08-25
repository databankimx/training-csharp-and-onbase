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

namespace CSharp.Ch08.Reflection.Models.Attributes
{
    /// <summary>
    /// Custom attribute recording which department catalog a Course belongs to, and how many
    /// credit hours it's worth. Applied to classes (see Objects.Course), and read back at
    /// runtime via reflection rather than being consumed by any compile-time mechanism.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class CourseCatalogAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Department the course is cataloged under
        /// </summary>
        public string Department { get; }

        /// <summary>
        /// Number of credit hours the course is worth
        /// </summary>
        public int CreditHours { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize a new instance of the CourseCatalogAttribute class
        /// </summary>
        /// <param name="department">Department the course is cataloged under</param>
        /// <param name="creditHours">Number of credit hours the course is worth</param>
        #pragma warning disable IDE0290 // Intentionally not using primary constructor syntax for clarity
        public CourseCatalogAttribute(string department, int creditHours)
        #pragma warning restore IDE0290
        {
            Department = department;
            CreditHours = creditHours;
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
