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

namespace CSharp.Ch08.Reflection.Models.Objects
{
    /// <summary>
    /// Defines an Employee as a derived specific class of the Person superclass
    /// </summary>
    public class Employee : Person
    {
        #region Properties
        /// <summary>
        /// Employee's Department
        /// </summary>
        public string Department { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create an instance of the Employee Class
        /// </summary>
        public Employee() { }

        /// <summary>
        /// Create and partially initialize a new instance of the Employee class
        /// </summary>
        /// <param name="firstName">Employee's First Name</param>
        public Employee(string firstName) : base(firstName)
        {
        }

        /// <summary>
        /// Create and partially initialize a new instance of the Employee class
        /// </summary>
        /// <param name="firstName">Employee's First Name</param>
        /// <param name="lastName">Employee's Last Name</param>
        public Employee(string firstName, string lastName) : base(firstName, lastName)
        {
        }

        /// <summary>
        /// Create and initialize a new instance of the Employee class
        /// </summary>
        /// <param name="firstName">Employee's First Name</param>
        /// <param name="lastName">Employee's Last Name</param>
        /// <param name="department">Employee's Department</param>
        public Employee(string firstName, string lastName, string department) : base(firstName, lastName)
        {
            // Validate the department
            if (string.IsNullOrEmpty(department))
                throw new ArgumentOutOfRangeException(nameof(department), department, "Department must not be null or blank!");

            // Store the Department
            Department = department;
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
