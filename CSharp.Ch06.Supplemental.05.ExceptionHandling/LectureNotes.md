# Chapter 6 Supplemental 05: Exception Handling

## What This Is

The deepest exception-handling lesson in this chapter, and structurally the most different project in the whole solution so far: a real `log4net`-based logging pipeline, a custom `TrainingException` type, a custom `ConfigurationSection` (`ProgramSettings`) read from `App.config`, catch-block ordering, `Debug.Assert`, `using` vs. `try`/`finally`, and four arithmetic-exception demonstrations (integer overflow checked/unchecked, float overflow, divide-by-zero).

This was originally an **old-style (non-SDK) `.csproj`** using `packages.config` for `log4net`, unlike every other project in this solution, all already SDK-style. Converted here to SDK-style with `log4net` as a `PackageReference` instead.

---

## No Functional Bugs, But Several Things Worth Knowing

### A typo in `App.config`, corrected

```xml
<!-- Original: -->
<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, Log4net"/>

<!-- Corrected: -->
<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
```

The assembly name in the `type` attribute was capitalized `Log4net` instead of `log4net`. Windows assembly binding is typically case-insensitive, so this most likely worked anyway, but it's inconsistent with the actual assembly name and worth fixing for robustness (.NET assembly binding rules aren't guaranteed case-insensitive on every platform).

### A namespace convention inconsistency, preserved

Every `.cs` file here uses `namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling`, escaping the leading digit with a lowercase `s` (`s05`). Compare that to Supplementals 01–04, which all use an underscore instead (`_01`, `_02`, `_03`, `_04`). Both are valid ways to work around C#'s "identifiers can't start with a digit" rule, this project just picked a different one. Preserved exactly as authored rather than "corrected" to match the other four, this is your own content, not a bug, just a small inconsistency worth knowing about if you ever go looking for why the namespaces don't match pattern.

### Two reference diagrams weren't carried over

The original project's `Resources` folder has two PNGs (`exception-classes.png`, `exception-classes-2.png`), presumably illustrating the .NET exception class hierarchy. There's no tool available to copy a binary file directly between two folders on your machine without either passing through my sandbox (lossy round-trip risk) or deleting the original via a move operation, so I left them out rather than risk either. They're still sitting in `developer-training-bb\CSharp.Ch06.Supplemental.05.ExceptionHandling\Resources\` if you want to copy them over yourself.

### Log output location

Logs land at `C:\Temp\CSharpTraining\Logs\ExceptionHandlingExample.log` (from `App.config`). Unlike the hardcoded `D:\FileStore\...` path that was a genuine bug in `Supplemental.03.Callbacks`, this one is a reasonably portable convention, any Windows machine has a `C:\` drive, `log4net`'s `FileAppender` creates missing directories automatically, so this was left unchanged. Worth knowing where to look if `ViewLog()` doesn't open anything.

---

## Worth Reading Closely: `SpecificToGeneral()`

```csharp
try
{
    var file = File.Open(@"C:\InvalidDirectory\InvalidFile.txt", FileMode.Append);
}
catch (TrainingException ex) { ... }
catch (DirectoryNotFoundException ex) { ... }
catch (FileNotFoundException ex) { ... }
catch (Exception ex) { ... }
```

Only the `DirectoryNotFoundException` catch actually fires for this specific line (the *directory* doesn't exist, so .NET never gets far enough to check whether the file does), the `FileNotFoundException` catch is effectively unreachable for this particular call, but it's still there to demonstrate the ordering rule: catches are checked top to bottom, and the *first* one that matches wins, even if a more specific one appears later. Catching from most-specific to least-specific is what makes that ordering meaningful, if `Exception` were listed first, none of the more specific catches below it would ever run at all.

---

## Worth Reading Closely: Four Arithmetic Exceptions, Four Different Outcomes

```csharp
IntegerOverflowUnchecked();  // silently wraps, no exception
IntegerOverflowChecked();    // throws OverflowException
FloatOverflowUnchecked();    // silently becomes Infinity, no exception, checked/unchecked doesn't matter
DivideByZero();               // 0f / 0f = NaN, no exception (float division, not integer)
```

This is a deliberately built comparison, not four unrelated demos. Integer overflow behavior depends entirely on `checked`/`unchecked` context. Floating-point overflow and division by zero don't throw *at all*, regardless of context, IEEE 754 floats represent those cases with `Infinity`/`NaN` instead of erroring. That's a common point of confusion worth sitting with: `checked` only changes integer arithmetic, it has no effect on `float`/`double`.

---

## Worth Noticing: `Random` Seeded From `Ticks`

```csharp
var rnd = new Random((int)DateTime.Now.Ticks);
```

Casting `DateTime.Now.Ticks` (a `long`) down to `int` truncates it, and doing this in quick succession (for example, in a tight loop) can produce identical or highly correlated seeds. It doesn't cause a problem in this specific demo (`PossibleException()` only runs once per program execution), but it's worth knowing as a pattern to avoid, `new Random()` with no arguments already seeds itself well and doesn't have this issue.
