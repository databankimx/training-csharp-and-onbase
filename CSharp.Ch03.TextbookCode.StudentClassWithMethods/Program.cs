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
 * This program is a standardized version of the code lab from:
 *   MCSD Certification Toolkit (Exam 70-483)
 *   https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * No functional bugs in the original download, only project-structure updates were
 *     needed. Casing (firstName, lastName, grade, concatenateName, displayName) is
 *     left as originally downloaded rather than converted to PascalCase,
 *     TextbookCode.* projects preserve the original naming even where it doesn't
 *     match our usual standard.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch03.TextbookCode.StudentClassWithMethods
{
    internal class Student
    {
        public static int StudentCount;
        public string firstName;
        public string lastName;
        public string grade;

        public string concatenateName()
        {
            string fullName = firstName + " " + lastName;
            return fullName;
        }

        public void displayName()
        {
            string name = concatenateName();
            Console.WriteLine(name);
        }
    }

    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                Student firstStudent = new();
                Student.StudentCount++;
                Student secondStudent = new();
                Student.StudentCount++;

                firstStudent.firstName = "John";
                firstStudent.lastName = "Smith";
                firstStudent.grade = "six";

                secondStudent.firstName = "Tom";
                secondStudent.lastName = "Thumb";
                secondStudent.grade = "two";

                firstStudent.displayName();
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
