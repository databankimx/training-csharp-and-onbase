# Chapter 6 Supplemental 05: Exception Handling

## What This Is

The deepest exception-handling lesson in this chapter, and structurally the most different project in the solution so far. It contains a real `log4net`-based logging pipeline, a custom `TrainingException` type, a custom `ConfigurationSection` (`ProgramSettings`) read from `App.config`, catch-block ordering, `Debug.Assert`, `using` vs. `try`/`finally`, and four arithmetic demonstrations.

It's also the only project here where `Main()` returns an `int` rather than `void` — an exit code, driven by whether an exception was caught.

### A note on the project file

This was originally an **old-style (non-SDK) `.csproj`** using `packages.config` for `log4net`, unlike every other project in this solution. Converted here to SDK-style with `log4net` as a `PackageReference` instead.

---

## The Program-Level `try` / `catch` / `finally`

```csharp
try
{
	Initialize();

	string startMessage = $"{TimeStamp()} - Start program...{Environment.NewLine}";
	if (settings.DebugMode) startMessage.TraceLog();
	GenericFunctions.Pause();

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
	GenericFunctions.Pause();
}
finally
{
	string endMessage = $"{TimeStamp()} - End program...";
	if (settings.DebugMode) endMessage.TraceLog();

	if (settings.Interactive)
	{
		Logging.ViewLog();
		GenericFunctions.Pause();
		GenericFunctions.Pause(final: true);
	}
}

return (int)status;
```

This is the shape of a real application entry point rather than a demo. Several things are being taught at once.

**`finally` runs regardless.** Whether the `try` completes normally, throws, or returns early, the `finally` block executes. That's what makes it the correct place for cleanup, closing, and final logging — anything that must happen on every path.

**The exit code matters.** `Status.Error` is written both to a local variable and to `Environment.ExitCode`. A console application's exit code is how schedulers, CI pipelines, and batch scripts determine success or failure. Returning `0` from a program that failed is a genuine operational bug: the orchestrator reports green while the work didn't happen.

**The warning about `Environment.Exit()` is the real lesson.** The source comments flag it twice, and it's worth taking seriously:

```csharp
// If we later call Environment.Exit(), that terminates the stack without executing finally blocks
```

`Environment.Exit()` tears down the process immediately. `finally` blocks don't run, `using` blocks don't dispose, buffered writes may be lost. Returning from `Main()` unwinds cleanly; `Environment.Exit()` does not. Prefer returning a value.

**`catch (Exception ex)` at the top level is appropriate here** — and only here. A top-level handler exists so the application logs the failure and reports a sensible exit code rather than dumping a raw stack trace at the user. Catching `Exception` in a *library* method or deep in business logic is a different thing entirely, and usually wrong: it swallows failures the caller needed to know about.

---

## `Initialize()`: Wrapping and Chaining Exceptions

```csharp
catch (Exception ex)
{
	throw new TrainingException("Error initializing global variables!", ex);
}
```

```csharp
catch (Exception ex)
{
	throw new TrainingException("Error creating time-stamp!", ex);
}
```

This is exception **wrapping**, and the critical detail is the second constructor argument. Passing `ex` as the inner exception preserves the original — the new `TrainingException` adds context about *where and why* without discarding *what actually went wrong*.

The failure mode to avoid:

```csharp
catch (Exception ex)
{
	throw new TrainingException("Something failed!");   // original exception destroyed
}
```

Now the stack trace, the message, and the type of the real problem are gone forever. The rule is simple: if you wrap, always pass the inner exception.

Also worth knowing the distinction between `throw;` and `throw ex;` inside a catch block. Bare `throw;` rethrows the original and preserves the stack trace. `throw ex;` resets the stack trace to the current line, making the exception look like it originated in your catch block. Use `throw;` unless you genuinely intend to wrap.

The reason to define a custom `TrainingException` at all is catchability. Callers can write `catch (TrainingException)` to handle *your* application's failures specifically, distinct from framework exceptions they don't own.

---

## `Assertions()`: Debug-Only Checks

