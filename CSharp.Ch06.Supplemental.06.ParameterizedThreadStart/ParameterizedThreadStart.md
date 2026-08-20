# Parameterized Thread Start

## Introduction

`Thread`'s constructor can take either a plain `ThreadStart` delegate (no parameters) or a `ParameterizedThreadStart` delegate (a single `object` parameter). This lesson demonstrates the parameterized version, using both a static and an instance method as the thread's entry point.

---

## Passing an Argument Into a New Thread

```csharp
private static void DoWork(object data)
{
    Console.WriteLine("Static thread procedure. Data='{0}'", data);
}
```

```csharp
var newThread = new Thread(Program.DoWork);
newThread.Start(42);
```

Because `DoWork` matches the shape `void(object)`, `Thread`'s constructor picks the `ParameterizedThreadStart` overload automatically. Whatever you pass to `.Start(...)` becomes `data` inside the method, here, `42` (boxed as `object`).

---

## Static vs. Instance Method as a Thread Target

```csharp
private void DoMoreWork(object data)
{
    Console.WriteLine("Instance thread procedure. Data='{0}'", data);
}
```

```csharp
var w = new Program();
newThread = new Thread(w.DoMoreWork);
newThread.Start("The answer.");
```

A static method is referenced through its class (`Program.DoWork`). An instance method needs an actual object to call it on (`w.DoMoreWork`), which is why this project creates an instance of `Program` itself, purely so both examples can live in the same file.

---

## Compare With the Plain `ThreadStart` Overload

`CSharp.Ch06.DelegatesEventsAndExceptions` starts a thread a different way:

```csharp
var t1 = new Thread(delegate ()
{
    MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
});
t1.Start();
```

No parameter, no argument passed to `Start()`. That's the plain `ThreadStart` overload. The difference comes down entirely to the signature of the method (or anonymous method) being passed in, one parameter selects `ParameterizedThreadStart`, none selects `ThreadStart`.

---

## Try It Yourself

Change `DoWork`'s parameter type from `object` to something more specific by casting inside the method, `(int)data`, and print `data + 1`. Then try starting that same thread with a string argument instead of `42` and see what happens at runtime (not compile time, since `object` accepts anything, the mismatch only shows up when the cast actually runs).
