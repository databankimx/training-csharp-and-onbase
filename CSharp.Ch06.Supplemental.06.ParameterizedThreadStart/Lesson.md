# Chapter 6 Supplemental 06: Parameterized Thread Start

## What This Is

A short, focused companion to `CSharp.Ch06.DelegatesEventsAndExceptions`'s background-thread demo. This one uses the `ParameterizedThreadStart` overload of `Thread`'s constructor — which lets you pass a single `object` argument into the thread's entry point — contrasted directly with the plain `ThreadStart` overload (no parameters) used in the main lesson.

Despite being a threading demo, the real subject is still delegates: which constructor overload gets selected, and what the delegate carries with it. No bugs found.

---

## Worth Noticing: `internal class Program`, Not `static`

Every other `Program.cs` in this training set is `internal static class Program`. This one is deliberately `internal class Program` — not static — because the lesson itself requires an actual **instance** to exist.

```csharp
// Static method delegate:
var newThread = new Thread(Program.DoWork);
newThread.Start(42);

// Instance method delegate:
var w = new Program();
newThread = new Thread(w.DoMoreWork);
newThread.Start("The answer.");
```

`DoWork` is `static`, so it's referenced through the class itself (`Program.DoWork`). `DoMoreWork` is an instance method, so it needs an actual object to be called on (`w.DoMoreWork`).

Making `Program` non-static isn't an oversight — it's the only way to have a static and an instance delegate example side by side in the same class. A `static class` cannot contain instance members or be instantiated, so the lesson wouldn't compile.

The `#pragma warning disable S2325` around `DoMoreWork` is the same acknowledgment from the analyzer's side: SonarQube correctly notices the method doesn't touch instance state and suggests making it static, which would defeat the entire demonstration. The suppression is deliberate and commented.

---

## The Two Delegates

```csharp
// By making this method static, when we use it, it becomes a static delegate
private static void DoWork(object data)
{
	Console.WriteLine("Static thread procedure. Data='{0}'", data);
}

// Although otherwise very similar, this non-static method becomes an instance delegate
private void DoMoreWork(object data)
{
	Console.WriteLine("Instance thread procedure. Data='{0}'", data);
}
```

Identical signatures — `void` returning, one `object` parameter — so both satisfy `ParameterizedThreadStart` equally. `Thread` neither knows nor cares which is which.

The difference is what the delegate carries. As covered in Supplemental 01, an instance-method delegate stores both the method *and* the target object (`w`), while a static-method delegate has a `null` target. That distinction becomes practically important with threads: the thread keeps the delegate alive for its entire lifetime, which means `w` cannot be garbage collected until the thread finishes. A long-running thread holding an instance delegate pins that object in memory.

---

## `ParameterizedThreadStart` vs. Plain `ThreadStart`

`Thread`'s constructor has two relevant overloads:

```csharp
public delegate void ThreadStart();
public delegate void ParameterizedThreadStart(object obj);
```

`new Thread(Program.DoWork)` matches the second, because `DoWork`'s signature — `void DoWork(object data)` — fits `ParameterizedThreadStart` and not `ThreadStart`. Nothing in the call states which overload is wanted; the compiler picks it by matching the shape of the method being passed in. That's method group conversion doing overload resolution, the same mechanism seen in Supplementals 01 and 02.

Having chosen that overload, `Start()` now requires an argument, and it flows straight through into `data`:

```csharp
newThread.Start(42);              // data == 42
newThread.Start("The answer.");   // data == "The answer."
```

Compare this to `CSharp.Ch06.DelegatesEventsAndExceptions`'s thread demo:

```csharp
var t1 = new Thread(delegate ()
{
	MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
});
t1.Start();
```

That one uses the plain `ThreadStart` overload — no parameters at all — which is why `Start()` there takes no argument. Two different `Thread` constructor overloads, chosen by the shape of the method passed in. Worth reading both back to back to see the distinction concretely rather than just conceptually.

---

## The `object` Parameter Is a Real Limitation

`ParameterizedThreadStart` takes exactly one `object`. That's the whole API, and it has consequences.

**You lose type safety.** `Start(42)` boxes the `int` into an `object`, and `DoWork` receives something it has to cast back before doing anything useful. Nothing at compile time prevents `Start("forty-two")` from being passed to a method expecting a number — you find out at runtime, on a background thread, where the exception is hardest to observe.

Note that this demo sidesteps the problem entirely: it only passes `data` to `Console.WriteLine`, which accepts `object` and calls `ToString()`. Real code has to cast, and should do so defensively:

```csharp
if (data is int value) { /* use value */ }
```

**You get one argument.** Passing multiple values means bundling them into a class, a tuple, or an array — every caller and every thread procedure agreeing on an untyped contract the compiler can't verify.

**The modern alternative.** A closure avoids both problems:

```csharp
int answer = 42;
var thread = new Thread(() => DoWork(answer));   // ThreadStart, fully typed
thread.Start();
```

The lambda captures `answer` with its real type, and any number of variables can be captured the same way. `ParameterizedThreadStart` is essentially a pre-C# 2.0 workaround for the absence of closures, which is why you'll encounter it in older code far more often than you'll write it.

Beyond that, `Task` and `Task.Run` have largely replaced raw `Thread` for new work. Chapter 7 covers that ground.

---

## What's Not Being Shown

Worth noting for accuracy, since this demo is small enough to look like a complete pattern:

- **No `Join()`.** Nothing waits for either thread to finish. `Start()` returns immediately, and the program only stays alive because `GenericFunctions.Pause()` follows.
- **No ordering guarantee.** The two threads may print in either order, or interleave. Source order does not determine execution order.
- **No exception handling inside the threads.** An unhandled exception on a background thread is not caught by the `try`/`catch` in `Main()` — it terminates the process. That's the concrete version of the point made in the main chapter lesson: exceptions surface where the invocation happens, not where the delegate was set up. A thread procedure generally needs its own `try`/`catch`.

---

## Takeaways

- `Thread` accepts either a `ThreadStart` (no parameters) or a `ParameterizedThreadStart` (one `object`); the overload is chosen by the shape of the method passed in.
- Whether `Start()` takes an argument follows from which overload was selected.
- Static and instance methods satisfy the same delegate type, but an instance delegate carries its target — and a running thread keeps that object alive.
- A `static class` can't host both examples, which is why `Program` here is deliberately non-static.
- The single `object` parameter means no type safety and no multiple arguments; cast defensively.
- Prefer a closure (`() => DoWork(answer)`) over `ParameterizedThreadStart` in new code, and prefer `Task` over raw `Thread`.
- Threads need their own exception handling; `Main()`'s `try`/`catch` won't cover them.
