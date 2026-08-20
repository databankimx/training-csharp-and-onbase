# Locking

## Introduction

Locking implements mutual exclusion, ensuring only one thread (or a limited number of threads) can access a shared resource at a time. This lesson covers three built-in tools for it: `Monitor`, `Mutex`, and `Semaphore`.

---

## `Monitor`: Exclusive Access to an Object

```csharp
Monitor.Enter(syncObject);
try
{
    syncObject.Id = iCopy + 1;
    Nap(2);
}
finally
{
    Monitor.Exit(syncObject);
}
```

`Monitor.Enter()` acquires an exclusive lock on an object, any other thread calling `Monitor.Enter()` on the *same* object blocks until it's released with `Monitor.Exit()`. Always pair these inside a `try`/`finally`. If an exception happened between `Enter()` and `Exit()` and there were no `finally`, the lock would never release, permanently blocking every other thread waiting on it. That's a deadlock, and it's exactly what the `finally` block prevents.

---

## `Mutex`: Exclusive Access, With a Timeout

```csharp
if (excludedObject.Mutex.WaitOne(5000))
{
    try
    {
        // do the work
    }
    finally
    {
        excludedObject.Mutex.ReleaseMutex();
    }
}
else
{
    Console.WriteLine("failed to acquire the mutex in time...");
}
```

A `Mutex` (short for "mutual exclusion") works similarly to `Monitor`, but it's a real operating-system object, which means it can coordinate across process boundaries, not just within one program. `WaitOne(5000)` waits up to 5 seconds for the mutex to become available, returning `false` if it times out instead of blocking forever. That gives you a graceful way to handle "this took too long" instead of getting stuck.

---

## `Semaphore`: Multiple Simultaneous Holders

```csharp
var pool = new Semaphore(0, 3);   // starts with 0 available, allows up to 3 at once

pool.WaitOne();   // take a slot (blocks if none available)
// ... do work ...
pool.Release();   // give a slot back
```

Where `Monitor` and `Mutex` allow exactly one holder, a `Semaphore` allows up to *N* at once, useful for things like limiting how many threads can hit a database connection pool or a rate-limited API simultaneously.

Notice the constructor here: `new Semaphore(0, 3)` starts with **zero** available slots (not the maximum of 3). Nothing can proceed until something explicitly calls `Release()` to make slots available:

```csharp
for (int i = 0; i < 5; i++)
{
    var thread = new Thread(ResourceWorkWithSemaphore);
    thread.Start(i + 1);
}

Nap(1);   // give all 5 threads time to start and block on WaitOne()

pool.Release(3);   // now 3 of the 5 waiting threads can proceed
```

Five threads spawn and immediately block, waiting for an available slot. After the main thread releases 3 slots, three of them proceed. As each finishes and releases its slot back, one of the remaining two waiting threads gets to go, until all five have run.

---

## Which One Do You Actually Want?

- **One thread at a time, within your own program**: `Monitor` (usually via the `lock` keyword, which is shorthand for `Monitor.Enter`/`Exit`).
- **One thread at a time, possibly across different programs/processes**: `Mutex`.
- **Up to N threads at a time**: `Semaphore`.

---

## Try It Yourself

Run the semaphore example and watch the thread numbers print as they request, enter, and release access. Change `new Semaphore(0, 3)` to `new Semaphore(0, 1)` (allowing only one at a time, like a mutex) and predict how the output changes before running it again.
