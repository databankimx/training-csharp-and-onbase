# Chapter 11: Input Validation, Debugging, and Instrumentation

## What This Is

Three related but distinct concerns packed into one chapter:

- **Input validation** -- making sure data entering your program is well-formed AND reasonable. Those are two different checks.
- **Debugging** -- preprocessor directives and `Debug`/`Trace`, for understanding what a program is doing while you're developing it.
- **Instrumentation** -- logging and profiling, for understanding what a program did or how it performed, often long after the fact, in production, where a debugger was never attached.

---

## How to Write This Program

### Mini-Program 1: Built-In TryParse

Clear `Main()` and write:

```csharp
string[] candidates = ["42", "not a number", "3.14", ""];

Console.WriteLine("int.TryParse():");
foreach (string candidate in candidates)
{
    bool isValid = int.TryParse(candidate, out int result);
    Console.WriteLine($" - \"{candidate}\" -> valid: {isValid}, value: {(isValid ? result.ToString() : "n/a")}");
}

bool priceIsValid = decimal.TryParse("19.99", out decimal price);
Console.WriteLine($"\ndecimal.TryParse(\"19.99\"): valid: {priceIsValid}, value: {price:C}");

bool dateIsValid = DateTime.TryParse("2026-08-25", CultureInfo.InvariantCulture,
    DateTimeStyles.None, out DateTime date);
Console.WriteLine($"DateTime.TryParse(\"2026-08-25\"): valid: {dateIsValid}, value: {(dateIsValid ? date.ToShortDateString() : "n/a")}");

GenericFunctions.Pause();
```

Run it. `TryParse` returns a `bool` and uses an `out` parameter for the parsed value -- no exceptions, no `try`/`catch`. This is the standard, safe way to check whether text represents a valid value of a given type.

`decimal.TryParse` and `DateTime.TryParse` follow the same pattern. The `CultureInfo.InvariantCulture` overload for `DateTime` is worth noting -- date formats vary by culture and relying on the system's current culture can produce surprising failures in international deployments.

### Mini-Program 2: String Methods

Clear `Main()` and write:

```csharp
string[] candidates = ["Hello", "", "   ", null];

Console.WriteLine("IsNullOrEmpty() vs IsNullOrWhiteSpace():");
foreach (string candidate in candidates)
{
    string display = candidate == null ? "(null)" : $"\"{candidate}\"";
    Console.WriteLine($" - {display,-12} IsNullOrEmpty: {string.IsNullOrEmpty(candidate),-6}  " +
                      $"IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(candidate)}");
}

GenericFunctions.Pause();
```

Run it. Pay attention to `"   "` (whitespace only): `IsNullOrEmpty` says it's fine -- it's not literally empty. `IsNullOrWhiteSpace` correctly flags it.

Which is "correct" depends on whether whitespace-only input is genuinely acceptable for the field in question. A name field probably isn't. A freeform notes field might be. The distinction exists precisely because the answer differs.

### Mini-Program 3: Regular Expressions

Clear `Main()` and write:

```csharp
// Matches a capitalized name: one or more words, each starting with a capital letter,
// optionally separated by hyphens, apostrophes, or spaces.
const string namePattern = @"^([A-Z][a-z]*[-' ]?)+$";

string[] candidates = ["Mary", "Mary-Jane", "O'Brien", "Van Der Berg", "mary", "Mary123", ""];

Console.WriteLine($"Pattern: {namePattern}");
foreach (string candidate in candidates)
{
    bool isMatch = Regex.IsMatch(candidate, namePattern, RegexOptions.Compiled, TimeSpan.FromSeconds(10));
    Console.WriteLine($" - \"{candidate}\" -> {isMatch}");
}

GenericFunctions.Pause();
```

Run it. Regular expressions validate that text matches an expected **shape**, not just that it parses as some type. `"Mary123"` is a perfectly valid string, and `int.TryParse` would reject it -- but so does this pattern, for a different reason: names don't contain digits.

