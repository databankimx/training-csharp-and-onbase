# Chapter 6 Supplemental 08: Assertions

## What This Was

Originally an empty stub, `Main()` had nothing in it but the chapter notes as a comment. You asked me to fill it in with a real, working demo, this document covers what was written and why.

---

## Four Demonstrations

1. **`BasicAssertions()`**: the basic `Debug.Assert(condition, message)` syntax, one passing (silent), one deliberately failing (halts and shows a real assertion dialog).
2. **`AssertionsVersusExceptions()`**: a method (`ApplyDiscount`) that validates its *public* input with an exception, but checks an *internal* invariant afterward with an assertion, showing both mechanisms in the same method, each doing the job the other can't.
3. **`DebugAssertVersusTraceAssert()`**: `Debug.Assert` vs. `Trace.Assert`, same visible behavior when they fire, very different compile-time behavior.
4. **`AssertingAnInternalInvariant()`**: a `BinarySearch()` implementation that asserts its precondition (the input must already be sorted) rather than re-verifying it at runtime, a genuinely realistic use case for an assertion.

---

## Important: This Runs Interactively

Two of the four demonstrations deliberately trigger a **real, failing assertion**. Outside of a debugger, .NET's default trace listener shows an actual Windows "Assertion Failed" dialog with **Abort / Retry / Ignore** buttons, you have to click one (**Ignore** is the safe choice) to let the program continue. This is not a bug or a mistake, it's the genuine, unmodified behavior `Debug.Assert`/`Trace.Assert` produce when they fire. Seeing the real dialog is arguably more instructive than reading about it.

---

## Assertions vs. Exceptions: The Actual Rule

```csharp
private static decimal ApplyDiscount(decimal price, decimal discountPercentage)
{
    if (discountPercentage < 0 || discountPercentage > 1)
        throw new ArgumentOutOfRangeException(nameof(discountPercentage), discountPercentage, "Discount percentage must be between 0 and 1.");

    decimal discounted = price * (1 - discountPercentage);

    Debug.Assert(discounted >= 0, "Discounted price should never be negative given validated input.");

    return discounted;
}
```

The exception guards against something that can legitimately go wrong at runtime, a caller passing a bad value, this can happen even in perfectly correct code, since the value comes from outside. The assertion guards against something that should be *impossible* if the method's own logic is correct, given a validated `discountPercentage`, `discounted` can never actually be negative unless there's a bug in this method itself. That distinction is the entire rule: **exceptions handle bad input, assertions catch broken logic**. Because assertions are compiled out of release builds, using one to validate external input would mean that validation silently disappears in production, exactly backwards from what you want.

---

## `Debug.Assert` vs. `Trace.Assert`

Both live in `System.Diagnostics` and behave identically when they fire. The difference is entirely about *when they're even compiled in*:

- `Debug.Assert` is decorated with `[Conditional("DEBUG")]` on the `Debug` class's methods, the call is compiled out entirely unless the `DEBUG` symbol is defined, which is only true in Debug builds by default.
- `Trace.Assert` is conditional on the `TRACE` symbol instead, which is defined by default in **both** Debug and Release configurations in this solution (and in most .NET project templates generally).

That makes `Trace.Assert` the right choice for a check you want active even in a shipped Release build, and `Debug.Assert` the right choice for checks that are purely a development-time aid, cheap enough to sprinkle liberally since they cost nothing once compiled out.

---

## A Genuinely Realistic Use Case: `BinarySearch()`

```csharp
private static int BinarySearch(int[] sortedArray, int target)
{
    Debug.Assert(IsSorted(sortedArray), "BinarySearch requires a sorted array, but the input was not sorted.");
    ...
}
```

Binary search's entire performance advantage depends on the array already being sorted. Actually *verifying* that with an `if`/`throw` on every call would mean sorting (or at least scanning) the array first, which defeats the purpose of using binary search at all. An assertion documents the precondition and catches a violation during development and testing, without adding any cost to a release build where, by the time you ship, that invariant should already be well-established as true.
