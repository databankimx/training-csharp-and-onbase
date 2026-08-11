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
 * The original textbook download named the class "studentStudent" and the variable
 *     "firstStudentStudent", a duplicated word, almost certainly a rename/copy-paste
 *     artifact rather than an intentional name. Renamed to "student" and
 *     "firstStudent" below. This is a naming defect fix, not a casing change,
 *     everything else keeps the original lowercase casing per our policy of
 *     preserving TextbookCode.* naming conventions as downloaded.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch03.TextbookCode.ValueTypePassing
{
    internal class student
    {
        public string firstName;
        public string lastName;
        public string grade;
    }

    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                int num1 = 2;
                int num2 = 3;
                int result;

                student firstStudent = new();

                firstStudent.firstName = "John";
                firstStudent.lastName = "Smith";
                firstStudent.grade = "six";

                result = sum(num1, num2);
                Console.Write("Sum is: ");
                Console.WriteLine(result);  // outputs 5
                Console.WriteLine();

                changeValues(num1, num2);
                Console.WriteLine();
                Console.WriteLine("Back from changeValues()");
                Console.WriteLine(num1);  // outputs 2
                Console.WriteLine(num2);  // outputs 3

                Console.WriteLine();
                Console.WriteLine("First name for firstStudent is " + firstStudent.firstName);
                changeName(firstStudent);
                Console.WriteLine();
                Console.WriteLine("First name for firstStudent is " + firstStudent.firstName);
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

        #region Helper Functions
        private static int sum(int value1, int value2)
        {
            Console.WriteLine("In method sum()");
            return value1 + value2;
        }

        private static void changeValues(int value1, int value2)
        {
            Console.WriteLine("In changeValues()");
            Console.WriteLine("value1 is " + value1);  // outputs 2
            Console.WriteLine("value2 is " + value2);  // outputs 3
            Console.WriteLine();
            Console.WriteLine("Changing values");

            value1--;
            value2 += 5;

            Console.WriteLine();
            Console.WriteLine("value1 is now " + value1);  // outputs 1
            Console.WriteLine("value2 is now " + value2);  // outputs 8
        }

        private static void changeName(student refValue)
        {
            Console.WriteLine();
            Console.WriteLine("In changeName()");
            refValue.firstName = "George";
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
