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

namespace CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects
{
    /// <summary>
    /// Here, we are inheriting an abstract class to define a subclass of student
    /// </summary>
    public class CollegeStudent : BaseStudent
    {
        #region Public Members
        /// <summary>
        /// Student first name
        /// </summary>
        public string FirstName;

        /// <summary>
        /// Student last name
        /// </summary>
        public string LastName;

        /// <summary>
        /// Student major subject
        /// </summary>
        public string Major;

        /// <summary>
        /// Student grade point average
        /// </summary>
        public double GPA;
        #endregion

        #region Implement BaseStudent
        // When an abstract method is declared in the parent class, it *must* be implemented in the derived class

        /// <summary>
        /// Display the details for the student object
        /// </summary>
        public override void OutputDetails()
        {
            Console.WriteLine($"Student {FirstName} {LastName} is enrolled in {Major} and has a GPA of {GPA}");
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
