# Chapter 11 Supplemental 03: Trace Listeners

## What This Is

`Trace.WriteLine()` and `Debug.WriteLine()` don't write anywhere by default in a console app. They write to whatever's registered in `Trace.Listeners` -- which, out of the box, is nothing visible. This project covers what those listeners actually are: the built-in ones (file, console, event log), writing your own, using several at once, and `TraceSwitch` for filtering by severity level.

`Debug` and `Trace` share the same `Listeners` collection. They're really two names for the same underlying mechanism, with one key difference: `Debug` calls are compiled out entirely in Release builds; `Trace` calls always compile in.

---

## How to Write This Program

### Step 1: TimestampedTraceListener

Create `TimestampedTraceListener.cs`. Only two methods are required to build a custom `TraceListener`: `Write()` and `WriteLine()`:

```csharp
public class TimestampedTraceListener : TraceListener
{
    // Tracks whether the next Write() call is starting a new line,
    // so the timestamp prefix is added once per line, not once per Write() call.
    private bool atLineStart = true;

    public override void Write(string message)
    {
        if (atLineStart)
        {
            Console.Write($"[{DateTime.Now:HH:mm:ss.fff}] ");
            atLineStart = false;
        }
        Console.Write(message);
    }

    public override void WriteLine(string message)
    {
        Write(message);
        Console.WriteLine();
        atLineStart = true;
    }
}
```

The `atLineStart` flag handles the detail that `Trace`'s machinery sometimes calls `Write()` multiple times before the final `WriteLine()` for one logical line -- you want one timestamp per line, not one per `Write()` call.

Also add the temp file path and `finally` cleanup to `Main()`:

```csharp
string tempLogPath = Path.Combine(Path.GetTempPath(), $"ch11-trace-demo-{Guid.NewGuid():N}.log");
// ... (in finally:)
Trace.Listeners.Clear();
if (File.Exists(tempLogPath)) File.Delete(tempLogPath);
```

### Mini-Program 1: TextWriterTraceListener (Writing to a File)

Clear `Main()` and write:

```csharp
var fileListener = new TextWriterTraceListener(tempLogPath);
Trace.Listeners.Add(fileListener);

Trace.WriteLine("This line was written via Trace, routed to a real file on disk.");

// TextWriterTraceListener buffers internally. Flush() forces the buffer out.
// Call it explicitly before reading the file back in the same run that wrote it.
Trace.Flush();

// Remove AND Dispose the listener BEFORE reading the file back.
// TextWriterTraceListener holds the file open until disposed. File.ReadAllText()
// on an open file throws IOException.
Trace.Listeners.Remove(fileListener);
fileListener.Dispose();

Console.WriteLine($"Wrote to: {tempLogPath}");
Console.WriteLine("Contents:");
Console.WriteLine(File.ReadAllText(tempLogPath));

GenericFunctions.Pause();
```

Run it. The file contains the trace line.

The `Flush()` / `Remove()` / `Dispose()` sequence is the right pattern when you need to read the file back in the same process run that wrote to it. `Flush()` ensures nothing is still in the buffer; `Dispose()` releases the file handle so the read can open it.

In a real application you'd typically flush and dispose at shutdown, not mid-run.

### Mini-Program 2: Custom TraceListener

Clear `Main()` and write:

```csharp
var customListener = new TimestampedTraceListener();
Trace.Listeners.Add(customListener);

Trace.WriteLine("This line went through a custom TraceListener, prefixed with a timestamp.");

Trace.Listeners.Remove(customListener);

GenericFunctions.Pause();
```

Run it. The output includes a timestamp prefix. The `TimestampedTraceListener` does nothing more exotic than format each line before writing it to the console -- but that's the point. A custom listener can route output anywhere: a database, a REST endpoint, a cloud logging service. Wherever you can write to, a `TraceListener` can route `Trace.WriteLine()` to it.

### Mini-Program 3: Multiple Listeners at Once

Clear `Main()` and write:

