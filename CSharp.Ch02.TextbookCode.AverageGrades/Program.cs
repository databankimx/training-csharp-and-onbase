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
 * The original textbook download divided two ints (total / gradeCount) before ever
 *     reaching the double result, silently truncating the average to a whole number.
 *     496 / 6 became 82 instead of 82.666..., a classic integer-division bug. Fixed
 *     below by casting total to double before dividing.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch02.TextbookCode.AverageGrades
{
    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                // foreach loop to average grades in an array
                // set up an integer array and assign some values
                int[] arrGrades = [78, 89, 90, 76, 98, 65];

                // create three variables to hold the sum, number of grades, and the average
                int total = 0;
                int gradeCount = 0;
                double average;

                // loop to iterate over each integer value in the array
                // foreach doesn't need to know the size initially as it is determined
                // at the time the array is accessed.
                foreach (int grade in arrGrades)
                {
                    total += grade; // add each grade value to total
                    gradeCount++;   // increment counter for use in average
                }

                // Defensive guard in case arrGrades is ever emptied out above
                if (gradeCount == 0) total = gradeCount = 1;

                // Casting total to double before dividing forces floating-point division
                //     instead of integer division, which is what the original bug was missing
                average = (double)total / gradeCount;
                Console.WriteLine($"{average:F2}");
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
