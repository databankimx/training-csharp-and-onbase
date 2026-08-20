# Chapter 6 Supplemental 06: Parameterized Thread Start

## What This Is

A short, focused companion to `CSharp.Ch06.DelegatesEventsAndExceptions`'s background-thread demo: this one uses the `ParameterizedThreadStart` overload of `Thread`'s constructor, which lets you pass a single `object` argument into the thread's entry point, contrasted directly with the plain `ThreadStart` overload (no parameters) used in the main lesson. No bugs found.

---

## Worth Noticing: `internal class Program`, Not `static`

Every other `Program.cs` in this training set is `internal static class Program`. This one is deliberately `internal class Program`, not static, because the lesson itself requires an actual **instance** to exist:

```csharp
// Static method delegate:
var newThread = new Thread(Program.DoWork);
newThread.Start(42);

// Instance method delegate:
var w = new Program();
newThread = new Thread(w.DoMoreWork);
newThread.Start("The answer.");
```

`DoWork` is `static`, so it's referenced through the class itself (`Program.DoWork`). `DoMoreWork` is an instance method, so it needs an actual object (`w`) to be called on, `w.DoMoreWork`. Making `Program` non-static isn't an oversight, it's the only way to have both a static and an instance delegate example side by side in the same class.

---

## `ParameterizedThreadStart` vs. Plain `ThreadStart`

```csharp
private static void DoWork(object data)
{
    Console.WriteLine("Static thread procedure. Data='{0}'", data);
}
```

`Thread`'s constructor has two relevant overloads: `ThreadStart` (no parameters) and `ParameterizedThreadStart` (a single `object` parameter). `new Thread(Program.DoWork)` matches the second overload because `DoWork`'s signature (`void DoWork(object data)`) fits it. The argument passed to `.Start(42)` flows straight into `data`.

Compare this to `CSharp.Ch06.DelegatesEventsAndExceptions`'s `StartThread()`:

```csharp
var t1 = new Thread(delegate ()
{
    MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
});
t1.Start();
```

That one uses the plain `ThreadStart` overload (no parameters at all), which is why `Start()` there takes no argument. Two different `Thread` constructor overloads, chosen by matching the shape of the method being passed in, worth reading both back to back to see the distinction concretely rather than just conceptually.
