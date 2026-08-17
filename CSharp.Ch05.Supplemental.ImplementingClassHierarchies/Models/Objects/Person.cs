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
using System.IO;
#endregion

namespace CSharp.Ch05.Supplemental.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a Person as a superclass for all instances in the hierarchy
    /// </summary>
    public class Person
    {
        #region Properties
        /// <summary>
        /// Person's First Name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Person's Last Name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Person's Middle Name
        /// </summary>
        public string MiddleName { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the Person class
        /// </summary>
        public Person() { }

        /// <summary>
        /// Create and initialize a new instance of the Person class
        /// </summary>
        /// <param name="firstName">Person's First Name</param>
        /// <param name="lastName">Person's Last Name</param>
        public Person(string firstName, string lastName)
        {
            // Validate the first and last names
            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentOutOfRangeException(nameof(firstName), firstName, @"First name must not be null or blank!");
            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentOutOfRangeException(nameof(lastName), lastName, @"Last name must not be null or blank!");

            FirstName = firstName;
            LastName = lastName;
        }

        /// <summary>
        /// Create and initialize a new instance of the Person class
        /// </summary>
        /// <param name="firstName">Person's First Name</param>
        /// <param name="middleName">Person's Middle Name</param>
        /// <param name="lastName">Person's Last Name</param>
        public Person(string firstName, string middleName, string lastName) : this(firstName, lastName)
        {
            // Validate the middle name
            if (string.IsNullOrEmpty(middleName))
                throw new ArgumentOutOfRangeException(nameof(middleName), middleName, @"Middle name must not be null or blank!");

            MiddleName = middleName;
        }
        #endregion

        #region Public Member Methods
        /// <summary>
        /// Full name of Person
        /// </summary>
        /// <param name="reverse">When true, the name will be reversed (e.g. LastName, FirstName)</param>
        /// <param name="includeMiddle">When true, the middle name will be included</param>
        /// <returns>Person's full name</returns>
        public string FullName(bool reverse = false, bool includeMiddle = false)
        {
            if (string.IsNullOrEmpty(LastName))
                throw new InvalidDataException("Last name is null or blank!");
            if (string.IsNullOrEmpty(FirstName))
                throw new InvalidDataException("First name is null or blank!");
            if (includeMiddle && string.IsNullOrEmpty(MiddleName))
                throw new InvalidDataException("Middle name is null or blank!");

            string front = includeMiddle ? $"{FirstName} {MiddleName}" : FirstName;
            return reverse ? $"{LastName}, {front}" : $"{front} {LastName}";
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
