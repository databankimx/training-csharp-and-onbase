# Thread Pool Example

## Introduction

The .NET thread pool maintains a set of reusable background threads so your program doesn't have to create a brand new thread every time it needs to do something in parallel. This lesson runs the same batch of work two ways, through the thread pool and sequentially, so you can directly compare how long each approach takes.

---

## Setting Up the Work

```csharp
for (int i = 1; i <= NumberOfThreads; i++)
{
    var thread = new ThreadTracker
    {
        Id = i,
        Handle = new EventWaitHandle(false, EventResetMode.AutoReset),
        SleepTime = Rand.Next(1, MaxSleep)
    };
    Threads.Add(thread);
}
```

Five `ThreadTracker` objects are created up front, each one gets a random sleep time between 1 and 5 seconds (standing in for some variable-length piece of work), and its own `EventWaitHandle`, a signal it can raise when its work is done.

---

## Running in the Thread Pool

```csharp
ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);

foreach (var thread in Threads)
{
    ThreadPool.QueueUserWorkItem(x => { Nap(thread); });
}

foreach (var thread in Threads)
{
    thread.Handle.WaitOne();
    Console.WriteLine($"End thread {thread.Id}");
}
```

All five pieces of work get queued to the thread pool at once. `SetMinThreads` tells the pool to have at least five threads ready immediately, rather than gradually ramping up, so the five items actually run concurrently instead of trickling out one at a time. The second loop waits on each thread's own handle in turn, once a thread finishes its nap, it calls `Handle.Set()`, and the corresponding `WaitOne()` unblocks.

---

## Running Sequentially

```csharp
foreach (var thread in Threads)
{
    Nap(thread);
}
```

The same five sleep times, but one after another, no thread pool involved at all.

---

## Compare the Two

Both approaches process the exact same randomly-generated sleep times. The threaded version's total time should land close to whichever single thread happened to get the longest sleep time (since they all run at once). The sequential version's total should land close to the *sum* of all five sleep times. Run the project and read the two "Total run-time" lines printed at the end of each section, that difference is the entire point of using a thread pool for genuinely independent work.

---

## Try It Yourself

Run the project a few times, since the sleep times are randomized, the exact numbers will differ each run, but the *relationship* between the two totals should hold consistently: threaded is always close to the longest individual sleep time, sequential is always close to the sum of all five.
