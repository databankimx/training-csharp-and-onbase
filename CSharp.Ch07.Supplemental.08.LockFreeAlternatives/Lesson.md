# Chapter 7 Supplemental 08: Lock-Free Alternatives

## What This Is

Locks are the right tool when a critical section spans multiple operations or multiple fields that need to stay consistent together. For simpler cases -- incrementing a counter, swapping a reference, doing a conditional update on one variable -- there's a cheaper option with no lock, no `try`/`finally`, and no possible deadlock: the `Interlocked` class. It was added here because it deserves more than a passing mention buried in `Supplemental.05`'s bug fix.

---

## How to Write This Program

Add the thread harness helper to `Program.cs` -- multiple mini-programs use it:

```csharp
private static void RunManyIncrementingThreads(int threadCount, int incrementsPerThread, Action incrementAction)
{
    var threads = new Thread[threadCount];

    for (int i = 0; i < threadCount; i++)
    {
        threads[i] = new Thread(() =>
        {
            for (int j = 0; j < incrementsPerThread; j++) incrementAction();
        });
        threads[i].Start();
    }

    foreach (var thread in threads) thread.Join();
}
```

Note this harness uses `Join` -- no guessed sleep durations, no "cheat resynchronization." Thread handles are retained in an array and every one is joined before the caller reads any results. This is how you actually wait for threads.

---

### Mini-Program 1: Unprotected vs. Interlocked.Increment

Clear `Main()` and write:

```csharp
const int threadCount = 100;
const int incrementsPerThread = 1000;

Console.WriteLine("Unprotected counter (expect this to come out wrong)...");
int unprotectedCounter = 0;
RunManyIncrementingThreads(threadCount, incrementsPerThread, () => unprotectedCounter++);
Console.WriteLine($"Expected: {threadCount * incrementsPerThread}, Actual: {unprotectedCounter}");

Console.WriteLine($"{Environment.NewLine}Interlocked-protected counter (expect this to always be correct)...");
int protectedCounter = 0;
RunManyIncrementingThreads(threadCount, incrementsPerThread, () => Interlocked.Increment(ref protectedCounter));
Console.WriteLine($"Expected: {threadCount * incrementsPerThread}, Actual: {protectedCounter}");

GenericFunctions.Pause();
```

Run it several times. The unprotected counter comes out wrong -- some number below 100,000, different every run. The protected counter is exactly 100,000, every time, with no lock anywhere.

`Supplemental.05` made its race visible by widening the race window to 100ms so two threads collided every time. This project takes the opposite approach: no artificial delay, but 100,000 operations across 100 threads. Volume instead of duration. A nanosecond-wide window hit once in a hundred tries becomes a certainty across a hundred thousand.

The wrong result is usually plausibly close to correct -- 99,200-something rather than, say, 50,000. **That's what makes this bug class dangerous in production.** A counter that's off by less than 1% looks like noise. Nobody investigates a metric that seems roughly right. It just quietly accumulates errors until someone notices the inventory doesn't match the orders, and by then the trail is cold.

`Interlocked.Increment` performs read-add-write as a single CPU instruction. There's no window for the scheduler to interrupt between steps, no acquire, no release, no queue of waiting threads. It is not a faster lock -- it is not a lock at all.

### Mini-Program 2: Add and Decrement

Clear `Main()` and write:

```csharp
int total = 0;

Console.WriteLine("Adding 10, 20, and 30 from three threads using Interlocked.Add...");
Parallel.Invoke(
    () => Interlocked.Add(ref total, 10),
    () => Interlocked.Add(ref total, 20),
    () => Interlocked.Add(ref total, 30));
Console.WriteLine($"Total (should always be 60): {total}");

Console.WriteLine($"{Environment.NewLine}Decrementing from four threads using Interlocked.Decrement...");
int remaining = 100;
Parallel.Invoke(
    () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
    () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
    () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
    () => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); });
Console.WriteLine($"Remaining (should always be 0): {remaining}");

GenericFunctions.Pause();
```

Run it. Both results are always correct.

`Interlocked.Add` generalizes `Increment` to any value. `Decrement` is the mirror of `Increment`. Both return the **new** value -- note this is different from `Exchange` and `CompareExchange` below, which return the **old** value. Getting that mixed up is a genuine source of bugs and the kind of thing that doesn't show up until you're reading the wrong number in a log and wondering where it came from.

`Parallel.Invoke` waits for all delegates to finish before returning, so reading `total` and `remaining` afterward is safe -- `Interlocked` handles the per-operation atomicity, `Parallel.Invoke` handles the completion timing.

### Mini-Program 3: Exchange

Clear `Main()` and write:

```csharp
string currentLeader = "Nobody";

string previousLeader = Interlocked.Exchange(ref currentLeader, "Alice");
Console.WriteLine($"Leader was '{previousLeader}', now '{currentLeader}'");

previousLeader = Interlocked.Exchange(ref currentLeader, "Bob");
Console.WriteLine($"Leader was '{previousLeader}', now '{currentLeader}'");

GenericFunctions.Pause();
```