The timeout argument (`TimeSpan.FromSeconds(10)`) is not paranoia. Certain patterns against certain inputs can cause catastrophic backtracking -- exponential time complexity that brings a server to its knees. Always pass a timeout for regex you don't fully control. `Supplemental.01.RegularExpressionsDeepDive` breaks down this exact pattern piece by piece.

### Mini-Program 4: Sanity Checks

Clear `Main()` and write:

```csharp
int[] ages = [25, -5, 150, 0];
const int minReasonableAge = 0;
const int maxReasonableAge = 120;

Console.WriteLine("Sanity-checking ages (syntactically valid integers, semantically questionable ones flagged):");
foreach (int age in ages)
{
    bool isReasonable = age >= minReasonableAge && age <= maxReasonableAge;
    Console.WriteLine($" - {age,4}: {(isReasonable ? "reasonable" : "UNUSUAL -- worth confirming with the user")}");
}

GenericFunctions.Pause();
```

Run it. `150` is a perfectly valid, parseable integer. It's still worth questioning as someone's age.

Validation has two distinct jobs:

- **Syntax failure** (text that won't parse, or doesn't match a required pattern) means the input is genuinely unusable. Block it outright.
- **Sanity check failure** (syntactically valid, but statistically unusual) is a different signal. Often worth a confirmation prompt -- "are you sure?" -- rather than an outright block, since the value might be perfectly correct, just rare.

`0` and `120` are real, possible ages. Rejecting them outright is wrong; flagging them for confirmation is reasonable. `CSharp.Ch11.TextbookCode.Ch11RealWorldScenario01` shows this two-tier pattern in a full interactive form: hard validation blocks the OK button entirely; sanity failures present a "some values look unusual, continue anyway?" dialog instead.

### Mini-Program 5: Assertions

Clear `Main()` and write:

```csharp
int quantity = 5;
decimal unitPrice = 9.99m;
decimal total = quantity * unitPrice;

// Debug.Assert() only fires in DEBUG builds -- compiled out entirely in Release.
// This assertion should ALWAYS pass. If it doesn't, that's a bug in THIS code,
// not bad user input.
Debug.Assert(total == quantity * unitPrice, "Total calculation is inconsistent!");

Console.WriteLine($"Assertion passed: total ({total:C}) matches quantity * unitPrice.");
Console.WriteLine("\nA FAILING Debug.Assert() shows an interactive dialog by default.");
Console.WriteLine("Know this before adding one to code that might run unattended --");
Console.WriteLine("it will hang waiting for someone to dismiss it.");

GenericFunctions.Pause();
```

Run it. `Debug.Assert` is for catching programmer errors (invariants that should always hold), not for validating user input. It's compiled out entirely in Release builds -- nothing you put in a `Debug.Assert` runs in production. The corollary: never put required logic inside one.

### Mini-Program 6: Preprocessor Directives

Clear `Main()` and write:

```csharp
#if DEBUG
Console.WriteLine("This build defines DEBUG -- this line was compiled in.");
#else
Console.WriteLine("This build does NOT define DEBUG -- the DEBUG branch above was never compiled.");
#endif

Console.WriteLine("\n#pragma warning disable/restore silences specific warnings for a specific section.");
Console.WriteLine("#region/#endregion are editor-folding only -- zero effect on compiled output.");
Console.WriteLine("See Supplemental.02 for a full demonstration of all of these.");

GenericFunctions.Pause();
```

Run it. The `#if`/`#else`/`#endif` block decides at compile time which branch even exists in the binary. The other branch is not compiled to "dead code that never runs" -- it's simply absent from the assembly.

### Mini-Program 7: Debug and Trace

Clear `Main()` and write:

