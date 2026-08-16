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
using CSharp.Ch05.ImplementingClassHierarchies.Models.Enumerations;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a Faculty member as a special class of Person
    /// </summary>
    public class Faculty : Employee
    {
        #region Properties
        /// <summary>
        /// Degree held by faculty member
        /// </summary>
        public Degree Degree { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create an instance of the Faculty class
        /// </summary>
        public Faculty(){ }

        /// <summary>
        /// Create and initialize an instance of the Faculty class
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="degree"></param>
        public Faculty(string firstName, string lastName, Degree degree) : base(firstName, lastName)
        {
            Degree = degree;
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
