# Chapter 7 Supplemental 07: Locking

## What This Is

`Supplemental.05` patched a single race condition with `Interlocked.Increment` -- the smallest possible fix for a single increment. This project covers the general solution for when your critical section is more involved than that: mutual exclusion locks. Three of them, each suited to a different scope and use case.

---

## How to Write This Program

### Step 1: The Model

Create `Models/Thing.cs`. It serves as the shared resource for all three sections, carrying whichever synchronization primitive a given section needs:

```csharp
public class Thing
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = null;
    public Mutex Mutex { get; set; } = null;
    public Semaphore SemaphorePool { get; set; } = null;

    ~Thing()
    {
        Mutex?.Dispose();
        SemaphorePool?.Dispose();
    }
}
```

Add a field and a helper to `Program.cs`:

```csharp
private static Thing excludedObject;
private static void Nap(int seconds) => Thread.Sleep(seconds * 1000);
```

---

### Mini-Program 1: Monitor

Clear `Main()` and write:

```csharp
var syncObject = new Thing();

for (int i = 0; i < 2; i++)
{
    int iCopy = i;
    Task.Run(() =>
    {
        Console.WriteLine($"Start task {iCopy}...");
        Monitor.Enter(syncObject);
        try
        {
            Console.WriteLine($"Object locked by task {iCopy}...");
            syncObject.Id = iCopy + 1;
            Console.WriteLine($"Object's ID is now {syncObject.Id}...");
            Nap(2);
        }
        finally
        {
            Monitor.Exit(syncObject);
            Console.WriteLine($"Object released by task {iCopy}...");
        }
    });
}

Nap(5);  // allow fire-and-forget tasks to complete
GenericFunctions.Pause();
```

Run it. Both "Start task N..." lines appear immediately -- they're printed before entering the lock. Then one "Object locked" line, a 2-second gap, an "Object released" line, and only then the second task's "Object locked" line. The gap is the second task blocked inside `Monitor.Enter`, politely waiting its turn.

The `try`/`finally` is mandatory, not stylistic. If anything threw between `Enter` and `Exit`, skipping the `finally` leaves the object **permanently locked** -- any other thread that tries to enter it blocks forever with no error and no stack trace pointing at the cause. A leaked lock is worse than a leaked file handle: a file handle wastes a resource; a leaked lock actively hangs other threads for the rest of the process's life.

In production, write `lock (syncObject) { ... }` instead. It compiles to exactly this `Monitor.Enter`/`try`/`finally`/`Exit` pattern, with the `finally` impossible to forget. The explicit form is shown here so you can see what the keyword actually does.

Two rules the compiler won't enforce but matter a great deal:

**Never lock on `this`, a public field, a `Type`, or a string literal.** Locking on something publicly reachable lets unrelated code accidentally acquire the same lock and either deadlock or serialize against your work for reasons nobody will ever figure out. The convention is a dedicated `private static readonly object` used for nothing else.

**Every thread must lock on the same instance.** If you moved `new Thing()` inside the loop, each task locks its own object -- zero protection, but the code compiles and looks defensively written. That bug class is particularly nasty.

### Mini-Program 2: Mutex

Add `UseResourceWithMutex()` to `Program.cs`:

```csharp
private static void UseResourceWithMutex()
{
    Console.WriteLine($"{Thread.CurrentThread.Name} is requesting the mutex...");
    if (excludedObject.Mutex.WaitOne(5000))
    {
        try
        {
            Console.WriteLine($"{Thread.CurrentThread.Name} has taken control of the mutex...");
            excludedObject.Name = Thread.CurrentThread.Name;
            Nap(2);
            Console.WriteLine($"{Thread.CurrentThread.Name} has completed work on the shared resource...");
        }
        finally
        {
            excludedObject.Mutex.ReleaseMutex();
            Console.WriteLine($"{Thread.CurrentThread.Name} has relinquished control of the mutex...");
        }
    }
    else
    {
        Console.WriteLine($"{Thread.CurrentThread.Name} has failed to acquire control of the mutex...");
    }
}
```

Clear `Main()` and write:

```csharp
excludedObject = new Thing { Mutex = new Mutex() };

for (int i = 0; i < 3; i++)
{
    var newThread = new Thread(UseResourceWithMutex) { Name = $"Thread {i + 1}" };
    newThread.Start();
}

Nap(7);  // allow threads to complete
GenericFunctions.Pause();
```

Run it. Three threads taking turns, one at a time, each holding the mutex for 2 seconds. Threads are named (`Name = $"Thread {i + 1}"`) so the output is readable. Naming threads costs nothing and is enormously useful in a debugger where the alternative is a list of anonymous numeric IDs.

The mutex is a property on the protected object (`excludedObject.Mutex`), which is good practice -- the lock travels with the resource, so you can't get a reference to one without having the other.

`WaitOne(5000)` is the key detail. Unlike bare `WaitOne()` which blocks forever, this gives up after 5 seconds and returns `false`, letting the `else` branch handle the timeout gracefully. With three threads each holding for 2 seconds, the worst case is 4 seconds -- comfortably inside the timeout -- so the `else` never fires in normal operation. That's fine. A timeout that never fires in testing is still correct, because it converts a silent hang into a handleable event when something actually goes wrong.

