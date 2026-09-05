# Chapter 7 Supplemental 04: Asynchronicity

## What This Is

Three ways to run the same simulated work twice, timed against each other: plain sequential calls, `Task`-returning methods waited on with `Task.WaitAll()`, and genuine `async`/`await` methods.

No functional bugs found. One standards fix applied: `internal class Program` corrected to `internal static class Program`, since every member is already `static`.

---

## `async` and `await`, in Brief

From the lesson notes:

```
Starting from C# 5.0, the "async" and "await" keywords were added to create a consistent,
  simple coding pattern for asynchronicity

When a method is marked with the "async" keyword, it is limited to three return types:
- void
- Task
- Task<T>

The reason for the return type being a Task rather than a normal value is to allow the method
  to be called with await
```

That return-type list deserves a caveat the notes don't give: **`async void` is a trap.** It exists solely so event handlers can be async (`private async void BtnGo_Click(...)`), because the event delegate signature demands `void`. Everywhere else it's a defect, for one specific reason: an exception thrown inside an `async void` method cannot be caught by the caller. There's no `Task` to carry the fault, so it goes straight to the process-level unhandled exception handler and typically kills the application.

**Rule: `async Task` unless you are literally writing an event handler.**

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

`SimulateWork()` is the one real piece of work in the whole project. Everything else is a different wrapper around calling it twice. Each section resets `counter = 0` first, so the printed instance numbers restart at 1 each time.

### 1. Sequential

```csharp
Console.WriteLine(SimulateWork());
Console.WriteLine(SimulateWork());
```

Two full 2-second sleeps, back to back. Output is strictly ordered: `Start 1`, `Stop 1`, `Start 2`, `Stop 2`. **~4 seconds.**

### 2. Task-returning, waited with `WaitAll`

```csharp
private static Task<double> SimulateWorkAsync() => Task.Run(SimulateWork);

// ...
Task[] tasks = [SimulateWorkAsync(), SimulateWorkAsync()];
Task.WaitAll(tasks);
```

Note the array initializer starts **both** tasks before `WaitAll` is reached — `SimulateWorkAsync()` is called twice during array construction, and each call queues work immediately. Starting the work and waiting for it are separate acts, and the overlap only exists because of the gap between them.

Note also that `SimulateWorkAsync` has **no** `async` keyword and no `await`. It just returns a `Task<double>` that somebody else can wait on. This is worth internalizing: `async` is not required to write asynchronous code. It's syntactic support for *consuming* tasks, not for producing them. **~2 seconds.**

### 3. Genuinely `async`/`await`

```csharp
private static async Task<double> SimulateWorkAwait()
{
	return await Task.Run(SimulateWork);
}

// ...
Task<double>[] tasks = [SimulateWorkAwait(), SimulateWorkAwait()];
foreach (var task in tasks) await task;
```

Same start-then-wait separation. **~2 seconds.**

The `foreach ... await` awaits in array order, not completion order — if task 2 finished first, its `await` simply returns immediately when reached. That's fine for waiting, but note `Task.WhenAll(tasks)` would be the idiomatic form, and it differs in one meaningful way: if *both* tasks fault, `foreach`/`await` throws on the first failure and the second exception is lost, while `WhenAll` aggregates them.

### Compare the Timings

Sequential lands near 4 seconds; the other two both land near 2. And here's the point the lecture notes make that's easy to miss:

> The `async`/`await` version isn't *faster* than the plain `Task.Run()`/`WaitAll()` version. They're doing the same underlying work in the same way.

`async`/`await` is a more composable **syntax** for waiting on tasks, not a different execution strategy. The speedup in both cases comes entirely from `Task.Run` overlapping the two sleeps. The keywords add nothing to throughput.

That's a genuinely common misconception — that `await` makes things fast. It doesn't. It makes waiting *non-blocking*, which matters enormously in a UI or a web server (see `Supplemental.02`), and it makes asynchronous code readable as straight-line logic. Neither is a speed improvement.

