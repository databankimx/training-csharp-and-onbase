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
 * The original textbook download had a copy-paste chain bug: after the myByte block,
 *     every subsequent sizeof() call still measured sizeof(byte) instead of the actual
 *     type being demonstrated. char, decimal, float, long, and short all printed "1"
 *     regardless of their real size. Each call below now measures its own type.
 */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
#endregion

namespace CSharp.Ch03.TextbookCode.UsingValueTypes
{
    internal static class Program
    {
        #region Main Executable Method
        private static void Main()
        {
            try
            {
                // declare some numeric data types
                int myInt;
                double myDouble;
                byte myByte;
                char myChar;
                decimal myDecimal;
                float myFloat;
                long myLong;
                short myShort;
                bool myBool;

                // assign values to these types and then
                // print them out to the console window
                // also use the sizeOf operator to determine
                // the number of bytes taken up be each type

                myInt = 5000;
                Console.WriteLine("Integer");
                Console.WriteLine(myInt);
                Console.WriteLine(myInt.GetType());
                Console.WriteLine(sizeof(int));
                Console.WriteLine();

                myDouble = 5000.0;
                Console.WriteLine("Double");
                Console.WriteLine(myDouble);
                Console.WriteLine(myDouble.GetType());
                Console.WriteLine(sizeof(double));
                Console.WriteLine();

                myByte = 254;
                Console.WriteLine("Byte");
                Console.WriteLine(myByte);
                Console.WriteLine(myByte.GetType());
                Console.WriteLine(sizeof(byte));
                Console.WriteLine();

                myChar = 'r';
                Console.WriteLine("Char");
                Console.WriteLine(myChar);
                Console.WriteLine(myChar.GetType());
                Console.WriteLine(sizeof(char));
                Console.WriteLine();

                myDecimal = 20987.89756M;
                Console.WriteLine("Decimal");
                Console.WriteLine(myDecimal);
                Console.WriteLine(myDecimal.GetType());
                Console.WriteLine(sizeof(decimal));
                Console.WriteLine();

                myFloat = 254.09F;
                Console.WriteLine("Float");
                Console.WriteLine(myFloat);
                Console.WriteLine(myFloat.GetType());
                Console.WriteLine(sizeof(float));
                Console.WriteLine();

                myLong = 2544567538754;
                Console.WriteLine("Long");
                Console.WriteLine(myLong);
                Console.WriteLine(myLong.GetType());
                Console.WriteLine(sizeof(long));
                Console.WriteLine();

                myShort = 3276;
                Console.WriteLine("Short");
                Console.WriteLine(myShort);
                Console.WriteLine(myShort.GetType());
                Console.WriteLine(sizeof(short));
                Console.WriteLine();

                myBool = true;
                Console.WriteLine("Boolean");
                Console.WriteLine(myBool);
                Console.WriteLine(myBool.GetType());
                Console.WriteLine(sizeof(bool));
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
