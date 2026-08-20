# Exception Handling

## Introduction

`try`/`catch`/`finally` lets a program handle unexpected errors instead of crashing outright. This lesson walks through the syntax, best practices around catch ordering, `Debug.Assert`, the relationship between `using` and `try`/`finally`, custom exceptions, and how integer vs. floating-point arithmetic behaves very differently when something goes wrong.

---

## The Basic Syntax

```csharp
try
{
    // Code that might throw an exception
}
catch (ExceptionType name)
{
    // Code to run if that exception occurs
}
finally
{
    // Code that always runs, whether or not an exception occurred
}
```

You don't need all three blocks together. `try`/`catch` alone handles an exception without anything guaranteed to run afterward. `try`/`finally` alone guarantees cleanup code runs regardless of what happens, but doesn't actually catch (stop) the exception, it still propagates upward after `finally` runs.

---

## Catch Order Matters

```csharp
try
{
    var file = File.Open(@"C:\InvalidDirectory\InvalidFile.txt", FileMode.Append);
}
catch (TrainingException ex) { Console.WriteLine("Caught a training exception!"); ... }
catch (DirectoryNotFoundException ex) { Console.WriteLine("Caught a directory not found exception!"); ... }
catch (FileNotFoundException ex) { Console.WriteLine("Caught a file not found exception!"); ... }
catch (Exception ex) { Console.WriteLine("Caught a general exception!"); ... }
```

The **first** `catch` block whose type matches (or is a base type of) the thrown exception is the one that runs, even if a later block would also match. That's why the convention is to order catches from most specific to least specific, if `catch (Exception ex)` came first, none of the more specific blocks below it would ever execute.

For this particular call, the target directory doesn't exist, so .NET throws `DirectoryNotFoundException`, that's the block that actually runs here.

---

## Debug Assertions

```csharp
const int max = 10;
int[] numbers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

Debug.Assert(numbers.Max() < max, $"Array max value is {max} or more!");   // passes, no trigger
Debug.Assert(numbers.Length < max, $"Array length reached {max} or more!"); // fails, triggers
```

`Debug.Assert(condition, message)` pauses execution (in a debug build, under a debugger) and shows the message and stack trace if `condition` is `false`. It's a development-time sanity check, not error handling for production, `Assert` calls are stripped out entirely in a release build.

---

## `using` Is a Special Case of `try`/`finally`

```csharp
using (var fred = new DisposableClass())
{
    fred.Name = "Fred Sanford";
}
```

is functionally identical to:

```csharp
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

`using` is shorthand, guarantee `Dispose()` runs no matter how the block exits (normal completion, an exception, an early `return`). Notice the manual version needs the variable declared *before* the `try`, so its scope reaches into the `finally` block, `using` handles that scoping for you automatically.

---

## Custom Exceptions

```csharp
[Serializable]
public class TrainingException : Exception
{
    public ErrorType ErrorType { get; set; }
    public string ExceptionType { get; set; }

    public TrainingException(string message, Exception innerException = null, ErrorType errorType = ErrorType.General)
        : base(message, innerException)
    {
        ErrorType = errorType;
        ExceptionType = "TrainingException";
    }
}
```

A custom exception type lets you attach domain-specific information (`ErrorType` here) and gives calling code something more precise to catch than the generic `Exception`. `: base(message, innerException)` chains to the built-in `Exception` constructor, preserving the standard `Message` and `InnerException` behavior everyone already expects.

---

## Integer Overflow: `checked` vs. `unchecked`

```csharp
// Unchecked (the default): silently wraps around, no exception
int a = 1000000000;
int b = 1000000000;
int c = a * b;   // wraps to a nonsensical negative number, no error

// Checked: throws OverflowException instead
checked
{
    int c = a * b;   // throws
}
```

By default, C# integer arithmetic that overflows just wraps around silently, the extra bits are discarded. Wrapping the same code in a `checked` block makes the runtime throw an `OverflowException` instead, catching the problem instead of letting bad data flow through silently.

---

## Floating-Point Doesn't Work the Same Way

```csharp
float a = 1e30f;
float b = 1e30f;
float c = a * b;   // Infinity, not an exception, checked or not

float x = 0f, y = 0f;
float z = x / y;   // NaN, not an exception
```

`checked`/`unchecked` only affects **integer** arithmetic. Floating-point types follow IEEE 754, which defines `Infinity` and `NaN` (Not a Number) as legitimate results for overflow and `0/0`, neither one throws an exception, regardless of context. This trips people up precisely because it looks like it should behave like integer division by zero (which *does* throw `DivideByZeroException`), it doesn't, because `float`/`double` division by zero is a completely different code path with different, well-defined semantics.

---

## Try It Yourself

Run the project and watch each demo's console output. For the arithmetic section specifically, predict each result *before* running it: will `1000000000 * 1000000000` throw or silently produce a wrong number in the unchecked case? What does `0f / 0f` print, and why is that different from what `0 / 0` (integers) would do?
