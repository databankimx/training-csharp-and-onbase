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
 * No functional bugs in the original download, only project-structure and standards
 *     updates were needed (plus a "foeach" typo in a console message).
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch02.TextbookCode.WorkingWithForLoops
{
    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                // using a for loop to count up by one
                Console.WriteLine("Count up by one");

                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine();

                // using a for loop to count down by one
                Console.WriteLine("Count down by one");

                for (int i = 10; i > 0; i--)
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine();

                // using a for loop to count up by 2
                Console.WriteLine("Count up by two");

                for (int i = 0; i < 10; i += 2)
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine();

                // using a for loop to increment by multiples of 5
                Console.WriteLine("Count up by multiples of 5");

                for (int i = 5; i < 1000; i *= 5)
                {
                    Console.WriteLine(i);
                }
                Console.WriteLine();

                // using a foreach loop with integers
                Console.WriteLine("foreach over an array of integers");

                int[] arrInts = [1, 2, 3, 4, 5];
                foreach (int number in arrInts)
                {
                    Console.WriteLine(number);
                }
                Console.WriteLine();

                // using a for each loop with strings
                Console.WriteLine("foreach over an array of strings");

                string[] arrStrings = ["First", "Second", "Third", "Fourth", "Fifth"];
                foreach (string text in arrStrings)
                {
                    Console.WriteLine(text);
                }
                Console.WriteLine();

                // using a while loop
                int whileCounter = 0;

                Console.WriteLine("Counting up by one using a while loop");
                while (whileCounter < 10)
                {
                    Console.WriteLine(whileCounter);
                    whileCounter++;
                }
                Console.WriteLine();

                // using a do-while loop
                int doCounter = 0;

                Console.WriteLine("Counting up using a do-while loop");
                do
                {
                    Console.WriteLine(doCounter);
                    doCounter++;
                } while (doCounter < 10);
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
