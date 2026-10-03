# Chapter 7 Supplemental 03: Task Parallel Library

## What This Is

The main lesson and S01 worked directly with threads and the thread pool - you created things that *run*. The Task Parallel Library raises the abstraction level: instead of a thread, you work with a `Task` - a thing that *will eventually produce a result*. The thread is an implementation detail the runtime manages so you don't have to think about it.

What's abstracted away is thread lifecycle management. You no longer create, start, or join threads. You no longer need to bundle parameters into a carrier object. You don't need to create and signal `EventWaitHandle` instances. You describe the work and how pieces of it depend on each other, and the runtime figures out which threads to use.

The performance improvement over raw threads is generally modest for simple cases. The real improvement is in expressiveness: continuations, dependency graphs, and the `Parallel.For` overloads let you express complex coordination as a data structure rather than as manual signaling code.

This project covers `Task` and `Task<T>`, `Parallel.For`, a `TaskScheduler` demo, and four dependency shapes for task continuations.

---

## How to Write This Program

Add a `Step` helper and `Initialize`/`LogAndReset` helpers to `Program.cs` - they're used throughout:

```csharp
private const int NumberOfIterations = 32;
private static Stopwatch sw;

private static void Step(int num, int seconds = 2)
{
    Console.WriteLine($"Step {num} start...");
    Thread.Sleep(seconds * 1000);
    Console.WriteLine($"Step {num} end...");
}

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

---

### Mini-Program 1: Sequential - The Baseline

Clear `Main()` and write:

```csharp
Initialize();

double result = 0d;
for (int i = 0; i < NumberOfIterations; i++)
    result += Ch07SharedFunctions.DoIntensiveCalculations();

Console.WriteLine($"Result: {result}");
LogAndReset();
GenericFunctions.Pause();
```

Run it. Note the elapsed time - 32 calculations in series. This is the number everything else will beat.

### Mini-Program 2: Task - Parallel, But Wrong

Clear `Main()` and write:

```csharp
Initialize();

double result = 0d;
var tasks = new Task[NumberOfIterations];

for (int i = 0; i < NumberOfIterations; i++)
    tasks[i] = Task.Run(() => result += Ch07SharedFunctions.DoIntensiveCalculations());

Console.WriteLine($"Result: {result}");
Console.WriteLine("We got the wrong result!");
LogAndReset();
GenericFunctions.Pause();
```

Run it. The result will almost certainly be wrong.

Note the irony: `tasks` is sitting right there, holding a handle to every piece of running work. All the information needed to wait properly is present. Nothing is ever done with it. The array of task handles is a monument to good intentions that went nowhere.

### Mini-Program 3: Task\<T\> - Fixed

Clear `Main()` and write:

```csharp
Initialize();

double result = 0d;
var tasks = new Task<double>[NumberOfIterations];

for (int i = 0; i < NumberOfIterations; i++)
    tasks[i] = Task.Run(Ch07SharedFunctions.DoIntensiveCalculations);

foreach (var task in tasks) result += task.Result;

Console.WriteLine($"Result: {result}");
LogAndReset();
GenericFunctions.Pause();
```

Run it. Correct result, faster than sequential.

The fix is switching to `Task<double>` and reading `.Result`. Reading `.Result` on a `Task<T>` blocks until that task finishes - the wait is implicit in the read.

`Task.Run(Ch07SharedFunctions.DoIntensiveCalculations)` with no lambda is a method group conversion - it works because `DoIntensiveCalculations` returns `double` with no parameters, matching `Func<double>`. The accumulation in `foreach` happens on a single thread after all tasks are done. Nothing is shared. No race.

### Mini-Program 4: Parallel.For - A Different Kind of Wrong

Clear `Main()` and write:

```csharp
Initialize();

double result = 0d;

Parallel.For(0, NumberOfIterations,
    i => result += Ch07SharedFunctions.DoIntensiveCalculations());

