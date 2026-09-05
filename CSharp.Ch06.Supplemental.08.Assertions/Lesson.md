# Chapter 6 Supplemental 08: Assertions

## What This Was

Originally an empty stub — `Main()` had nothing in it but the chapter notes as a comment. It was filled in with a real, working demonstration; this document covers what was written and why.

---

## Four Demonstrations

1. **`BasicAssertions()`** — the basic `Debug.Assert(condition, message)` syntax, one passing (silent), one deliberately failing (halts and shows a real assertion dialog).
2. **`AssertionsVersusExceptions()`** — a method (`ApplyDiscount`) that validates its *public* input with an exception, but checks an *internal* invariant afterward with an assertion. Both mechanisms in one method, each doing the job the other can't.
3. **`DebugAssertVersusTraceAssert()`** — same visible behavior when they fire, very different compile-time behavior.
4. **`AssertingAnInternalInvariant()`** — a `BinarySearch()` implementation that asserts its precondition rather than re-verifying it at runtime. A genuinely realistic use case.

---

## Important: This Runs Interactively

Two of the four demonstrations deliberately trigger a **real, failing assertion**.

Outside of a debugger, .NET's default trace listener shows an actual Windows "Assertion Failed" dialog with **Abort / Retry / Ignore** buttons. You have to click one — **Ignore** is the safe choice — to let the program continue.

This is not a bug or an oversight. It's the genuine, unmodified behavior `Debug.Assert` and `Trace.Assert` produce when they fire. Seeing the real dialog is arguably more instructive than reading about it.

The three buttons, since they're worth knowing:

- **Abort** — terminates the process immediately.
- **Retry** — breaks into the debugger at the assertion, if one is attached.
- **Ignore** — continues execution as though the assertion had passed.

Under a debugger in Visual Studio, you get a break at the assertion line rather than a dialog, which is the experience the feature is really designed around.

---

## `BasicAssertions()`: Passing and Failing

```csharp
int[] scores = [...];
const int maxPossibleScore = ...;

Console.WriteLine("Checking that no score exceeds the maximum possible score...");
Debug.Assert(scores.Max() <= maxPossibleScore, "Found a score above the maximum possible score!");
Console.WriteLine("...passed silently, as expected.\n");

Console.WriteLine("Checking that the scores array has more than 10 entries (it doesn't)...");
Console.WriteLine("A real assertion dialog is about to appear, click Ignore to continue.");
Debug.Assert(scores.Length > 10, $"Expected more than 10 scores, but found {scores.Length}.");
Console.WriteLine("...execution resumed after the assertion.\n");
```

The pairing is the point. A passing assertion is completely invisible — no output, no cost, nothing. A failing one stops everything.

Both forms of the API are available:

```csharp
Debug.Assert(condition);
Debug.Assert(condition, message);
```

Always supply the message. When an assertion fires six months later on someone else's machine, `Expected more than 10 scores, but found 7` tells them what went wrong; a bare condition tells them nothing but a line number. Note the second assert uses interpolation to include the *actual* value — the expected and the observed together are what make a failure diagnosable.

Also worth noticing: execution *resumes* after clicking Ignore. An assertion is not an exception. It doesn't unwind the stack or transfer control; it interrupts, and then the program carries on from exactly where it was.

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

Both mechanisms in one short method, each doing something the other can't.

**The exception guards against something that can legitimately go wrong at runtime.** `discountPercentage` comes from outside this code — a caller, a form field, a config file — so it can be wrong even when every line of this program is correct. That's a runtime circumstance, and circumstances are what exceptions are for.

**The assertion guards against something that should be *impossible*.** Given an already-validated `discountPercentage` between 0 and 1, `discounted` cannot be negative unless the arithmetic on the line above is wrong. If this assertion ever fires, it means there's a bug in *this method*, not bad input.

That distinction is the entire rule: **exceptions handle bad input, assertions catch broken logic.**

The consequence follows directly from `Debug.Assert` being compiled out of Release builds. Using an assertion to validate external input would mean that validation silently disappears in production — precisely backwards from what you want. Input validation must survive to production; internal sanity checks needn't.

Two further details worth copying from this method:

**`nameof(discountPercentage)`** rather than the string `"discountPercentage"`. Renaming the parameter updates the exception automatically, and a typo becomes a compile error rather than a misleading message.

**The three-argument `ArgumentOutOfRangeException` overload**, which includes the offending value. "Discount percentage must be between 0 and 1" is useful; knowing it was `1.5` is more useful.

---

## `Debug.Assert` vs. `Trace.Assert`

