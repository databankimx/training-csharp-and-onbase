# Chapter 7: Multithreading and Asynchronous Processing

## What This Lesson Is

A console app with a menu-driven code lab. It runs the same time-consuming work four different ways — sequential, manually-forked thread, thread pool without synchronization, thread pool with an `EventWaitHandle` — and lets you watch how each behaves differently for both **correctness** and **elapsed time**.

The menu loops, so you can run the same mode repeatedly and see how consistent (or inconsistent) each approach really is. That repetition matters here in a way it hasn't in earlier chapters: concurrency bugs are probabilistic, and one run tells you almost nothing.

Chapter 7 has nine supplemental projects, listed at the bottom and documented separately.

---

## Threads, in Brief

The chapter notes embedded in `Program.cs` are worth reading in full, but the load-bearing idea is the **fork/join pattern**:

1. Create the new thread and assign the work it will perform
2. Start the thread (**FORK**)
3. Do other work in the main process until the thread's results are needed
4. Wait for the thread to finish (**JOIN**)
5. Continue working in the main process

Almost every threading approach in this chapter is a variation on that shape, including the ones that hide it behind an abstraction.

### The Windows Thread Scheduler

The notes summarize how threads actually get CPU time, which is worth understanding because it explains why concurrent output is unpredictable:

- Every thread gets a **priority** when created. Creating a thread does not start it.
- A started thread joins a **queue** of runnable threads.
- The scheduler runs the highest-priority thread available.
- Threads of equal priority are scheduled **round-robin**.
- When a thread's time slice expires, it's **suspended** and moved to the back of the queue.
- A thread that **blocks** — on I/O, on a lock — is removed from the queue entirely, and something else runs.
- When the block clears, the thread rejoins the queue.

The practical consequence: your code does not control when it runs, only when it *can* run. Two threads started microseconds apart may execute in either order, interleave arbitrarily, or one may finish entirely before the other starts. Any correctness that depends on timing is not correctness.

### Foreground vs. Background Threads

```
Note that while you can (and should) create background threads, when all non-background
threads (including Main) complete, the application ends, even if non-terminated background
threads were still executing.
```

This distinction causes real bugs. Threads created with `new Thread(...)` are **foreground** by default — the process stays alive until they finish. Thread pool threads are always **background** — the process exits out from under them without warning.

That's why `RunInThreadPool` below can fail silently, and why a "fire and forget" background operation may simply never complete.

### The Cost

```
There is some non-trivial overhead (thread-switching and memory use) involved in threading,
so only use a multi-threaded design when there is an advantage because offloaded work is
time-consuming.
```

Each thread reserves stack space (1 MB by default on Windows) and every context switch costs CPU cycles. Threading a fast operation makes it slower. The four modes below are worth timing precisely because they make that tradeoff measurable rather than theoretical.

---

## Four Approaches, Same Underlying Work

Every mode does the same two things and sums the results:

- **`Ch07SharedFunctions.SimulateReadDataFromIo()`** — a 2-second `Thread.Sleep`, standing in for I/O
- **`Ch07SharedFunctions.DoIntensiveCalculations()`** — roughly 134 million iterations of arithmetic, CPU-bound

The pairing is deliberate. I/O-bound work *waits*; CPU-bound work *computes*. They're the two categories that behave differently under every technique in this chapter, and having one of each makes the difference visible.

The whole lab is wrapped in a `Stopwatch`:

```csharp
var sw = Stopwatch.StartNew();

switch (userEntry) { /* ... */ }

Console.WriteLine("We're done in {0}!", sw.Elapsed);
```

### Sequential (`RunSequential`)

```csharp
double result = 0d;

result += Ch07SharedFunctions.SimulateReadDataFromIo();
result += Ch07SharedFunctions.DoIntensiveCalculations();

Console.WriteLine("The result is {0}", result);
```

One thing at a time. Total time is roughly the **sum** of both operations, and the result is always correct. This is the baseline both for speed and for what the right answer looks like.

Note that `RunSequential` is the `default` case in the switch, with `//case "s":` commented out just above it. Anything unrecognized falls through to sequential — a reasonable default, since it's the mode that always works.

