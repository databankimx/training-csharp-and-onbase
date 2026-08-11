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
 * Unlike the lottery and average-grades labs, the original download here had no
 *     functional bugs, only project-structure and standards updates were needed.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch02.TextbookCode.UsingIfStatements
{
    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                // declare some variables for use in the code and assign initial values
                int first = 2;
                int second = 0;

                // use a single if statement to evaluate a condition and output some text
                // indicating the results

                Console.WriteLine("Single if statement");

                if (first == 2)
                {
                    Console.WriteLine("The if statement evaluated to true");
                }
                Console.WriteLine("This line outputs regardless of the if condition");

                Console.WriteLine();

                // create an if statement that evaluates two conditions and executes
                // statements only if both are true
                Console.WriteLine("An if statement using && operator.");

                if (first == 2 && second == 0)
                {
                    Console.WriteLine("The if statement evaluated to true");
                }
                Console.WriteLine("This line outputs regardless of the if condition");

                Console.WriteLine();

                // create nested if statements

                Console.WriteLine("Nested if statements.");

                if (first == 2)
                {
                    if (second == 0)
                    {
                        Console.WriteLine("Both outer and inner conditions are true.");
                    }
                    Console.WriteLine("Outer condition is true, inner may be true.");
                }
                Console.WriteLine("This line outputs regardless of the if condition");

                Console.WriteLine();
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