```csharp
Console.WriteLine("Debug.Assert(false, ...) only fires in a Debug build, [Conditional(\"DEBUG\")]");
Console.WriteLine("  compiles the call out entirely in Release, the check itself never runs there.");
Console.WriteLine();
Console.WriteLine("Trace.Assert(false, ...) fires whenever TRACE is defined, Debug AND Release alike.");
Console.WriteLine("A real assertion dialog is about to appear, click Ignore to continue.");
Trace.Assert(1 + 1 == 3, "Deliberately false, to demonstrate Trace.Assert firing regardless of build configuration.");
Console.WriteLine("...execution resumed after the Trace.Assert.\n");
```

Both live in `System.Diagnostics` and behave identically when they fire. The difference is entirely about *when they're even compiled in*:

| | Conditional on | Active in |
|---|---|---|
| `Debug.Assert` | `DEBUG` | Debug builds only |
| `Trace.Assert` | `TRACE` | Debug **and** Release, by default |

`Debug`'s methods are decorated with `[Conditional("DEBUG")]`, so the call is compiled out entirely unless the `DEBUG` symbol is defined — true only in Debug builds by default. `Trace.Assert` is conditional on `TRACE` instead, which is defined by default in both configurations in this solution and in most .NET project templates generally.

That makes `Trace.Assert` the right choice for a check you want active in a shipped Release build, and `Debug.Assert` the right choice for development-time aids — cheap enough to sprinkle liberally, since they cost nothing once compiled out.

### The trap that follows from `[Conditional]`

The attribute removes the entire **call site**, arguments included. So anything with side effects inside an assertion disappears in Release:

```csharp
Debug.Assert(TryInitialize());          // never runs in Release
Debug.Assert(list.Remove(item));        // the removal never happens in Release
Debug.Assert(++attempts < maxRetries);  // attempts never increments in Release
```

Each of these produces a program that behaves differently between configurations — the hardest class of bug to diagnose, because it can't be reproduced in the debugger. An assertion must *observe*, never *do*.

Note that `Debug.Assert(1 + 1 == 3)` would also draw a compiler warning for a constant condition; using `Trace.Assert` here sidesteps that while making the configuration point directly.

---

## A Genuinely Realistic Use Case: `BinarySearch()`

```csharp
private static int BinarySearch(int[] sortedArray, int target)
{
	Debug.Assert(IsSorted(sortedArray), "BinarySearch requires a sorted array, but the input was not sorted.");

	int low = 0;
	int high = sortedArray.Length - 1;

	while (low <= high)
	{
		int mid = low + (high - low) / 2;
		if (sortedArray[mid] == target) return mid;
		if (sortedArray[mid] < target) low = mid + 1;
		else high = mid - 1;
	}

	return -1;
}
```

```csharp
private static bool IsSorted(int[] array)
{
	for (int i = 1; i < array.Length; i++)
	{
		if (array[i] < array[i - 1]) return false;
	}
	return true;
}
```

This is the best argument in the project for why assertions exist as a separate mechanism.

Binary search's entire value is being O(log n). Its correctness depends on the array already being sorted — a genuine precondition. But *verifying* that precondition with an `if`/`throw` costs O(n) on every call, which is worse than the O(log n) search it's protecting. Enforcing the requirement would cost more than the operation itself.

An assertion resolves the conflict. During development and testing, `IsSorted` runs and a violation is caught immediately with a clear message. In Release, the call vanishes and binary search runs at full speed. By the time you ship, the invariant should be well-established by the tests that ran with assertions enabled.

The assertion is also documentation the compiler participates in. `Debug.Assert(IsSorted(sortedArray), ...)` states the contract more precisely than a comment, and unlike a comment it will actually complain when someone violates it.

One incidental detail worth catching: `int mid = low + (high - low) / 2` rather than `(low + high) / 2`. The obvious version can overflow `int` when `low` and `high` are both large — a famous bug that sat undetected in the JDK's binary search for nearly a decade. This is the integer overflow from Supplemental 05 showing up in real code.

---

## Takeaways

- Assertions catch programmer errors; exceptions handle runtime circumstances.
- Validate external input with exceptions — that check must survive to production.
- Assert internal invariants — conditions that can only be false if your own code is wrong.
- Always supply a message, and include the actual value alongside the expectation.
- `Debug.Assert` is `[Conditional("DEBUG")]` — compiled out of Release builds.
- `Trace.Assert` is `[Conditional("TRACE")]` — active in Release too, by default.
- `[Conditional]` removes the whole call including its arguments; never put side effects in an assertion.
- A failing assertion interrupts but does not unwind — execution resumes on Ignore.
- Assertions are ideal for preconditions too expensive to enforce, like "this array is sorted."
- Use `nameof(...)` in argument exceptions, and the overload that reports the offending value.
- Compute a midpoint as `low + (high - low) / 2` to avoid integer overflow.