Run it. Nobody -> Alice -> Bob, with each displaced leader returned.

`Exchange` sets a variable to a new value and returns what was there before, as a single atomic operation. Without it, "read the old value, then write the new one" is two separate operations and another thread can sneak in between them. Two threads could both read `"Nobody"`, both write their name, and both believe they were the one who displaced `"Nobody"`. `Exchange` closes that window.

The generic overload works on reference types, not just numerics. That makes it the tool for atomically replacing a whole object -- a freshly loaded configuration, a rebuilt lookup table -- where readers see either the complete old object or the complete new one, never a half-updated state.

A common idempotent disposal pattern:

```csharp
var toDispose = Interlocked.Exchange(ref _resource, null);
toDispose?.Dispose();
```

Exactly one thread receives the non-null value and disposes it, no matter how many race to do so. No lock required.

### Mini-Program 4: CompareExchange

Clear `Main()` and write:

```csharp
int flag = 0;

// "If flag is currently 0, set it to 1."
// Always returns the original value -- compare it against what you expected
// to find out whether your swap actually happened.
int originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
bool weSetIt = originalValue == 0;
Console.WriteLine($"First attempt: flag was {originalValue} before, is {flag} now. We set it: {weSetIt}");

// flag is now 1 -- this attempt expects 0, so it will NOT swap.
originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
weSetIt = originalValue == 0;
Console.WriteLine($"Second attempt: flag was {originalValue} before, is {flag} now. We set it: {weSetIt}");

GenericFunctions.Pause();
```

Run it. First attempt succeeds (flag was 0, becomes 1, returns 0). Second attempt finds flag is already 1, does nothing, returns 1.

`CompareExchange(ref location, newValue, comparand)` reads as: "if `location` currently equals `comparand`, set it to `newValue`." It **always** returns the original value, whether or not the swap happened. Comparing the return value against your expected value tells you whether your specific update won.

Mind the argument order: `(location, newValue, comparand)`. The value you're **setting** comes before the value you're **comparing against**. That reads backwards from the English description and is a genuine, compile-silently source of mistakes. Getting it backwards means `CompareExchange` never swaps -- the program still runs, just produces wrong results while looking perfectly correct.

This is the primitive that most real lock-free algorithms are built on. The retry loop:

```csharp
int current, updated;
do
{
    current = counter;
    updated = current + 1;
} while (Interlocked.CompareExchange(ref counter, updated, current) != current);
```

Read, compute, attempt to swap in. If something else changed the value in the meantime, try again. `Interlocked.Increment` is this loop, done for you, for the specific case of "add 1." Lock-free means *no thread blocks another* -- not that it's always faster, since under heavy contention threads can spend more time retrying than working.

---

## Worth Knowing: `lock` Is Shorthand for `Monitor.Enter`/`Exit`

`Supplemental.07.Locking` demonstrates the explicit `Monitor.Enter()`/`try`/`finally`/`Exit()` form. The `lock` keyword compiles to exactly that pattern:

```csharp
lock (syncObject)
{
    // critical section
}
```

`lock` is the general-purpose tool: any critical section, any complexity, any number of shared variables that need to stay consistent together. `Interlocked` is the specialized, lighter-weight option for the single-operation case -- no lock acquired, no possible deadlock, no `try`/`finally` required. Use `Interlocked` when it fits; reach for `lock` when it doesn't.

---

## Worth Knowing: Interlocked Protects One Variable, One Operation

Two `Interlocked` calls in sequence are not atomic together. Another thread can observe state between them.

Once you need two fields to change together -- a balance and a transaction log, a count and the array slot it indexes -- reach for `lock`. `Interlocked` protects one variable per call, full stop. Attempting to build a multi-variable invariant out of chained `Interlocked` calls is the kind of thing that looks elegant and fails in ways that take days to reproduce.

---

## Takeaways

- `Interlocked` is not a lock -- it's a single atomic CPU instruction.
- No lock means no deadlock, no `try`/`finally`, no kernel transition.
- `Increment`, `Decrement`, and `Add` return the **new** value. `Exchange` and `CompareExchange` return the **old** one.
- Plain reads of 64-bit values aren't atomic on 32-bit platforms. That's what `Read` is for.
- `Interlocked` protects one variable per call. Two calls in sequence are not atomic together.
- Lost updates produce plausible-looking numbers -- that's why they go uninvestigated in production.
- Retaining thread handles and calling `Join` beats sleeping and hoping.
- `Exchange` on reference types swaps whole objects atomically -- useful for config and cache replacement.
- `CompareExchange` argument order is `(location, newValue, comparand)` -- new value before comparand. It reads backwards.
- Read, compute, `CompareExchange`, retry on mismatch is the basis of optimistic concurrency.
- Lock-free means no thread blocks another. It does not mean always faster under contention.
