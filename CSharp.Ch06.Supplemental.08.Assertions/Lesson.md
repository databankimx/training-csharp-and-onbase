# Chapter 6 Supplemental 08: Assertions

## What This Is

Four demonstrations that together make the argument for why `Debug.Assert` exists as a separate mechanism from exceptions: passing vs. failing assertions side by side, the exact rule for when to use each, `Debug.Assert` vs. `Trace.Assert`, and a realistic use case where assertions do something exceptions genuinely can't.

---

## Important: This Runs Interactively

Two of the four demonstrations deliberately trigger a failing assertion. Outside a debugger, .NET's default trace listener shows a real Windows "Assertion Failed" dialog with **Abort / Retry / Ignore** buttons. You have to click one to continue.

- **Abort** - terminates the process immediately.
- **Retry** - breaks into the debugger at the assertion line.
- **Ignore** - continues execution from where the assertion fired.

This is not a bug. It's the genuine, unmodified behavior `Debug.Assert` produces. Seeing the real dialog is more instructive than reading about it.

---

## How to Write This Program

### Mini-Program 1: Passing and Failing Side by Side

```csharp
int[] scores = [72, 88, 95, 61, 84, 91, 78, 55, 99, 73];
const int maxPossibleScore = 100;

Console.WriteLine("Checking that no score exceeds the maximum possible score...");
Debug.Assert(scores.Max() <= maxPossibleScore, "Found a score above the maximum possible score!");
Console.WriteLine("...passed silently, as expected.\n");

Console.WriteLine("Checking that the scores array has more than 10 entries (it doesn't)...");
Console.WriteLine("A real assertion dialog is about to appear. Click Ignore to continue.");
Debug.Assert(scores.Length > 10, $"Expected more than 10 scores, but found {scores.Length}.");
Console.WriteLine("...execution resumed after the assertion.\n");
```

Run it. The first assert is invisible. The second fires, shows the dialog, and when you click Ignore, execution resumes on the next line.

A passing assertion is completely invisible. A failing one stops everything. Always supply a message - "Expected more than 10 scores, but found 7" tells someone what went wrong; a bare condition tells them nothing but a line number. Include the actual value alongside the expectation, as the second assert does here with string interpolation.

Also notice: execution resumes after clicking Ignore. An assertion is not an exception. It doesn't unwind the stack. It interrupts, then the program carries on from exactly where it was.

### Mini-Program 2: Assertions vs. Exceptions - The Actual Rule

```csharp
private static decimal ApplyDiscount(decimal price, decimal discountPercentage)
{
    // Exception: validates external input that can legitimately be wrong
    if (discountPercentage < 0 || discountPercentage > 1)
        throw new ArgumentOutOfRangeException(
            nameof(discountPercentage),
            discountPercentage,
            "Discount percentage must be between 0 and 1.");

    decimal discounted = price * (1 - discountPercentage);

    // Assertion: checks an internal invariant -- if this fires, it's a bug in this method
    Debug.Assert(discounted >= 0,
        "Discounted price should never be negative given validated input.");

    return discounted;
}
```

```csharp
Console.WriteLine(ApplyDiscount(100m, 0.2m));  // succeeds: 80
Console.WriteLine(ApplyDiscount(100m, 1.5m));  // throws ArgumentOutOfRangeException
```

Run it. The first call succeeds; the second throws.

Both mechanisms in one short method, each doing something the other can't:

| | Assertion | Exception |
|---|---|---|
| Guards against | A bug in your own code | Something that can legitimately go wrong at runtime |
| Example | An impossible result given valid input | A caller passing an invalid argument |
| Compiled into Release? | No (`Debug.Assert`) | Always |
| Who fixes it? | The developer, before shipping | The caller, by handling the exception |

**The exception guards against something that can legitimately go wrong at runtime.** `discountPercentage` comes from outside this code - a caller, a form field, a config file - so it can be wrong even when every line of this program is correct. That's a runtime circumstance, and circumstances are what exceptions are for.

**The assertion guards against something that should be impossible.** Given an already-validated `discountPercentage` between 0 and 1, `discounted` cannot be negative unless the arithmetic on the line above is wrong. If this assertion ever fires, it means there's a bug in *this method*, not bad input.

**The rule: exceptions handle bad input; assertions catch broken logic.**

The consequence follows from `Debug.Assert` being compiled out of Release builds. Using an assertion to validate external input would mean that validation silently disappears in production - precisely backwards from what you want. Input validation must survive to production. Internal sanity checks needn't.

Copy the three-argument `ArgumentOutOfRangeException` form: it includes the offending value, which turns "must be between 0 and 1" into "must be between 0 and 1, but was 1.5." `nameof(discountPercentage)` rather than a string literal means a renamed parameter updates the exception automatically.

### Mini-Program 3: Debug.Assert vs. Trace.Assert