```csharp
var consoleListener = new ConsoleTraceListener();
var fileListener    = new TextWriterTraceListener(tempLogPath, "SecondPass");
Trace.Listeners.Add(consoleListener);
Trace.Listeners.Add(fileListener);

// ONE call reaches BOTH listeners.
Trace.WriteLine("This single Trace.WriteLine() call reached two listeners at once.");
Trace.Flush();

Trace.Listeners.Remove(consoleListener);
Trace.Listeners.Remove(fileListener);
fileListener.Dispose();

Console.WriteLine("\n(The line above printed to this console via ConsoleTraceListener, and was");
Console.WriteLine("also appended to the log file via TextWriterTraceListener -- from one call.)");

GenericFunctions.Pause();
```

Run it. The line appears on the console and in the file.

`Trace.Listeners` is a collection. One `Trace.WriteLine()` call delivers to every registered listener in sequence. This is how you get simultaneous console output, file logging, and event log writes from a single instrumentation call, without changing any of the code that calls `Trace`.

### Mini-Program 4: Indentation

Clear `Main()` and write:

```csharp
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

Console.WriteLine("\nNote the increasing indentation -- useful for nested or recursive operations.");
Console.WriteLine("Makes trace output far easier to read back later.");

GenericFunctions.Pause();
```

Run it. The hierarchical structure is immediately readable in the output.

`Trace.Indent()` and `Trace.Unindent()` adjust `Trace.IndentLevel`. Every listener applies the current indent level to each line. For anything with a genuine nested structure -- a recursive algorithm, a multi-stage pipeline -- this turns an otherwise flat wall of log lines into something you can actually follow.

### Mini-Program 5: TraceSwitch

Clear `Main()` and write:

```csharp
var consoleListener = new ConsoleTraceListener();
Trace.Listeners.Add(consoleListener);

// Normally configured via App.config's <system.diagnostics> section,
// so the level can change without recompiling.
// Set directly here for a self-contained demo.
// TraceLevel.Warning means: show Error and Warning, but NOT Info or Verbose.
var mySwitch = new TraceSwitch("DemoSwitch", "Demonstration switch")
{
    Level = TraceLevel.Warning
};

Console.WriteLine($"Switch level: {mySwitch.Level}");
Trace.WriteLineIf(mySwitch.TraceError,   "ERROR-level message (prints -- Error <= Warning).");
Trace.WriteLineIf(mySwitch.TraceWarning, "WARNING-level message (prints -- Warning <= Warning).");
Trace.WriteLineIf(mySwitch.TraceInfo,    "INFO-level message (does NOT print -- Info > Warning).");
Trace.WriteLineIf(mySwitch.TraceVerbose, "VERBOSE-level message (does NOT print -- Verbose > Warning).");

Trace.Listeners.Remove(consoleListener);

Console.WriteLine("\nOnly two of the four lines printed. The switch's Level controlled that entirely.");
Console.WriteLine("In a real application, configure the level in App.config so verbosity can be");
Console.WriteLine("turned up in production temporarily -- without recompiling or redeploying.");

GenericFunctions.Pause();
```

Run it. Error and Warning print; Info and Verbose don't.

`TraceSwitch.Level` is a threshold: messages at or below the level print; messages above it don't. `TraceLevel.Warning` means "show me anything this serious or more severe." The levels in order: `Off`, `Error`, `Warning`, `Info`, `Verbose`.

The practical value is configurability. In App.config:

```xml
<system.diagnostics>
  <switches>
    <add name="DemoSwitch" value="2" />  <!-- 2 = Warning -->
  </switches>
</system.diagnostics>
```

Change `2` to `4` (Verbose), recycle the app, and you get full diagnostic output without touching the source code or redeploying.

---

## Takeaways

- `Trace.Listeners` is a collection. `Trace.WriteLine()` delivers to every registered listener.
- `TextWriterTraceListener` writes to a file. `ConsoleTraceListener` writes to the console. Both are built-in.
- `Flush()` before reading a file written by a `TextWriterTraceListener`. `Dispose()` before reading the same file in the same process.
- A custom `TraceListener` requires only `Write()` and `WriteLine()`. Route output anywhere from there.
- `Trace.Indent()` / `Trace.Unindent()` make nested trace output readable.
- `TraceSwitch` filters by severity. Configure the level in App.config so it can change without a recompile.
- `Debug` and `Trace` share `Listeners`. `Debug` calls are compiled out in Release; `Trace` calls always compile in.
