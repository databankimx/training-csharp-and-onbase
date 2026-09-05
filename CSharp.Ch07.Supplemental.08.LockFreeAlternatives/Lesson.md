# Chapter 7 Supplemental 08: Lock-Free Alternatives

## What This Is

New content — not ported from an existing download. It was added to fill a gap identified by comparing this chapter against the full textbook topic outline: the textbook covers `Interlocked` under "Lock-Free Alternatives" as its own subsection right after locking, but nothing in the existing chapter content touched it directly. It appeared only as an implementation detail buried inside `Supplemental.05.RaceConditions`'s `CountdownEvent` fix.

This project gives it a proper, dedicated treatment: the same race-condition-vs-fixed comparison used throughout the chapter, then a tour of `Interlocked`'s other methods.

---

## Why Lock-Free At All

From the Chapter Notes:

```
Locking (Monitor, Mutex, Semaphore - see CSharp.Ch07.Supplemental.07.Locking) is both
  dangerous (deadlocks, if used carelessly) and resource-intensive. Sometimes you just need
  to perform one simple operation (like incrementing a number) and make sure it happens
  atomically, without a full lock's overhead.

For that, .NET offers the Interlocked class (System.Threading), a set of static methods
  that perform simple operations as a single, uninterruptible (atomic) step, no scheduler
  context switch can happen in the middle of one.
```

That last clause is the crux, and it's worth being precise about the mechanism. `Interlocked` isn't a faster lock — it's **not a lock at all**. These methods compile down to single CPU instructions (`lock xadd`, `lock cmpxchg` on x86) that the processor guarantees are indivisible. There's no acquire, no release, no queue of waiting threads, and no way to forget a `finally`.

The consequences follow directly:

- **No deadlock is possible.** You can't deadlock on an operation that never waits.
- **No `try`/`finally` needed.** There's nothing to release, so the `Supplemental.07` hazard of a leaked lock simply doesn't exist.
- **No kernel transition.** Uncontended, this is dramatically cheaper than even a `Monitor`.

### The Method Set

```
- Increment(ref int/long)                 Adds 1, returns the new value
- Decrement(ref int/long)                 Subtracts 1, returns the new value
- Add(ref int/long, value)                Adds "value", returns the new value
- Exchange(ref T, value)                  Sets the variable to "value", returns the OLD value
- CompareExchange(ref T, value, comparand) If the variable currently equals "comparand",
										   sets it to "value". Always returns the ORIGINAL
										   value, regardless of whether the swap happened.
- Read(ref long)                          Atomically reads a 64-bit value (only actually
										   necessary on 32-bit platforms, where a plain read
										   of a 64-bit value isn't guaranteed atomic)
```

Note the inconsistency worth memorizing, because mixing them up is a real bug source: **`Increment`, `Decrement`, and `Add` return the NEW value. `Exchange` and `CompareExchange` return the OLD one.**

`Read(ref long)` deserves a moment. On a 32-bit platform, reading a 64-bit value takes two instructions, so another thread can write between them and you get half of the old value and half of the new — a number that was never actually stored. This is the phenomenon called *word tearing*. It's a non-issue on 64-bit runtimes, which is nearly everywhere today, but it's a good reminder that even a plain *read* isn't automatically atomic.

### When to Reach for Which

```
- Interlocked: a single, simple update to one variable (a counter, a flag, a reference swap)
- Monitor/lock: anything more involved, multiple related fields that need to stay consistent
  together, or a critical section spanning more than one operation
```

The boundary is sharp and worth stating plainly: **`Interlocked` protects one variable, one operation.** The moment you need two fields to change together — a balance and a transaction log, a count and an array slot — `Interlocked` can't help. Two atomic operations in sequence are not one atomic operation, and another thread can observe the state between them.

That's the trap. Code using `Interlocked` everywhere *looks* thoroughly synchronized while providing no consistency guarantee across variables at all.

---

