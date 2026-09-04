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
 * Title:           MCSD Certification Toolkit (Exam 70-483)
 * Authors:         Covaci, Tiberiu; Stephens, Rod; Varallo, Vincent; O'Brien, Gerry.
 * ISBN:			978-1-118-61209-5
 * Info:			https://www.wiley.com/en-us/MCSD+Certification+Toolkit+%28Exam+70+483%29%3A+Programming+in+C%23-p-9781118612095
 * Downloads:		https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 * Cheat Sheets:	https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Toolkit%20Cheat%20Sheets%20&%20Key%20Terms.zip
 * Errata:			https://www.wiley.com/en-us/MCSD+Certification+Toolkit+%28Exam+70+483%29%3A+Programming+in+C%23-p-9781118612095#errata-section
 *
 * Note:	MCSD/MCPD (exam 70-483) certs are discontinued
 *          However, this textbook is still very useful for understanding how DataBank uses C#
 */

// Motto: pay4books
#endregion

#region Further Reading
// Documentation on C#:     https://learn.microsoft.com/en-us/dotnet/csharp/
// Microsoft Code Samples:  https://learn.microsoft.com/en-us/samples/browse/?languages=csharp
// .NET Documentation:		https://learn.microsoft.com/en-us/dotnet/api/?view=netframework-4.8
// Specific class:		    https://learn.microsoft.com/en-us/dotnet/api/<<namespace.class>>?view=netframework-4.8
// 	 e.g.:			        https://learn.microsoft.com/en-us/dotnet/api/system.applicationexception?view=netframework-4.8
#endregion

// Using the #region decoration has no effect on the compiled code at runtime,
//   but it does provide a way to easily mark functional areas in the code for debugging and support

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch01.HelloWorld
{
    // By default, in a console (CMD window) project, the runnable class is called "Program"
    // You can change this if desired
    internal static class Program
    {
        #region Main Executable Method
        // What makes a class runnable is the presence of the Main() method
        // By default, the method receives an array (args) containing any command-line arguments sent
        private static void Main(string[] args)
        {
            // Note how throughout our code, we indent everything that occurs within a set of curly-braces
            // This assists in understanding where a block is subordinate to another one

            // We always surround our code with try/catch, so that we can handle any exceptions that occur
            // You'll learn more about this in chapter 5
            try
            {
                // This is the classic, single-line introduction to a coding language
                Console.WriteLine("Hello world!");

                // Executable code does not need to appear in the "Main" method. You can call a separate method
                // Pause and await user interaction before executing the next block of code
                Pause();
                
                // We can modify the printout to greet a person named in a command-line argument
                // Note: This will throw an error if no command-line argument is provided
                string name = args[0];

                // The classic way to embed a variable value in a string is using string.Format
                Console.WriteLine(string.Format("Hello {0}!", name));

                // The WriteLine() method can interpolate formatting without needing "string.Format"
                Console.WriteLine("Hello {0}!", name);

                // In newer versions of C#, we can accomplish the same thing using string interpolation
                Console.WriteLine($"Hello {name}!");

                // Pause and await user interaction before executing the next block of code
                Pause();

                // We can also take in input from the user
                Console.WriteLine("Enter your name to continue...");
                name = Console.ReadLine();
                Console.WriteLine($"Hello {name}!");
            }
            catch (Exception ex)
            {
                // It's important to catch all exceptions down to the root error
                // For later lessons, I have moved this to a separate class in the "SharedLibrary" project
                while (ex != null)
                {
                    Console.WriteLine(ex);
                    ex = ex.InnerException;
                }
            }
            finally
            {
                // For testing purposes, we want to ensure that the window remains open after the program executes
                // For later lessons, I have moved this to a separate class in the "SharedLibrary" project
                if (!Debugger.IsAttached)
                {
                    Console.WriteLine("\nDone!\n\nPress any key to exit!");
                    Console.ReadKey();
                }
            }
        }
        #endregion

        #region Helper Functions
        // Pause and await user interaction before executing the next block of code
        private static void Pause()
        {
            // Notice that I am not including try/catch here
            // Although that is sometimes advantageous, exceptions thrown in a called function
            //   will bubble up to the calling method and can be handled there
            Console.WriteLine($"\nPress any key to continue...");
            Console.ReadKey();
            Console.Clear();
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
