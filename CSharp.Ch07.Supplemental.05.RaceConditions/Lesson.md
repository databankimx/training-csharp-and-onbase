# Chapter 7 Supplemental 05: Race Conditions

## What This Is

S03 and S04 both had races noted in passing and moved on. This project is devoted entirely to the failure mode: what a race condition actually is, why it's silent, and two different fixes - one that works by giving up concurrency entirely, and one that actually keeps it. Understanding the difference between those two outcomes is arguably the most important practical skill in this entire chapter.

What's being abstracted here is nothing - that's the point. Previous supplementals gave you higher-level tools that handled synchronization for you. This project deliberately steps back to show what happens when you have no synchronization at all, then shows two different levels of response: the naive fix that restores correctness by destroying concurrency, and the correct fix that restores correctness while keeping concurrency intact.

---

## How to Write This Program

Add one shared field to `Program.cs`:

```csharp
private static int sharedRegister;
```

Also add the deliberately-racy worker - read it before running anything:

```csharp
private static void UpdateSharedResource(int num)
{
    Console.WriteLine($"Start thread {num}...");
    int s = sharedRegister;   // step 1: read
    Thread.Sleep(100);        // the race window, held open deliberately
    s++;                      // step 2: add
    sharedRegister = s;       // step 3: write
    Console.WriteLine($"Thread {num} incremented shared register...");
    Console.WriteLine($"End thread {num}...");
}
```

The 100ms sleep between read and write is the most important line in the project. In a real increment (`sharedRegister++`), the window is measured in nanoseconds - possible to race, but unlikely to catch in a short demo. Widening it to 100ms makes it fire every single time. The demo is reliable precisely because the bug has been made embarrassingly easy to hit.

---

### Mini-Program 1: Race Condition - The Problem

Clear `Main()` and write:

```csharp
Console.WriteLine("Not Synchronizing Threads");
Console.WriteLine("-------------------------");
sharedRegister = 0;
Console.WriteLine($"Start Value: {sharedRegister}\n");

foreach (int n in new[] { 1, 2 })
{
    var thread = new Thread(() => UpdateSharedResource(n));
    thread.Start();
}

// Cheat resynchronization - don't do this in production code
Thread.Sleep(200);

Console.WriteLine($"\nExpected Value: 2\nActual Value:  {sharedRegister}");
GenericFunctions.Pause();
```

Run it. Expected 2, actual 1, every time.

Both threads read `0`, both compute `1`, both write `1`. Thread 1's increment is silently discarded. Nothing crashes. No exception is thrown. The program reports a wrong number with complete confidence.

That's the defining characteristic of a race condition: the failure mode is silent data corruption, not a crash. A race that fails 100% of the time is actually the easiest kind to find. Real races produce wrong numbers *intermittently*, under load, in production, on machines with more cores than your laptop. By then someone has already blamed the database.

The `Thread.Sleep(200)` labeled "cheat resynchronization" is the wrong way to wait for threads. It works until the machine is slower or busier than assumed. The next two mini-programs do it properly.

### Mini-Program 2: EventWaitHandle - Correct, But Not Concurrent

Add a helper:

```csharp
private static void UpdateSharedResourceWithEvent(int num, EventWaitHandle done)
{
    UpdateSharedResource(num);
    done.Set();
}
```

Clear `Main()` and write:

```csharp
Console.WriteLine("Using an EventWaitHandle");
Console.WriteLine("------------------------");
sharedRegister = 0;
Console.WriteLine($"Start Value: {sharedRegister}\n");

foreach (int n in new[] { 1, 2 })
{
    var done = new EventWaitHandle(false, EventResetMode.AutoReset);

    ThreadPool.QueueUserWorkItem(x => UpdateSharedResourceWithEvent(n, done));

    // This defeats the point of being multi-threaded, but by forcing the threads
    // to be sequential, we avoid race conditions on the shared resource.
    done.WaitOne();
}

Console.WriteLine($"\nExpected Value: 2\nActual Value:  {sharedRegister}");
GenericFunctions.Pause();
```

Run it. Result is always 2. Now read where `done.WaitOne()` sits: **inside the loop**, before the next item is even queued. Thread 2 doesn't start until thread 1 has completely finished.

The comment says it outright - this "defeats the point of being multi-threaded." The result is correct because it isn't concurrent. This is a real pattern that gets shipped: synchronization applied so broadly that all parallelism is serialized away, leaving the overhead of threading with none of the benefit.

