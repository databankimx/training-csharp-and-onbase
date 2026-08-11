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

#region Directives
using System;
using System.Diagnostics;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.SharedLibrary.HelperClasses
{
    /// <summary>
    /// Generic methods used in other classes
    /// </summary>
    public static class GenericFunctions
    {
        #region Public Methods
        /// <summary>
        /// Pause to await user interaction
        /// </summary>
        /// <param name="clear">When true, clear the screen before continuing</param>
        /// <param name="final">When true, message end of program</param>
        public static void Pause(bool clear = true, bool final = false)
        {
            try
            {
                if (final && Debugger.IsAttached) return;
                string next = "continue";
                if (final)
                {
                    Console.Write("Done! ");
                    next = "exit program";
                }
                Console.WriteLine($"\nPress any key to {next}...");
                Console.ReadKey();
                if (clear) Console.Clear();
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error in Pause() method!", ex);
            }
        }

        /// <summary>
        /// Log chapter info to the console
        /// </summary>
        /// <param name="codeSamples">Path to code samples</param>
        /// <param name="cheatSheet">Path to cheat sheet</param>
        /// <param name="chapter">Chapter number</param>
        /// <param name="topic">Chapter topic</param>
        public static void FinishChapter(string codeSamples, string cheatSheet, int chapter, string topic)
        {
            try
            {
                Console.Clear();
                Console.WriteLine($"Chapter {chapter} Complete!");
                Console.WriteLine($"You have now learned the basics of {topic}.");
                Console.WriteLine();
                Console.WriteLine("Textbook code samples can be found in...");
                Console.WriteLine(codeSamples);
                Console.WriteLine();
                Console.WriteLine("Textbook cheat sheets can be found in...");
                Console.WriteLine(cheatSheet);
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error in FinishChapter() method!", ex);
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
