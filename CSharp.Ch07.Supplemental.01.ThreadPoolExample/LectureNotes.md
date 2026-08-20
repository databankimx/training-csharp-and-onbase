# Chapter 7 Supplemental 01: Thread Pool Example

## What This Is

A more thorough thread pool demonstration than the main lesson's: five `ThreadTracker` objects, each with a randomized sleep time (1-5 seconds simulating variable-length work), run first in the thread pool (all in parallel), then sequentially, so you can directly compare the total elapsed time both ways. No bugs found.

One standards-compliance fix applied, matching this solution's conventions for non-`TextbookCode.*` projects: `throw new ApplicationException(...)` replaced with `throw new DatabankException(...)` in three places (`CreateThreads()`, `RunSequential()`, `RunThreaded()`).

A `Thread Tracker.pdf` reference file exists alongside the original download but wasn't carried over, the same binary-copy limitation noted for the diagrams in `CSharp.Ch06.Supplemental.05.ExceptionHandling`, no tool available to copy a file directly between two folders on your machine without risking a lossy round-trip. It's still in `developer-training-bb\CSharp.Ch07.Supplemental.01.ThreadPoolExample\` if you want to copy it over yourself.

---

## Watch the Numbers, Not Just the Code

```csharp
ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);
...
foreach (var thread in Threads)
{
    ThreadPool.QueueUserWorkItem(x => { Nap(thread); });
}
...
foreach (var thread in Threads)
{
    thread.Handle.WaitOne();
    Console.WriteLine($"End thread {thread.Id}");
}
```

Five threads, each sleeping somewhere between 1 and 5 seconds. Run threaded, and the total time printed should land close to whichever thread happened to draw the *longest* sleep time, all five run concurrently. Run sequential, and the total should land close to the *sum* of all five sleep times, since each one waits for the previous to finish before starting. The difference between those two numbers, on the same randomly-generated data, is the entire value proposition of using a thread pool for genuinely independent, parallelizable work made concrete.

---

## Worth Noticing: `SetMinThreads` Before Queuing Work

```csharp
ThreadPool.SetMinThreads(NumberOfThreads, NumberOfThreads);
```

The .NET thread pool doesn't necessarily create new threads immediately when work is queued, by default it can ramp up gradually, only spinning up additional worker threads over time as demand persists. For a small, short demo like this one, that gradual ramp-up could mean the five work items don't all actually start in parallel right away, undermining the comparison this project is trying to make. `SetMinThreads` tells the pool to keep at least this many threads ready immediately, so the "run threaded" numbers reflect genuine parallelism rather than an artifact of the pool's own warm-up behavior.

---

## Compare Against the Main Lesson

`CSharp.Ch07.MultithreadingAndAsynchronousProcessing`'s thread pool sections use a single `EventWaitHandle` to coordinate with just one pooled work item. This project scales that same pattern up to five, one `ThreadTracker` per thread, each with its own `Handle`, waited on individually in a loop. Worth reading both, the underlying coordination technique (queue work, get a signal back, wait on the signal) is identical, this project just demonstrates it holds up cleanly when there's more than one thread to track.
