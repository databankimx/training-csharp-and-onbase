---
title: "Event Log and Stopwatch Profiling"
chapter: 11
index: 3
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        // --- Windows Event Log ---
        // Check elevation first so the messaging is specific.
        // Creating a new event source requires admin -- the source name must be
        // registered in the registry before any write can succeed.
        // Writing to an already-registered source does NOT need admin.
        bool isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                           .IsInRole(WindowsBuiltInRole.Administrator);

        const string source = "CSharp.Ch11.InputValidationDebuggingAndInstrumentation";
        const string log    = "Application";

        if (!isAdmin)
        {
            Console.WriteLine("Event log demo: not running as administrator.");
            Console.WriteLine($"  Source \"{source}\" may not be registered yet.");
            Console.WriteLine("  To run this section fully:");
            Console.WriteLine("  1. Close Visual Studio");
            Console.WriteLine("  2. Right-click Visual Studio -> Run as administrator");
            Console.WriteLine("  3. Run this step again -- source registration succeeds,");
            Console.WriteLine("     and subsequent non-admin runs can write to it.");
            Console.WriteLine("  Attempting the write anyway in case the source already exists...");
            Console.WriteLine();
        }

        try
        {
            // SourceExists() searches the Security log -- requires admin.
            // Skip it and attempt WriteEntry() directly. If the source isn't
            // registered yet this throws ArgumentException with a clear message.
            // If it IS registered (from a prior admin run), this succeeds without
            // needing admin at all -- which is the normal production pattern.
            EventLog.WriteEntry(source, "CSharp.Ch11 lesson ran successfully.", EventLogEntryType.Information);
            Console.WriteLine($"Wrote to the \"{log}\" event log under source \"{source}\".");
            Console.WriteLine("Open eventvwr.msc -> Windows Logs -> Application to see it.");
        }
        catch (ArgumentException)
        {
            // Source not registered -- needs admin to create it.
            Console.WriteLine("Source not yet registered. Register it once by running as administrator:");
            Console.WriteLine($"  EventLog.CreateEventSource(\"{source}\", \"{log}\");");
            Console.WriteLine("After that, WriteEntry() works from a non-admin process.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Event log write failed: {ex.Message}");
        }

        // --- Stopwatch profiling: string concatenation vs StringBuilder ---
        const int iterations = 100_000;

        var sw = Stopwatch.StartNew();
        string concatenated = "";
        for (int i = 0; i < iterations; i++)
            concatenated += "x";
        sw.Stop();
        Console.WriteLine($"\nString concatenation ({iterations:N0} iterations): {sw.ElapsedMilliseconds} ms  " +
                          $"({concatenated.Length:N0} chars)");

        sw.Restart();
        var builder = new StringBuilder();
        for (int i = 0; i < iterations; i++)
            builder.Append("x");
        sw.Stop();
        Console.WriteLine($"StringBuilder.Append()  ({iterations:N0} iterations): {sw.ElapsedMilliseconds} ms  " +
                          $"({builder.Length:N0} chars)");

        Console.WriteLine("\nSee Supplemental.04 for JIT warm-up, GC memory measurement,");
        Console.WriteLine("custom PerformanceCounters, and when to use a real profiler.");
    }
}
```
