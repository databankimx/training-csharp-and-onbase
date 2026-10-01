# Chapter 7: Multithreading and Asynchronous Processing

## What This Is

A code lab that runs the same two pieces of work four different ways, so you can watch how each one behaves for both correctness and elapsed time. The menu loops so you can run any mode repeatedly -- which matters more here than in any previous chapter, because concurrency bugs are probabilistic. One run tells you almost nothing.

Chapter 7 has nine supplemental projects. They're listed at the bottom and documented separately.

---

## The Shared Work

Both pieces of work live in `CSharp.SharedLibrary.HelperClasses.Ch07SharedFunctions`:

- `SimulateReadDataFromIo()` -- sleeps 2 seconds and returns `10d`. Stands in for any I/O-bound operation.
- `DoIntensiveCalculations()` -- runs ~134 million iterations of arithmetic and returns a result. Stands in for CPU-bound work.

The pairing is deliberate. I/O-bound work *waits* with the CPU sitting idle; CPU-bound work *computes* and pegs the core. They respond differently to every technique in this chapter, and having one of each makes the difference measurable rather than theoretical.

---

## How to Write This Program

### Mini-Program 1: Sequential

Clear `Main()` and write this:

```csharp
var sw = Stopwatch.StartNew();

double result = 0d;
result += Ch07SharedFunctions.SimulateReadDataFromIo();
result += Ch07SharedFunctions.DoIntensiveCalculations();

Console.WriteLine($"Result: {result}");
Console.WriteLine($"Elapsed: {sw.Elapsed}");
GenericFunctions.Pause();
```

Run it. Note the elapsed time -- this is the baseline everything else will be compared against. Total time is roughly the **sum** of both operations.

The result is always correct. Sequential code has no concurrency, so it has no concurrency bugs. That's not a coincidence -- it's a trade-off you're making explicit by moving away from it.

### Mini-Program 2: Manually-Forked Thread

Clear `Main()` and write this:

```csharp
var sw = Stopwatch.StartNew();

double result = 0d;

// 1. Create the thread and assign its work
var thread = new Thread(() => result = Ch07SharedFunctions.SimulateReadDataFromIo());

// 2. FORK
thread.Start();

// 3. Do other work on the main thread while the forked thread runs
double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

// 4. JOIN -- wait for the forked thread to finish before using its result
thread.Join();

// 5. Combine
result += result2;

Console.WriteLine($"Result: {result}");
Console.WriteLine($"Elapsed: {sw.Elapsed}");
GenericFunctions.Pause();
```

Run it. The elapsed time should land near the **longer** of the two individual operations, not their sum. The I/O simulation and the calculations ran at the same time -- `max(I/O, calculation)` instead of `I/O + calculation`. That gap is the entire value proposition of threading for genuinely independent work, made visible on your own machine.

Three things worth noticing while you're here.

The lambda `() => result = Ch07SharedFunctions.SimulateReadDataFromIo()` is a closure capturing `result` and writing to it from another thread. The Chapter 6 closure mechanism, now doing real concurrent work.

`Join()` is what makes this correct. It blocks the main thread until the forked thread finishes before `result` is read. Remove it and you have exactly the bug the next mini-program demonstrates on purpose.

The two threads write to *different* variables -- `result` from the forked thread, `result2` from the main thread. If both did `result +=`, that's a read-modify-write race. Separate variables, combine after the join: the cleanest synchronization strategy is not sharing mutable state in the first place.

### Mini-Program 3: Thread Pool, No Synchronization

Clear `Main()` and write this:

```csharp
var sw = Stopwatch.StartNew();

double result = 0d;

ThreadPool.QueueUserWorkItem(x => result += Ch07SharedFunctions.SimulateReadDataFromIo());

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

// Note: no way to wait for the pooled work item -- result is used before it may have finished
result += result2;

Console.WriteLine($"Result: {result}");
Console.WriteLine($"Elapsed: {sw.Elapsed}");
GenericFunctions.Pause();
```

Run it several times. The result will often be wrong -- the I/O contribution is missing because `result` gets combined with `result2` before the pooled work item has necessarily finished. Or started.

