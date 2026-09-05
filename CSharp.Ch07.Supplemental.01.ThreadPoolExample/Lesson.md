# Chapter 7 Supplemental 01: Thread Pool Example

## What This Is

A more thorough thread pool demonstration than the main lesson's. Five `ThreadTracker` objects, each with a randomized sleep time simulating variable-length work, run first in the thread pool (all in parallel), then sequentially — so you can directly compare total elapsed time both ways on the **same** randomly-generated data.

That last part is the design choice that makes this project work. The trackers are created once in `CreateThreads()` and reused by both runs, so the comparison isn't muddied by different random values on each pass.

---

## The Shape of the Program

`Main()` is three steps with a pause between each:

```csharp
Console.WriteLine("Creating list of thread trackers...");
CreateThreads();
GenericFunctions.Pause();

Console.WriteLine("Running operations in thread pool...");
RunThreaded();
GenericFunctions.Pause();

Console.WriteLine("Running operations sequentially...");
RunSequential();
GenericFunctions.Pause();
```

Note the ordering: **threaded first, sequential second.** That's deliberate. You see the fast number first, then watch the slow one accumulate in real time. Reversed, the threaded run would feel anticlimactic — it'd be over before you registered it started.

### The Tracker

```csharp
var thread = new ThreadTracker
{
	Id = i,
	Handle = new EventWaitHandle(false, EventResetMode.AutoReset),
	SleepTime = Rand.Next(1, MaxSleep)
};
```

`ThreadTracker` bundles the three things you need to manage a pooled work item: an identity for logging, a signal to wait on, and the work parameter. This is a useful pattern in itself — since `QueueUserWorkItem` gives you only one `object state` slot, packaging everything into a single object is how you get around that limitation cleanly.

Note each tracker has its **own** `EventWaitHandle`. One shared handle would not work: with `AutoReset`, the first `Set()` would release exactly one waiter, and the remaining four would block forever.

---

## Watch the Numbers, Not Just the Code

### Threaded

```csharp
ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);

sw = Stopwatch.StartNew();

foreach (var thread in Threads)
{
	ThreadPool.QueueUserWorkItem(x => { Nap(thread); });
}

// ...in the finally block:
foreach (var thread in Threads)
{
	thread.Handle.WaitOne();
	Console.WriteLine($"End thread {thread.Id}");
}

Console.WriteLine($"Total run-time: {(double)sw.ElapsedMilliseconds / 1000} seconds...");
```

All five sleep concurrently, so the total lands close to whichever thread drew the **longest** sleep time.

### Sequential

```csharp
sw = Stopwatch.StartNew();

foreach (var thread in Threads)
{
	Nap(thread);
}
```

Each waits for the previous to finish, so the total lands close to the **sum** of all five sleep times.

The difference between those two numbers, on identical data, is the entire value proposition of a thread pool for genuinely independent, parallelizable work — made concrete rather than asserted.

---

## Worth Noticing: `SetMinThreads` Before Queuing Work

```csharp
ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);
```

The .NET thread pool doesn't necessarily create new threads immediately when work is queued. By default it ramps up gradually, spinning up additional worker threads over time only as demand persists — roughly one new thread per 500ms once the pool is saturated. That heuristic exists because most pooled work is short, and thrashing thread creation for brief tasks would cost more than it saves.

For a small, short demo like this one, that gradual ramp-up could mean the five work items don't all actually start in parallel right away, undermining the comparison the project is trying to make. `SetMinThreads` tells the pool to keep at least this many threads ready immediately, so the threaded numbers reflect genuine parallelism rather than an artifact of the pool's own warm-up behavior.

Watch the `Starting thread {Id}...` lines when you run it. They should all appear essentially at once. Without `SetMinThreads`, they'd trickle out.

---

## Worth Noticing: Waiting in a `finally` Block

```csharp
finally
{
	foreach (var thread in Threads)
	{
		thread.Handle.WaitOne();
		Console.WriteLine($"End thread {thread.Id}");
	}

	Console.WriteLine($"Total run-time: ...");
}
```

Putting the waits in `finally` rather than after the queuing loop means they run even if queuing threw partway through. That matters more than it looks: work items already queued are running on **background** threads, and abandoning them without waiting means they'd be torn down mid-execution when the process exits, or worse, keep writing to the console while the exception handler is trying to report the failure.

The general principle: if you've started concurrent work, waiting for it is cleanup, and cleanup belongs in `finally`.

Note also that the waits happen in **tracker order**, not completion order. Thread 3 might finish first, but its `End thread 3` line won't print until threads 1 and 2 have been waited on. If a handle is already signaled when you reach it, `WaitOne()` returns immediately — so this costs nothing in time, it just reorders the output. The `Thread {Id} waited {n} seconds...` lines from inside `Nap()` *do* appear in true completion order, so you can see both orderings in the same run.