```csharp
Console.WriteLine("Debug.Assert fires only in Debug builds.");
Console.WriteLine("Trace.Assert fires in Debug AND Release builds.");
Console.WriteLine("A dialog is about to appear. Click Ignore.");
Trace.Assert(1 + 1 == 3, "Deliberately false, to show Trace.Assert firing in Release too.");
Console.WriteLine("Execution resumed.\n");
```

Run it in both Debug and Release configurations.

Both live in `System.Diagnostics` and behave identically when they fire. The difference is when they're compiled in:

| | Conditional on | Active in |
|---|---|---|
| `Debug.Assert` | `DEBUG` | Debug builds only |
| `Trace.Assert` | `TRACE` | Debug and Release (by default) |

`Debug`'s methods are decorated with `[Conditional("DEBUG")]` - the entire call disappears unless the `DEBUG` symbol is defined, which is only true in Debug builds by default. `Trace.Assert` is conditional on `TRACE`, which is defined in both configurations by default.

`Trace.Assert` is the right choice for a check you want active in a shipped Release build. `Debug.Assert` is for development-time aids - cheap enough to sprinkle liberally, since they cost nothing once compiled out.

The trap that follows from `[Conditional]`: the attribute removes the entire call site, including arguments. Anything with side effects inside an assertion disappears in Release:

```csharp
Debug.Assert(TryInitialize());       // TryInitialize never runs in Release
Debug.Assert(list.Remove(item));     // the removal never happens in Release
```

An assertion must *observe*, never *do*.

### Mini-Program 4: A Genuinely Realistic Use Case

```csharp
private static int BinarySearch(int[] sortedArray, int target)
{
    Debug.Assert(IsSorted(sortedArray),
        "BinarySearch requires a sorted array, but the input was not sorted.");

    int low = 0, high = sortedArray.Length - 1;
    while (low <= high)
    {
        int mid = low + (high - low) / 2; // avoids overflow vs (low + high) / 2
        if (sortedArray[mid] == target) return mid;
        if (sortedArray[mid] < target)  low  = mid + 1;
        else                            high = mid - 1;
    }
    return -1;
}

private static bool IsSorted(int[] array)
{
    for (int i = 1; i < array.Length; i++)
        if (array[i] < array[i - 1]) return false;
    return true;
}
```

```csharp
int[] sorted   = [1, 3, 5, 7, 9, 11, 13];
int[] unsorted = [5, 1, 9, 3, 7];

Console.WriteLine(BinarySearch(sorted, 7));    // 3
Console.WriteLine(BinarySearch(sorted, 4));    // -1
Console.WriteLine(BinarySearch(unsorted, 5));  // assertion fires
```

Run it. The first two calls succeed. The third fires the assertion because the precondition is violated.

This is the best argument in the project for why assertions exist as a separate mechanism. Binary search's correctness depends on the array already being sorted - a genuine precondition. But *verifying* that precondition with an `if`/`throw` costs O(n) on every call, which is worse than the O(log n) search it's protecting. An assertion resolves the conflict: during development and testing, `IsSorted` runs and violations are caught immediately. In Release, the call vanishes and binary search runs at full speed.

The assertion is also documentation the compiler participates in. It states the contract more precisely than a comment, and unlike a comment it will complain when someone violates it.

One incidental detail worth catching: `int mid = low + (high - low) / 2` rather than `(low + high) / 2`. The obvious version can overflow `int` when both `low` and `high` are large. This is the integer overflow from Supplemental 05 showing up in real code.

---

## Try It Yourself

Run the project and click through the two assertion dialogs (Ignore both). Then pass an unsorted array to `BinarySearch()` and predict what you'll see before running it.

Then try this: make `BinarySearch()` accept a `null` array and add an assertion that guards against it:

```csharp
Debug.Assert(sortedArray != null, "sortedArray must not be null.");
Debug.Assert(IsSorted(sortedArray), "BinarySearch requires a sorted array.");
```

Call it with `null` and observe which assertion fires, and when.

---

## Takeaways

- Assertions catch programmer errors; exceptions handle runtime circumstances.
- Validate external input with exceptions - that check must survive to production.
- Assert internal invariants - conditions that can only be false if your own code is wrong.
- Always supply a message, and include the actual value alongside the expectation.
- `Debug.Assert` is `[Conditional("DEBUG")]` - compiled out of Release.
- `Trace.Assert` is `[Conditional("TRACE")]` - active in Release too, by default.
- `[Conditional]` removes the whole call including arguments - never put side effects in an assertion.
- A failing assertion interrupts but doesn't unwind - execution resumes on Ignore.
- Assertions are ideal for preconditions too expensive to enforce at runtime.
- Use `nameof(...)` in argument exceptions, and the overload that includes the offending value.
- Compute midpoints as `low + (high - low) / 2` to avoid integer overflow.
