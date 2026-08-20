# Multithreading and Asynchronous Processing

## Introduction

Modern CPUs have multiple cores, capable of running several things at once. A **thread** is a separate path of execution your program can create to take advantage of that, offloading time-consuming work so the rest of the program isn't stuck waiting for it.

---

## The Fork/Join Pattern

1. A process is running and needs to do something time-consuming.
2. It **forks**, spawning a separate thread to do that work, while the original process keeps going.
3. Once the main process actually needs the forked thread's result, it **joins**, waiting for that thread to finish.
4. After joining, both paths of execution have reconverged, and the program continues.

You can fork more than one thread at a time, this pattern isn't limited to just one.

---

## How Windows Schedules Threads

Every thread gets a priority when created (though it doesn't start running until you explicitly start it). Once started, it joins a queue of runnable threads. The scheduler picks the highest-priority thread to run; threads at the same priority take turns in round-robin order. When a thread's time slice runs out, or it blocks waiting on I/O or a lock, it's set aside and another thread gets a turn.

A running .NET application automatically has several threads already: the garbage collector, the finalizer, `Main`, and (for a UI application, not a console app) a dedicated UI thread.

---

## Four Threading Approaches in This Lesson

### 1. Sequential (No Threading)

```csharp
result += Ch07SharedFunctions.SimulateReadDataFromIo();
result += Ch07SharedFunctions.DoIntensiveCalculations();
```

Straightforward, one operation after another. Total time is the sum of both.

### 2. A Manually-Created Thread

```csharp
var thread = new Thread(() => result = Ch07SharedFunctions.SimulateReadDataFromIo());
thread.Start();                                    // FORK

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

thread.Join();                                     // JOIN
result += result2;
```

The I/O simulation runs on its own thread while the calculation runs on the main thread. `Join()` blocks until the forked thread finishes, guaranteeing `result` is fully set before it's added to `result2`. Because the two operations overlap instead of running back to back, total time drops to roughly whichever one takes longer, not their sum.

### 3. The Thread Pool (Without Synchronization)

```csharp
ThreadPool.QueueUserWorkItem(x => result += Ch07SharedFunctions.SimulateReadDataFromIo());

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

result += result2;   // Wrong! The pooled work item might not have finished yet.
```

`ThreadPool.QueueUserWorkItem` hands work off to a pool of reusable background threads, but you get back no handle to `Join()` on. This version adds the results together immediately, with no way to know whether the pooled work item has actually completed. Run it a few times, you'll sometimes get the correct total and sometimes get a result that's missing the I/O contribution entirely, depending on timing.

### 4. The Thread Pool, Correctly Synchronized

```csharp
var calculationDone = new EventWaitHandle(false, EventResetMode.AutoReset);

ThreadPool.QueueUserWorkItem(x => {
    result += Ch07SharedFunctions.SimulateReadDataFromIo();
    calculationDone.Set();      // Signal that this work item is done.
});

double result2 = Ch07SharedFunctions.DoIntensiveCalculations();

calculationDone.WaitOne();      // Block until the signal fires.
result += result2;
```

An `EventWaitHandle` gives the caller something to actually wait on, since a pooled thread can't be `Join()`ed directly. The pooled work item calls `.Set()` when it's finished; the main thread calls `.WaitOne()`, which blocks until that happens. This restores correctness while still using the thread pool's automatic thread management.

---

## Why This Matters

Approach 3 versus approach 4 is really the whole lesson: **using a thread doesn't automatically mean your code is correct**. If you can't wait for a thread to actually finish before using its result, you have a race condition, whether or not it shows up every time you run the program. Approach 4 fixes it not by avoiding the thread pool, but by adding a proper way to coordinate with it.

---

## Try It Yourself

Run the sequential mode and note the elapsed time. Then run the threaded mode and compare, it should be noticeably faster, even though the exact same two operations are happening. Then run the unsynchronized thread-pool mode several times in a row and watch the printed result, see how often it's wrong before switching to the events-based version and confirming it's always correct.
