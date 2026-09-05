# Chapter 7 Supplemental 03: Task Parallel Library

## What This Is

The deepest Task Parallel Library (TPL) lesson in this chapter: `Task`/`Task<T>`, `Parallel.For` (twice — broken, then fixed), a `TaskScheduler` demo showing exactly why UI updates need the UI thread, and four increasingly complex task-continuation scenarios.

**One bug found and fixed** — see the section on `RunParallelForCorrected()` below.

Note the project is a console app that also pops up a WinForms dialog partway through (`OutputType=Exe`, not `WinExe`), preserved as originally structured. That's unusual but intentional: it lets a console-driven lesson demonstrate a UI-thread constraint without becoming a UI application.

---

## What TPL Actually Is

From the chapter notes:

```
The Task Parallel Library (TPL) introduces the "Task" as a unit of asynchronous work.
This uses the ThreadPool but further abstracts the inner workings of the threads from the developer.
```

That first sentence is the key reframing. Everything before this project dealt in **threads** — things that run. TPL deals in **tasks** — units of work that will eventually produce a result. The thread is an implementation detail the runtime manages.

This shift is what makes composition possible. You can't easily say "run this thread after that thread finishes"; you can trivially say `task1.ContinueWith(...)`.

### The Patterns TPL Replaced

TPL introduces **TAP** (Task-based Asynchronous Pattern), superseding two older models you'll still encounter in legacy code:

- **APM** (Asynchronous Programming Model) — built on `IAsyncResult`, requiring paired `BeginXxx`/`EndXxx` methods.
- **EAP** (Event-based Asynchronous Pattern) — a `XxxAsync` method plus a completion event. `BackgroundWorker` from `Supplemental.02` is EAP.

TAP replaced both with a single object that represents the operation, can be waited on, composed, cancelled, and inspected for failure.

### Two Forms

```
Task                 // used when no return value is needed
Task<TResult>        // used when a return is needed (specifies the return type expected)
```

That distinction turns out to be the entire fix for the first bug below.

### Ways to Create One

```
1. Create a task directly (var t = new Task(<delegate>)) and start it (t.Start())
2. Use the factory (TaskFactory.StartNew)
3. Use the shorthand (Task.Run) which wraps TaskFactory.StartNew
4. Use one of the continuation methods
```

`Task.Run` is the right default. `StartNew` exists for when you need the extra parameters — a cancellation token, creation options, or a scheduler — which is exactly why the `TaskScheduler` demo below uses it.

---

## `Task` vs. `Task<double>`: A Deliberate, Then-Fixed Race Condition

### Broken

```csharp
double result = 0d;
var tasks = new Task[NumberOfIterations];

for (int i = 0; i < NumberOfIterations; i++)
{
	tasks[i] = Task.Run(() => result += Ch07SharedFunctions.DoIntensiveCalculations());
}

Console.WriteLine($"{Environment.NewLine}Result: {result}");
Console.WriteLine("We got the wrong result!");
```

Same shape of bug as the main lesson's `RunInThreadPool()`, and deliberately so. This project re-teaches the exact same lesson — **don't use a result before you've actually waited for it** — one abstraction level up.

Note the irony: `tasks` is populated with every task handle needed to wait properly. The information is right there. Nothing is ever done with it. That's realistic; this bug usually looks like an omission rather than a mistake.

### Fixed

```csharp
double result = 0d;
var tasks = new Task<double>[NumberOfIterations];

for (int i = 0; i < NumberOfIterations; i++)
	// Note: We are only executing the method in the Task - we'll get the value later
	tasks[i] = Task.Run(Ch07SharedFunctions.DoIntensiveCalculations);

// We can wait for all the tasks to complete
// Task.WaitAll(tasks);
// But that is optional here, because "Wait" is implicit when we call Task<T>.Result below

foreach (var task in tasks) result += task.Result;
```

The fix isn't adding an explicit wait — it's switching to `Task<double>` and reading `.Result`, which **blocks until that specific task finishes**.

Three things worth noticing:

**`Task.Run(Ch07SharedFunctions.DoIntensiveCalculations)` is a method group conversion.** No lambda, no parentheses. The comment in the source spells out the equivalent explicit form. It works because the method's signature (`double` return, no parameters) matches `Func<double>`.

