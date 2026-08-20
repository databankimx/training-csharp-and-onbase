# Ch07 Textbook Code: Method Synchronization

## What This Is

The `[MethodImpl(MethodImplOptions.Synchronized)]` attribute, a way to synchronize an entire method's body without writing an explicit `lock` block. Genuinely new content, this attribute doesn't appear anywhere else in this training set. Four threads, all ultimately locking on the exact same object, though it takes some tracing to see that clearly.

---

## The Bug That Was Here (Program-Breaking)

```csharp
CountdownEvent ce = new CountdownEvent(4);

WaitHandle.WaitAny(new WaitHandle[] { ce.WaitHandle });   // removed

var instance = new SingleThreaded();
new Thread(() => { ... }).Start();
// ...three more threads...

ce.Wait();
```

`WaitHandle.WaitAny(new WaitHandle[] { ce.WaitHandle })` sat between creating the `CountdownEvent` and spawning any of the four threads that eventually call `ce.Signal()`. `ce.WaitHandle` only becomes signaled once the countdown reaches zero, all four signals received. Since none of the four threads had been created yet when this line ran, it blocked `Main()` forever, right there, before a single thread was ever spawned. The whole program would hang immediately on launch.

**Removed.** The actual, correctly-placed wait already exists at the bottom of `Main()` (`ce.Wait();`), this stray line duplicated that same wait, just misplaced far too early to ever work.

---

## The Real Payoff: All Four Threads Contend for One Lock

This takes tracing through carefully, the connection isn't obvious from reading any single method in isolation.

```csharp
[MethodImpl(MethodImplOptions.Synchronized)]
public void OneCallInstance1() { ... }

[MethodImpl(MethodImplOptions.Synchronized)]
public void OneCallInstance2() { ... }

public void OneCallLockThis()
{
    lock (this) { ... }
}
```

`[MethodImpl(MethodImplOptions.Synchronized)]` on an *instance* method is exactly equivalent to wrapping the whole method body in `lock (this) { ... }`. So `OneCallInstance1()`, `OneCallInstance2()`, and `OneCallLockThis()` (via its explicit `lock (this)`) all lock on the *same* object, whatever `this` happens to be for that particular call, once they're all called on the same `instance`.

```csharp
new Thread(() => { instance.OneCallInstance1(); ce.Signal(); }).Start();
new Thread(() => { lock (instance) { ... } ce.Signal(); }).Start();
new Thread(() => { instance.OneCallInstance2(); ce.Signal(); }).Start();
new Thread(() => { instance.OneCallLockThis(); ce.Signal(); }).Start();
```

All four threads call methods (or, for the second thread, lock directly) on the exact same `instance`. That means **all four are contending for the exact same lock**, the `Synchronized` attribute on two of them, the explicit `lock (this)` inside `OneCallLockThis()`, and the explicit `lock (instance)` from outside, in the second thread, are all locking on the identical object.

---

## Try It Yourself

Run this and only *one* thread's `Console.ReadLine()` prompt will be "live" at a time, the other three are genuinely blocked waiting for the lock, even though all four threads have already started. Press Enter once, that thread finishes and releases the lock, and whichever thread the OS scheduler grants it to next becomes live. You'll need to press Enter four times total, once per thread, in whatever order the scheduler happens to grant the lock (not necessarily the order the threads were started in).

---

## Worth Connecting to `LockThisBadSample`

`CSharp.Ch07.TextbookCode.Locking`'s `LockThisBadSample.cs` warns that `lock (this)` is dangerous specifically because *external* code holding a reference to the same object can lock on it too, entirely independent of the class's own internals. This project makes that exact scenario concrete: the second thread's `lock (instance)` is precisely that external code, locking on the same object `OneCallInstance1()`, `OneCallInstance2()`, and `OneCallLockThis()` all separately, internally, lock on via `[MethodImpl(Synchronized)]`/`lock (this)`. Four independent-looking pieces of code, all quietly fighting over one lock they didn't coordinate on choosing.

---

## Worth Noticing: Unused Variables and Commented-Out Threads

`handle`, `handle2`, and `handle3` (three different `WaitHandle` variants) are created at the top of `Main()` but never referenced again anywhere, harmless leftover exploratory code, preserved as-is. The two commented-out `Thread` blocks at the bottom (`OneCallStatic1`/`OneCallStatic2`) are consistent with the `CountdownEvent(4)` count as written, uncommenting either one without also bumping that count to 5 or 6 would leave `ce.Wait()` blocking forever, waiting for a signal count that will never arrive.