```csharp
// Debug.WriteLine() is compiled out in Release builds and, even in Debug builds,
// only goes to the debugger's Output window -- not the console.
Debug.WriteLine("This only appears in a debugger's Output window (Debug build only).");

// Trace.WriteLine() always compiles in. By default it also goes nowhere visible
// in a console app, because no listeners are registered. Adding one changes that.
Trace.Listeners.Add(new ConsoleTraceListener());
Trace.WriteLine("This goes through Trace.Listeners -- visible now that a ConsoleTraceListener was added.");

Console.WriteLine("\nThe Debug.WriteLine() above did not print here -- by design.");
Console.WriteLine("The Trace.WriteLine() did, because of the explicit listener.");
Console.WriteLine("See Supplemental.03.TraceListeners for the full story.");

GenericFunctions.Pause();
```

Run it. `Debug` and `Trace` look similar but differ in one important way: `Debug` is compiled out in Release builds. `Trace` always compiles in. Both write to whatever's in `Trace.Listeners`, and by default that's nothing visible in a console app.

### Mini-Program 8: Windows Event Log

Clear `Main()` and write:

```csharp
const string source = "CSharp.Ch11.InputValidationDebuggingAndInstrumentation";
const string log    = "Application";

try
{
    if (!EventLog.SourceExists(source))
        EventLog.CreateEventSource(source, log);

    EventLog.WriteEntry(source, "CSharp.Ch11 lesson ran successfully.", EventLogEntryType.Information);
    Console.WriteLine($"Wrote an entry to the \"{log}\" event log under source \"{source}\".");
    Console.WriteLine("Open eventvwr.msc -> Windows Logs -> Application to see it.");
}
catch (Exception ex)
{
    Console.WriteLine("Could not write to the event log (likely needs admin privileges");
    Console.WriteLine($"to create the event source the first time): {ex.Message}");
}

GenericFunctions.Pause();
```

Run it. Creating a new event source requires administrator privileges; writing to an existing one doesn't. The `try`/`catch` means the lesson doesn't fail outright if it isn't run as admin. If it does write successfully, open Windows Event Viewer and find the entry.

### Mini-Program 9: Stopwatch Profiling

Clear `Main()` and write:

```csharp
const int iterations = 100_000;
var stopwatch = Stopwatch.StartNew();

string concatenated = "";
for (int i = 0; i < iterations; i++)
    concatenated += "x";

stopwatch.Stop();
Console.WriteLine($"String concatenation in a loop ({iterations:N0} iterations): {stopwatch.ElapsedMilliseconds} ms");

stopwatch.Restart();

var builder = new System.Text.StringBuilder();
for (int i = 0; i < iterations; i++)
    builder.Append("x");

stopwatch.Stop();
Console.WriteLine($"StringBuilder.Append() in a loop ({iterations:N0} iterations): {stopwatch.ElapsedMilliseconds} ms");

Console.WriteLine("\nStringBuilder should measure noticeably faster. String concatenation in a loop");
Console.WriteLine("re-allocates a brand new string on every iteration (strings are immutable).");
Console.WriteLine("See Supplemental.04 for more on profiling techniques.");

GenericFunctions.Pause();
```

Run it. `StringBuilder` wins by a significant margin. This is the simplest possible answer to "which of these two approaches is actually faster" -- run both under a `Stopwatch` at realistic scale.

---

## Takeaways

- `TryParse` is the correct way to validate parseable input -- no exceptions, clean `bool` return.
- `IsNullOrEmpty` and `IsNullOrWhiteSpace` answer different questions. Pick the one that matches your field's requirements.
- Regular expressions validate shape, not just parseability. Always pass a timeout.
- Validation means well-formed AND reasonable. Sanity checks on semantics are different from syntax rejection.
- `Debug.Assert` catches programmer errors in DEBUG builds only. Never put required logic inside one.
- `#if`/`#endif` branches are absent from the binary, not just dead code.
- `Debug.WriteLine` is compiled out in Release; `Trace.WriteLine` always compiles in. Both go nowhere visible by default.
- Event log source creation needs admin; writing to an existing source doesn't.
- `Stopwatch` is the simplest tool for answering "is A faster than B."