**The commented-out `Task.WaitAll(tasks)` is correct but redundant.** `.Result` waits implicitly. Including both is harmless; the comment explains why only one is needed.

**The race is gone by construction, not by locking.** In the broken version, 32 tasks all did `result +=` on one shared variable. In the fixed version, each task returns its own value and the summing happens on a single thread in a `foreach`. There is nothing to synchronize because nothing is shared. This is the same principle as the main lesson's separate `result`/`result2` variables, and it's the single most reliable concurrency technique available: **don't share mutable state.**

The source also notes the LINQ equivalent, `tasks.Sum(task => task.Result)`, for after Chapter 8.

---

## `Parallel.For`: Same Symptom, Different Root Cause

### Broken

```csharp
double result = 0d;

Parallel.For(0, NumberOfIterations,
	i => result += Ch07SharedFunctions.DoIntensiveCalculations());

Console.WriteLine($"{Environment.NewLine}Result: {result}");
Console.WriteLine("We got the wrong result!");
```

This is broken for a **different reason** than the `Task` version above, and the distinction is the most valuable thing in this project.

`Parallel.For` **does** wait for all iterations to complete before returning. The timing issue from `RunTasks()` genuinely does not apply here. Adding a wait would fix nothing, because the wait already happened.

This is a true **race condition**. Multiple iterations run on different threads simultaneously and perform `result += ...` on the same shared variable. `+=` is not atomic — it's read, add, write as three separate steps:

```
Thread A reads result (100.0)
Thread B reads result (100.0)     <- before A writes
Thread A writes 100.0 + 5 = 105.0
Thread B writes 100.0 + 5 = 105.0 <- A's update silently lost
```

The result comes out as *some* number, just not reliably the correct one, and the wrongness varies between runs. `Supplemental.05.RaceConditions` is devoted entirely to this failure mode.

**Two bugs that look identical from the console are not the same bug.** One is a missing wait; one is unsynchronized shared state. Fixing the wrong one produces code that still fails, just less often — which is worse than failing consistently.

### Fixed

```csharp
Parallel.For(0, NumberOfIterations,
	// Interim result = 0d
	() => 0d,

	(i, state, interimResult) => interimResult + Ch07SharedFunctions.DoIntensiveCalculations(),

	// Final step after the calculations
	// we add the result to the final result
	(lastInterimResult) => result += lastInterimResult
);
```

The three-delegate overload — `localInit`, `body`, `localFinally` — gives each participating thread its own private `interimResult` accumulator. No thread ever touches another thread's running total.

Reading the delegates in order:

1. **`() => 0d`** — runs once per participating thread, producing that thread's starting accumulator.
2. **`(i, state, interimResult) => interimResult + ...`** — runs once per iteration. Note it **returns** the new accumulator rather than mutating anything; the returned value is threaded into the next iteration on that same thread. Purely local, no shared state.
3. **`(lastInterimResult) => result += lastInterimResult`** — runs once per *thread*, not per iteration, after that thread's iterations finish.

Only the final combine touches shared `result`, and it does so once per thread rather than once per iteration. With 32 iterations across (say) 8 threads, that's 8 shared writes instead of 32 — far fewer opportunities for a race.

**Worth being precise about this, though:** fewer opportunities is not zero opportunities. `result += lastInterimResult` in `localFinally` is still an unsynchronized read-modify-write on shared state, and it can still race in principle. A fully rigorous version would use a lock or `Interlocked` there. This code is safe in practice because the combines are staggered by the wildly different completion times of the CPU-heavy work — but "safe in practice" is a category worth naming rather than glossing over. `Supplemental.07.Locking` and `Supplemental.08.LockFreeAlternatives` supply the tools to close the gap properly.

The general shape is still the right one, and worth memorizing: **keep the hot, per-iteration work thread-local; merge shared state once at the end, under protection.**

### The `state` Parameter

The middle delegate's unused `state` is a `ParallelLoopState`, which exposes:

```
- Stop           // Stops all loop iterations
- Break          // Stops all iterations higher than the current one
```

`Stop()` means "we're done, abandon everything." `Break()` means "finish everything before me, skip everything after" — the parallel analogue of a sequential `break`, preserving the guarantee that all lower indices complete.

