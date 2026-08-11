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
 * Two things were fixed here, neither a casing change:
 *   1. The class was named "studentStudent," the same duplicated-word artifact found
 *      in ValueTypePassing, renamed to "student." The duplication had even leaked
 *      into the age validation message ("StudentStudent age must be greater than 6").
 *   2. Main() was completely empty in the original download, the class and its
 *      properties were defined but never exercised, so running it produced no
 *      output at all. A short usage demo was added below.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch03.TextbookCode.UsingProperties
{
    internal class student
    {
        private string firstName;
        private char middleInitial;
        private string lastName;
        private int age;
        private string program;
        private double gpa;

        public student(string first, string last)
        {
            firstName = first;
            lastName = last;
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public char MiddleInitial
        {
            get { return middleInitial; }
            set { middleInitial = value; }
        }

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

        public string Program
        {
            get { return program; }
            set { program = value; }
        }

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
                var firstStudent = new student("Jane", "Doe");
                firstStudent.MiddleInitial = 'Q';
                firstStudent.Age = 20;
                firstStudent.Program = "Computer Science";
                firstStudent.GPA = 3.8;

                firstStudent.displayDetails();
                Console.WriteLine();

                // Exercising the two validation branches, this should print a
                // rejection message and leave Age/GPA unchanged rather than
                // accepting the out-of-range value
                firstStudent.Age = 3;
                firstStudent.GPA = 4.5;
                firstStudent.displayDetails();
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