---

## Worth Noticing: The Sleep Is a Lie

```csharp
Thread.Sleep(2000);
```

`SimulateWork` is CPU-thread-blocking, not truly asynchronous. `Task.Run(SimulateWork)` doesn't make it non-blocking — it just moves the blocking onto a **pool thread** instead of the calling thread. Two tasks means two pool threads, both sitting there doing nothing for two seconds.

Real asynchronous I/O doesn't work that way. `await Task.Delay(2000)` or `await httpClient.GetAsync(...)` consumes **no thread at all** while waiting; the thread returns to the pool and the continuation resumes later. That's why an async web server can hold thousands of concurrent requests on a handful of threads.

The distinction is worth carrying:

| | Threads used while waiting |
|---|---|
| `Task.Run(() => Thread.Sleep(2000))` | one, blocked |
| `await Task.Delay(2000)` | none |

`Task.Run` around blocking work is the right tool when you're stuck with a synchronous API. It is not a substitute for a genuinely async one. The rule of thumb: **`Task.Run` for CPU-bound work, native `async` APIs for I/O-bound work.** Wrapping a synchronous database call in `Task.Run` gets you off the UI thread but doesn't scale on a server, because you've consumed a thread either way.

---

## Worth Noticing: `++counter` Is a Race

```csharp
int instance = ++counter;
```

In `SynchronousCalls()` this is perfectly safe — one thread, sequential calls, instances 1 and 2 every time.

In `WaitingForAsyncTasks()` and `AwaitingAsyncTasks()`, two pool threads execute `SimulateWork()` concurrently and both increment the same shared `static int counter`. `++` is read-modify-write, exactly like the `+=` in `Supplemental.03`, and exactly the failure `Supplemental.05.RaceConditions` covers in full.

In practice you'll almost always see `Start 1` and `Start 2`, because the increment happens in the first nanoseconds of each task while the other is still being scheduled. But it is not *guaranteed*, and with enough tasks you would eventually see two instances claim the same number.

This isn't a bug worth fixing here — the counter is cosmetic, and the demo works. It's called out because it's a nice illustration of how ordinary single-threaded code silently becomes unsafe the moment it's handed to `Task.Run`. **The defect wasn't introduced by changing `SimulateWork`; it was introduced by changing who calls it.** That's what makes concurrency bugs hard to review: the broken line and the line that broke it are in different methods.

`Interlocked.Increment(ref counter)` would make it correct — see `Supplemental.08.LockFreeAlternatives`.

The shared `static Stopwatch sw` has the same property, though it's safe here because only one section runs at a time.

---

## Worth Noticing: `SimulateWorkAwait()` Doesn't Need `await Task.Run(...)`

```csharp
private static async Task<double> SimulateWorkAwait()
{
	// In a method using the "async" keyword, you return the result of the Task
	return await Task.Run(SimulateWork);

	// Note: Could have done this, but it would be silly
	// `return await SimulateWorkAsync();`
}
```

The commented-out alternative is worth reading. `SimulateWorkAsync()` already does `Task.Run(SimulateWork)` and hands back the resulting `Task<double>`. Awaiting that would work identically — `await` doesn't care whether the `Task` it's given was constructed inline or returned from another method. It just needs something awaitable.

The comment calling this "silly" is about redundancy, not correctness. There's no reason to have two methods making the same `Task.Run` call.

There's a further observation lurking here: `SimulateWorkAwait` could drop `async`/`await` **entirely**:

```csharp
private static Task<double> SimulateWorkAwait() => Task.Run(SimulateWork);
```

...which is precisely `SimulateWorkAsync`. When a method's only `await` is on its return expression, the `async` machinery is pure overhead — the compiler builds a state machine to do nothing but unwrap a task and rewrap it. Returning the task directly is measurably cheaper.

