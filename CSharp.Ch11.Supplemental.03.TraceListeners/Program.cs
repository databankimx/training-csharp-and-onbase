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
using System.Diagnostics;
using System.IO;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

#pragma warning disable S6670
namespace CSharp.Ch11.Supplemental._03.TraceListeners
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Trace.WriteLine()/Debug.WriteLine() don't write ANYWHERE by default in a console
         *   app, they write to whatever's registered in Trace.Listeners (Debug and Trace
         *   share the SAME Listeners collection, they're really two names for the same
         *   underlying mechanism, Debug's calls just get compiled out entirely in Release
         *   builds). This Supplemental covers what those listeners actually are: the
         *   built-in ones (console, a text file, the Windows Event Log), writing your OWN,
         *   using SEVERAL at once, and TraceSwitch for filtering by severity level.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            string tempLogPath = Path.Combine(Path.GetTempPath(), $"ch11-trace-demo-{Guid.NewGuid():N}.log");

            try
            {
                #region Chapter Lessons
                UsingTextWriterTraceListener(tempLogPath);
                GenericFunctions.Pause();

                UsingACustomTraceListener();
                GenericFunctions.Pause();

                UsingMultipleListenersAtOnce(tempLogPath);
                GenericFunctions.Pause();

                UsingTraceIndentation();
                GenericFunctions.Pause();

                UsingTraceSwitch();
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
                Trace.Listeners.Clear();
                if (File.Exists(tempLogPath)) File.Delete(tempLogPath);
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // TextWriterTraceListener: routes Trace output to a real file
        private static void UsingTextWriterTraceListener(string logPath)
        {
            var fileListener = new TextWriterTraceListener(logPath);
            Trace.Listeners.Add(fileListener);

            Trace.WriteLine("This line was written via Trace, routed to a real file on disk.");

            // TextWriterTraceListener buffers internally, Flush() forces anything buffered
            //   out to the file immediately, worth calling explicitly before reading a log
            //   file back in the same run that just wrote to it.
            Trace.Flush();

            // Remove AND Dispose the listener BEFORE reading the file back: TextWriterTraceListener
            //   holds the file open (via its own internal StreamWriter) until disposed, trying to
            //   File.ReadAllText() the same path while it's still open throws IOException ("The
            //   process cannot access the file... because it is being used by another process").
            Trace.Listeners.Remove(fileListener);
            fileListener.Dispose();

            Console.WriteLine($"Wrote a line to: {logPath}");
            Console.WriteLine("Contents read back from that file:");
            Console.WriteLine(File.ReadAllText(logPath));
        }

        // A hand-written TraceListener (see TimestampedTraceListener.cs)
        private static void UsingACustomTraceListener()
        {
            var customListener = new TimestampedTraceListener();
            Trace.Listeners.Add(customListener);

            Trace.WriteLine("This line went through a custom TraceListener, prefixed with a timestamp.");

            Trace.Listeners.Remove(customListener);
        }

        // Multiple listeners at once: Trace.WriteLine() writes to EVERY registered listener
        private static void UsingMultipleListenersAtOnce(string logPath)
        {
            var consoleListener = new ConsoleTraceListener();
            var fileListener = new TextWriterTraceListener(logPath, "SecondPass");
            Trace.Listeners.Add(consoleListener);
            Trace.Listeners.Add(fileListener);

            // ONE call, but it reaches BOTH listeners, this line shows up on the console
            //   (via consoleListener) AND gets appended to the log file (via fileListener)
            Trace.WriteLine("This single Trace.WriteLine() call reached two listeners at once.");
            Trace.Flush();

            Trace.Listeners.Remove(consoleListener);
            Trace.Listeners.Remove(fileListener);
            fileListener.Dispose();

            Console.WriteLine($"{Environment.NewLine}(The line above printed to THIS console via ConsoleTraceListener, and was ALSO");
            Console.WriteLine("appended to the log file via TextWriterTraceListener, from that one call.)");
        }

        // Trace.Indent()/Unindent(): hierarchical output for nested operations
        private static void UsingTraceIndentation()
        {
            var consoleListener = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleListener);

            Trace.WriteLine("Starting outer operation...");
            Trace.Indent();
            Trace.WriteLine("Starting inner step 1...");
            Trace.WriteLine("Inner step 1 complete.");
            Trace.Indent();
            Trace.WriteLine("Starting deeply nested step...");
            Trace.WriteLine("Deeply nested step complete.");
            Trace.Unindent();
            Trace.WriteLine("Starting inner step 2...");
            Trace.WriteLine("Inner step 2 complete.");
            Trace.Unindent();
            Trace.WriteLine("Outer operation complete.");

            Trace.Listeners.Remove(consoleListener);

            Console.WriteLine($"{Environment.NewLine}Notice the increasing indentation above, worth using for anything with a real");
            Console.WriteLine("nested/hierarchical structure (a recursive operation, nested method calls), makes");
            Console.WriteLine("the resulting trace output far easier to actually read back later.");
        }

        // TraceSwitch: filtering trace output by severity level
        private static void UsingTraceSwitch()
        {
            var consoleListener = new ConsoleTraceListener();
            Trace.Listeners.Add(consoleListener);

            // Normally configured via App.config's <system.diagnostics> section (so the
            //   severity level can change without recompiling), set directly here instead
            //   for a self-contained demo. TraceLevel.Warning means: show Error and
            //   Warning, but NOT Info or Verbose.
            var mySwitch = new TraceSwitch("DemoSwitch", "Demonstration switch") { Level = TraceLevel.Warning };

            Console.WriteLine($"TraceSwitch level set to: {mySwitch.Level}");

            #pragma warning disable S6675
            Trace.WriteLineIf(mySwitch.TraceError, "This ERROR-level message prints (Error <= Warning).");
            Trace.WriteLineIf(mySwitch.TraceWarning, "This WARNING-level message prints (Warning <= Warning).");
            Trace.WriteLineIf(mySwitch.TraceInfo, "This INFO-level message does NOT print (Info > Warning).");
            Trace.WriteLineIf(mySwitch.TraceVerbose, "This VERBOSE-level message does NOT print (Verbose > Warning).");
            #pragma warning restore S6675

            Trace.Listeners.Remove(consoleListener);

            Console.WriteLine($"{Environment.NewLine}Only two of the four WriteLineIf() calls above actually printed anything, the");
            Console.WriteLine("switch's Level property controls that entirely, worth setting via App.config in a");
            Console.WriteLine("real application specifically so the verbosity can be turned up in production,");
            Console.WriteLine("temporarily, to investigate something, without recompiling or redeploying.");
        }
        #endregion
    }
}
#pragma warning restore S6670

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