`QueueUserWorkItem` has no `Join()` equivalent. Pool threads are background threads -- you hand off the work item and lose direct control. That's the trade-off: the pool amortizes thread creation cost by reusing a set of threads, but you give up the ability to wait on any specific one.

Run this at least five times and count the wrong results. It may not fail every time, and *that* is the real lesson. A bug that reproduces intermittently is far more dangerous than one that fails consistently, because it survives testing and shows up in production under load.

### Mini-Program 4: Thread Pool With an EventWaitHandle

Clear `Main()` and write this:

```csharp
var sw = Stopwatch.StartNew();

double result = 0d;
var calculationDone = new EventWaitHandle(false, EventResetMode.AutoReset);

ThreadPool.QueueUserWorkItem(x =>
{
    result += Ch07SharedFunctions.SimulateReadDataFromIo();
    calculationDone.Set();
});

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

calculationDone.WaitOne();

result += result2;

Console.WriteLine($"Result: {result}");
Console.WriteLine($"Elapsed: {sw.Elapsed}");
GenericFunctions.Pause();
```

Run it. Correct result, every time, and the elapsed time matches the threaded version.

Same thread pool as Mini-Program 3. The difference is the `EventWaitHandle`: the work item signals it when done (`calculationDone.Set()`), and the main thread waits on it (`calculationDone.WaitOne()`) before combining results. That restores the correctness guarantee `Join()` provided without needing a direct reference to the thread.

The two constructor arguments matter. `false` starts the handle unsignaled -- `true` would make `WaitOne()` return immediately and reintroduce the bug. `AutoReset` resets the handle automatically after releasing one waiter, which matters if you run this in a loop.

This is the general pattern for coordinating with any thread you don't control: give it a signal to raise when it finishes, and wait on that signal.

---

## Now Put It All Together

The real program uses a menu loop so you can run all four modes repeatedly and compare. The four implementations above become `RunSequential()`, `RunWithThreads()`, `RunInThreadPool()`, and `RunInThreadPoolWithEvents()` inside a `CodeLabUsingThreads()` method. Each wraps its own `Stopwatch` and prints elapsed time at the end.

```csharp
Console.WriteLine("We're done in {0}!", sw.Elapsed);
GenericFunctions.Pause();
```

Run each mode several times. Sequential is consistently slowest. Threaded and EventsInPool both land near `max(I/O, calculation)`. Pooled is fast and unreliable -- note that it prints the *correct elapsed time* even when the result is wrong, because the two operations still overlapped. Speed is not evidence of correctness in concurrent code.

---

## Takeaways

- Fork/join is the fundamental pattern: start work on another thread, do something else, wait, combine.
- The scheduler decides when threads run. Correctness that depends on timing is not correctness.
- Threading turns "sum of durations" into "max of durations" -- only worthwhile when the work is genuinely time-consuming.
- Foreground threads keep the process alive; pool threads don't.
- The pool amortizes thread creation but gives up `Join()`, priority, and interruption.
- Without synchronization, there is no way to know when pooled work finished.
- `EventWaitHandle` + `Set()`/`WaitOne()` is the general signal-and-wait pattern for threads you don't control.
- Separate mutable state beats synchronization -- if threads don't share it, there's nothing to coordinate.
- Intermittent failures are more dangerous than consistent ones. Run concurrent code repeatedly.

---

## Also in Chapter 7

Nine supplemental projects accompany this one, each documented separately:

1. `CSharp.Ch07.Supplemental.01.ThreadPoolExample`
2. `CSharp.Ch07.Supplemental.02.UnblockingTheUI`
3. `CSharp.Ch07.Supplemental.03.TaskParallelLibrary`
4. `CSharp.Ch07.Supplemental.04.Asynchronicity`
5. `CSharp.Ch07.Supplemental.05.RaceConditions`
6. `CSharp.Ch07.Supplemental.06.Barriers`
7. `CSharp.Ch07.Supplemental.07.Locking`
8. `CSharp.Ch07.Supplemental.08.LockFreeAlternatives`
9. `CSharp.Ch07.Supplemental.09.ConcurrentCollections`