Two differences from `Monitor` that matter:

A `Mutex` can be **named**, making it a kernel object visible to every process on the machine. That's how "only one instance of this application may run at a time" is implemented. `Monitor` cannot do this.

A `Mutex` has **thread affinity**: only the thread that acquired it may release it. `ReleaseMutex()` from a different thread throws. This rules out acquiring in one place and releasing in a `ContinueWith`. These features make a `Mutex` substantially more expensive than a `Monitor` -- every acquire is a kernel transition. If you don't need cross-process coordination or a configurable timeout, use `lock`.

### Mini-Program 3: Semaphore

Add `ResourceWorkWithSemaphore()` to `Program.cs`:

```csharp
private static void ResourceWorkWithSemaphore(object num)
{
    Console.WriteLine($"Thread {num} requesting semaphore access...");
    excludedObject.SemaphorePool.WaitOne();

    Console.WriteLine($"Thread {num} enters the semaphore...");
    Nap(1);

    Console.WriteLine($"Thread {num} releases the semaphore...");
    Console.WriteLine($"Thread {num} previous semaphore count {excludedObject.SemaphorePool.Release()}...");
}
```

Clear `Main()` and write:

```csharp
Console.WriteLine("Main thread creates semaphore allowing three threads to access simultaneously...");

excludedObject = new Thing
{
    SemaphorePool = new Semaphore(0, 3)
};

for (int i = 0; i < 5; i++)
{
    var thread = new Thread(ResourceWorkWithSemaphore);
    thread.Start(i + 1);
}

Nap(1);

Console.WriteLine("Main thread releases 3 semaphore positions...");
excludedObject.SemaphorePool.Release(3);

Console.WriteLine("Main thread exits...");
Nap(5);  // allow threads to complete
GenericFunctions.Pause();
```

Run it. Five threads spawn and immediately block on `WaitOne()` -- the semaphore starts at 0. After a 1-second pause, the main thread releases 3 slots at once, letting 3 of the 5 through. As each finishes and calls `Release()`, a slot frees for one of the remaining 2, until all 5 have run.

`new Semaphore(0, 3)` starts with 0 available slots and a maximum of 3. This acts as a starting gate: the main thread decides exactly when the race begins. `new Semaphore(3, 3)` would let the first three threads through immediately on arrival -- useful in different scenarios, just less dramatically demonstrable.

`Semaphore.Release()` returns the count **before** this release -- how many slots were free the instant before this thread gave its back. A `0` means the semaphore was fully saturated; anything higher means capacity was going unused. In a real system that's how you tune a connection pool or rate limiter.

Unlike a `Mutex`, a `Semaphore` has no thread affinity. Any thread may call `Release()` regardless of whether it called `WaitOne()`. Calling `Release()` without a matching `WaitOne()` silently inflates available capacity -- exceeding the maximum eventually throws `SemaphoreFullException`, but staying under it just quietly allows more concurrent access than your design intended. The only guard is your own discipline.

Note `ResourceWorkWithSemaphore` has no `try`/`finally` around the `Nap(1)`. Nothing in `Thread.Sleep` can throw, so it's safe here. But compare it against the `Monitor` and `Mutex` sections, both of which use `try`/`finally` correctly. The inconsistency is worth noticing: in any real method where the protected work could throw, the omission would silently shrink the pool by one slot per exception and eventually deadlock every waiting thread with no error reported anywhere.

---

## Compare All Three

| | Scope | Simultaneous holders | Timeout | Thread affinity | Cost |
|---|---|---|---|---|---|
| `Monitor` / `lock` | In-process | 1 | via `TryEnter(obj, ms)` | Reentrant | Cheapest |
| `Mutex` | Cross-process when named | 1 | via `WaitOne(ms)` | Acquiring thread only | Kernel-level |
| `Semaphore` | Cross-process when named | N | via `WaitOne(ms)` | None | Kernel-level |

Default to `lock`. Reach for `Mutex` when you need cross-process coordination. Reach for `Semaphore` when you're rate-limiting access to a genuinely finite pool -- database connections, licence slots, outbound API calls.

---

## Takeaways

- Locking provides mutual exclusion -- the general fix for the race conditions in `Supplemental.05`.
- Always `try`/`finally` around held locks. A leaked lock hangs other threads silently with no error.
- `lock (obj) { }` is `Monitor.Enter`/`try`/`finally`/`Exit`. Use `lock` in real code.
- Never lock on `this`, a public field, a `Type`, or a string literal.
- Every thread must lock on the same instance. Per-thread lock objects compile fine and protect nothing.
- `Monitor` is in-process, reentrant, and cheap when uncontended.
- `Mutex` can coordinate across processes when named, at kernel-transition cost.
- A `Mutex` has thread affinity -- only the acquiring thread may release it.
- A bounded `WaitOne(ms)` converts a silent hang into a handleable event, even if it never fires in testing.
- `Semaphore` allows N concurrent holders and tracks only a count, not ownership.
- Unbalanced `Release()` silently inflates capacity. The only guard is your own discipline.
- A safety pattern applied inconsistently in one file is how it eventually disappears from the codebase entirely.
