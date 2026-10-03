# Chapter 7 Supplemental 04: Asynchronicity

## What This Is

S03 introduced `Task` as the abstraction over threads. This project introduces `async`/`await` as the abstraction over waiting on tasks. Where S03 used `.Result` and `Task.WaitAll()` to block synchronously while waiting for results, `async`/`await` lets you express the same wait without blocking the calling thread at all.

What's being abstracted is the continuation wiring. Without `await`, expressing "do this, then when it's done do that" requires explicit `ContinueWith` chains or blocking waits. `await` makes that wiring invisible - the compiler generates the continuation state machine behind the scenes, and the code reads like ordinary sequential logic.

The performance improvement over `Task.WaitAll` is most significant in UI and server contexts where blocking the calling thread has a cost. In a console app like this one, where the calling thread has nothing else to do, the elapsed times for `Task.WaitAll` and `async`/`await` will be nearly identical - which is itself the lesson. `async`/`await` is not a faster execution strategy; it's a more composable and non-blocking way to express the same work.

---

## How to Write This Program

Add fields and helpers to `Program.cs`:

```csharp
private static Stopwatch sw;
private static int counter;

private static void Initialize()
{
    sw ??= new Stopwatch();
    sw.Start();
}

private static void LogAndReset()
{
    if (sw == null) return;
    sw.Stop();
    Console.WriteLine($"Time Elapsed: {sw.Elapsed:c}");
    sw.Reset();
}
```

Also add the underlying work method - this is the only real work in the entire project, everything else is a different wrapper around calling it twice:

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

`counter` resets to zero before each section so the instance numbers restart at 1 each time. Note `++counter` is a race waiting to happen in the parallel sections - two threads incrementing the same `static int` is the same bug `Supplemental.05` covers in full. In practice it's nearly impossible to observe here because the increment takes nanoseconds while the other thread is still being scheduled. "Nearly impossible to observe" is not the same as correct, but it doesn't affect the lesson.

---

### Mini-Program 1: Sequential - The Baseline

Clear `Main()` and write:

```csharp
Initialize();
counter = 0;
Console.WriteLine(SimulateWork());
Console.WriteLine(SimulateWork());
LogAndReset();
GenericFunctions.Pause();
```

Run it. Output is strictly ordered: Start 1, Stop 1, Start 2, Stop 2. Elapsed time is close to 4 seconds - two full sleeps back to back. This is the baseline.

### Mini-Program 2: Task-Returning Method With WaitAll

Add this method above `Main()`:

```csharp
private static Task<double> SimulateWorkAsync()
{
    return Task.Run(SimulateWork);
}
```

Clear `Main()` and write:

```csharp
Initialize();
counter = 0;
Task[] tasks = [SimulateWorkAsync(), SimulateWorkAsync()];
Task.WaitAll(tasks);
LogAndReset();
GenericFunctions.Pause();
```

Run it. Both sleeps overlap. Elapsed time is close to 2 seconds.

`SimulateWorkAsync` has no `async` keyword and no `await`. It just wraps `SimulateWork` in `Task.Run` and returns the task. `async` is not required to write a method that returns a `Task` - it's syntax for *consuming* tasks, not for producing them. A method can participate fully in async code without the keyword.

Also notice how `Main()` builds the array. Both calls to `SimulateWorkAsync()` happen during array construction - the work is queued before `WaitAll` is ever reached. Starting the work and waiting for it are two separate acts, and the overlap only exists because of the gap between them.

### Mini-Program 3: async/await

Add this method above `Main()`:

```csharp
private static async Task<double> SimulateWorkAwait()
{
    return await Task.Run(SimulateWork);
}
```

Clear `Main()` and write:

```csharp
bool done = Task.Run(async () =>
{
    Initialize();
    counter = 0;
    Task<double>[] tasks = [SimulateWorkAwait(), SimulateWorkAwait()];
    foreach (var task in tasks) await task;
    LogAndReset();
    return true;
}).Result;
while (!done) Thread.Sleep(1);
GenericFunctions.Pause();
```

