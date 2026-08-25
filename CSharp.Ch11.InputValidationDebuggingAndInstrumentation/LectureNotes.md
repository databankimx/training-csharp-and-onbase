# Chapter 11: Input Validation, Debugging, and Instrumentation

## What This Is

Three related but genuinely distinct concerns: making sure data coming into a program is trustworthy (Input Validation), understanding what a program is doing while you're actively developing it (Debugging), and understanding what a program did after the fact, often in production, with no debugger anywhere nearby (Instrumenting Applications).

---

## Validation Has Two Different Jobs

```csharp
bool isValid = int.TryParse(candidate, out int result);          // syntax check
bool isReasonable = age >= 0 && age <= 120;                       // sanity check
```

Worth separating these clearly, because they call for different responses. A **syntax** failure (text that doesn't parse as the expected type, or doesn't match a required pattern) means the input is genuinely unusable, block it outright. A **sanity check** failure (a value that's syntactically fine but statistically unusual, an age of 150, a unit price of $0.01) is a different kind of signal, often worth a confirmation prompt ("are you sure?") rather than an outright block, since the value might be perfectly correct, just rare. `CSharp.Ch11.TextbookCode.Ch11RealWorldScenario01` shows this exact two-tier pattern in a full, interactive form: hard validation blocks the OK button entirely, sanity-check failures instead show a "some values look unusual, continue anyway?" dialog.

---

## `IsNullOrEmpty()` vs. `IsNullOrWhiteSpace()`

```csharp
string.IsNullOrEmpty("   ");        // false, "   " is not literally empty
string.IsNullOrWhiteSpace("   ");   // true, correctly catches whitespace-only input
```

A genuinely common, easy-to-miss gotcha: a text field containing only spaces passes `IsNullOrEmpty()` (it has a non-zero length) but should almost always be treated as blank by a real application. `IsNullOrWhiteSpace()` is the safer default for validating actual user input; reach for `IsNullOrEmpty()` specifically when whitespace genuinely is meaningful content for the field in question.

---

## `Debug.Assert()`: For Bugs, Not Bad Input

```csharp
Debug.Assert(total == quantity * unitPrice, "Total calculation is inconsistent!");
```

An assertion states something that should **always** be true if the code is correct, it's a tool for catching your own bugs during development, not for validating anything that came from a user (use the validation techniques above for that). Two things worth knowing: `Debug.Assert()` is entirely compiled out of Release builds (see the preprocessor section below for the mechanism), so it costs nothing in production, and a *failing* assertion shows an interactive "Assert Failed" dialog by default in a Debug build, worth remembering before adding one anywhere that might run unattended (a scheduled task, a background service), it will hang waiting for someone to click a button that's never coming.

---

## Preprocessor Directives Decide What Gets Compiled, Not What Runs

```csharp
#if DEBUG
    Console.WriteLine("Compiled in for Debug builds only.");
#else
    Console.WriteLine("Compiled in for everything else.");
#endif
```

This is fundamentally different from an ordinary `if` statement: an ordinary `if` is evaluated *while the program runs*; `#if`/`#endif` are evaluated *while the program is compiled*, the losing branch isn't just skipped, it's never even turned into IL at all. This is exactly the mechanism that makes `Debug.Assert()`/`Debug.WriteLine()` free in Release builds, they're wrapped in `[Conditional("DEBUG")]` internally, which works the same way. See `CSharp.Ch11.Supplemental.02.PreprocessorDirectivesDeepDive` for `#warning`/`#error`/`#pragma warning` and the rest.

---

## `Debug` vs. `Trace`: A Real, Practical Difference

```csharp
Debug.WriteLine("...");   // compiled OUT of Release builds entirely
Trace.WriteLine("...");   // always compiled in, Debug and Release both
```

Both write through the same `Listeners` mechanism (nothing shows up anywhere by default in a console app, until a listener like `ConsoleTraceListener` is added), but `Debug.WriteLine()` disappears completely outside a Debug build while `Trace.WriteLine()` doesn't. This makes `Trace` the right choice for anything that needs to keep working in a shipped, Release-configured application (which is most real logging), and `Debug` the right choice for output that's genuinely only useful while actively developing. See `CSharp.Ch11.Supplemental.03.TraceListeners` for building custom listeners and routing output to files, the event log, and beyond.

---

## Logging to the Windows Event Log

```csharp
if (!EventLog.SourceExists(source)) EventLog.CreateEventSource(source, log);
EventLog.WriteEntry(source, message, EventLogEntryType.Information);
```

Worth knowing the permission boundary here specifically: creating a brand-new event *source* the first time requires administrator privileges, but writing to a source that already exists does not. This lesson wraps the whole thing in a `try`/`catch` so it degrades gracefully (prints an explanation) rather than crashing outright when run without elevated permissions.

---

## Profiling by Hand: `Stopwatch`

```csharp
var stopwatch = Stopwatch.StartNew();
// ... work ...
stopwatch.Stop();
Console.WriteLine($"{stopwatch.ElapsedMilliseconds} ms");
```

The simplest, most direct way to answer "is A actually faster than B": wrap each candidate in a `Stopwatch` and compare. This lesson's specific comparison (string concatenation in a loop vs. `StringBuilder.Append()`) is a genuinely real, common performance gotcha, not just a contrived example, strings are immutable in .NET, so `str += "x"` inside a loop allocates a brand-new string on every single iteration, `StringBuilder` grows one internal buffer instead. See `CSharp.Ch11.Supplemental.04.PerformanceCountersAndProfiling` for `PerformanceCounter`, the system-level equivalent of this same idea.
