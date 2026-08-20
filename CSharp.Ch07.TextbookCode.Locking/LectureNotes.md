# Ch07 Textbook Code: Locking

## What This Is

Two short files. `Program.cs` walks through `Monitor.Enter`/`Exit` (unsafe, then exception-safe via `try`/`finally`), then the `lock` keyword shorthand for the same thing, a nice minimal companion to `CSharp.Ch07.Supplemental.07.Locking`'s fuller treatment and `CSharp.Ch07.Supplemental.08.LockFreeAlternatives`'s note about the same shorthand.

`LockThisBadSample.cs` is the textbook's dedicated demonstration of an anti-pattern this training set hadn't covered anywhere else: `lock(this)`. Neither class is ever instantiated or called from `Main()`, they exist purely to be read, not run.

No bugs found, and none to fix, `LockThisBadSample` is deliberately, correctly bad, that's the entire point of the file.

---

## Why `lock(this)` Is a Real Problem, Not Just a Style Nitpick

```csharp
public class LockThisBadSample
{
    public void MyMethod()
    {
        lock (this)
        {
            // Do Something here
        }
    }
}

public class UsingTheLockedObject
{
    public void OneMethod()
    {
        LockThisBadSample lockObject = new LockThisBadSample();
        lock (lockObject)
        {
            // Do something else
        }
    }
}
```

`lock (this)` locks on the object instance itself, which is `public`, any other code anywhere that has a reference to that same instance can also `lock` on it, entirely independently of `LockThisBadSample`'s own internals. `UsingTheLockedObject.OneMethod()` shows exactly that: it takes a `LockThisBadSample` instance and locks on it directly (`lock (lockObject)`), completely unrelated to whatever `MyMethod()` itself is doing internally.

That's the actual danger: `LockThisBadSample` has no way to know, or control, who else might be locking on the same object it uses for its own internal synchronization. Two entirely separate, uncoordinated pieces of code can end up contending for the exact same lock, for reasons that have nothing to do with each other, a classic setup for a hard-to-diagnose deadlock once the codebase grows large enough that nobody can see both sides of the problem at once.

---

## The Fix Is Simple: Use a Private, Dedicated Lock Object

```csharp
public class LockThisBetterSample
{
    private readonly object syncLock = new object();

    public void MyMethod()
    {
        lock (syncLock)
        {
            // Do Something here
        }
    }
}
```

A `private` field dedicated purely to locking can't be accessed or locked on by any code outside the class. Nobody else can even attempt to contend for it, because nobody else can obtain a reference to it in the first place. This is worth internalizing as a default habit, every class that needs its own lock should have its own private, dedicated lock object, `this` (or worse, any other `public` field) should never be the thing you lock on.