Console.WriteLine($"Result: {result}");
Console.WriteLine("We got the wrong result!");
LogAndReset();
GenericFunctions.Pause();
```

Run it. Wrong result again - but for a completely different reason than Mini-Program 2.

`Parallel.For` **does** wait for all iterations to complete before returning. The timing issue from Mini-Program 2 genuinely does not apply here. Adding a wait would fix nothing.

This is a true race condition. Multiple iterations run simultaneously on different threads, and all of them do `result +=`, which is read-add-write - three separate, interruptible steps. Two threads can both read the same starting value before either writes back, and one update gets silently lost. The result is always *some* number, just not reliably the correct one, and the wrong value differs between runs.

Two bugs that look identical on the console are not the same bug. One is a missing wait; the other is unsynchronized shared state. Fixing the wrong one produces code that still fails - just less consistently, which is worse.

### Mini-Program 5: Parallel.For\<TLocal\> - Fixed

Clear `Main()` and write:

```csharp
Initialize();

double result = 0d;

Parallel.For(0, NumberOfIterations,
    () => 0d,
    (i, state, interimResult) => interimResult + Ch07SharedFunctions.DoIntensiveCalculations(),
    (lastInterimResult) => result += lastInterimResult
);

Console.WriteLine($"Result: {result}");
Console.WriteLine("This time we got the right result!");
LogAndReset();
GenericFunctions.Pause();
```

Run it. Correct result every time.

The three-delegate overload gives each participating thread its own private accumulator:

1. `() => 0d` - runs once per thread to produce its starting value.
2. `(i, state, interimResult) => interimResult + ...` - runs once per iteration. Returns the new accumulator; doesn't mutate anything shared.
3. `(lastInterimResult) => result += lastInterimResult` - runs once per thread after that thread's iterations finish. The only step that touches shared `result`.

The unused `state` parameter in the middle delegate is a `ParallelLoopState`, which exposes `Stop()` (abandon all iterations) and `Break()` (finish everything before this index, abandon the rest) for early exits.

### Mini-Program 6: TaskScheduler - Who Gets to Touch the UI?

This one opens a WinForms dialog from a console app - unusual but intentional. Add `ParentForm` to the project via the designer. Add two buttons (`BtnCannot` and `BtnCan`) and a label `LblSource`. Add a helper to the form's code-behind:

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
            MessageBox.Show($@"{ex.GetType().Name}: {ex.Message}{nl}{nl}Stack Trace:{nl}{ex.StackTrace}",
                @"Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            ex = ex.InnerException;
        }
    }
}
```

Wire `BtnCannot_Click`:

```csharp
private void BtnCannot_Click(object sender, EventArgs e)
{
    Task.Factory.StartNew(() => UpdateLabel("BtnCannot"));
}
```

Wire `BtnCan_Click`:

```csharp
private void BtnCan_Click(object sender, EventArgs e)
{
    Task.Factory.StartNew(() => UpdateLabel("BtnCan"), CancellationToken.None, TaskCreationOptions.None,
        TaskScheduler.FromCurrentSynchronizationContext());
}
```

Clear `Main()` and write:

```csharp
Console.WriteLine("Example in separate Windows form.");
new ParentForm().ShowDialog();
GenericFunctions.Pause();
```

Run it. Click "Cannot" - a message box shows the cross-thread exception. Click "Can" - the label updates without complaint.

`Task.Factory.StartNew` without a scheduler queues the work on a pool thread. That thread isn't the UI thread, and WinForms prohibits touching controls from any other thread.

`TaskScheduler.FromCurrentSynchronizationContext()` captures the UI thread's synchronization context - called from a button click handler, that's the UI thread - and routes the work back through it. Same mechanism `BackgroundWorker` uses automatically, now explicit.

Note `Task.Factory.StartNew` instead of `Task.Run`: the scheduler is the fourth parameter, and `Task.Run` doesn't expose it. That's the one case where the more verbose factory method earns its verbosity.

The `try`/`catch` inside `UpdateLabel` is also worth noticing. Without it, the "Cannot" exception disappears silently - exceptions in fire-and-forget tasks are captured into `Task.Exception` and surface only when you `Wait()`, read `.Result`, or `await`. Neither button handler does any of that. Fire-and-forget tasks swallow their exceptions. Every task you don't await needs its own error handling, or failures vanish without a trace.

### Mini-Program 7: Sequential Steps - The Baseline

Clear `Main()` and write:

```csharp
Console.WriteLine("Running steps sequentially...");
Step(1);
Step(2);
Step(3);
GenericFunctions.Pause();
```

Run it. Three 2-second steps in strict order - about 6 seconds total. This is the baseline for the continuation scenarios.

### Mini-Program 8: Four Dependency Shapes

Clear `Main()` and write all four scenarios in sequence, each with its own pause:

