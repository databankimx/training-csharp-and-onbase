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
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch06.Supplemental._04.MulticastDelegates
{
    internal static class Program
    {
        #region Private Members
        // Define a custom delegate that has a string parameter and returns void.
        internal delegate void CustomDel(string s);
        #endregion

        #region Main Executable Method
        // Presence of Main() method renders the class runnable
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Declare instances of the custom delegate.

                // In this example, you can omit the custom delegate if you 
                // want to and use Action<string> instead.
                //`Action<string> hiDel, byeDel, multiDel, multiMinusHiDel;`

                // Create the delegate object hiDel that references the
                // method Hello.
                CustomDel hiDel = Hello;

                // Create the delegate object byeDel that references the
                // method Goodbye.
                CustomDel byeDel = Goodbye;

                // The two delegates, hiDel and byeDel, are combined to 
                // form multiDel. 
                CustomDel multiDel = hiDel + byeDel;

                // Remove hiDel from the multicast delegate, leaving byeDel,
                // which calls only the method Goodbye.
                CustomDel multiMinusHiDel = multiDel - hiDel;

                Console.WriteLine("Invoking delegate hiDel:");
                hiDel("A");
                Console.WriteLine("Invoking delegate byeDel:");
                byeDel("B");
                Console.WriteLine("Invoking delegate multiDel:");
                multiDel("C");
                Console.WriteLine("Invoking delegate multiMinusHiDel:");
                multiMinusHiDel("D");
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
        // Define two methods that have the same signature as CustomDel.
        private static void Hello(string s)
        {
            Console.WriteLine("  Hello, {0}!", s);
        }

        private static void Goodbye(string s)
        {
            Console.WriteLine("  Goodbye, {0}!", s);
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
