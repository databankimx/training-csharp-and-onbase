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
 *     updates were needed. The "new int()" / "new System.Int32()" syntax below is left
 *     intact on purpose, that's the entire point of this lab: showing the explicit
 *     alias-versus-.NET-type construction syntax side by side.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

#pragma warning disable S1481   // Unused local variables are part of lesson design
#pragma warning disable CS0168  // Unused local variables are part of lesson design
#pragma warning disable IDE0090 // Use 'new(...)' is ignored as part of lesson design
#pragma warning disable S125    // Commented code is intentionally left in place for lesson design
namespace CSharp.Ch03.TextbookCode.ValueTypeAlias
{
    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                // create a variable to hold a value type using the alias form
                // but don't assign a variable
                int myInt;
                int myNewInt = new int();

                // create a variable to hold a .NET value type
                // this type is the .NET version of the alias form int
                // note the use of the keyword new, we are creating an object from
                // the System.Int32 class
                System.Int32 myInt32 = new System.Int32();

                // you will need to comment out this first Console.WriteLine statement
                // as Visual Studio will generate an error about using an unassigned
                // variable. This is to prevent using a value that was stored in the
                // memory location prior to the creation of this variable

                // Console.WriteLine(myInt);

                // print out the default value assigned to an int variable
                // that had no value assigned previously
                Console.WriteLine(myNewInt);

                // this statement will work fine and will print out the default value for
                // this type, which in this case is 0
                Console.WriteLine(myInt32);
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
#pragma warning restore S125
#pragma warning restore IDE0090
#pragma warning restore CS0168
#pragma warning restore S1481

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
