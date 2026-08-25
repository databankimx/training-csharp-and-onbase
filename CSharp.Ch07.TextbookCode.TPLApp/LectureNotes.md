# Ch07 Textbook Code: TPL App

## What This Is

The direct source `CSharp.Ch07.Supplemental.03.TaskParallelLibrary`'s `RunSequential()`/`RunTasks()`/`RunTasksCorrected()`/`RunParallelFor()`/`RunParallelForCorrected()` methods were adapted from, confirmed by the identical `NUMBER_OF_ITERATIONS = 32` constant and the same progression from broken to fixed.

`Main()` calls `RunParallelForCorrected()`, the fully-fixed version, the other four methods (`RunSequencial`, `RunParallelFor`, `RunTasks`, `RunTasksCorrected`) are defined but never called, available to swap in manually.

No bugs found.

---

## Worth Noticing: `RunTasks()`'s Commented-Out "Fix"

```csharp
static void RunTasks()
{
    double result = 0d;
    Task[] tasks = new Task[NUMBER_OF_ITERATIONS];
    for (int i = 0; i < NUMBER_OF_ITERATIONS; i++)
    {
        tasks[i] = Task.Run(() => result += Utils.CommonFunctions.DoIntensiveCalculations());
    }

    Task.WaitAll(tasks);

    //// We collect the results
    //foreach (var task in tasks) {
    //    result += task.Result;
    //}

    Console.WriteLine("The result is {0}", result);
}
```

Unlike the main lesson's equivalent method (`RunTasks()` in `Supplemental.03.TaskParallelLibrary`, adapted from this one), this version *does* correctly call `Task.WaitAll(tasks)` before printing, so the timing bug (printing before the tasks finish) isn't present here. What *is* still present is the same race condition `Supplemental.03.TaskParallelLibrary`'s lecture notes describe for `RunParallelFor()`: 32 tasks all doing `result += ...` on the same shared `double` with no synchronization is a genuine race, `Task.WaitAll()` guarantees all 32 finish before `Console.WriteLine` runs, but it doesn't stop them from stepping on each other's updates while they're running concurrently.

The commented-out block right below shows what the actual fix looks like, `RunTasksCorrected()`'s approach, each task returning its own value via `Task<double>` and summing the results afterward, rather than every task mutating one shared variable. Worth reading the commented block as a labeled "here's the wrong instinct" next to the working `RunTasksCorrected()` method further down the same file.

---

## Compare Against `Supplemental.03.TaskParallelLibrary`

Same five methods, same progression, this project is the original, unwrapped shape; the Supplemental version adds the `TaskScheduler`/UI demo and the four-scenario continuation comparison on top. Worth reading both if the race-condition-vs-timing-bug distinction didn't fully land the first time, seeing the identical logic in two slightly different presentations often helps.
