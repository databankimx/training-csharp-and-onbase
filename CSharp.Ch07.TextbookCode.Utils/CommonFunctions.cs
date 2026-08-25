/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utils
{
    public static class CommonFunctions
    {
        public static double ReadDataFromIO()
        {
            // We are simulating an I/O by putting the current thread to sleep.
            Thread.Sleep(2000);
            return 10d;
        }

        public static Task<double> ReadDataFromIOAsync()
        {
            return Task.Run(new Func<double>(ReadDataFromIO));
        }
        public static double DoIntensiveCalculations()
        {

            // We are simulating intensive calculations 
            // by doing nonsens divisions and multiplications

            double result = 10000d;
            var maxValue = Int32.MaxValue >> 4;
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

        public static void WaitForKeyWhehDebugging()
        {

            if (Debugger.IsAttached)
            {
                Console.Write("Press any key to continue . . .");
                Console.ReadKey(true);
            }

        }
    }
}
