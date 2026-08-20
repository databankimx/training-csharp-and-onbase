# Asynchronicity

## Introduction

Since C# 5.0, the `async` and `await` keywords give asynchronous code a consistent, readable shape, code that waits for something without blocking the thread it's running on. This lesson compares three ways of running the same simulated work twice: plain sequential calls, `Task`-based calls, and genuine `async`/`await`.

---

## The `async` Method Contract

```csharp
private static async Task<double> SimulateWorkAwait()
{
    return await Task.Run(SimulateWork);
}
```

A method marked `async` can only return one of three things: `void`, `Task`, or `Task<T>`. Returning a `Task` (rather than the value directly) is exactly what makes the method awaitable, `await` needs something to actually wait on.

---

## Sequential vs. Task vs. Async/Await

```csharp
// Sequential
Console.WriteLine(SimulateWork());
Console.WriteLine(SimulateWork());
```

Two full 2-second operations, one after the other, roughly 4 seconds total.

```csharp
// Task, waited on with WaitAll
Task[] tasks = { SimulateWorkAsync(), SimulateWorkAsync() };
Task.WaitAll(tasks);
```

```csharp
// async/await
Task<double>[] tasks = { SimulateWorkAwait(), SimulateWorkAwait() };
foreach (var task in tasks) await task;
```

Both of the last two run the two operations concurrently, roughly 2 seconds total instead of 4. Run the project and compare the printed elapsed times directly, sequential should be about double the other two.

---

## `async`/`await` Is a Syntax, Not a Different Engine

In this comparison, the `Task`-based version and the `async`/`await` version take the same amount of time, they're both built on `Task.Run()` underneath. The real value of `async`/`await` is how it reads and composes: `await task;` looks like ordinary sequential code, even though the method is genuinely yielding control while it waits, no manual callback wiring, no `ContinueWith()` chains needed for simple cases.

---

## Calling an `async` Method From a Non-`async` `Main()`

```csharp
bool done = Task.Run(async () => await AwaitingAsyncTasks()).Result;
```

A plain `static void Main()` can't use `await` directly. One workaround is to wrap the call in `Task.Run(async () => ...)` and block on `.Result`, which forces the calling thread to wait synchronously for the async work to finish. It works, but it's worth knowing that many modern console apps sidestep this entirely by declaring `static async Task Main()` instead, which lets you `await` right there in `Main()`.

---

## Try It Yourself

Run the project and watch the "Start"/"Stop" messages print for each approach. In the sequential section, both "Start"/"Stop" pairs complete fully before the next begins. In the other two sections, watch both "Start" messages appear close together, before either "Stop" message, that interleaving is the concurrency made visible.