```csharp
const int max = 10;

int[] numbers = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];

// This Assert will not trigger a stack trace
Debug.Assert(numbers.Max() < max, $"Array max value is {max} or more!");

// This Assert will trigger a stack trace
Debug.Assert(numbers.Length < max, $"Array length reached {max} or more!");
```

The two asserts are deliberately paired. `numbers.Max()` is `9`, so `9 < 10` passes and nothing happens. `numbers.Length` is `10`, so `10 < 10` is false and the assertion fires — halting execution in the debugger with the message and stack trace.

That off-by-one contrast is the point: `Max()` is the largest *value*, `Length` is the *count*. They differ by one for a zero-based sequence, and confusing them is a classic source of bounds errors.

The essential property of `Debug.Assert`: **it is compiled out entirely in a Release build.** The `[Conditional("DEBUG")]` attribute means the call disappears — not "the condition is false and nothing happens," but the call site doesn't exist in the IL.

The consequence is a real trap. Never put required logic inside an assertion:

```csharp
Debug.Assert(TryInitialize());   // never runs in Release
```

Assertions state what you believe must be true — conditions that indicate a programming error if violated. Exceptions handle what might legitimately go wrong at runtime: bad input, missing files, network failures. Assertions are for bugs; exceptions are for circumstances.

`CSharp.Ch06.Supplemental.08.Assertions` goes further on this.

---

## `SpecificToGeneral()`: Catch Block Ordering

```csharp
try
{
	var file = File.Open(@"C:\InvalidDirectory\InvalidFile.txt", FileMode.Append);
}
catch (TrainingException ex)          { Console.WriteLine("Caught a training exception!"); ex.HandleException(); }
catch (DirectoryNotFoundException ex) { Console.WriteLine("Caught a directory not found exception!"); ex.HandleException(); }
catch (FileNotFoundException ex)      { Console.WriteLine("Caught a file not found exception!"); ex.HandleException(); }
catch (Exception ex)                  { Console.WriteLine("Caught a general exception!"); ex.HandleException(); }
```

Only the `DirectoryNotFoundException` catch actually fires for this line. The *directory* doesn't exist, so .NET never gets far enough to check whether the file does. The `FileNotFoundException` catch is effectively unreachable for this particular call, but it's there to demonstrate the rule:

**Catch blocks are checked top to bottom, and the first one that matches wins** — even if a more specific one appears later.

That's why most-specific-to-least-specific ordering matters. If `catch (Exception)` were listed first, none of the more specific catches below it would ever run, because every exception matches `Exception`. The compiler actually prevents the most obvious version of this mistake — listing a base type before a derived type is a compile error — but it can't catch every case, particularly with unrelated hierarchies.

Note also that `FileMode.Append` was chosen deliberately. A mode like `FileMode.Open` on a valid directory would produce `FileNotFoundException` instead, which is presumably what that catch block was written to demonstrate before the invalid *directory* took precedence.

The general principle: catch the narrowest exception type you can actually do something about. A `catch` block you can't meaningfully respond to is usually better left unwritten, so the exception reaches someone who can.

---

## `CompareToUsing()`: `using` Is `try`/`finally`

```csharp
// So this code...
using (var fred = new DisposableClass())
{
	fred.Name = "Fred Sanford";
}

// ... is functionally identical to this code
DisposableClass lamont = null;
try
{
	lamont = new DisposableClass { Name = "Lamont Sanford" };
}
finally
{
	lamont?.Dispose();
}
```

This connects directly back to the `IDisposable` material in Chapter 5. A `using` block is not a distinct language feature — the compiler expands it into exactly this `try`/`finally`. The resource is disposed whether the block completes, returns, or throws.

Two details called out by the source comments:

**Scope.** `lamont` must be declared *before* the `try` block so the `finally` block can see it. Anything declared inside `try` is out of scope in `finally`. That's the awkwardness `using` removes.