## The Core Comparison: Unprotected vs. `Interlocked.Increment`

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
```

100 threads, each incrementing a shared counter 1,000 times. Expected 100,000.

The unprotected version reliably comes out wrong — the same read-add-write race `Supplemental.05` describes. The protected version is always exactly 100,000, with no lock anywhere in sight.

### Note the Different Demonstration Strategy

`Supplemental.05` made its race visible by **widening the window** — a `Thread.Sleep(100)` wedged between the read and the write, so the bug fired every time with only two threads.

This project takes the opposite approach: no sleep at all, but **100,000 chances** for a nanosecond-wide window to be hit. Volume instead of duration.

Both are legitimate, and having seen both is genuinely useful. The second is closer to how real races behave — no artificial help, just enough iterations that a low-probability event becomes a certainty. It's also why the unprotected result is *unpredictably* wrong (some number below 100,000, different every run) rather than consistently wrong: you're watching an accumulation of independent lost updates.

Note the loss is usually small relative to the total — perhaps 99,000-something out of 100,000. **That's what makes this bug class so dangerous in production.** A number that's off by 1% looks plausible. Nobody investigates a counter that seems roughly right.

### The Harness

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

Worth noticing: this harness is **correct**, and it's the first place in the chapter where the "spawn work then sleep and hope" cheat has been replaced with a real wait. The thread references are retained in an array and every one is `Join`ed. That's why this project needs no `Nap()` calls in `Main()`.

Note also that passing the operation in as an `Action` means the two runs share identical threading code — the only variable is the increment itself. That's the same isolation discipline `Supplemental.02` used by calling the same `Nap()` from both buttons.

One subtlety: `Interlocked.Increment(ref protectedCounter)` requires a `ref` to a local captured by the lambda. That works because the compiler hoists the captured local into a heap-allocated closure object, giving it a stable address. `ref` to a field of that object is fine.

---

## `Add` and `Decrement`: The Same Idea, More Operations

```csharp
Console.WriteLine("Adding 10, 20, and 30 from three different threads using Interlocked.Add...");
int total = 0;
Parallel.Invoke(
	() => Interlocked.Add(ref total, 10),
	() => Interlocked.Add(ref total, 20),
	() => Interlocked.Add(ref total, 30));
Console.WriteLine($"Total (should always be 60): {total}");
```

```csharp
int remaining = 100;
Parallel.Invoke(
	() => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
	() => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
	() => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); },
	() => { for (int i = 0; i < 25; i++) Interlocked.Decrement(ref remaining); });
Console.WriteLine($"Remaining (should always be 0): {remaining}");
```

`Interlocked.Add` generalizes `Increment` to any value, still as a single atomic step. `Decrement` is the mirror of `Increment`. Both exist because some operations are common and simple enough that a dedicated atomic version beats reaching for a lock.

Note the `Parallel.Invoke` usage ties back to `Supplemental.03` — and note that `Parallel.Invoke` waits for all delegates, so no additional synchronization is needed to read `total` afterward. Completion tracking and atomicity, handled separately, exactly as `Supplemental.05` framed it.

The `Decrement` example is the countdown-to-zero shape you'd use for "how many workers are still running" — and is, in effect, a hand-rolled `CountdownEvent` without the blocking `Wait()`.

---

## `Exchange`: Atomic Swap, With the Old Value Returned

```csharp
string currentLeader = "Nobody";

string previousLeader = Interlocked.Exchange(ref currentLeader, "Alice");
Console.WriteLine($"Leader was '{previousLeader}', now '{currentLeader}'");

previousLeader = Interlocked.Exchange(ref currentLeader, "Bob");
Console.WriteLine($"Leader was '{previousLeader}', now '{currentLeader}'");
```

`Exchange` sets a variable to a new value and hands back whatever was there **before** the swap, in one atomic step.

Worth noticing why that matters: without `Interlocked`, "read the old value, then write the new one" is two separate operations, and another thread could sneak in between them. Two threads could both read `"Nobody"`, both write their own name, and both believe they were the one who replaced `"Nobody"`. `Exchange` closes that gap — exactly one caller can receive any given prior value.

Note this works on **reference types** (`string` here) as well as numerics, via a generic `Exchange<T>` overload. That makes it the tool for atomically swapping in a whole new object — a freshly-loaded configuration, a rebuilt cache — where readers see either the complete old object or the complete new one, never a half-updated state.

A common real use is idempotent disposal:

```csharp
var toDispose = Interlocked.Exchange(ref _resource, null);
toDispose?.Dispose();
```

Only the thread that actually received the non-null value disposes it, no matter how many threads race. No lock required.

---

## `CompareExchange`: The Building Block Behind Everything Else

```csharp
int flag = 0;

int originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
bool weSetIt = originalValue == 0;
Console.WriteLine($"First attempt: flag was {originalValue} before, is {flag} now. We set it: {weSetIt}");

