---
title: "Trace Listeners - File, Console, Custom, Multiple, Indentation, TraceSwitch"
chapter: 11
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

// Custom TraceListener: timestamps every line.
// The _atLineStart flag ensures one timestamp per logical line, not one per Write() call --
// Trace's machinery sometimes calls Write() several times before the final WriteLine().
public class TimestampedTraceListener : TraceListener
{
    private bool _atLineStart = true;

    public override void Write(string message)
    {
        if (_atLineStart)
        {
            Console.Write($"[{DateTime.Now:HH:mm:ss.fff}] ");
            _atLineStart = false;
        }
        Console.Write(message);
    }

    public override void WriteLine(string message)
    {
        Write(message);
        Console.WriteLine();
        _atLineStart = true;
    }
}

internal static class Program
{
    private static void Main()
    {
        // Debug and Trace share the SAME Listeners collection.
        // Debug calls are compiled out in Release; Trace calls always compile in.
        // Clear the default DefaultTraceListener so demo output is predictable.
        Trace.Listeners.Clear();

        // Create the file explicitly up front so TextWriterTraceListener has
        // something to open. The listener opens the file lazily on first write;
        // if the path doesn't exist yet and the open fails silently, ReadAllText
        // throws FileNotFoundException. Creating it first avoids that.
        string tempPath = Path.Combine(Path.GetTempPath(), $"ch11-trace-{Guid.NewGuid():N}.log");
        File.WriteAllText(tempPath, string.Empty);

        try
        {
            // --- TextWriterTraceListener: file output ---
            // Use a StreamWriter with AutoFlush so nothing sits in a buffer.
            // Dispose the listener before reading the file back -- it holds the
            // file open (via its internal StreamWriter) until disposed.
            // File.ReadAllText() on an open file throws IOException.
            var fileWriter   = new StreamWriter(tempPath, append: false) { AutoFlush = true };
            var fileListener = new TextWriterTraceListener(fileWriter);
            Trace.Listeners.Add(fileListener);
            Trace.WriteLine("This line was written via Trace, routed to a real file on disk.");
            Trace.Listeners.Remove(fileListener);
            fileListener.Dispose(); // closes the underlying StreamWriter and file handle

            Console.WriteLine("TextWriterTraceListener -- file contents:");
            Console.WriteLine(File.ReadAllText(tempPath));

            // --- Custom TraceListener ---
            var custom = new TimestampedTraceListener();
            Trace.Listeners.Add(custom);
            Trace.WriteLine("Custom TimestampedTraceListener: each line prefixed with HH:mm:ss.fff.");
            Trace.Listeners.Remove(custom);

            // --- Multiple listeners: one call, every listener receives it ---
            var console      = new ConsoleTraceListener();
            var fileWriter2  = new StreamWriter(tempPath, append: true) { AutoFlush = true };
            var fileListener2 = new TextWriterTraceListener(fileWriter2);
            Trace.Listeners.Add(console);
            Trace.Listeners.Add(fileListener2);
            Trace.WriteLine("Multiple listeners: this line went to console AND the file simultaneously.");
            Trace.Listeners.Remove(console);
            Trace.Listeners.Remove(fileListener2);
            fileListener2.Dispose();
            Console.WriteLine("(That line above printed via ConsoleTraceListener and was also written to the file.)");

            // --- Indentation ---
            var indent = new ConsoleTraceListener();
            Trace.Listeners.Add(indent);
            Console.WriteLine("\nIndentation:");
            Trace.WriteLine("Outer operation...");
            Trace.Indent();
            Trace.WriteLine("Inner step 1...");
            Trace.Indent();
            Trace.WriteLine("Deeply nested step.");
            Trace.Unindent();
            Trace.WriteLine("Inner step 2...");
            Trace.Unindent();
            Trace.WriteLine("Outer operation complete.");
            Trace.Listeners.Remove(indent);

            // --- TraceSwitch: filter by severity ---
            // Normally set in App.config so verbosity can change without recompile.
            // TraceLevel.Warning -> Error and Warning print; Info and Verbose do not.
            var switchListener = new ConsoleTraceListener();
            var tsw            = new TraceSwitch("Demo", "Demo switch") { Level = TraceLevel.Warning };
            Trace.Listeners.Add(switchListener);
            Console.WriteLine($"\nTraceSwitch level: {tsw.Level}");
            Trace.WriteLineIf(tsw.TraceError,   "ERROR:   prints (Error <= Warning).");
            Trace.WriteLineIf(tsw.TraceWarning, "WARNING: prints (Warning <= Warning).");
            Trace.WriteLineIf(tsw.TraceInfo,    "INFO:    does NOT print (Info > Warning).");
            Trace.WriteLineIf(tsw.TraceVerbose, "VERBOSE: does NOT print (Verbose > Warning).");
            Trace.Listeners.Remove(switchListener);
            Console.WriteLine("Only ERROR and WARNING printed. Change Level to see more.");
        }
        finally
        {
            Trace.Listeners.Clear();
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }
}
```
