# Chapter 6 Supplemental 05: Exception Handling

## What This Is

The deepest exception-handling lesson in the chapter set. It contains a real `log4net` logging pipeline, a custom `TrainingException` type, a custom `ConfigurationSection`, catch-block ordering, `Debug.Assert`, `using` vs. `try`/`finally`, and four arithmetic demonstrations that together make the case for why `checked` exists and what its limits are.

It's also the only project in this set where `Main()` returns an `int` -- an exit code driven by whether something went wrong.

---

## How to Write This Program

This project is structured as a real application entry point rather than a demo, so the walkthrough follows its layered structure: `Main()` first, then each method it calls.

### Step 1: The Program-Level try/catch/finally

```csharp
private static int Main()
{
    try
    {
        Initialize();
        Assertions();
        SpecificToGeneral();
        CompareToUsing();
        PossibleException();
        ArithmeticExceptions();
    }
    catch (Exception ex)
    {
        status = Status.Error;
        Environment.ExitCode = (int)status;
        ex.HandleException();
    }
    finally
    {
        // Log the end of the program, display the log, prompt to exit
    }
    return (int)status;
}
```

Run it empty (everything stubbed out) and confirm it builds and exits cleanly with code `0`. That proves the infrastructure before adding any content.

Three things this shape teaches:

**`finally` runs on every path** -- normal completion, exception, or early return. That's what makes it the correct place for cleanup and final logging.

**The exit code matters.** A console application's exit code is how schedulers and CI pipelines determine success or failure. Returning `0` from a program that actually failed is a genuine operational bug -- the orchestrator reports green while the work didn't happen. `Environment.ExitCode` is also set directly as a belt-and-suspenders measure.

**Don't use `Environment.Exit()`.** It tears down the process immediately: `finally` blocks don't run, `using` blocks don't dispose, buffered writes may be lost. Return from `Main()` instead.

### Step 2: Initialize() -- Wrapping Exceptions

```csharp
private static void Initialize()
{
    try
    {
        settings = (ProgramSettings)ConfigurationManager.GetSection(ProgramSettings.SectionName);
    }
    catch (Exception ex)
    {
        throw new TrainingException("Error initializing global variables!", ex);
    }
}
```

This is exception **wrapping**. The critical detail is the second argument. Passing `ex` as the inner exception preserves the original -- the new `TrainingException` adds context about *where and why* without discarding *what actually went wrong*.

The failure mode to memorize:

```csharp
catch (Exception ex)
{
    throw new TrainingException("Something failed!"); // original exception destroyed
}
```

Stack trace, message, and type of the real problem are gone forever. The rule: if you wrap, always pass the inner exception.

Also know the difference between `throw;` and `throw ex;`. Bare `throw;` rethrows with the original stack trace intact. `throw ex;` resets the stack trace to the current line, making the exception look like it originated in your catch block. Use `throw;` unless you genuinely intend to wrap.

The reason to define `TrainingException` at all: callers can write `catch (TrainingException)` to handle your application's failures specifically, distinct from framework exceptions they don't own.

### Step 3: Assertions() -- Debug-Only Checks

```csharp
private static void Assertions()
{
    const int max = 10;
    int[] numbers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

    // Passes silently -- numbers.Max() is 9, which is less than 10
    Debug.Assert(numbers.Max() < max, $"Array max value is {max} or more!");

    // Fails -- numbers.Length is 10, which is NOT less than 10
    Debug.Assert(numbers.Length < max, $"Array length reached {max} or more!");
}
```

Run it. The first assert passes silently. The second fires -- if you're running under a debugger, execution breaks at that line; if not, a dialog appears. The difference between `Max()` (the largest value, `9`) and `Length` (the count, `10`) is off by one, and that's the point.

`Debug.Assert` is `[Conditional("DEBUG")]` -- compiled out entirely in a Release build. The call disappears, not "the condition evaluates to false." This has a concrete trap:

```csharp
Debug.Assert(TryInitialize());   // TryInitialize never runs in Release
```

Assertions state what you believe must be true. Exceptions handle what might legitimately go wrong at runtime. Assertions are for bugs; exceptions are for circumstances. `Supplemental.08.Assertions` goes further on this.

### Step 4: SpecificToGeneral() -- Catch Block Ordering

```csharp
private static void SpecificToGeneral()
{
    try
    {
        File.Open(@"C:\InvalidDirectory\InvalidFile.txt", FileMode.Append);
    }
    catch (TrainingException ex)          { ex.HandleException(); }
    catch (DirectoryNotFoundException ex) { Console.WriteLine("Directory not found!"); ex.HandleException(); }
    catch (FileNotFoundException ex)      { Console.WriteLine("File not found!"); ex.HandleException(); }
    catch (Exception ex)                  { Console.WriteLine("General exception!"); ex.HandleException(); }
}
```

Run it. Only the `DirectoryNotFoundException` fires -- the directory doesn't exist, so .NET never gets far enough to check whether the file does.

Catch blocks are checked top to bottom, first match wins. If `catch (Exception)` were listed first, no specific catch below it would ever run, because every exception matches `Exception`. The compiler prevents the most obvious version of this mistake (listing a base type before a derived type is a compile error), but it can't catch every case.