// Try again: flag is now 1, so this attempt (which also expects 0) will NOT change it.
originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
weSetIt = originalValue == 0;
Console.WriteLine($"Second attempt: flag was {originalValue} before, is {flag} now. We set it: {weSetIt}");
```

`CompareExchange(ref location, newValue, comparand)` reads as: **"if `location` currently equals `comparand`, set it to `newValue`."**

It **always** returns the value `location` held right before the call, regardless of whether the swap happened. Comparing that returned value against what you expected tells you whether your specific update won.

Note the argument order is `(location, newValue, comparand)` — the value you're *setting* comes before the value you're *comparing against*. That reads backwards from the English description and is a genuinely common source of mistakes. Getting it wrong compiles fine and silently never swaps.

The demo makes the semantics concrete: the first attempt succeeds (flag was 0, becomes 1, returns 0). The second fails (flag is 1, stays 1, returns 1). Both calls return the prior value; only the return value distinguishes success from failure.

### Why This Is the Foundation

This is worth sitting with, because it's genuinely what most real lock-free algorithms are built on. The pattern:

1. Read the current value.
2. Compute what you want the new value to be.
3. `CompareExchange` it in, using the value you read as the comparand.
4. If the returned "before" value doesn't match what you read, **something else changed it in the meantime** — so retry the whole cycle.

That retry loop is the essence of lock-free programming. Instead of preventing others from interfering (a lock), you detect that they did and try again. Written out:

```csharp
int current, updated;
do
{
	current = counter;
	updated = current + 1;
} while (Interlocked.CompareExchange(ref counter, updated, current) != current);
```

`Interlocked.Increment` is exactly this, done for you, for the specific case of "add 1."

The technique is called **optimistic concurrency** — assume no conflict, verify afterward, retry if wrong. Note it's the same idea as row versioning in a database: no locks held, just a check that nothing changed underneath you.

Its trade-off is worth naming: under heavy contention, threads can spend more time retrying than doing work, and a lock may actually be faster. Lock-free means *no thread can block another*, not *always faster*.

---

## Worth Knowing: The `lock` Keyword Is Shorthand

The textbook places this section directly after introducing `lock` as shorthand for `Monitor.Enter`/`Exit` in a `try`/`finally`:

```csharp
object syncObject = new object();
lock (syncObject)
{
	// Code updating some shared data
}
```

`Supplemental.07.Locking` demonstrates the explicit form. `lock (syncObject) { ... }` is exactly equivalent, just shorter.

Comparing the two is really the whole lesson of this project:

| | `lock` / `Monitor` | `Interlocked` |
|---|---|---|
| Scope | Any critical section, any complexity | One variable, one operation |
| Multiple fields consistently | Yes | **No** |
| Deadlock possible | Yes | No |
| Requires `try`/`finally` | Yes | No |
| Blocks other threads | Yes | No |
| Cost | Higher | Single CPU instruction |

**Use `Interlocked` when it fits, and `lock` the moment it doesn't.** The failure mode of forcing `Interlocked` into a multi-variable problem is silent inconsistency — much harder to find than the deadlock you'd risk with a lock.

---

## Try It Yourself

- Run the first comparison several times; note the unprotected result is a *different* wrong number each run.
- Drop `incrementsPerThread` to 10 and watch the race become intermittent — sometimes correct. This is why volume matters for reproducing real races.
- Replace `unprotectedCounter++` with a `lock`ed increment and compare elapsed time against the `Interlocked` version at high thread counts.
- Implement the `CompareExchange` retry loop above and confirm it produces exactly 100,000.
- Swap `CompareExchange`'s second and third arguments and watch it silently stop working.
- Try to use `Interlocked` to keep two counters in sync (e.g. always `a == b`) and observe that you can't.

---

## Takeaways

- `Interlocked` is not a lock — it's a single atomic CPU instruction.
- No lock means no deadlock, no `try`/`finally`, and no kernel transition.
- `Increment`/`Decrement`/`Add` return the new value; `Exchange`/`CompareExchange` return the old one.
- Plain reads of 64-bit values aren't atomic on 32-bit platforms; that's what `Read` is for.
- `Interlocked` protects one variable and one operation — never a multi-field invariant.
- Two atomic operations in sequence are not one atomic operation.
- Races can be demonstrated by widening the window or by sheer volume; production only offers the latter.
- Lost updates usually produce a plausible-looking number, which is why they go uninvestigated.
- Retaining thread handles and calling `Join` beats sleeping and hoping.
- `Exchange` on reference types swaps whole objects atomically — ideal for config or cache replacement.
- `CompareExchange(ref location, newValue, comparand)` sets only if the current value matches, and always returns the original.
- Its argument order puts the new value before the comparand, which reads backwards.
- Read, compute, `CompareExchange`, retry on mismatch is the basis of optimistic concurrency.
- Lock-free guarantees no thread blocks another; it does not guarantee better performance under contention.
