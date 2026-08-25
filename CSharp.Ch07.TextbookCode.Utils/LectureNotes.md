# Ch07 Textbook Code: Utils

## What This Is

A class library, not a runnable lesson on its own, `Utils.CommonFunctions` is the shared helper class referenced by `CSharp.Ch07.TextbookCode.SimpleApp` and `CSharp.Ch07.TextbookCode.TPLApp`. This is the **direct source** `CSharp.SharedLibrary.HelperClasses.Ch07SharedFunctions` (used throughout the main lesson and most of the Chapter 7 `Supplemental.*` projects) was adapted from.

No bugs found. `ReadDataFromIO()` (a 2-second `Thread.Sleep`) and `DoIntensiveCalculations()` (the same "nonsense divisions and multiplications" loop, comment typo and all) map directly onto `Ch07SharedFunctions.SimulateReadDataFromIo()` and `DoIntensiveCalculations()`.

---

## This Confirms Where the Main Lesson's Bug Came From

```csharp
public static void WaitForKeyWhehDebugging()
{
    if (Debugger.IsAttached)
    {
        Console.Write("Press any key to continue . . .");
        Console.ReadKey(true);
    }
}
```

Note the typo in the method name itself, "Wheh" instead of "When", preserved exactly here per the "unedited textbook code" policy. This is the literal origin of the bug found and fixed in `CSharp.Ch07.MultithreadingAndAsynchronousProcessing`'s `Program.cs`: a pause that only fires when a debugger is attached, meaning it silently does nothing when a lesson is run normally (including via `LessonRunner`). The typo carried through faithfully into `Ch07SharedFunctions.WaitForKeyWhenDebugging()` (spelling corrected there, since that's original, non-`TextbookCode.*` content, but the debugger-only behavior itself was preserved until it was identified as a real usability bug and fixed to pause unconditionally instead.

Worth noting: this specific bug is much less disruptive in `SimpleApp` than it was in the main lesson. The main lesson had an interactive menu loop that immediately cleared the console on its next iteration, wiping out a result the user never got a chance to read. `SimpleApp` (and `TPLApp`) just call one method and exit, no loop, no `Console.Clear()` afterward, so skipping the pause here just means the program exits a little faster when run outside a debugger, not a lost result. Preserved as originally authored, since there's no actual harm done in this shape of the code.

---

## Worth Noticing: `ReadDataFromIOAsync()` Is Defined but Never Called

```csharp
public static Task<double> ReadDataFromIOAsync()
{
    return Task.Run(new Func<double>(ReadDataFromIO));
}
```

Neither `SimpleApp` nor `TPLApp` calls this method anywhere, it exists purely as a convenience for whichever textbook code lab needed an async-wrapped version of `ReadDataFromIO()`. Worth noticing the `new Func<double>(ReadDataFromIO)` syntax, an explicit delegate construction wrapping a method group, functionally identical to just writing `Task.Run(ReadDataFromIO)` directly (a slightly older, more verbose style, worth comparing against the terser method-group-conversion syntax used elsewhere in this training set).
