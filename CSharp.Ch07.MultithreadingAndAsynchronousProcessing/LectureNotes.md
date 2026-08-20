# Chapter 7: Multithreading and Asynchronous Processing

## What This Lesson Is

A console app with a menu-driven code lab, run the same time-consuming work four different ways (sequential, manually-forked thread, thread pool without synchronization, thread pool with an `EventWaitHandle`), and watch how each one behaves differently for both correctness and elapsed time. No bugs found, this is well-crafted original content.

---

## Threads, in Brief

The chapter notes embedded in `Program.cs` are worth reading in full, but the load-bearing idea is the **fork/join pattern**: a process forks off a separate thread to do time-consuming work while the original continues, then joins (waits for) that thread once its result is actually needed. Windows' thread scheduler handles the mechanics (priority queues, round-robin scheduling, suspending blocked threads), but from the application's point of view, fork/join is the shape almost every threading approach in this lesson follows in some form.

---

## Four Approaches, Same Underlying Work

Every mode does the same two things, an I/O simulation (`Ch07SharedFunctions.SimulateReadDataFromIo()`, a 2-second `Thread.Sleep`) and a CPU-bound calculation (`Ch07SharedFunctions.DoIntensiveCalculations()`, ~134 million iterations of arithmetic), then sums the two results.

### Sequential (`RunSequential`)

```csharp
result += Ch07SharedFunctions.SimulateReadDataFromIo();
result += Ch07SharedFunctions.DoIntensiveCalculations();
```

One thing at a time. Total time is roughly the sum of both operations.

### Manually-Forked Thread (`RunWithThreads`)

```csharp
var thread = new Thread(() => result = Ch07SharedFunctions.SimulateReadDataFromIo());
thread.Start();

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

thread.Join();
result += result2;
```

Fork the I/O simulation onto its own thread, do the CPU-bound work on the main thread while it runs, `Join()` before combining results. Total time is roughly `max(I/O time, calculation time)` instead of their sum, this is the actual payoff of threading made visible in the elapsed-time output.

### Thread Pool, No Synchronization (`RunInThreadPool`)

```csharp
ThreadPool.QueueUserWorkItem(x => result += Ch07SharedFunctions.SimulateReadDataFromIo());

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

// Note: Because we have no way to determine when the thread completes, we will get the wrong result here
result += result2;
```

This is **deliberately incorrect**, and the code says so directly. `ThreadPool.QueueUserWorkItem` has no `Join()` equivalent, there's no way to wait for a pooled work item from the caller's side, so `result` is added to `result2` before the pooled work item has necessarily finished, or even started. Run this mode a few times and watch the printed result come out wrong (missing the I/O contribution) at least some of the time. This isn't a bug to fix, it's the setup for the next method's payoff.

### Thread Pool, With an Event (`RunInThreadPoolWithEvents`)

```csharp
var calculationDone = new EventWaitHandle(false, EventResetMode.AutoReset);

ThreadPool.QueueUserWorkItem(x => {
    result += Ch07SharedFunctions.SimulateReadDataFromIo();
    calculationDone.Set();
});

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

calculationDone.WaitOne();
result += result2;
```

Same thread pool approach, but now an `EventWaitHandle` gives the caller something to actually wait on. `calculationDone.WaitOne()` blocks until the pooled work item calls `calculationDone.Set()`, restoring correctness without needing `Join()`. This is the general pattern for coordinating with any thread you don't have direct control over: give it a signal to raise when it's done, and wait on that signal instead of trying to interact with the thread itself.

---

## Try It Yourself

Run each mode a few times in a row and watch the elapsed time printed at the end (`We're done in {elapsed}!`). Sequential should be consistently the slowest. Threaded and thread-pool-with-events should both land around the *longer* of the two individual operations, not their sum. Pay attention to `RunInThreadPool`'s printed result specifically, run it several times and see how often it comes out wrong.