**The null check.** `lamont?.Dispose()` is necessary because if the constructor itself throws, `lamont` is still `null` when `finally` runs. Calling `Dispose()` on it unconditionally would replace the real exception with a `NullReferenceException` — the original failure lost, the misleading one reported. This is a common way for a genuine error to get masked by its own cleanup code.

`using` handles both correctly and in fewer lines. Prefer it; write the `try`/`finally` by hand only when the lifetime genuinely doesn't fit a block.

---

## `PossibleException()`: A Nondeterministic Throw

```csharp
var rnd = new Random((int)DateTime.Now.Ticks);
int val = rnd.Next(1, 100);

if (val % 2 == 0) throw new TrainingException($"Oh no! [{val}] is an even number!");
if (val > 50)     throw new TrainingException($"Oh no! [{val}] is over fifty!");
```

Roughly a 75% chance of throwing on any given run — even numbers, plus the odd numbers above 50. It's deliberately nondeterministic so you see both the success and failure paths across repeated runs, and so you can watch the top-level `catch` and `finally` in `Main()` actually do their jobs.

Note the ordering: an even number over 50 reports "even," never "over fifty," because the first `throw` exits the method immediately. Only one exception ever escapes.

### Worth Noticing: `Random` Seeded From `Ticks`

```csharp
var rnd = new Random((int)DateTime.Now.Ticks);
```

Casting `DateTime.Now.Ticks` (a `long`) down to `int` truncates it, and doing this in quick succession — in a tight loop, for instance — can produce identical or highly correlated seeds, because the system clock's resolution is coarser than the loop.

It causes no problem here, since `PossibleException()` runs once per program execution. But it's a pattern to avoid. `new Random()` with no arguments already seeds itself well and doesn't have this issue. On modern .NET, `Random.Shared` is better still.

---

## `ArithmeticExceptions()`: Four Cases, Four Different Outcomes

```csharp
IntegerOverflowUnchecked();  // silently wraps, no exception
IntegerOverflowChecked();    // throws OverflowException
FloatOverflowUnchecked();    // silently becomes Infinity, no exception
DivideByZero();              // 0f / 0f = NaN, no exception
```

This is a deliberately constructed comparison, not four unrelated demos.

### Integer overflow, unchecked

```csharp
int a = 1000000000;
int b = 1000000000;
int c = a * b;
Console.WriteLine($"{a} * {b} = {c}");
```

The true product is 10<sup>18</sup>; `int` maxes out near 2.1 × 10<sup>9</sup>. The high bits are simply discarded and you get a meaningless — often negative — number. **No exception.** The program continues confidently with wrong data, which is considerably worse than crashing.

This is C#'s default context, and it's the default for performance reasons.

### Integer overflow, checked

```csharp
checked
{
	try
	{
		int a = 1000000000;
		int b = 1000000000;
		int c = a * b;
		Console.WriteLine($"{a} * {b} = {c}");
	}
	catch (Exception ex)
	{
		ex.HandleException();
	}
}
```

Identical arithmetic, wrapped in `checked`. Now the CLR verifies the result fits and throws `OverflowException` when it doesn't.

Note the block nesting: `checked` outside, `try` inside. Either arrangement works, but `checked` must enclose the arithmetic itself for the context to apply.

`checked` is also available as an expression — `checked(a * b)` — and as a project-wide compiler setting. Use it where correctness beats speed: financial calculations, sizes, counters, anything where a silently wrong number causes real damage.

### Float overflow

```csharp
float a = 1e30f;
float b = 1e30f;
float c = a * b;      // Infinity
```

10<sup>60</sup> exceeds `float`'s range, and the result is `Infinity`. **No exception, and `checked` makes no difference whatsoever.**

### Float divide by zero

```csharp
float a = 0f;
float b = 0f;
float c = a / b;      // NaN
```

`NaN` — "not a number." Again no exception.

### The point of the comparison

`checked`/`unchecked` affects **integer arithmetic only**. Floating-point types follow IEEE 754, which defines `Infinity` and `NaN` as legitimate representable values rather than error conditions. There is nothing to throw, because as far as the standard is concerned, nothing went wrong.

