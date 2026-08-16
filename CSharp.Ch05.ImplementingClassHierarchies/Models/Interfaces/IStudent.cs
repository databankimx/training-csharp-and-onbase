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
using System.Collections.Generic;
using CSharp.Ch05.ImplementingClassHierarchies.Models.Objects;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Interfaces
{
    /// <summary>
    /// Interface to implement a Student
    /// </summary>
    public interface IStudent
    {
        /*
         * An interface is different from a class. By itself, it does not typically provide an object that you
         * can use in your code. Instead, it provides a contract, defining the responsibilities for classes that implement it
         */

        // NOTE: Members and method signatures in an interface do not implement code,
        //       and they do not require access modifiers

        #region Properties
        /// <summary>
        /// The student's list of current courses
        /// </summary>
        List<Course> Courses { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Print the student's current grades (must be implemented by the class that implements this interface)
        /// </summary>
        void PrintGrades();
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
