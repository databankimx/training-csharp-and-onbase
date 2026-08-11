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
 * Two things were fixed, neither a casing change:
 *   1. The same "studentStudent" duplicated-word bug found in the other Ch03 labs,
 *      this time it had also spread into the variable names (studentStudent1,
 *      studentStudent2, studentStudent3). Renamed to "student" and student1/2/3.
 *   2. Main() created three instances via the three overloaded constructors but
 *      never printed anything, so running it produced no visible output and the
 *      whole point of the lesson, seeing what each overload actually built, was
 *      invisible. Added output showing each instance's fields after construction.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch03.TextbookCode.OverloadingConstructors
{
    internal class student
    {
        public string firstName;
        public string lastName;
        public int grade;
        public string schoolName;

        public student()
        {
        }

        public student(string first, string last)
        {
            firstName = first;
            lastName = last;
        }

        public student(string first, string last, int grade, string school)
        {
            firstName = first;
            lastName = last;
            this.grade = grade;
            schoolName = school;
        }
    }

    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                student student1 = new();
                Console.WriteLine($"student1: firstName=[{student1.firstName}], lastName=[{student1.lastName}], " +
                                   $"grade=[{student1.grade}], schoolName=[{student1.schoolName}]");

                student student2 = new("Tom", "Jones");
                Console.WriteLine($"student2: firstName=[{student2.firstName}], lastName=[{student2.lastName}], " +
                                   $"grade=[{student2.grade}], schoolName=[{student2.schoolName}]");

                student student3 = new("Mike", "Myers", 5, "My School");
                Console.WriteLine($"student3: firstName=[{student3.firstName}], lastName=[{student3.lastName}], " +
                                   $"grade=[{student3.grade}], schoolName=[{student3.schoolName}]");
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