The general principle: catch the narrowest exception type you can actually do something about. A `catch` block you can't meaningfully respond to is usually better left unwritten.

### Step 5: CompareToUsing() -- What `using` Actually Is

```csharp
private static void CompareToUsing()
{
    // using block:
    using (var fred = new DisposableClass())
    {
        fred.Name = "Fred Sanford";
    }

    // Functionally identical to:
    DisposableClass lamont = null;
    try
    {
        lamont = new DisposableClass { Name = "Lamont Sanford" };
    }
    finally
    {
        lamont?.Dispose();
    }
}
```

Run it. Both paths show the same disposal sequence.

A `using` block is not a distinct language feature -- the compiler expands it into exactly this `try`/`finally`. The resource is disposed whether the block completes, returns, or throws. Notice the null check: `lamont?.Dispose()` is necessary because if the constructor throws, `lamont` is still `null` when `finally` runs. Calling `Dispose()` unconditionally would replace the real exception with a `NullReferenceException`. `using` handles both correctly. Prefer it; write the `try`/`finally` by hand only when the resource lifetime genuinely doesn't fit a block.

### Step 6: PossibleException() -- A Nondeterministic Throw

```csharp
private static void PossibleException()
{
    var rnd = new Random();
    int val = rnd.Next(1, 100);

    if (val % 2 == 0) throw new TrainingException($"[{val}] is an even number!");
    if (val > 50)     throw new TrainingException($"[{val}] is over fifty!");
}
```

Roughly 75% chance of throwing on any run (even numbers, plus the odd numbers above 50). Run it several times and watch both the success and failure paths. Note that an even number over 50 reports "even" -- the first `throw` exits the method immediately; you never reach the second check.

The original code seeded `Random` from `(int)DateTime.Now.Ticks`. That cast from `long` to `int` truncates, and successive calls in a tight loop can produce identical or correlated seeds. `new Random()` with no arguments seeds itself correctly without the truncation issue.

### Step 7: ArithmeticExceptions() -- Four Cases, Four Different Outcomes

```csharp
// Case 1: integer overflow, unchecked (default)
int a = 1000000000, b = 1000000000;
int c = a * b;
Console.WriteLine($"{a} * {b} = {c}"); // meaningless negative number, no exception

// Case 2: integer overflow, checked
checked
{
    try
    {
        c = a * b;
    }
    catch (OverflowException ex)
    {
        Console.WriteLine($"Caught: {ex.Message}");
    }
}

// Case 3: float overflow (checked makes no difference)
float fa = 1e30f, fb = 1e30f;
float fc = fa * fb;
Console.WriteLine(fc); // Infinity -- no exception

// Case 4: float divide by zero
float fd = 0f, fe = 0f;
float ff = fd / fe;
Console.WriteLine(ff); // NaN -- no exception
```

Run it. Case 1 prints a wrong number silently. Case 2 throws. Cases 3 and 4 produce `Infinity` and `NaN` without throwing.

The table worth memorizing:

| Operation | Type | Result | Throws? |
|---|---|---|---|
| Overflow | `int` (unchecked) | Wraps silently | No |
| Overflow | `int` (checked) | `OverflowException` | Yes |
| Overflow | `float` | `Infinity` | No |
| `0 / 0` | `float` | `NaN` | No |
| `0 / 0` | `int` | -- | Yes (`DivideByZeroException`) |

`checked`/`unchecked` affects **integer arithmetic only**. Floating-point follows IEEE 754, which defines `Infinity` and `NaN` as legitimate representable values. `NaN` poisons every subsequent calculation (`NaN + 1` is `NaN`), and `NaN == NaN` is `false`, so equality checks don't detect it -- use `float.IsNaN()` and `float.IsInfinity()` where it matters.

---

## Configuration and Logging Notes

Logs land at `C:\Temp\CSharpTraining\Logs\ExceptionHandlingExample.log`. Unlike the hardcoded `D:\FileStore` path in Supplemental 03, this is a reasonably portable convention on Windows -- `log4net`'s `FileAppender` creates missing directories automatically. Change it in `App.config` if needed.

`settings.DebugMode` and `settings.Interactive` gate trace logging and the log-viewing prompt respectively. Same binary, different behavior depending on who's running it and in what context -- a small but real pattern worth noticing.

---

## Takeaways

- `finally` runs on every path. `Environment.Exit()` skips it. Return from `Main()` instead.
- Return a meaningful exit code; silent success on failure breaks automation.
- Catch `Exception` at the top level only. Not in library or business logic.
- Always pass the original as the inner exception when wrapping.
- `throw;` preserves the stack trace. `throw ex;` resets it.
- Catch blocks match top to bottom, first match wins -- most specific first.
- `using` compiles to null-safe `try`/`finally`. Prefer it.
- `Debug.Assert` vanishes in Release -- never put required logic in one.
- Assertions are for programmer errors; exceptions are for runtime circumstances.
- `checked`/`unchecked` affects integers only. Floating-point overflow never throws.
- Integer `0/0` throws; floating-point `0f/0f` returns `NaN`.