---

## Worth Knowing: `Rand.Next(1, MaxSleep)` Is Exclusive on the Upper Bound

```csharp
private const int MaxSleep = 5;
// ...
SleepTime = Rand.Next(1, MaxSleep)
```

`Random.Next(minValue, maxValue)` returns a value **greater than or equal to** `minValue` and **strictly less than** `maxValue`. With `MaxSleep = 5`, the actual range is 1 through 4 seconds — never 5, despite the constant's name.

This doesn't affect the lesson at all; the comparison works identically whether the ceiling is 4 or 5. It's called out because the half-open interval is one of the most consistently misread API contracts in the framework, and a constant named `MaxSleep` that never occurs is exactly how that misreading survives review. `Rand.Next(1, MaxSleep + 1)` would make the name honest.

Worth remembering as a general rule: in .NET, integer ranges are almost always **inclusive lower, exclusive upper** — `Random.Next`, `Enumerable.Range`'s count semantics, `string.Substring`'s length semantics, array indexing. The exceptions are rarer than the rule.

---

## Worth Knowing: The Closure Is Safe Here

```csharp
foreach (var thread in Threads)
{
	ThreadPool.QueueUserWorkItem(x => { Nap(thread); });
}
```

Each lambda captures `thread`, the loop variable. In C# 5 and later, the `foreach` iteration variable is a **fresh variable per iteration**, so each closure captures its own tracker and this works correctly.

In C# 4 and earlier, `foreach` shared a single variable across all iterations, and this exact code would have queued five work items that all captured the *same* variable — most likely all napping on tracker #5. It was one of the most notorious gotchas in the language, common enough that the C# team took the unusual step of making a breaking change to fix it.

The trap still exists for `for` loops:

```csharp
for (int i = 0; i < 5; i++)
	ThreadPool.QueueUserWorkItem(x => Console.WriteLine(i));  // captures one shared i
```

That's still one shared `i`, and it will print unpredictable values, quite possibly `5` five times. If you need per-iteration capture in a `for` loop, copy to a local inside the body first.

---

## A Standards Fix Applied

Three `throw new ApplicationException(...)` calls were replaced with `throw new DatabankException(...)` — in `CreateThreads()`, `RunSequential()`, and `RunThreaded()` — matching this solution's conventions for non-`TextbookCode` projects.

`ApplicationException` was originally intended as a base class for custom application exceptions, but Microsoft's own guidance has recommended against using it since .NET 2.0. It adds nothing over `Exception` and provides no meaningful catch granularity. `DatabankException` carries the solution's logging behavior via `.Log()`, visible in `Main()`'s catch block.

---

## A Missing Reference File

A `Thread Tracker.pdf` reference file exists alongside the original download but wasn't carried over — the same binary-copy limitation noted for the diagrams in `CSharp.Ch06.Supplemental.05.ExceptionHandling`. It's still in `developer-training-bb\CSharp.Ch07.Supplemental.01.ThreadPoolExample\` if you want to copy it over yourself.

---

## Compare Against the Main Lesson

`CSharp.Ch07.MultithreadingAndAsynchronousProcessing`'s thread pool sections use a single `EventWaitHandle` to coordinate with just one pooled work item. This project scales that same pattern up to five — one `ThreadTracker` per thread, each with its own `Handle`, waited on individually in a loop.

Worth reading both. The underlying coordination technique is identical: **queue work, get a signal back, wait on the signal.** This project just demonstrates it holds up cleanly when there's more than one thread to track — and shows the bookkeeping cost of doing so manually, which is precisely the problem `Supplemental.03.TaskParallelLibrary` solves.

---

## Try It Yourself

- Run it several times. The random sleep times change, but threaded should always land near the max and sequential near the sum.
- Comment out `SetMinThreads` and watch whether the `Starting thread...` lines still appear simultaneously.
- Raise `NumberOfThreads` to 50 and observe how the gap widens, and where `SetMinThreads` starts to strain.
- Change `MaxSleep` to `6` and confirm you now see 5-second sleeps but never 6.

---

## Takeaways

- Reusing the same data for both runs is what makes the timing comparison meaningful.
- Bundling identity, signal, and parameters into one object works around `QueueUserWorkItem`'s single-`object` limit.
- Each concurrent operation needs its own `AutoReset` handle; sharing one deadlocks the rest.
- `SetMinThreads` defeats the pool's gradual ramp-up so short demos measure real parallelism.
- Waiting on started work belongs in `finally` — it's cleanup.
- Waiting in a fixed order costs nothing but reorders output relative to true completion.
- `Random.Next(min, max)` excludes `max`; inclusive-lower/exclusive-upper is the .NET norm.
- `foreach` variables are captured per-iteration since C# 5; `for` variables still aren't.
- Prefer `DatabankException` over the deprecated `ApplicationException`.
