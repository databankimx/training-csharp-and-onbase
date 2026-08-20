# Race Conditions

## Introduction

A race condition happens when two threads try to update the same data at the same time, and the exact timing of that overlap changes the result. This lesson demonstrates one happening, then two different ways to prevent it correctly.

---

## Why It Happens

`sharedData++` looks like one step, but the CPU actually does it in three:

1. Read `sharedData` into a register.
2. Add 1 to the register.
3. Write the register back to `sharedData`.

If two threads both read the value *before* either one writes back, they both compute the same "next" value and one thread's update gets silently overwritten by the other's. Starting from 0, two threads each doing `+1` should end at 2, but if both read 0 before either writes, you end up at 1 instead, one increment simply vanishes.

---

## Seeing It Happen

```csharp
foreach (int n in new[] {1, 2})
{
    var thread = new Thread(() => UpdateSharedResource(n));
    thread.Start();
}
Thread.Sleep(200);
```

```csharp
private static void UpdateSharedResource(int num)
{
    int s = sharedRegister;
    Thread.Sleep(100);   // widens the window so the race reliably happens
    s++;
    sharedRegister = s;
}
```

Two real threads, both reading `sharedRegister`, both sleeping (deliberately, to make the race condition reliably reproduce rather than leaving it to chance), both writing back. Run this and the final value is consistently 1, not the expected 2.

---

## Fix 1: `EventWaitHandle`

```csharp
foreach (int n in new[] { 1, 2 })
{
    var done = new EventWaitHandle(false, EventResetMode.AutoReset);
    ThreadPool.QueueUserWorkItem(x => { UpdateSharedResourceWithEvent(n, done); });
    done.WaitOne();
}
```

Queue one piece of work, then block (`WaitOne()`) until it signals `done` before queuing the next. This guarantees the two updates never overlap, but it also means they never actually run at the same time either, you've traded away the parallelism to get correctness.

---

## Fix 2: `CountdownEvent` (Done Right)

```csharp
var countdown = new CountdownEvent(2);

foreach (int n in new[] { 1, 2 })
{
    ThreadPool.QueueUserWorkItem(x => UpdateSharedResourceWithCountdown(n, countdown));
}

countdown.Wait();
```

Unlike Fix 1, both pieces of work are queued immediately and genuinely run at the same time, `countdown.Wait()` only blocks until *both* have signaled completion, it doesn't force them to run one after another. A `CountdownEvent` is the right tool when you need to wait for a whole group of parallel workers rather than just one.

But queuing two genuinely concurrent threads reintroduces the exact same problem `RaceCondition()` had, unless something also protects `sharedRegister` while both threads are running, not just after. That's what `Interlocked.Increment` is for:

```csharp
private static void UpdateSharedResourceWithCountdown(int num, CountdownEvent countdown)
{
    Thread.Sleep(100);
    Interlocked.Increment(ref sharedRegister);
    countdown.Signal();
}
```

`Interlocked.Increment` does the read-add-write sequence as one atomic step, no other thread can interleave in the middle of it. Combined, `CountdownEvent` tells you *when* it's safe to look at the result, and `Interlocked` guarantees the result itself is actually correct. Neither one alone would have been enough here.

---

## The General Lesson

Using *a* synchronization primitive doesn't automatically mean you've solved *your specific* problem. `CountdownEvent` solves "wait for N things to finish." It does not solve "prevent two threads from corrupting the same variable." Those are different problems, and sometimes (like here) you genuinely need more than one tool working together, not just one impressive-looking one.

---

## Try It Yourself

Run `RaceCondition()` a few times in a row, timing-dependent bugs don't always reproduce on every run. Then try removing `Interlocked.Increment` from `UpdateSharedResourceWithCountdown` and replacing it with the same read/sleep/write pattern `UpdateSharedResource()` uses, and watch the "Using a CountdownEvent" section start producing the wrong result too, proof that the `CountdownEvent` itself was never what was protecting the data.