### Manually-Forked Thread (`RunWithThreads`)

```csharp
double result = 0d;

// Note that our callback here is an anonymous delegate
var thread = new Thread(() => result = Ch07SharedFunctions.SimulateReadDataFromIo());

thread.Start();                                                  // FORK

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();  // meanwhile, on the main thread

thread.Join();                                                   // JOIN

result += result2;

Console.WriteLine("The result is {0}", result);
```

Fork the I/O simulation onto its own thread, do the CPU-bound work on the main thread while it runs, then `Join()` before combining. Total time is roughly **`max`(I/O time, calculation time)** rather than their sum — the actual payoff of threading, made visible in the elapsed-time output.

Three details worth noticing:

**The lambda is a closure.** `() => result = ...` captures the local variable `result` and assigns to it from another thread. That's the Chapter 6 closure mechanism doing real work — and note it neatly sidesteps the `ParameterizedThreadStart` awkwardness covered in `Ch06.Supplemental.06`. No `object` parameter, no casting.

**`Join()` is what makes this correct.** It blocks the main thread until the forked thread finishes. Remove it and you have exactly the bug the next mode demonstrates.

**Why `result2` exists.** The two threads deliberately write to *different* variables. Having both do `result +=` would be a read-modify-write race — precisely the failure `Supplemental.05.RaceConditions` is built to demonstrate. This code avoids it by construction, which is the cleanest way to handle shared state: don't share it.

### Thread Pool, No Synchronization (`RunInThreadPool`)

The notes explain the pool itself first:

```
1. The program adds a work item to the thread pool
2. If there is an idle thread, the work item is executed there
3. If not (and assuming we're using fewer than the maximum available threads), a new
   background thread is created and the work item executed in the new thread

Note: Because these are abstracted background threads, you cannot interrupt or join them
or set their priority
```

The pool exists because creating threads is expensive. It keeps a set of reusable threads alive and hands out work items, amortizing creation cost across many short operations. The tradeoff is control — and that's exactly what breaks here.

```csharp
double result = 0d;

ThreadPool.QueueUserWorkItem(x => result += Ch07SharedFunctions.SimulateReadDataFromIo());

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

// Note: Because we have no way to determine when the thread completes, we will get the wrong result here
result += result2;

Console.WriteLine("The result is {0}", result);
```

This is **deliberately incorrect**, and the code says so directly.

`ThreadPool.QueueUserWorkItem` has no `Join()` equivalent. There is no handle to wait on, so `result` is combined with `result2` before the pooled work item has necessarily finished — or even started. The result comes out wrong, missing the I/O contribution.

Run it several times. It may not fail every time, and *that* is the real lesson. A bug that reproduces intermittently is far more dangerous than one that fails consistently, because it survives testing and appears in production under load. Everything here is timing-dependent: machine speed, core count, current pool pressure.

Note the `x` parameter in the lambda — that's the `object state` argument `QueueUserWorkItem` always passes. It's unused, but the signature requires it, the same single-`object` limitation seen in `ParameterizedThreadStart`.

This isn't a bug to fix. It's the setup for the next method's payoff.

### Thread Pool, With an Event (`RunInThreadPoolWithEvents`)

```csharp
double result = 0d;

var calculationDone = new EventWaitHandle(false, EventResetMode.AutoReset);

ThreadPool.QueueUserWorkItem(x => {
	result += Ch07SharedFunctions.SimulateReadDataFromIo();
	calculationDone.Set();
});

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

calculationDone.WaitOne();

result += result2;

Console.WriteLine("The result is {0}", result);
```

Same thread pool, but now an `EventWaitHandle` gives the caller something to wait on. `WaitOne()` blocks until the pooled work item calls `Set()`, restoring correctness without needing `Join()`. Timing is back to `max` of the two operations, and the answer is right every time.

The constructor arguments matter:

