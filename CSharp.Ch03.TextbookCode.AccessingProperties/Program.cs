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

#region Textbook Information
/*
 * This program is a corrected, standardized version of the code lab from:
 *   MCSD Certification Toolkit (Exam 70-483)
 *   https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * Same class as UsingProperties, and the same "studentStudent" duplicated-word bug,
 *     renamed to "student" here too, including the age validation message that had
 *     the duplication baked into user-facing text. Unlike UsingProperties, Main()
 *     here already exercised the class, no demo needed to be added.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch03.TextbookCode.AccessingProperties
{
    internal class student
    {
        // First name is a string, so we use string type.
        private string firstName;

        // Middle initial is a single character, so we use char type.
        private char middleInitial;

        // Last name is a string, so we use string type.
        private string lastName;

        // Age is an integer, so we use int type.
        private int age;

        // Program is a string, so we use string type.
        private string program;

        // Grade Point Average (GPA) is a double value, which can be fractional, so we use double type.
        private double gpa;

        /// <summary>
        /// Initializes a new instance of the student class with the specified first and last names.
        /// </summary>
        /// <param name="first">The first name.</param>
        /// <param name="last">The last name.</param>
        public student(string first, string last)
        {
            firstName = first;
            lastName = last;
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        /// <summary>
        /// Gets or sets the middle initial.
        /// </summary>
        public char MiddleInitial
        {
            get { return middleInitial; }
            set { middleInitial = value; }
        }

        /// <summary>
        /// Gets or sets the student's age.
        /// </summary>
        /// <remarks>Only values greater than 6 are accepted; otherwise, the assigned value is ignored and
        /// a message is written to the console.</remarks>
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
                    Console.WriteLine("Student age must be greater than 6");
                }
            }
        }

        /// <summary>
        /// Gets or sets the program name.
        /// </summary>
        public string Program
        {
            get { return program; }
            set { program = value; }
        }

        /// <summary>
        /// Gets or sets the grade point average.
        /// </summary>
        /// <remarks>Values greater than 4.0 are rejected, and a message is written to the
        /// console.</remarks>
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

        /// <summary>
        /// Writes the student's full name and GPA to the console.
        /// </summary>
        public void displayDetails()
        {
            Console.WriteLine(FirstName + " " + MiddleInitial + " " + LastName);
            Console.WriteLine("Has a GPA of " + GPA);
        }
    }

    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                var myStudent = new student("Tom", "Thumb");
                myStudent.MiddleInitial = 'R';
                myStudent.Age = 15;
                myStudent.GPA = 3.5;
                myStudent.displayDetails();
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.WriteLine($"\n{ex.GetType().Name}: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
                    ex = ex.InnerException;
                }
            }
            finally
            {
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("\nDone!\n\nPress any key to exit!");
                    Console.ReadKey();
                }
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
