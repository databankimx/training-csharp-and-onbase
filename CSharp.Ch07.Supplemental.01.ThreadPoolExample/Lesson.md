# Chapter 7 Supplemental 01: Thread Pool Example

## What This Is

The main lesson coordinated with one pooled work item via one `EventWaitHandle`. This project scales that same pattern up to five work items - five `ThreadTracker` objects with randomized sleep times, run first in parallel then sequentially against the same random data. The abstraction step here is moving from "one thing I'm waiting for" to "a group of things I need to coordinate with," each carrying its own signal handle.

The improvement is purely demonstrative: same random sleep times, two strategies, two very different elapsed totals. The gap between those numbers is the thread pool's value proposition made concrete on your own machine rather than asserted in a textbook.

---

## How to Write This Program

### Step 1: The Model

Create `Models/Objects/ThreadTracker.cs`. The thread pool only accepts one `object` argument per work item, so when you need to pass multiple things, you bundle them into one:

```csharp
public class ThreadTracker
{
    public int Id { get; set; }
    public EventWaitHandle Handle { get; set; }
    public int SleepTime { get; set; }
}
```

Three properties, zero logic. Its entire reason for existing is `QueueUserWorkItem`'s inconvenient single-argument limit.

### Step 2: Fields and Helpers

```csharp
internal static class Program
{
    private static readonly Random Rand = new Random();
    private static readonly List<ThreadTracker> Threads = new List<ThreadTracker>();
    private const int NumberOfThreads = 5;
    private const int MaxSleep = 5;
    private static Stopwatch sw;

    private static void Nap(ThreadTracker thread)
    {
        Console.WriteLine($"Starting thread {thread.Id}...");
        Thread.Sleep(thread.SleepTime * 1000);
        Console.WriteLine($"Thread {thread.Id} waited {thread.SleepTime} seconds...");
        thread.Handle.Set();
    }
}
```

### Mini-Program 1: Create the Trackers

Clear `Main()` and write:

```csharp
for (int i = 1; i <= NumberOfThreads; i++)
{
    var thread = new ThreadTracker
    {
        Id = i,
        Handle = new EventWaitHandle(false, EventResetMode.AutoReset),
        SleepTime = Rand.Next(1, MaxSleep)
    };
    Console.WriteLine($"Created thread #{thread.Id}, will run for {thread.SleepTime} seconds...");
    Threads.Add(thread);
}
GenericFunctions.Pause();
```

Run it. Five trackers printed with random sleep times between 1 and 4 seconds.

Each tracker gets its own `EventWaitHandle`. One shared handle wouldn't work - `AutoReset` releases exactly one waiter per `Set()`, so only the first thread to finish would ever let anyone through. The rest would block forever.

Also notice `Rand.Next(1, MaxSleep)` with `MaxSleep = 5` produces values from 1 to 4, never 5. `Random.Next(min, max)` excludes `max`. Inclusive-lower, exclusive-upper is the .NET norm.

The sleep times are generated here and reused by both run modes. That's the design choice that makes the comparison meaningful - same data, different strategies.

### Mini-Program 2: Run Threaded

Add `RunThreaded()` then call it from `Main()` after the setup:

```csharp
private static void RunThreaded()
{
    ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);

    sw = Stopwatch.StartNew();
    try
    {
        foreach (var thread in Threads)
        {
            ThreadPool.QueueUserWorkItem(x => { Nap(thread); });
        }
    }
    catch (Exception ex)
    {
        throw new DatabankException("Error running threads...", ex);
    }
    finally
    {
        foreach (var thread in Threads)
        {
            thread.Handle.WaitOne();
            Console.WriteLine($"End thread {thread.Id}");
        }
        Console.WriteLine($"Total run-time: {(double)sw.ElapsedMilliseconds / 1000} seconds...");
    }
}
```

Run it. All five "Starting thread N..." lines appear nearly simultaneously, then completions trickle in. The total time should land near the **longest** individual sleep - all five ran concurrently.

`SetMinThreads` is called before queuing anything. The pool ramps up gradually by default, potentially adding only one new thread every 500ms. For five short work items, that delay could mean they start sequentially rather than in parallel, undermining the whole comparison. `SetMinThreads` tells the pool to keep threads ready immediately.

The waits happen in the `finally` block and wait in tracker order, not completion order. Thread 3 might finish first, but its "End thread 3" line won't appear until threads 1 and 2 have been waited on. `WaitOne()` on an already-signaled handle returns immediately, so this costs nothing in time - it just reorders the final output slightly. The `Thread N waited N seconds...` lines from inside `Nap()` still appear in true completion order.

Putting the waits in `finally` is the right call: if queuing threw partway through, already-running background threads would keep executing, and abandoning them without waiting could tear down threads mid-execution when the process exits.

### Mini-Program 3: Run Sequential

Add `RunSequential()` then call it from `Main()` after the threaded run:

```csharp
private static void RunSequential()
{
    sw = Stopwatch.StartNew();
    try
    {
        foreach (var thread in Threads)
        {
            Nap(thread);
        }
    }
    catch (Exception ex)
    {
        throw new DatabankException("Error running sequentially!", ex);
    }
    finally
    {
        Console.WriteLine($"Total run-time: {(double)sw.ElapsedMilliseconds / 1000} seconds...");
    }
}
```

Run it. Each waits for the previous to finish, so the total lands near the **sum** of all five sleep times.

Compare that number against the threaded total. Same random data, completely different results.

---

## Worth Knowing: The foreach Capture Is Safe Here

```csharp
foreach (var thread in Threads)
{
    ThreadPool.QueueUserWorkItem(x => { Nap(thread); });
}
```

Each lambda captures `thread`. Since C# 5, `foreach` creates a fresh variable per iteration, so each closure captures its own tracker. The same code with a `for` loop is a different story:

```csharp
for (int i = 0; i < Threads.Count; i++)
    ThreadPool.QueueUserWorkItem(x => { Nap(Threads[i]); }); // all capture the same i
```

All five closures capture one shared `i`. By the time any of them run, `i` is probably 5, and you get either an `ArgumentOutOfRangeException` or five work items all operating on the same tracker.

---

## Summary: What You Should See

| Mode | Expected elapsed | Why |
|---|---|---|
| Threaded | ~longest individual sleep | All five ran concurrently |
| Sequential | ~sum of all five sleeps | Each waited for the previous |

The ratio between those two numbers varies with the random data each run - but threaded will always win, and the margin will always be roughly "the sum minus the longest."

---

## Takeaways

- Bundle parameters for `QueueUserWorkItem` into a single object. That is `ThreadTracker`'s entire job.
- Each concurrent work item needs its own `AutoReset` handle. Sharing one deadlocks all but the first.
- `SetMinThreads` defeats the pool's gradual ramp-up so parallelism in short demos is real.
- Waiting in `finally` means cleanup happens even if queuing throws partway through.
- Waiting in a fixed order costs nothing - an already-signaled handle returns immediately.
- `Random.Next(min, max)` excludes `max`. Inclusive-lower, exclusive-upper is the .NET norm.
- `foreach` variables have been captured per-iteration since C# 5. `for` variables still haven't.
