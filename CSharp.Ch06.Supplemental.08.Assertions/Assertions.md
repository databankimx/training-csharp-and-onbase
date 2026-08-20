# Assertions

## Introduction

An assertion checks something that should always be true, if your code is correct. Unlike an exception, it's not meant to handle something going wrong at runtime, it's meant to catch a bug *during development*, and it costs nothing in a shipped build.

---

## Basic Syntax

```csharp
Debug.Assert(condition);
Debug.Assert(condition, message);
```

If `condition` is `true`, nothing happens, execution just continues. If `condition` is `false`, execution halts and shows the message along with a stack trace.

```csharp
int[] scores = { 88, 92, 79, 95, 84 };

Debug.Assert(scores.Max() <= 100, "Found a score above the maximum possible score!");   // passes silently
Debug.Assert(scores.Length > 10, $"Expected more than 10 scores, but found {scores.Length}.");  // fails, halts
```

**Heads up**: when a failing assertion actually fires outside a debugger, a real Windows dialog appears with Abort/Retry/Ignore buttons. Click **Ignore** to let the program continue. That's the actual, expected behavior, not an error in this project.

---

## Assertions Are Compiled Out of Release Builds

```csharp
public static class Debug
{
    [Conditional("DEBUG")]
    public static void Assert(bool condition, string message) { ... }
}
```

(This is roughly what `Debug.Assert`'s declaration looks like internally.) `[Conditional("DEBUG")]` means the compiler removes every call to `Debug.Assert` entirely when the `DEBUG` symbol isn't defined, which is the case in a Release build. Not "the check runs but does nothing", the call itself simply isn't there. This has a real consequence: **never use `Debug.Assert` to validate anything that must actually be checked in production.**

---

## Assertions vs. Exceptions

These solve different problems.

| | Assertion | Exception |
|---|---|---|
| Guards against | A bug in your own code (should be impossible) | Something that can legitimately go wrong at runtime |
| Example | An internal calculation producing an impossible result | A caller passing an invalid argument |
| Compiled into Release build? | No (for `Debug.Assert`) | Always |
| Who "fixes" it? | The developer, before shipping | The calling code, by handling the exception |

```csharp
private static decimal ApplyDiscount(decimal price, decimal discountPercentage)
{
    // External input can legitimately be wrong: throw an exception.
    if (discountPercentage < 0 || discountPercentage > 1)
        throw new ArgumentOutOfRangeException(nameof(discountPercentage), discountPercentage, "Discount percentage must be between 0 and 1.");

    decimal discounted = price * (1 - discountPercentage);

    // Given validated input, this can never actually happen unless there's a bug right here.
    Debug.Assert(discounted >= 0, "Discounted price should never be negative given validated input.");

    return discounted;
}
```

If you used an assertion instead of the exception for the input check, a Release build would silently accept an invalid `discountPercentage` and produce garbage output, no error, no warning, nothing. That's the mistake to avoid.

---

## `Debug.Assert` vs. `Trace.Assert`

Both behave the same way when they fire. The difference is which build configurations they're actually active in:

- `Debug.Assert`: active only when `DEBUG` is defined (Debug builds).
- `Trace.Assert`: active whenever `TRACE` is defined, which includes both Debug **and** Release builds by default.

Use `Trace.Assert` for a check you want to remain active even after shipping. Use `Debug.Assert` for checks that are purely a development aid.

---

## A Realistic Example: Documenting a Precondition

```csharp
private static int BinarySearch(int[] sortedArray, int target)
{
    Debug.Assert(IsSorted(sortedArray), "BinarySearch requires a sorted array, but the input was not sorted.");
    ...
}
```

Binary search only works correctly on a sorted array. Actually verifying that on every call would require scanning (or sorting) the array first, defeating the whole point of using binary search. The assertion documents the requirement and catches a violation while you're developing and testing, without costing anything once the code ships.

---

## Try It Yourself

Run the project and click through the two assertion dialogs that appear (Ignore both). Then find `BinarySearch()`'s caller and pass in an array that *isn't* sorted, predict what you'll see before running it again.