Run it. Elapsed time again close to 2 seconds - same as the `WaitAll` version.

The timings for Mini-Programs 2 and 3 are nearly identical because they're doing the same thing. `async`/`await` is a different way to express "start this work and wait for it later," not a different or faster execution strategy.

`SimulateWorkAwait` could drop `async`/`await` entirely and become identical to `SimulateWorkAsync`:

```csharp
private static Task<double> SimulateWorkAwait() => Task.Run(SimulateWork);
```

When a method's only `await` is its return expression, the `async` machinery is pure overhead - the compiler builds a state machine to unwrap a task and rewrap it. Both versions exist here specifically so you can place them side by side, confirm the timings match, and understand why.

The `foreach (var task in tasks) await task` pattern awaits in array order, not completion order. If task 2 finishes first, its `await` returns immediately when reached. The more idiomatic form is `await Task.WhenAll(tasks)`, and it differs in one meaningful way: if both tasks fault, `foreach`/`await` reports only the first exception while `WhenAll` aggregates all of them.

The `Task.Run(async () => ...)` wrapper in `Main()` is the workaround for `Main()` not being `async`. Calling `.Result` directly on an async method can deadlock in contexts with a synchronization context - the async continuation tries to resume on the calling thread, but the calling thread is blocked on `.Result`. `Task.Run` moves the work onto a pool thread with no synchronization context, so continuations run freely. The `while (!done)` loop afterward is redundant since `.Result` already blocks, but harmless. Since C# 7.1, `private static async Task Main()` is valid and eliminates the whole awkward dance.

---

## Worth Knowing: Task.Run Around Blocking Code Is Not Truly Async

`SimulateWork` calls `Thread.Sleep`, which blocks a thread. `Task.Run(SimulateWork)` doesn't make that non-blocking - it moves the blocking onto a pool thread instead of the calling thread. Two tasks means two pool threads sitting idle for two seconds each.

Real async I/O works differently. `await Task.Delay(2000)` or `await httpClient.GetAsync(...)` consumes no thread while waiting - the thread returns to the pool and the continuation resumes later. That's how an async web server handles thousands of concurrent requests on a small number of threads.

The practical rule: use `Task.Run` for CPU-bound work that needs to run off the calling thread. Use native async APIs (anything that already returns a `Task`) for I/O-bound work. Wrapping a synchronous database call in `Task.Run` gets you off the UI thread but still ties up a pool thread on a server. You've moved the problem, not solved it.

---

## Summary: Three Approaches, Two Outcomes

| Approach | Elapsed | Threads blocked during wait | Code clarity |
|---|---|---|---|
| Sequential | ~4s | Main thread, no choice | Highest - reads top to bottom |
| `Task` + `WaitAll` | ~2s | Main thread blocked at `WaitAll` | Moderate |
| `async`/`await` | ~2s | Calling thread *not* blocked during await | Highest for complex coordination |

The elapsed time improvement from sequential to either parallel approach is real - roughly 50% when the two work items take similar time. The improvement from `WaitAll` to `async`/`await` is zero in a console app. In a server or UI context where the calling thread has other work to do, `await` frees that thread during the wait - which is the entire point of `async`/`await` at scale.

---

## Takeaways

- `async` return types are `void`, `Task`, and `Task<T>`. `async void` is for event handlers only - exceptions escape to the process-level handler and typically kill the application.
- A method can return a `Task` without the `async` keyword. `async` is syntax for consuming tasks, not producing them.
- `async`/`await` is composable syntax for waiting, not a different or faster execution strategy.
- Starting tasks and awaiting them are separate acts. The parallelism lives in the gap between them.
- `Task.WhenAll` aggregates multiple failures; `foreach`/`await` reports only the first.
- `Task.Run` around blocking code moves the block to a pool thread - it doesn't eliminate it.
- Use `Task.Run` for CPU-bound work, native async APIs for I/O-bound work.
- `.Result` and `.Wait()` can deadlock in contexts with a synchronization context. `Task.Run` wrapping avoids it.
- `async Task Main()` has been valid since C# 7.1 and is cleaner than any workaround.
