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
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CSharp.Ch06.Supplemental.s05.ExceptionHandling.HelperClasses.Extensions;
using CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Configuration;
using CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Enumerations;
using CSharp.Ch06.Supplemental.s05.ExceptionHandling.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using log4net;
#endregion

#pragma warning disable S125
namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling
{
    // Default class for console executable
    internal static class Program
    {
        #region Private Globals
        // Use a meaningful enum to define the values
        private static Status status = Status.Success;

        // WHen logging errors to an external file, we have standardized on the use of Log4Net
        private static ILog logger;
        private const string LogType = "Text";

        // When true, additional trace information will be logged
        private static ProgramSettings settings;
        #endregion

        #region Main Executable Method
        /*
         * One way that we use error handling is when we want our program to return an execution status upon completion.
         * A great example of this is the requirement to return an integer value from a preprocessor.
         *
         * Note how I have changed the return data type for the Main() method below
         */

        // Presence of Main() method renders the class runnable
        private static int Main()
        {
            /*
             * Chapter Notes:
             *  Try/Catch/Finally blocks allow the program to handle unexpected exceptions
             *
             * The basic syntax is this:
             *
             * try
             * {
             *     // This is where your program code goes in case an exception is thrown
             * }
             * catch (ExceptionType name)
             * {
             *     // This bock contains code to execute in the event an exception occurs
             * }
             * finally
             * {
             *     // This block contains code that will execute at the end regardless of whether an exception was thrown
             * }
             *
             * You can use all three blocks as illustrated above, or you can use only catch or finally
             *
             * A try/catch will behave as described above, except no code is aligned to execute at the end
             *
             * A try/finally includes code that executes regardless of whether or not an error occurs,
             *   but it does not trap the exception in the process
             */

            try
            {
                Initialize();

                string startMessage = $"{TimeStamp()} - Start program...{Environment.NewLine}";
                if (settings.DebugMode) startMessage.TraceLog();
                GenericFunctions.Pause();

                // Demonstrate a debug assertion
                Assertions();
                GenericFunctions.Pause();

                // This method handles exceptions in the method itself
                SpecificToGeneral();
                GenericFunctions.Pause();

                // Compare using block to try/finally
                CompareToUsing();
                GenericFunctions.Pause();

                // This method will sometimes throw an exception
                PossibleException();
                GenericFunctions.Pause();

                // This method will demonstrate some arithmetic exceptions
                ArithmeticExceptions();
                GenericFunctions.Pause();
            }
            catch (Exception ex)
            {
                // Here we are setting the status value because an error was caught
                status = Status.Error;

                // Alternately, we can set the exit-code value (if we left the method void)
                Environment.ExitCode = (int)status;
                // But, we should be careful with this.
                // If we later call Environment.Exit(), that terminates the stack without executing finally blocks
                
                // Now, we'll call our exception handler
                ex.HandleException();
                GenericFunctions.Pause();
            }
            finally
            {
                string endMessage = $"{TimeStamp()} - End program...";
                if (settings.DebugMode) endMessage.TraceLog();

                if (settings.Interactive)
                {
                    Logging.ViewLog();
                    GenericFunctions.Pause();
                    GenericFunctions.Pause(final: true);
                }
            }

            return (int)status;

            // Because the end of the Main method terminates this application, we could do this here instead of the return
            // Environment.Exit((int)status);
            // But we need to remember to be careful of where we use this.
        }
        #endregion

        #region Example Functions
        // Example of assertion for debugging
        private static void Assertions()
        {
            /*
             * Chapter Notes:
             *
             * Using an assertion allows the developer to pause execution in Visual Studio (or in a debug build)
             *   and view the stack trace
             *
             * The basic syntax is:
             *     Debug.Assert(condition);
             *
             * When debugging, if an assertion occurs, but its condition is not met, the program halts,
             *   and the stack trace is displayed
             *
             * In a release build, all Assert statements are ignored
             */

            const int max = 10;

            int[] numbers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

            // This Assert will not trigger a stack trace
            Debug.Assert(numbers.Max() < max, $"Array max value is {max} or more!");

            // This Assert will trigger a stack trace
            Debug.Assert(numbers.Length < max, $"Array length reached {max} or more!");
        }

        // Order to catch exceptions
        private static void SpecificToGeneral()
        {
            try
            {
                // Best practices dictate that you catch exceptions in order from most- to least-specific
                // This is important if you want to handle different exceptions differently,
                //   because the first "catch" block that matches your exception is the only one that will execute
                //   even if it is an ancestor of the type you want to catch

                #pragma warning disable S2930 // In this lesson, we can ignore disposing this item, since it will not be created successfully
                #pragma warning disable S1075 // This is just an example of an invalid path, so we can ignore the hard-coded path warning
                #pragma warning disable S1481 // This is just an example to throw an exception, so we can ignore the unused variable warning
                var file = File.Open(@"C:\InvalidDirectory\InvalidFile.txt", FileMode.Append);
                #pragma warning restore S1481
                #pragma warning restore S1075
                #pragma warning restore S2930
            }
            catch (TrainingException ex)
            {
                Console.WriteLine("Caught a training exception!");
                ex.HandleException();
                // Do something specific to TrainingException
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("Caught a directory not found exception!");
                ex.HandleException();
                // Do something specific to DirectoryNotFoundException
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("Caught a file not found exception!");
                ex.HandleException();
                // Do something specific to FileNotFoundException
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught a general exception!");
                ex.HandleException();
                // Do something for all other exceptions
            }
        }

        // Compare using block to try/finally
        private static void CompareToUsing()
        {
            // Recall our lesson on IDisposable
            // The "using" block structure is a special case of try/finally

            // So this code...
            using (var fred = new DisposableClass())
            {
                fred.Name = "Fred Sanford";
            }

            // ... is functionally identical to this code
            DisposableClass lamont = null;
            try
            {
                lamont = new DisposableClass {Name = "Lamont Sanford"};
            }
            finally
            {
                lamont?.Dispose();
            }
            // Note that we had to declare the variable before the "try" block to allow its scope to include the "finally" block
        }

        // Example exception throwing method
        private static void PossibleException()
        {
            #pragma warning disable S2245 // Cryptographic strength is not a consideration in this lesson
            var rnd = new Random((int)DateTime.Now.Ticks);
            #pragma warning restore S2245
            int val = rnd.Next(1, 100);

            if (val % 2 == 0) throw new TrainingException($"Oh no! [{val}] is an even number!");
            if (val > 50) throw new TrainingException($"Oh no! [{val}] is over fifty!");

            Console.WriteLine($"Goody! [{val}] is an odd number under fifty...");
        }

        // Demonstrate some ways you can cause arithmetic exceptions
        private static void ArithmeticExceptions()
        {
            Console.WriteLine("integer overflow in an unchecked context...");
            IntegerOverflowUnchecked();
            GenericFunctions.Pause();

            Console.WriteLine("integer overflow in a checked context...");
            IntegerOverflowChecked();
            GenericFunctions.Pause();

            Console.WriteLine("float overflow in an unchecked context...");
            FloatOverflowUnchecked();
            GenericFunctions.Pause();

            Console.WriteLine("dividing by zero...");
            DivideByZero();
        }
        #endregion

        #region Helper Functions
        // Initialize global variables
        private static void Initialize()
        {
            /*
             * You can rely on the default "bubble-up" behavior of exceptions and catch them at the top level,
             * ... or you can explicitly throw an exception to provide context for where it occurred
             */

            try
            {
                // Be sure to set the logger first so that you can log any error later in the method
                if (logger == null)
                {
                    log4net.Config.XmlConfigurator.Configure();
                    logger = LogManager.GetLogger(LogType);
                }

                Logging.Logger = logger;

                // Initialize the item form the configuration section
                settings = (ProgramSettings)ConfigurationManager.GetSection(ProgramSettings.SectionName);
            }
            catch (Exception ex)
            {
                throw new TrainingException("Error initializing global variables!", ex);
            }
        }

        // Provide timestamp for logging
        private static string TimeStamp(DateTime? date = null)
        {
            try
            {
                if (date == null) date = DateTime.Now;
                return ((DateTime)date).ToString("MM-dd-yyyy HH:mm:ss");
            }
            catch (Exception ex)
            {
                throw new TrainingException("Error creating time-stamp!", ex);
            }
        }

        // Demonstrate integer overflow in an unchecked context
        private static void IntegerOverflowUnchecked()
        {
            // This method will not throw an exception; it will just overflow the integer value
            try
            {
                int a = 1000000000;
                int b = 1000000000;
                int c = a * b;
                Console.WriteLine($"{a} * {b} = {c}");
            }
            catch (Exception ex)
            {
                ex.HandleException();
            }
        }

        // Demonstrate integer overflow in a checked context
        private static void IntegerOverflowChecked()
        {
            // Because of the "checked" context, this method will throw an exception
            checked
            {
                try
                {
                    int a = 1000000000;
                    int b = 1000000000;
                    int c = a * b;
                    Console.WriteLine($"{a} * {b} = {c}");
                }
                catch (Exception ex)
                {
                    ex.HandleException();
                }
            }
        }

        // Demonstrate float overflow in an unchecked context
        private static void FloatOverflowUnchecked()
        {
            try
            {
                float a = 1e30f;
                float b = 1e30f;
                float c = a * b;
                Console.WriteLine($"{a} * {b} = {c}");
            }
            catch (Exception ex)
            {
                ex.HandleException();
            }
        }
        
        // Demonstrate the effect of dividing by zero
        private static void DivideByZero()
        {
            try
            {
                float a = 0f;
                float b = 0f;
                float c = a / b;
                Console.WriteLine($"{a} / {b} = {c}");
            }
            catch (Exception ex)
            {
                ex.HandleException();
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
