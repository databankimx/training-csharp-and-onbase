# Chapter 7 Supplemental 04: Asynchronicity

## What This Is

Three ways to run the same simulated work twice, timed against each other: plain sequential calls, `Task`-returning methods waited on with `Task.WaitAll()`, and genuine `async`/`await` methods. No bugs found. All methods are `static`, `internal class Program` corrected to `internal static class Program`.

---

## Three Shapes, Same Underlying Work

```csharp
private static double SimulateWork()
{
    int instance = ++counter;
    Console.WriteLine($"Start {instance}...");
    Thread.Sleep(2000);
    Console.WriteLine($"Stop {instance}...");
    return 2.0d;
}
```

`SimulateWork()` is the one real piece of work in this whole project, everything else is a different wrapper around calling it twice.

```csharp
// Sequential: two full 2-second sleeps, back to back
Console.WriteLine(SimulateWork());
Console.WriteLine(SimulateWork());
```

```csharp
// Task-returning, waited with WaitAll
private static Task<double> SimulateWorkAsync() => Task.Run(SimulateWork);
...
Task[] tasks = { SimulateWorkAsync(), SimulateWorkAsync() };
Task.WaitAll(tasks);
```

```csharp
// Genuinely async/await
private static async Task<double> SimulateWorkAwait()
{
    return await Task.Run(SimulateWork);
}
...
Task<double>[] tasks = { SimulateWorkAwait(), SimulateWorkAwait() };
foreach (var task in tasks) await task;
```

Compare the elapsed time each section prints: sequential should land close to 4 seconds (two full sleeps), the other two should both land close to 2 seconds (both sleeps overlapping). The `async`/`await` version isn't *faster* than the plain `Task.Run()`/`WaitAll()` version here, they're doing the same underlying work in the same way, `async`/`await` is a different, more composable *syntax* for waiting on tasks, not a different execution strategy in this particular comparison.

---

## Worth Noticing: `SimulateWorkAwait()` Doesn't Actually Need `await Task.Run(...)`

```csharp
private static async Task<double> SimulateWorkAwait()
{
    return await Task.Run(SimulateWork);

    // Note: Could have done this, but it would be silly
    // return await SimulateWorkAsync();
}
```

The commented-out alternative is worth reading: `SimulateWorkAsync()` already does `Task.Run(SimulateWork)` and returns the resulting `Task<double>`. Awaiting that would work identically, `await` doesn't care whether the `Task` it's given was constructed inline or handed back from another method, it just needs something awaitable. The comment calling this "silly" is really about redundancy, not correctness, there's no reason to have two methods doing the exact same `Task.Run()` call when one could simply await the other's result.

---

## Worth Noticing: Resynchronizing an `async` Method From `Main()`

```csharp
bool done = Task.Run(async () => await AwaitingAsyncTasks()).Result;
while (!done) Thread.Sleep(1);
```

`Main()` here is not itself `async` (matching the console `Main()` signature convention used throughout this whole training set), so it can't `await` directly. This is a workaround: wrap the call in `Task.Run(async () => ...)`, then block on `.Result` to force synchronous waiting from a synchronous context. The `while (!done)` loop right after is redundant given `.Result` already blocks until the task completes, `done` is guaranteed to be `true` by the time that line runs, the loop body never actually executes more than zero times. It's harmless, but worth recognizing as belt-and-suspenders rather than necessary. A cleaner (and, since C# 7.1, fully supported) alternative many modern console apps use instead is simply declaring `private static async Task Main()` and `await`ing directly, sidestepping this whole pattern.
