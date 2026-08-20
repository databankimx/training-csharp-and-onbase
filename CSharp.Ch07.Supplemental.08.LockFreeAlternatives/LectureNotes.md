# Chapter 7 Supplemental 08: Lock-Free Alternatives

## What This Is

New content (not ported from an existing download), added to fill a gap identified by comparing this chapter against the full textbook topic outline: the textbook covers `Interlocked` under "Lock-Free Alternatives" (its own subsection, right after locking), but nothing in the existing chapter content touched on it directly, only as an implementation detail buried inside `Supplemental.05.RaceConditions`'s `CountdownEvent` fix. This project gives it a proper, dedicated treatment: the same race-condition-vs-fixed comparison used throughout this chapter, then a tour of `Interlocked`'s other methods.

---

## The Core Comparison: Unprotected vs. `Interlocked.Increment`

```csharp
int unprotectedCounter = 0;
RunManyIncrementingThreads(100, 1000, () => unprotectedCounter++);
// Expected: 100,000. Actual: consistently less.

int protectedCounter = 0;
RunManyIncrementingThreads(100, 1000, () => Interlocked.Increment(ref protectedCounter));
// Expected: 100,000. Actual: always exactly 100,000.
```

100 threads, each incrementing a shared counter 1,000 times, no lock, no `Interlocked`, just `counter++`. Reliably comes out wrong, the same read-add-write race `Supplemental.05.RaceConditions` and this chapter's own `Chapter Notes` describe. Swap in `Interlocked.Increment(ref protectedCounter)`, same 100 threads, same 1,000 iterations each, and the result is always exactly correct, with no lock anywhere in sight. This is the entire pitch for `Interlocked`: correctness without the overhead (or deadlock risk) of `Monitor`/`lock`.

---

## `Add` and `Decrement`: The Same Idea, More Operations

```csharp
Parallel.Invoke(
    () => Interlocked.Add(ref total, 10),
    () => Interlocked.Add(ref total, 20),
    () => Interlocked.Add(ref total, 30));
```

`Interlocked.Add` generalizes `Increment` to add any value, not just 1, still as a single atomic step. `Interlocked.Decrement` is the mirror image of `Increment`. Both exist for the same reason: some operations are common enough (and simple enough) that a dedicated atomic version is worth having, rather than reaching for a lock every time.

---

## `Exchange`: Atomic Swap, With the Old Value Returned

```csharp
string previousLeader = Interlocked.Exchange(ref currentLeader, "Alice");
```

`Exchange` sets a variable to a new value and hands back whatever was there *before* the swap, in one atomic step. Worth noticing why that matters: without `Interlocked`, "read the old value, then write the new one" is two separate operations, and another thread could sneak in between them. `Exchange` closes that gap entirely.

---

## `CompareExchange`: The Building Block Behind Everything Else

```csharp
int originalValue = Interlocked.CompareExchange(ref flag, 1, 0);
bool weSetIt = originalValue == 0;
```

`CompareExchange(ref location, newValue, comparand)` reads as: "if `location` currently equals `comparand`, set it to `newValue`." It **always** returns the value `location` held right before the call, regardless of whether the swap actually happened, comparing that returned value against what you expected tells you whether your specific update won.

This is worth sitting with, since it's genuinely the foundation most real lock-free algorithms are built on: read a value, compute what you want the new value to be, then `CompareExchange` it in. If the returned "before" value doesn't match what you originally read, something else changed it in the meantime, so you retry the whole read-compute-swap cycle. `Interlocked.Increment` is really just this exact pattern, done for you, for the specific case of "add 1."

---

## Worth Knowing: The `lock` Keyword Is Shorthand

The textbook's own text places this section directly after introducing `lock` as shorthand for `Monitor.Enter`/`Exit` wrapped in `try`/`finally`:

```csharp
object syncObject = new object();
lock (syncObject)
{
    // Code updating some shared data
}
```

`CSharp.Ch07.Supplemental.07.Locking` demonstrates the explicit `Monitor.Enter()`/`Monitor.Exit()` form (with its own `try`/`finally`), worth knowing that `lock (syncObject) { ... }` is exactly equivalent, just shorter to write. Comparing this against `Interlocked` is really the whole lesson of this project: `lock` is the general-purpose tool (any critical section, any complexity), `Interlocked` is the specialized, lighter-weight tool for the single-operation case.
