# Chapter 6 Supplemental 06: Parameterized Thread Start

## What This Is

A short, focused companion to the main chapter's background-thread demo. That one used `ThreadStart` (no parameters). This one uses `ParameterizedThreadStart`, which lets you pass a single `object` into the thread's entry point. Despite being a threading demo, the real subject is still delegates: which overload gets selected, and what the delegate carries.

---

## Worth Noticing Before You Write Anything: `internal class Program`

Every other `Program.cs` in this training set is `internal static class Program`. This one is deliberately non-static, because the lesson requires an actual instance to exist alongside a static method -- to show both side by side in the same class. A `static class` cannot contain instance members or be instantiated, so this shape is the only one that makes the example compile.

---

## How to Write This Program

### Step 1: Both Thread Entry Points

```csharp
internal class Program
{
    // Static method -- the delegate that results has a null Target
    private static void DoWork(object data)
    {
        Console.WriteLine("Static thread procedure. Data='{0}'", data);
    }

    // Instance method -- the delegate carries the instance it's bound to
    private void DoMoreWork(object data)
    {
        Console.WriteLine("Instance thread procedure. Data='{0}'", data);
    }
}
```

Both have identical signatures: `void`, one `object` parameter. Both satisfy `ParameterizedThreadStart` equally. `Thread` neither knows nor cares which is which.

### Step 2: Start Both Threads

```csharp
private static void Main()
{
    // Static method delegate
    var newThread = new Thread(Program.DoWork);
    newThread.Start(42);

    // Instance method delegate -- requires an actual instance
    var w = new Program();
    newThread = new Thread(w.DoMoreWork);
    newThread.Start("The answer.");
}
```

Run it. Both threads print their data, in whichever order the OS schedules them.

`new Thread(Program.DoWork)` matches `ParameterizedThreadStart` because `DoWork`'s signature -- `void DoWork(object data)` -- fits that delegate and not the plain `ThreadStart` (which takes no parameters). The compiler picks the overload by matching the shape of the method passed in. `Start(42)` flows straight through into `data`.

Compare to the main chapter's demo:

```csharp
var t1 = new Thread(delegate () { MessageBox.Show(...); });
t1.Start(); // no argument
```

That one used `ThreadStart` -- anonymous method, no parameters, `Start()` takes no argument. Two different `Thread` constructor overloads, chosen by the shape of the method being passed in.

---

## Static vs. Instance Binding

As covered in Supplemental 01: an instance-method delegate carries both the method *and* the target object (`w`). The thread holds that delegate for its entire lifetime, which means `w` cannot be garbage collected until the thread finishes. A long-running thread holding an instance delegate pins that object in memory.

A static-method delegate has a `null` target -- no object kept alive.

---

## The `object` Parameter Is a Real Limitation

`ParameterizedThreadStart` takes exactly one `object`. Consequences:

**No type safety.** `Start(42)` boxes the `int` to `object`. Nothing prevents `Start("forty-two")` being passed to a method expecting a number -- you find out at runtime, on a background thread, where the exception is hardest to observe.

**One argument.** Multiple values require bundling into a class, tuple, or array.

**The modern alternative.** A closure avoids both problems:

```csharp
int answer = 42;
var thread = new Thread(() => DoWork(answer)); // ThreadStart, fully typed
thread.Start();
```

The lambda captures `answer` with its real type, and any number of variables can be captured the same way. `ParameterizedThreadStart` is essentially a pre-C# 2.0 workaround for the absence of closures. `Task.Run` has largely replaced raw `Thread` for new code entirely -- Chapter 7 covers that.

---

## What's Not Being Shown

For accuracy, since this demo is small enough to look complete:

- **No `Join()`** -- nothing waits for either thread to finish.
- **No ordering guarantee** -- the two threads may print in either order.
- **No exception handling inside the threads** -- an unhandled exception on a background thread is not caught by `Main()`'s `try`/`catch`. Thread procedures generally need their own.

---

## Takeaways

- `Thread` accepts `ThreadStart` (no params) or `ParameterizedThreadStart` (one `object`); the overload is chosen by the method's shape.
- Static and instance methods satisfy the same delegate type, but an instance delegate carries its target -- a running thread keeps that object alive.
- `Program` is non-static deliberately -- a `static class` can't host both examples.
- The single `object` parameter means no type safety and one argument. Cast defensively.
- Prefer a closure over `ParameterizedThreadStart` in new code, and prefer `Task` over raw `Thread`.
- Thread procedures need their own exception handling.
