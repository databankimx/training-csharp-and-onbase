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

namespace CSharp.Ch04.UsingTypes.Models.Objects
{
    /// <summary>
    /// Defines a sub-class of Person as an Employee
    /// </summary>
    public class Employee : Person
    {
        #region Properties
        /// <summary>
        /// Employee department
        /// </summary>
        public string Department { get; set; }

        /// <summary>
        /// Employee job title
        /// </summary>
        public string JobTitle { get; set; }

        /// <summary>
        /// Employee ID
        /// </summary>
        public int Id { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Employee sub-class
        /// </summary>
        public Employee() { }

        /// <summary>
        /// Create a new instance of the Employee class
        /// </summary>
        /// <param name="id">Employee ID</param>
        public Employee(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Create and initialize a new instance of the Employee sub-class
        /// </summary>
        /// <param name="firstName">Employee first name</param>
        /// <param name="lastName">Employee last name</param>
        /// <param name="department">Employee department</param>
        /// <param name="jobTitle">Employee job title</param>
        public Employee(string firstName, string lastName, string department, string jobTitle) : base(firstName, lastName)
        {
            Department = department;
            JobTitle = jobTitle;
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