- **`false`** — the handle starts unsignaled. Starting it `true` would make `WaitOne()` return immediately and reintroduce the bug.
- **`EventResetMode.AutoReset`** — the handle resets itself after releasing one waiter. `ManualReset` would stay signaled, releasing every subsequent `WaitOne()` immediately, which matters when the lab is run repeatedly from the menu loop.

This is the general pattern for coordinating with any thread you don't control: **give it a signal to raise when it's done, and wait on that signal instead of trying to interact with the thread itself.**

It's also the primitive underneath higher-level constructs. `Task.Wait()`, `await`, and `Barrier` all solve this same problem with better ergonomics — but this is the mechanism they're built on.

---

## Worth Knowing: A Visibility Fix

```csharp
// Ch07SharedFunctions.WaitForKeyWhenDebugging() only pauses when a debugger
//     is attached, which meant this result was invisible when run normally
//     (including via LessonRunner), Console.Clear() at the top of the next
//     loop iteration wiped it out instantly. Pause unconditionally instead.
GenericFunctions.Pause();
```

The lab originally ended each iteration with `WaitForKeyWhenDebugging()`, which pauses **only when a debugger is attached**. Run normally — including through `LessonRunner` — nothing paused, and `Console.Clear()` at the top of the next loop iteration erased the result and the elapsed time before they could be read.

The entire point of this lesson is comparing printed results and timings across modes, so a mode that produced invisible output was effectively broken outside the debugger. Replaced with an unconditional `GenericFunctions.Pause()`.

Worth generalizing: debug-only behavior that changes what the user *sees* is a category of bug that's structurally hard to catch, because it works perfectly in the environment where you test it. It's the same hazard as `Debug.Assert` from `Ch06.Supplemental.08` — code whose presence depends on build configuration or debugger state needs deliberate verification in the configuration you actually ship.

---

## Try It Yourself

Run each mode several times and watch the elapsed time printed at the end (`We're done in {elapsed}!`).

| Mode | Expected time | Correct? |
|---|---|---|
| `[S]equential` | I/O + calculation | Always |
| `[T]hreaded` | ≈ `max`(I/O, calculation) | Always |
| `[P]ooled` | ≈ `max`(I/O, calculation) | **Often wrong** |
| `[E]ventsInPool` | ≈ `max`(I/O, calculation) | Always |

Sequential should be consistently slowest. The other three should all land near the *longer* of the two individual operations, not their sum.

Pay attention to `[P]ooled`'s printed result specifically. Run it several times and count how often it comes out wrong — and note that it's just as fast as the correct version, which is exactly why speed alone is never evidence that concurrent code works.

---

## Chapter Takeaways

- Fork/join is the fundamental shape: start work, do something else, wait, combine.
- The scheduler decides when threads run. Never depend on timing for correctness.
- Threading helps when work is genuinely time-consuming; it costs memory and context switches otherwise.
- Overlapping work turns "sum of durations" into "max of durations."
- Foreground threads keep the process alive; thread pool threads don't.
- The thread pool amortizes thread creation but gives up `Join()`, priority, and interruption.
- Without a synchronization mechanism, you cannot know when pooled work finished.
- `EventWaitHandle` + `Set()`/`WaitOne()` is the general signal-and-wait pattern.
- Avoid shared mutable state where you can — separate variables beat synchronization.
- Intermittent concurrency bugs are more dangerous than consistent ones; run repeatedly.
- Debug-only behavior can hide real defects from every non-debugger run.

---

## Also in Chapter 7

Nine supplemental projects accompany this one, documented separately:

1. `CSharp.Ch07.Supplemental.01.ThreadPoolExample`
2. `CSharp.Ch07.Supplemental.02.UnblockingTheUI`
3. `CSharp.Ch07.Supplemental.03.TaskParallelLibrary`
4. `CSharp.Ch07.Supplemental.04.Asynchronicity`
5. `CSharp.Ch07.Supplemental.05.RaceConditions`
6. `CSharp.Ch07.Supplemental.06.Barriers`
7. `CSharp.Ch07.Supplemental.07.Locking`
8. `CSharp.Ch07.Supplemental.08.LockFreeAlternatives`
9. `CSharp.Ch07.Supplemental.09.ConcurrentCollections`