```csharp
// Scenario 1: all independent
Console.WriteLine("Steps 1, 2, and 3 are all independent");
Parallel.Invoke(() => Step(1), () => Step(2), () => Step(3));
GenericFunctions.Pause();

// Scenario 2: Step 3 depends on Step 1 only
Console.WriteLine("Step 3 depends on Step 1");
Task task1 = Task.Run(() => Step(1));
Task task2 = Task.Run(() => Step(2));
Task task3 = task1.ContinueWith(antecedent => Step(3));
Task.WaitAll(task2, task3);  // task1 is implicitly covered by task3
GenericFunctions.Pause();

// Scenario 3: Step 3 depends on both 1 and 2
Console.WriteLine("Step 3 depends on both Step 1 and Step 2");
task1 = Task.Run(() => Step(1));
task2 = Task.Run(() => Step(2));
task3 = Task.Factory.ContinueWhenAll([task1, task2], antecedent => Step(3));
task3.Wait();  // task1 and task2 are implicitly covered by task3
GenericFunctions.Pause();

// Scenario 4: Step 3 depends on whichever of 1 or 2 finishes first
Console.WriteLine("Step 3 depends on either Step 1 or Step 2");
task1 = Task.Run(() => Step(1));
task2 = Task.Run(() => Step(2));
task3 = Task.Factory.ContinueWhenAny([task1, task2], antecedent => Step(3));
Task.WaitAll(task1, task2, task3);  // the loser isn't in task3's dependency chain
GenericFunctions.Pause();
```

Run each and watch the step start/end lines and elapsed time.

Scenario 1: ~2 seconds. All three overlap.
Scenario 2: ~4 seconds. Steps 1 and 2 overlap; Step 3 waits for Step 1, then runs.
Scenario 3: ~4 seconds. Steps 1 and 2 overlap; Step 3 waits for whichever finishes last.
Scenario 4: ~4 seconds. Step 3 starts as soon as the first of 1/2 finishes, without waiting for the other.

In Scenario 4, `task2` must be waited on explicitly even though `task3` doesn't depend on it. `ContinueWhenAny`'s loser is not part of `task3`'s dependency chain - leaving it un-waited means abandoning it mid-execution.

Continuations encode their prerequisites, so you wait on the leaves of the dependency graph, not every node. The dependency graph sets the minimum possible runtime. No amount of parallelism beats your longest dependency chain.

---

## Summary: What the Numbers Look Like

| Mini-Program | Expected result | Approx. elapsed vs. sequential | Why |
|---|---|---|---|
| Sequential | Correct | Baseline (~32x calc time) | One at a time |
| `Task` (broken) | Wrong | Fast but wrong | No wait; race on `result` |
| `Task<double>` (fixed) | Correct | Faster (parallelized across cores) | `.Result` waits; accumulation is single-threaded |
| `Parallel.For` (broken) | Wrong | Fast but wrong | Race on `result +=` |
| `Parallel.For<TLocal>` (fixed) | Correct | Fastest | Thread-local accumulation, minimal contention |
| Continuation scenarios | N/A | ~2s / ~4s / ~4s / ~4s | Dependency graph sets minimum |

`Parallel.For<TLocal>` is consistently fastest for this accumulation pattern - no shared state during the hot path means no contention, and the merge step is trivial. The continuation scenarios don't speed up the work itself; they let you express *which* work depends on *which other* work as code, rather than as manual signaling.

---

## Takeaways

- TPL raises the unit of work from a thread (a thing that runs) to a task (a thing that produces a result).
- `Task.Run` is the sensible default; `StartNew` exists for cancellation tokens, options, and schedulers.
- `Task<T>.Result` waits implicitly - a separate `WaitAll` is often redundant.
- A missing wait and a race condition produce identical-looking wrong output and require different fixes.
- `Parallel.For` waits for all iterations - its bugs are never timing bugs.
- The three-delegate `Parallel.For` overload keeps per-iteration work thread-local and merges once per thread.
- Fire-and-forget tasks swallow exceptions silently. Every un-awaited task needs its own error handling.
- `TaskScheduler.FromCurrentSynchronizationContext()` is explicit UI-thread marshaling for tasks.
- `ContinueWhenAny`'s loser isn't in the continuation's dependency chain and must be waited on separately.
- The dependency graph sets the minimum runtime. Parallelism cannot beat the longest chain.
