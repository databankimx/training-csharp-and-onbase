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
    /// Student class with encapsulated member variables
    /// </summary>
    public class EncapsulatedStudent
    {
        /*
         * We'll use properties in order to encapsulate (or hide) the member variables
         */

        #region Private Members
        // Student first name
        private string firstName;

        // Student middle initial
        private char middleInitial;

        // Student last name
        private string lastName;

        // Student age
        private int age;

        // Student program
        private string program;

        // Student grade point average
        private double gpa;
        #endregion

        #region Properties
        /// <summary>
        /// Student first name
        /// </summary>
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        /// <summary>
        /// Student last name
        /// </summary>
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        /// <summary>
        /// Student middle initial
        /// </summary>
        public char MiddleInitial
        {
            get { return middleInitial; }
            set { middleInitial = value; }
        }

        /// <summary>
        /// Student age
        /// </summary>
        public int Age
        {
            get { return age; }
            set
            {
                if (value > 6)
                {
                    age = value;
                }
                else
                {
                    Console.WriteLine("EncapsulatedStudent age must be greater than 6");
                }
            }
        }

        /// <summary>
        /// Student program
        /// </summary>
        public string Program
        {
            get { return program; }
            set { program = value; }
        }

        /// <summary>
        /// Student grade point average
        /// </summary>
        public double GPA
        {
            get { return gpa; }
            set
            {
                if (value <= 4.0)
                {
                    gpa = value;
                }
                else
                {
                    Console.WriteLine("GPA cannot be greater than 4.0");
                }
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create an instance of the EncapsulatedStudent class
        /// </summary>
        /// <param name="first">First name</param>
        /// <param name="last">Last name</param>
        public EncapsulatedStudent(string first, string last)
        {
            firstName = first;
            lastName = last;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Write out the student's details to the console
        /// </summary>
        public void DisplayDetails()
        {
            Console.WriteLine($"{FirstName} {MiddleInitial} {LastName} has a GPA of {GPA}");
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