The one real exception: if the method has a `try`/`catch` or a `using` around the await, you **must** keep `async`/`await`. Returning the task directly would let the method exit — disposing resources, leaving the `try` — before the task completes.

Both methods existing separately is pedagogically deliberate: it lets you see `Task`-returning and `async`-keyword styles side by side and confirm they produce identical timings.

---

## Worth Noticing: Resynchronizing an `async` Method From `Main()`

```csharp
// This syntax is a little funny - resynchronizing an async...
bool done = Task.Run(async () => await AwaitingAsyncTasks()).Result;
while (!done) Thread.Sleep(1);
```

`Main()` here is not `async` — matching the console `Main()` signature convention used throughout this training set — so it can't `await` directly. This is the workaround: wrap the call in `Task.Run(async () => ...)`, then block on `.Result` to force a synchronous wait from a synchronous context.

The `while (!done)` loop is **redundant**. `.Result` already blocks until the task completes, so `done` is guaranteed `true` by the time that line is reached, and the loop body never executes. Harmless, but worth recognizing as belt-and-suspenders rather than necessary.

### Why the `Task.Run` Wrapper Is There

You might reasonably ask why it isn't just `AwaitingAsyncTasks().Result`. In a console app, that would work — but in a UI or classic ASP.NET application, that exact pattern **deadlocks**:

1. The UI thread blocks on `.Result`.
2. The async method's continuation is scheduled back onto the UI thread (via the captured `SynchronizationContext`).
3. The UI thread is blocked waiting, so it never runs the continuation.
4. The task never completes, so `.Result` never returns.

Wrapping in `Task.Run` moves the async work onto a pool thread with **no** synchronization context, so its continuations run freely and the deadlock is avoided. That's why this "funny syntax" became a widespread idiom — it's a real workaround for a real hazard, not just noise.

### The Modern Alternative

Since C# 7.1, `Main` can be async:

```csharp
private static async Task Main()
{
	// ...
	await AwaitingAsyncTasks();
	GenericFunctions.Pause();
}
```

No wrapper, no `.Result`, no deadlock risk, no redundant loop. This sidesteps the whole pattern and is what modern console apps do. The existing code is preserved to match the training set's convention, but the async `Main` is the one to reach for in new work.

The general principle: **blocking on async code is where async bugs come from.** `.Result` and `.Wait()` are the two most common causes of deadlocks in .NET. Once you start awaiting, await all the way up.

---

## Try It Yourself

- Confirm sequential is ~4s and both parallel versions are ~2s.
- Replace `Thread.Sleep(2000)` with `await Task.Delay(2000)` in an async variant and note the timings are the same while thread usage isn't.
- Change the arrays to four calls and watch the parallel versions stay near 2 seconds.
- Delete the `while (!done)` loop and confirm nothing changes.
- Convert `Main` to `private static async Task Main()` and await directly — the cleanest version of this program.

---

## Takeaways

- `async` return types are `void`, `Task`, and `Task<T>`; `async void` is for event handlers only.
- Exceptions in `async void` can't be caught by the caller and usually kill the process.
- A method can be asynchronous without `async` — just return a `Task`.
- `async`/`await` is composable syntax for waiting, not a faster execution strategy.
- Starting tasks and awaiting them are separate acts; the overlap lives in the gap between.
- `Task.WhenAll` aggregates multiple failures where `foreach`/`await` reports only the first.
- `Task.Run` around blocking code moves the block to a pool thread; it doesn't eliminate it.
- Use `Task.Run` for CPU-bound work and native async APIs for I/O-bound work.
- `++` on shared state is a race, and code becomes unsafe by being *called* differently.
- When the only `await` is on the return expression, returning the task directly is cheaper — unless a `try` or `using` wraps it.
- `.Result` and `.Wait()` deadlock in contexts with a synchronization context; `Task.Run` wrapping avoids it.
- `async Task Main()` has been supported since C# 7.1 and beats every workaround.
