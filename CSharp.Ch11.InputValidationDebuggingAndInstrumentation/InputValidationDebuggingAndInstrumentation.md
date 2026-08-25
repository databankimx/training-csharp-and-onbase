# Input Validation, Debugging, and Instrumentation

## Introduction

Three practical skills every real application needs: checking that input is trustworthy, understanding what your code is doing while you build it, and understanding what it did afterward, especially once it's running somewhere you can't attach a debugger.

---

## Two Kinds of Validation

```csharp
bool isValid = int.TryParse(text, out int result);   // is it well-formed?
bool isReasonable = age >= 0 && age <= 120;            // is it plausible?
```

A value can be perfectly well-formed and still worth double-checking, "150" parses as a valid integer just fine, but it's a strange age for a person. Well-formed failures should block the input outright; "unusual but technically valid" failures are often better handled with a confirmation prompt instead.

---

## Checking for Blank Input

```csharp
string.IsNullOrEmpty("   ");        // false!
string.IsNullOrWhiteSpace("   ");   // true
```

A field containing only spaces isn't "empty," but it's almost never what you actually want. `IsNullOrWhiteSpace()` is the safer default for validating real user input.

---

## Assertions Catch Your Bugs, Not Bad Input

```csharp
Debug.Assert(total == quantity * unitPrice, "This should never be false!");
```

An assertion says "this should always be true if my code is correct." It's for catching mistakes in your own logic during development, not for checking whether a user typed something valid. Assertions disappear entirely from release builds, and a failed one pops up an interactive dialog box, worth avoiding in anything that runs unattended.

---

## `#if`/`#endif`: Deciding What Even Gets Compiled

```csharp
#if DEBUG
    Console.WriteLine("Only exists in debug builds.");
#endif
```

Unlike a normal `if`, this is decided when your code is *compiled*, not when it runs. The losing branch isn't skipped, it's never even turned into a working program at all. This is why things like `Debug.Assert()` cost literally nothing in a release build.

---

## `Debug` vs. `Trace`

```csharp
Debug.WriteLine("...");   // gone entirely in release builds
Trace.WriteLine("...");   // always present
```

Use `Trace` for anything that needs to keep working once your app ships. Use `Debug` for output you only care about while actively developing.

---

## Logging to Windows Event Log

```csharp
EventLog.WriteEntry(source, message, EventLogEntryType.Information);
```

A standard place to log meaningful events on Windows. Creating a brand-new log source the first time needs admin rights; writing to one that already exists doesn't.

---

## Timing Your Own Code

```csharp
var stopwatch = Stopwatch.StartNew();
// do something
stopwatch.Stop();
Console.WriteLine(stopwatch.ElapsedMilliseconds);
```

The simplest way to find out whether one approach is actually faster than another: time both and compare. This lesson's example (string concatenation vs. `StringBuilder`) is a real, common performance trap worth internalizing on its own.

---

## Try It Yourself

Run the project and watch the string-concatenation timing versus the `StringBuilder` timing, the gap should be immediately obvious even at a modest iteration count.
