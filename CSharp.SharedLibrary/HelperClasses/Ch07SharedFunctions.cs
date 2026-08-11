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
using System.Threading;
using System.Threading.Tasks;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.SharedLibrary.HelperClasses
{
    /// <summary>
    /// Defines functions shared among methods in Program class
    /// </summary>
    public static class Ch07SharedFunctions
    {
        #region Public Methods
        /// <summary>
        /// Simulate a task that takes time
        /// </summary>
        /// <returns></returns>
        public static double SimulateReadDataFromIo()
        {
            try
            {
                // We are simulating an I/O wait by putting the current thread to sleep.
                Thread.Sleep(2000);
                return 10d;
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error simulating IO wait!", ex);
            }
        }

        /// <summary>
        /// Asynchronous method using the above simulated wait as a delegate
        /// </summary>
        /// <returns></returns>
        public static Task<double> SimulateReadDataFromIoAsync()
        {
            try
            {
                return Task.Run(new Func<double>(SimulateReadDataFromIo));
                // In C# 6, can be simplified as shown below:
                // return Task.Run(SimulateReadDataFromIo); // NOSONAR
            }
            catch (Exception ex)
            {
                throw new DatabankException("Error in asynchronous call to simulate IO wait!", ex);
            }
        }

        /// <summary>
        /// Simulate a resource-intensive task
        /// </summary>
        /// <returns></returns>
        public static double DoIntensiveCalculations()
        {
            try
            {
                // We are simulating intensive calculations 
                // by doing nonsense divisions and multiplications
                double result = 10000d;
                const int maxValue = int.MaxValue >> 4;
                for (int i = 1; i < maxValue; i++)
                {
                    if (i % 2 == 0)
                    {
                        result /= i;
                    }
                    else
                    {
                        result *= i;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        /// <summary>
        /// Creates a pause step only when in debugging mode
        /// </summary>
        public static void WaitForKeyWhenDebugging()
        {
            if (!Debugger.IsAttached) return;
            Console.Write("Press <ENTER> to continue . . .");
            Console.ReadLine();
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