Also notice the racy code was never fixed. `UpdateSharedResource` still has its deliberate read/sleep/write. If someone later moved `WaitOne()` outside the loop as a "performance improvement," the race would come straight back.

### Mini-Program 3: CountdownEvent - Correct and Actually Concurrent

Add a helper that uses `Interlocked.Increment` instead of the racy pattern:

```csharp
private static void UpdateSharedResourceWithCountdown(int num, CountdownEvent countdown)
{
    Console.WriteLine($"Start thread {num}...");
    Thread.Sleep(100);
    Interlocked.Increment(ref sharedRegister);
    Console.WriteLine($"Thread {num} incremented shared register...");
    Console.WriteLine($"End thread {num}...");
    countdown.Signal();
}
```

Clear `Main()` and write:

```csharp
Console.WriteLine("Using a CountdownEvent");
Console.WriteLine("----------------------");
sharedRegister = 0;
Console.WriteLine($"Start Value: {sharedRegister}\n");

var countdown = new CountdownEvent(2);

foreach (int n in new[] { 1, 2 })
{
    ThreadPool.QueueUserWorkItem(x => UpdateSharedResourceWithCountdown(n, countdown));
}

// Wait outside the loop - both work items are running concurrently
countdown.Wait();

Console.WriteLine($"\nExpected Value: 2\nActual Value:  {sharedRegister}");
GenericFunctions.Pause();
```

Run it. Result is always 2, and both threads genuinely run concurrently - you can see both "Start thread N..." lines appear before either "End thread N..." line.

`countdown.Wait()` is outside the loop. Both work items are queued before anything is waited on. That's the structural difference from Mini-Program 2 that preserves the concurrency.

Two tools are doing two different jobs here. `CountdownEvent(2)` starts with a count of 2; each `Signal()` call decrements it; `Wait()` blocks until it reaches zero. It answers "when is everyone done." It does **not** stop two threads from touching `sharedRegister` at the same time while they're running.

`Interlocked.Increment` handles that. It performs read-add-write as a single atomic CPU operation with no window for the scheduler to interrupt. Both tools are needed because they're solving two different problems: `CountdownEvent` provides correct timing, `Interlocked` provides correct data.

Picking only one of them would still produce wrong results - just in a different, more confusing way.

---

## Compare All Three

| Method | Concurrent? | Correct? | Why |
|---|---|---|---|
| `RaceCondition()` | Yes | No | No synchronization; race window held open deliberately |
| `UsingEventWaitHandle()` | No | Yes | Correctness bought by serializing everything |
| `UsingCountdownEvent()` | Yes | Yes | Right tool for timing + right tool for data |

Only the third gets both. It took two different tools because it was solving two different problems. This pattern - reaching for the first thing that makes a test pass, rather than the thing that actually addresses the failure mode - is responsible for a substantial percentage of the concurrency bugs that make it to production.

---

## Summary: The Performance Story

Mini-Program 2 and Mini-Program 3 both produce the correct result. On a machine with multiple cores, Mini-Program 3 is measurably faster - both sleeps run concurrently rather than sequentially. But the more important difference is not speed: it's that Mini-Program 2 is brittle. Moving `WaitOne()` outside the loop would restore the race instantly. Mini-Program 3's correctness is structural - the racy `UpdateSharedResource` was replaced with a genuinely atomic operation, so there's no version of "move the wait" that brings the bug back.

Correct and fast. That's the goal. One without the other is either useless or a ticking clock.

---

## Takeaways

- A race condition corrupts data silently. No crash, no exception, just a wrong answer.
- `++` and `+=` are read-modify-write. The scheduler can interrupt between any two of the three steps.
- Widening the race window with a sleep makes a probabilistic bug deterministic - useful for demos, honest about the mechanism.
- Sleeping a guessed duration is not synchronization.
- Waiting inside the loop serializes the work and discards the benefit of threading.
- Preventing concurrency is not the same as fixing unsafe code.
- `CountdownEvent` answers "is everyone done" - it provides no mutual exclusion.
- `Interlocked.Increment` makes read-add-write a single atomic operation.
- Correct timing and correct data are two different problems requiring two different tools.
- A promised signal must be delivered on every exit path, or waiters deadlock forever and silently.
