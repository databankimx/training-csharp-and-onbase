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

namespace CSharp.Ch08.Supplemental._01.CustomAttributes.Models.Attributes
{
    /// <summary>
    /// Maps one external data column to one property on the class it's applied to. Unlike
    /// CourseCatalogAttribute in the main Chapter 8 lesson, this allows being applied MULTIPLE
    /// times to the same class, one mapping per external column, letting a single class carry
    /// its entire mapping table as stacked attributes.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class DataMappingAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Name of the external (e.g. database) column
        /// </summary>
        public string ColumnName { get; }

        /// <summary>
        /// Name of the property on this class that column maps to
        /// </summary>
        public string PropertyName { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create and initialize a new instance of the DataMappingAttribute class
        /// </summary>
        /// <param name="columnName">Name of the external column</param>
        /// <param name="propertyName">Name of the property it maps to</param>
#pragma warning disable IDE0290 // Use primary constructor
        public DataMappingAttribute(string columnName, string propertyName)
#pragma warning restore IDE0290 // Use primary constructor
        {
            ColumnName = columnName;
            PropertyName = propertyName;
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
