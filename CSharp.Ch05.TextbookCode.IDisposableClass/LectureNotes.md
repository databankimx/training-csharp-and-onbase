# Ch05 Textbook Code: IDisposable Class

## What This Is

The most hands-on `IDisposable` lab in this chapter set, three buttons: **Create & Dispose** (explicit, deterministic cleanup), **Create** (an object left for the garbage collector to eventually finalize), and **Collect Garbage** (forces `GC.Collect()` on demand, so "eventually" doesn't mean waiting around).

No bugs found.

---

## Worth Actually Clicking Through

Click **Create & Dispose** a few times, you'll see immediate, deterministic console output for each one: `FreeResources`, `Dispose of managed resources`, `Dispose of unmanaged resources`, right as you click.

Click **Create** a few times instead, and nothing prints at all. Those objects are still sitting in memory, eligible for collection but not yet collected, the GC hasn't run.

Now click **Collect Garbage**. Watch the console output finally appear, for every uncollected `Create`d object at once, all in a batch, in whatever order the GC happens to process them. That's non-deterministic finalization made visible: the cleanup work is identical either way (`FreeResources` runs the same code path), what differs is *when* it runs and *whether you control the timing*.

---

## Worth Noticing: The Commented-Out `using` Alternative

```csharp
DisposableClass obj = new DisposableClass();
obj.Name = "CreateAndDispose " + ObjectNumber.ToString();
ObjectNumber++;
obj.Dispose();

// Version that uses using.
// Make an object.
//using (DisposableClass obj = new DisposableClass())
//{
//    obj.Name = "CreateAndDispose " + ObjectNumber.ToString();
//    ObjectNumber++;
//}
```

Both versions produce identical output, the commented-out `using` block is the idiomatic way to write exactly what the active code does manually: create, use, and guarantee `Dispose()` runs, even if an exception is thrown in between (which the manual version above it does *not* guarantee). Worth uncommenting and comparing side by side, since `using` doesn't change *what* gets disposed, only *how reliably* it happens.
