# Chapter 11 Supplemental 03: Trace Listeners

## What This Is

`Trace.WriteLine()`/`Debug.WriteLine()` don't write anywhere by default in a console app, both write through the same `Trace.Listeners` collection (Debug and Trace genuinely share it, `Debug`'s calls are just compiled out of Release builds entirely). This Supplemental covers what listeners actually are: the built-in ones, writing your own, using several at once, and filtering output by severity with `TraceSwitch`.

---

## `TextWriterTraceListener`: Routing Output to a Real File

```csharp
var fileListener = new TextWriterTraceListener(logPath);
Trace.Listeners.Add(fileListener);
Trace.WriteLine("...");
Trace.Flush();
```

`Trace.Flush()` matters here specifically: `TextWriterTraceListener` buffers internally, worth calling explicitly before reading a log file back in the same run that just wrote to it, otherwise the read might happen before the buffered write actually reaches disk.

Worth going a step further, too: `TextWriterTraceListener` holds the file open (via its own internal `StreamWriter`) until it's removed from `Trace.Listeners` *and* disposed. Trying to `File.ReadAllText()` the same path while the listener is still holding it open throws `IOException` ("The process cannot access the file... because it is being used by another process"), the exact same class of bug already documented in `CSharp.Ch09.TextbookCode.Serialization`'s lecture notes, a stream left open colliding with something else trying to touch the same file. The fix here is ordering: `Remove()` and `Dispose()` the listener *before* reading the file back, not after.

---

## Writing a Custom `TraceListener`

```csharp
public class TimestampedTraceListener : TraceListener
{
    public override void Write(string message) { ... }
    public override void WriteLine(string message) { ... }
}
```

`TraceListener` is the base class every built-in listener (`ConsoleTraceListener`, `TextWriterTraceListener`, `EventLogTraceListener`) derives from too. Only two methods are strictly required to build your own: `Write()` (no trailing newline) and `WriteLine()` (with one). `TimestampedTraceListener` (see that file directly) prefixes every line with the current time before handing it to the console, worth noticing it tracks whether it's at the start of a new line internally, since `Trace`'s own machinery sometimes calls `Write()` more than once before a final `WriteLine()` for what's logically one line, and the timestamp should only be added once per line, not once per `Write()` call.

---

## Multiple Listeners at Once

```csharp
Trace.Listeners.Add(consoleListener);
Trace.Listeners.Add(fileListener);
Trace.WriteLine("...");   // reaches BOTH listeners from this one call
```

`Trace.Listeners` is a genuine collection, not a single slot. One `Trace.WriteLine()` call fans out to *every* registered listener, worth using deliberately: a real application might want output on the console during development AND simultaneously logged to a file, both from the exact same trace calls, no code duplicated for each destination.

---

## `Trace.Indent()`/`Trace.Unindent()`: Hierarchical Output

```csharp
Trace.WriteLine("Starting outer operation...");
Trace.Indent();
Trace.WriteLine("Starting inner step...");
Trace.Unindent();
```

Every `TraceListener` respects an internal indent level, `Indent()` increases it, `Unindent()` decreases it, and each line gets prefixed with that many indent units automatically. Worth reaching for in anything with genuine nested structure, a recursive operation, nested method calls, makes trace output dramatically easier to actually read back later, the indentation visually mirrors the call structure that produced it.

---

## `TraceSwitch`: Filtering by Severity

```csharp
var mySwitch = new TraceSwitch("DemoSwitch", "Demonstration switch") { Level = TraceLevel.Warning };

Trace.WriteLineIf(mySwitch.TraceError, "...");     // prints, Error <= Warning
Trace.WriteLineIf(mySwitch.TraceWarning, "...");   // prints, Warning <= Warning
Trace.WriteLineIf(mySwitch.TraceInfo, "...");      // does NOT print, Info > Warning
Trace.WriteLineIf(mySwitch.TraceVerbose, "...");   // does NOT print, Verbose > Warning
```

`TraceLevel` has four severities, in order: `Error`, `Warning`, `Info`, `Verbose`. Setting `mySwitch.Level = TraceLevel.Warning` means the switch's `TraceError`/`TraceWarning` properties evaluate `true` (those severities are at or above the configured level), while `TraceInfo`/`TraceVerbose` evaluate `false`. `Trace.WriteLineIf(condition, message)` only actually writes when `condition` is `true`, combined, this gives you severity-filtered logging with a single line per call site. This demo set `Level` directly in code, but a real application typically configures it through `App.config`'s `<system.diagnostics>` section instead, specifically so the verbosity can be turned up temporarily in a production environment, to investigate something, without recompiling or redeploying anything.
