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
using CSharp.Ch08.Reflection.Models.Interfaces;
#endregion

namespace CSharp.Ch08.Reflection.Models.Objects
{
    /// <summary>
    /// Defines a Student as a special class of Person (and implements the IStudent Interface)
    /// </summary>
    public class Student : Person, IStudent
    {
        #region IStudent
        /// <summary>
        /// The student's list of current courses
        /// </summary>
        public List<Course> Courses { get; set; }

        /// <summary>
        /// Print the student's current grades
        /// </summary>
        public void PrintGrades()
        {
            foreach (var course in Courses)
            {
                Console.WriteLine($"{course.Name}: {course.LetterGrade} ({course.RawGrade})");
            }
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