### One More Caveat from the Notes

```
* Note:  None of these methods guarantee parallel threads; they attempt this based on the state of the ThreadPool
```

`Parallel.For` may run everything on one thread if the pool is saturated. Never write code whose *correctness* depends on iterations actually running concurrently.

---

## A Bug Found and Fixed

`RunParallelForCorrected()` ended with:

```csharp
Console.WriteLine($"{Environment.NewLine}Result: {result}");
Console.WriteLine("We got the wrong result!");   // <- in the CORRECTED method
```

A copy-paste leftover from `RunParallelFor()`. The entire point of the method is that it produces the **right** result, and it announced the opposite.

This matters more than a typo normally would. The lesson's whole payoff is watching the corrected version print a correct, consistent value where the broken one didn't — and a learner comparing the two outputs would see identical "We got the wrong result!" messages and reasonably conclude the fix didn't work. A misleading message in teaching code teaches the wrong thing.

Changed to:

```csharp
Console.WriteLine("This time we got the right result!");
```

Note that `RunTasksCorrected()` was already correct — it prints no such line at all. Only the `Parallel.For` correction carried the stale message. Build verified after the change.

---

## `TaskScheduler.FromCurrentSynchronizationContext()`

```csharp
// BtnCannot_Click: throws
// Because this is not executed by the UI thread, it will throw an exception when attempting to update the UI
Task.Factory.StartNew(() => UpdateLabel("BtnCannot"));

// BtnCan_Click: works
// Here, we ensure that the UI thread executes the Task, so it can update the UI
Task.Factory.StartNew(() => UpdateLabel("BtnCan"), CancellationToken.None, TaskCreationOptions.None,
	TaskScheduler.FromCurrentSynchronizationContext());
```

Click **"Run Task that Cannot Update the UI"** and watch the actual cross-thread exception appear in a message box — a live demonstration of the exact constraint `Supplemental.02.UnblockingTheUI` describes: only the UI thread may touch UI controls.

`TaskScheduler.FromCurrentSynchronizationContext()` captures the current (UI) synchronization context and tells the task factory to run the work back on that context specifically, rather than on an arbitrary pool thread. This is `BackgroundWorker`'s automatic marshaling, made explicit and available to `Task`-based code.

Note this is why the demo uses `Task.Factory.StartNew` rather than `Task.Run` — the scheduler is the fourth parameter, and `Task.Run` doesn't expose it. That's the concrete case where the more verbose creation method earns its verbosity.

### Why the Exception Is Visible at All

```csharp
private void UpdateLabel(string message)
{
	try
	{
		LblSource.Text = message;
	}
	catch (Exception ex)
	{
		string nl = Environment.NewLine;
		while (ex != null)
		{
			MessageBox.Show($@"{ex.GetType().Name}: {ex.Message}{nl}{nl}Stack Trace:{nl}{ex.StackTrace}", ...);
			ex = ex.InnerException;
		}
	}
}
```

This detail is easy to skip past and shouldn't be. **An exception thrown inside a `Task` does not propagate to the caller.** It's captured into the task's `Exception` property and surfaces only when you `Wait()`, read `.Result`, or `await`. Neither click handler does any of those.

So without this `try`/`catch` inside `UpdateLabel`, clicking "Cannot" would appear to do *nothing at all* — no error, no label change, no crash. The demonstration only works because the exception is caught at the point it's thrown.

That's a genuine hazard worth carrying forward: **fire-and-forget tasks swallow their exceptions silently.** Any task you don't await needs its own error handling, or failures vanish.

The `while (ex != null)` loop walks the `InnerException` chain, echoing the exception-drilling technique from `Ch06.Supplemental.05.ExceptionHandling`. Useful here because TPL wraps faults in `AggregateException`, so the real cross-thread message is typically one level down.

---

## Four Continuation Scenarios

All four run the same `Step(1)`, `Step(2)`, `Step(3)` — each a 2-second sleep with start/end console output. Only the **dependency structure** changes. `SequentialSteps()` runs them plainly first, giving you a ~6-second baseline.

### Scenario 1 — All independent

```csharp
Parallel.Invoke(
	() => Step(1),
	() => Step(2),
	() => Step(3));
```

No ordering constraint at all. All three run concurrently. `Parallel.Invoke` blocks until all finish. **~2 seconds.**

