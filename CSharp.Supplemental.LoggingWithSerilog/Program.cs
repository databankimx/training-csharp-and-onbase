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

using CSharp.Supplemental.LoggingWithSerilog.HelperClasses;
using CSharp.Supplemental.LoggingWithSerilog.Models;
using System.Diagnostics;

namespace CSharp.Supplemental.LoggingWithSerilog;

internal static class Program
{
    #region Private Members
    // Array to populate prime sieve
    private static bool[]? isPrimeArray = null;

    // Maximum number to check
    private const int Max = 20;
    #endregion

    #region Main
    private static void Main()
    {
        try
        {
            // We can call the static functions from the `Logging` class
            Logging.Info("Program Starting...");

            // Or we can use the `Logging` class as an extension of `string`
            string message = "Sample log entry";
            message.Trace();
            message.Info();
            message.Warn();
            message.Error();
            message.FatalError();

            Logging.Info("Checking for primes...");

            for (int i = 0; i <= Max; i++)
            {
                if (IsPrime(i))
                {
                    // We can pass a formatted string with arguments
                    Logging.Info("{i} is prime!", i);
                }
                else
                {
                    Logging.Info("{i} is not prime...", i);
                }
            }

            // Test the `HandleException` method
            try
            {
                throw new DatabankException("Sample error for log testing!");
            }
            catch (Exception ex)
            {
                ex.HandleException();
            }
        }
        catch (Exception ex)
        {
            ex.HandleException();
        }
        finally
        {
            Logging.Info("Program Exiting...");
            // Unlike log4net and Databank.Logging (both anchored to the assembly's own
            // BaseDirectory), this hand-rolled setup explicitly reads appsettings.json via
            // SetBasePath(Directory.GetCurrentDirectory()) above, and Serilog's file sink
            // resolves its own relative "logs/..." paths against that same current directory -
            // not BaseDirectory. Anchoring here to match.
            string baseDir = Directory.GetCurrentDirectory();
            string today = DateTime.Now.ToString("yyyyMMdd");
            OpenLogFile(Path.Combine(baseDir, "logs", $"debug-log-{today}.txt"));
            OpenLogFile(Path.Combine(baseDir, "logs", $"error-log-{today}.txt"));
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
    #endregion

    #region Helper Functions
    // Open a log file in whatever application Windows has associated with its extension.
    // UseShellExecute = true is what makes Process.Start resolve the file association rather
    // than trying to execute the path directly. If that fails with "no application associated
    // with this file type" (Win32Exception), fall back to launching Notepad explicitly, since
    // it ships with every Windows install and can always be invoked directly regardless of
    // file associations.
    private static void OpenLogFile(string path)
    {
        if (!File.Exists(path)) return;
        try
        {
            Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            try
            {
                Process.Start("notepad.exe", $"\"{path}\"");
            }
            catch (Exception ex)
            {
                Logging.Warn("Could not open log file [{0}] in Notepad either: {1}", path, ex.Message);
            }
        }
        catch (Exception ex)
        {
            Logging.Warn("Could not open log file [{0}]: {1}", path, ex.Message);
        }
    }

    // Check if number is prime
    private static bool IsPrime(int number)
    {
        if (isPrimeArray == null) Sieve(Max);
        return isPrimeArray != null && isPrimeArray[number];
    }

    // Sieve of Eratosthenes
    private static void Sieve(int max)
    {
        Logging.Info("Sieving primes up to {0}...", max);
        isPrimeArray = Enumerable.Repeat(true, max + 1).ToArray();
        isPrimeArray[0] = isPrimeArray[1] = false;
        int p = 2;
        while (p * p < max)
        {
            if (isPrimeArray[p])
            {
                Logging.Trace("Prime found: {0}", p);
                for (int i = p * p; i <= max; i += p)
                {
                    isPrimeArray[i] = false;
                }
            }
            p++;
        }
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
