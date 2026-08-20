# Chapter 7 Supplemental 03: Task Parallel Library

## What This Is

The deepest Task Parallel Library (TPL) lesson in this chapter: `Task`/`Task<T>`, `Parallel.For` (twice, broken then fixed), a `TaskScheduler` demo showing exactly why UI updates need the UI thread, and four increasingly complex task-continuation scenarios. No bugs found. Console app that also pops up a WinForms dialog partway through (`OutputType=Exe`, not `WinExe`), preserved as originally structured.

---

## `Task` vs. `Task<double>`: A Deliberate, Then-Fixed Race Condition

```csharp
// RunTasks(): broken
var tasks = new Task[NumberOfIterations];
for (int i = 0; i < NumberOfIterations; i++)
{
    tasks[i] = Task.Run(() => result += Ch07SharedFunctions.DoIntensiveCalculations());
}
Console.WriteLine($"Result: {result}");   // wrong: tasks haven't necessarily finished yet
```

```csharp
// RunTasksCorrected(): fixed
var tasks = new Task<double>[NumberOfIterations];
for (int i = 0; i < NumberOfIterations; i++)
    tasks[i] = Task.Run(Ch07SharedFunctions.DoIntensiveCalculations);

foreach (var task in tasks) result += task.Result;   // correct: .Result implicitly waits
```

Same shape of bug as the main lesson's `RunInThreadPool()`, and deliberately so, this project re-teaches the exact same lesson (don't use a result before you've actually waited for it) one abstraction level up, `Task` instead of raw `ThreadPool`. The fix isn't adding an explicit wait, it's switching to `Task<double>` and reading `.Result`, which blocks until that specific task finishes. 32 iterations running in parallel, each one adding into a shared `result` variable with no synchronization, is itself a second, subtler bug worth knowing about even in the "corrected" version, see the note on race conditions below.

---

## `Parallel.For`: Same Bug, Twice, Different Root Cause

```csharp
// RunParallelFor(): wrong result
Parallel.For(0, NumberOfIterations, i => result += Ch07SharedFunctions.DoIntensiveCalculations());
```

This one is broken for a different reason than the `Task` version above, `Parallel.For` **does** wait for all iterations to complete before returning, so the timing issue from `RunTasks()` doesn't apply here. This is a genuine **race condition**: multiple iterations running on different threads simultaneously perform `result += ...` on the same shared variable at the same time, and `+=` isn't atomic, it's read, add, write as three separate steps. Two threads can both read the same starting value before either writes back, and one update gets silently lost. The result comes out *some* number, just not reliably the correct one, and the wrongness isn't consistent between runs.

```csharp
// RunParallelForCorrected(): fixed with thread-local accumulation
Parallel.For(0, NumberOfIterations,
    () => 0d,                                                                    // per-thread initial value
    (i, state, interimResult) => interimResult + Ch07SharedFunctions.DoIntensiveCalculations(),  // per-thread accumulation
    (lastInterimResult) => result += lastInterimResult                            // combine once, per thread, at the end
);
```

The three-delegate overload gives each participating thread its own private `interimResult` accumulator, no thread ever touches another thread's running total. Only the final combine step (`result += lastInterimResult`) touches the shared `result` variable, and it does so once per thread rather than once per iteration, far fewer opportunities for a race, and specifically structured so the remaining additions don't overlap in practice. This is the standard shape for parallelizing an accumulation safely: keep the hot, per-iteration work thread-local, and only merge shared state at the very end.

---

## `TaskScheduler.FromCurrentSynchronizationContext()`: Why This Matters

```csharp
// BtnCannot_Click: throws
Task.Factory.StartNew(() => UpdateLabel("BtnCannot"));

// BtnCan_Click: works
Task.Factory.StartNew(() => UpdateLabel("BtnCan"), CancellationToken.None, TaskCreationOptions.None,
    TaskScheduler.FromCurrentSynchronizationContext());
```

Click "Run Task that Cannot Update the UI" and watch the actual cross-thread exception appear in a message box (`UpdateLabel`'s own `catch` displays it), a live demonstration of the exact constraint `CSharp.Ch07.Supplemental.02.UnblockingTheUI`'s lecture notes describe: only the UI thread may touch UI controls. `TaskScheduler.FromCurrentSynchronizationContext()` captures the current (UI) synchronization context and tells the task factory to run the given work back on that context specifically, rather than on an arbitrary thread pool thread. This is `BackgroundWorker`'s automatic UI-thread marshaling, made explicit and available for `Task`-based code instead.

---

## Four Continuation Scenarios: Same Three Steps, Four Different Dependency Shapes

All four scenarios in `StepsWithContinuation()` run the same `Step(1)`, `Step(2)`, `Step(3)`, only the *dependency structure* between them changes:

1. **All independent** (`Parallel.Invoke`): all three run concurrently, no ordering constraint at all.
2. **Step 3 depends on Step 1 only** (`task1.ContinueWith(...)`): Steps 1 and 2 start together, Step 3 begins only once Step 1 finishes, whether or not Step 2 has.
3. **Step 3 depends on both 1 and 2** (`Task.Factory.ContinueWhenAll(...)`): Step 3 waits for whichever of Steps 1/2 finishes last.
4. **Step 3 depends on either 1 or 2** (`Task.Factory.ContinueWhenAny(...)`): Step 3 begins as soon as the *first* of Steps 1/2 finishes, not waiting for the other at all.

Worth timing each scenario as it runs (the console output includes each step's start/end), the total elapsed time for each scenario is a direct, visible consequence of its dependency shape, scenario 1 finishes fastest (pure parallelism), scenario 3 finishes slowest (must wait for the slower of two prerequisites), scenarios 2 and 4 land somewhere in between depending on which step happens to finish first.
