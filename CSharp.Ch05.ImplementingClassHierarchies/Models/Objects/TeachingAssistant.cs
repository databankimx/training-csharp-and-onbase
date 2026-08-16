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
using CSharp.Ch05.ImplementingClassHierarchies.Models.Interfaces;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a TeachingAssistant as a special class of Faculty and as a Student
    /// </summary>
    public class TeachingAssistant : Faculty, IStudent
    {
        #region Private Members
        // Defines the TA's student identity as a member variable
        // This is done so that the IStudent implementations in the Student class do not need to be duplicated
        #pragma warning disable IDE0090 // In lessons, not simplifying (to `new()`) to avoid confusion for students
        private readonly Student myStudent = new Student();
        #pragma warning restore IDE0090
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets a formatted credential statement for the teaching assistant.Write out the TA's name and degree
        /// </summary>
        /// <returns>A sentence that includes the teaching assistant's first name, last name, and degree.</returns>
        public string Credentials()
        {
            return $"TA {FirstName} {LastName} has a {Degree} degree.";
        }
        #endregion

        #region IStudent
        /// <summary>
        /// Encapsulates the IStudent Courses property
        /// </summary>
        public List<Course> Courses
        {
            get => myStudent.Courses;
            set => myStudent.Courses = value;
        }

        /// <summary>
        /// Encapsulates the IStudent PrintGrades() method
        /// </summary>
        public void PrintGrades()
        {
            myStudent.PrintGrades();
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
