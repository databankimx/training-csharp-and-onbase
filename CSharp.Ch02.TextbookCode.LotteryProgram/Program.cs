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
 * The original textbook download wrapped the number-selection loop in a pointless
 *     second outer loop that reran the entire selection 49 times, only the final pass
 *     ever mattered, every earlier pass was discarded work. That outer loop has been
 *     removed below; everything else follows the same logic as the original.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch02.TextbookCode.LotteryProgram
{
    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                // used to set up a range of values to choose from
                int[] range = new int[49];

                // used to simulate lottery numbers chosen
                int[] picked = new int[6];

                // set up a random number generator
                Random rnd = new();

                // populate the range with values from 1 to 49
                for (int i = 0; i < 49; i++)
                {
                    range[i] = i + 1;
                }

                // pick 6 random numbers
                for (int select = 0; select < 6; select++)
                {
                    picked[select] = range[rnd.Next(49)];
                }

                Console.WriteLine("Your lotto numbers are:");
                for (int j = 0; j < 6; j++)
                {
                    Console.Write(" " + picked[j] + " ");
                }
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