### Scenario 2 — Step 3 depends on Step 1 only

```csharp
Task task1 = Task.Run(() => Step(1));
Task task2 = Task.Run(() => Step(2));
// Here, task3 only begins as a continuation of task1
Task task3 = task1.ContinueWith(antecedent => Step(3));
// We don't have to wait for task 1, since task 3 only starts after it has finished
Task.WaitAll(task2, task3);
```

Steps 1 and 2 start together; Step 3 begins once Step 1 finishes, whether or not Step 2 has. **~4 seconds.**

Note the comment on the wait: waiting on `task3` implicitly covers `task1`, because `task3` cannot even start until `task1` completes. Continuations encode their prerequisites, so you only wait on the leaves of the dependency graph.

The `antecedent` parameter is the completed `Task` that triggered the continuation — available for inspecting its result or checking `IsFaulted`. Unused here.

### Scenario 3 — Step 3 depends on both

```csharp
task3 = Task.Factory.ContinueWhenAll([task1, task2], antecedent => Step(3));
// We only need to wait for task 3, since both tasks 1 and 2 implicitly wait before task 3 can begin
task3.Wait();
```

Step 3 waits for whichever of Steps 1 and 2 finishes **last**. **~4 seconds** here (both steps take 2s), but this is the scenario that degrades worst when prerequisites have uneven durations — it's gated by the slowest.

### Scenario 4 — Step 3 depends on either

```csharp
task3 = Task.Factory.ContinueWhenAny([task1, task2], antecedent => Step(3));
// We don't know which task continues with task 3, so we wait for them all
Task.WaitAll(task1, task2, task3);
```

Step 3 begins as soon as the **first** of Steps 1/2 finishes, not waiting for the other.

Note the different wait strategy, and why: with `ContinueWhenAny`, waiting on `task3` alone would **not** cover both prerequisites — one of them may still be running. The loser of the race is not part of `task3`'s dependency chain, so it must be waited on explicitly or it'd be abandoned. This is exactly the kind of detail that's easy to get wrong and produces a fire-and-forget task with no error handling.

`ContinueWhenAny` is the right tool for redundant requests — query three mirrors, proceed with the first response.

### Watch the Timings

The console output includes each step's start and end. The total elapsed time for each scenario is a direct, visible consequence of its dependency shape. **The lesson isn't that parallelism is fast — it's that the dependency graph determines the floor.** No amount of parallelism beats your longest dependency chain.

---

## Try It Yourself

- Run `RunTasks()` several times and note how the wrong result varies.
- Change `Step`'s default duration per-call — `Step(1, 5)`, `Step(2, 1)` — and re-predict each scenario's total before running.
- Remove the `try`/`catch` from `UpdateLabel` and confirm the "Cannot" button silently does nothing.
- Add `Task.WaitAll(tasks)` to the broken `RunTasks()` and observe that it *still* comes out wrong — because the `result +=` race remains even after the timing is fixed. This is the clearest way to prove the two bugs are distinct.

---

## Takeaways

- TPL raises the unit of work from a thread (a thing that runs) to a task (a thing that will produce a result).
- `Task.Run` is the sensible default; `StartNew` exists for cancellation tokens, options, and schedulers.
- `Task<T>.Result` blocks implicitly, so an explicit `WaitAll` is often redundant.
- Returning values from tasks and summing on one thread beats sharing an accumulator.
- A missing wait and a race condition produce identical symptoms and require different fixes.
- `Parallel.For` waits for all iterations — its bugs are never timing bugs.
- `+=` is read-modify-write, not atomic, and loses updates under concurrency.
- The `localInit`/`body`/`localFinally` overload keeps per-iteration work thread-local.
- Reducing shared writes from per-iteration to per-thread narrows a race without fully closing it.
- Parallel methods never *guarantee* concurrency; never depend on it for correctness.
- Exceptions in un-awaited tasks are captured silently and disappear.
- `TaskScheduler.FromCurrentSynchronizationContext()` is explicit UI-thread marshaling for tasks.
- Continuations encode prerequisites, so wait on leaves — except with `ContinueWhenAny`, where the loser needs its own wait.
- The dependency graph, not the thread count, sets the minimum possible runtime.