The practical consequence: floating-point errors propagate silently. `NaN` poisons every subsequent calculation — `NaN + 1` is `NaN`, and `NaN == NaN` is `false`, so even an equality check won't detect it. Use `float.IsNaN()` and `float.IsInfinity()` when it matters.

One further trap worth knowing: integer division by zero *does* throw `DivideByZeroException`. Only the floating-point version returns `NaN`. Same operator, same-looking code, entirely different behavior depending on the operand types.

---

## Configuration and Logging Notes

### A typo in `App.config`, corrected

```xml
<!-- Original: -->
<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, Log4net"/>

<!-- Corrected: -->
<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net"/>
```

The assembly name in the `type` attribute was capitalized `Log4net` instead of `log4net`. Windows assembly binding is typically case-insensitive, so this most likely worked anyway, but it's inconsistent with the actual assembly name and worth fixing for robustness — .NET assembly binding rules aren't guaranteed case-insensitive on every platform.

### Log output location

Logs land at `C:\Temp\CSharpTraining\Logs\ExceptionHandlingExample.log`, configured in `App.config`.

Unlike the hardcoded `D:\FileStore\...` path that was a genuine bug in `Supplemental.03.Callbacks`, this one is a reasonably portable convention — any Windows machine has a `C:\` drive, and `log4net`'s `FileAppender` creates missing directories automatically — so it was left unchanged. Worth knowing where to look if `ViewLog()` doesn't open anything.

The meaningful difference between the two cases: this path is *configuration*, changeable without a rebuild, and it's a location the program creates. The Callbacks path was a *compiled-in assumption* about a location that had to already exist.

### `ProgramSettings` and behavior flags

`settings.DebugMode` and `settings.Interactive` gate the trace logging and the log-viewing prompt respectively. That's a small but real pattern: the same binary behaves differently in an interactive developer session versus an unattended scheduled run, without recompiling.

### A namespace convention inconsistency, preserved

Every `.cs` file here uses `namespace CSharp.Ch06.Supplemental.s05.ExceptionHandling`, escaping the leading digit with a lowercase `s` (`s05`). Compare that to Supplementals 01–04, which all use an underscore (`_01`, `_02`, `_03`, `_04`).

Both are valid ways to work around C#'s "identifiers can't start with a digit" rule; this project just picked a different one. Preserved exactly as authored rather than "corrected" to match the other four — not a bug, just a small inconsistency worth knowing about if you ever go looking for why the namespaces don't match pattern.

### Two reference diagrams weren't carried over

The original project's `Resources` folder has two PNGs (`exception-classes.png`, `exception-classes-2.png`), presumably illustrating the .NET exception class hierarchy. They weren't copied over during the migration. They're still in `developer-training-bb\CSharp.Ch06.Supplemental.05.ExceptionHandling\Resources\` if you want to bring them across.

---

## Takeaways

- `finally` runs on every path — normal completion, exception, or early return.
- `Environment.Exit()` skips `finally` blocks and `Dispose()` calls. Return from `Main()` instead.
- Return a meaningful exit code; silent success on failure breaks automation.
- Catch `Exception` at the top level only, to log and report. Not in library or business code.
- Always pass the original as the inner exception when wrapping, or the real cause is lost.
- Use `throw;` to rethrow, not `throw ex;` — the latter resets the stack trace.
- Catch blocks match top to bottom, first match wins. Order most specific to least.
- `using` compiles to `try`/`finally` with a null-safe `Dispose()`. Prefer it.
- `Debug.Assert` vanishes in Release builds — never put required logic in one.
- Assertions are for programmer errors; exceptions are for runtime circumstances.
- `checked`/`unchecked` affects integers only. Unchecked integer overflow wraps silently.
- Floating-point overflow yields `Infinity`, `0f/0f` yields `NaN`, and neither ever throws.
- Integer division by zero *does* throw; floating-point division by zero does not.
- Don't seed `Random` from truncated `Ticks`. Use `new Random()` or `Random.Shared`.
