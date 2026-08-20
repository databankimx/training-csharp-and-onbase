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

#region Using Directives
using System;
using System.Threading;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch06.Supplemental._06.ParameterizedThreadStart
{
    // Example passing a delegate as a thread parameter
    internal class Program
    {
        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // A very common example of using a callback is passing an argument to a new thread
                // We'll cover threading in detail in the next chapter, but for now review this example

                // Start a thread that calls a parameterized static method (callback).
                var newThread = new Thread(Program.DoWork);
                newThread.Start(42);

                // Start a thread that calls a parameterized instance method (callback).
                var w = new Program();
                newThread = new Thread(w.DoMoreWork);
                newThread.Start("The answer.");

                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Delegate Methods
        // By making this method static, when we use it, it becomes a static delegate
        private static void DoWork(object data)
        {
            Console.WriteLine("Static thread procedure. Data='{0}'", data);
        }

        #pragma warning disable S2325 // Method is intentionally non-static
        // Although otherwise very similar, this non-static method becomes an instance delegate
        private void DoMoreWork(object data)
        {
            Console.WriteLine("Instance thread procedure. Data='{0}'", data);
        }
        #pragma warning restore S2325
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
