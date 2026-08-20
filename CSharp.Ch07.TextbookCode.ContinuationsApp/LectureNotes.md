# Ch07 Textbook Code: Continuations App

## What This Is

The minimal, distilled version of the continuation-dependency concept `CSharp.Ch07.Supplemental.03.TaskParallelLibrary`'s `StepsWithContinuation()` covers at length: `Step3` runs only after whichever of `Step1`/`Step2` finishes first.

```csharp
Task step1Task = Task.Run(() => Step1());
Task step2Task = Task.Run(() => Step2());
Task step3Task = Task.Factory.ContinueWhenAny(
    new Task[] { step1Task, step2Task },
    (previousTask) => Step3());

step3Task.Wait();
```

No bugs found. `Step1`, `Step2`, `Step3` are trivial (`Console.WriteLine` each), so there's no shared state and no race condition risk, only the console output's exact ordering varies between runs, since `Step1`/`Step2` complete near-instantaneously and whichever happens to finish first triggers `Step3`.

---

## Worth Noticing: Four Equivalent Alternatives, Left as Comments

Three commented-out alternatives sit at the very bottom of the file, after the closing brace of the `namespace` block, an unusual placement, but syntactically harmless (a comment is a comment regardless of where it sits), preserved exactly as downloaded:

```csharp
//Task step3Task = Task.Factory.ContinueWhenAll(
//        new Task[] { step1Task, step2Task },
//        (previousTask) => Step3());
////Task step3Task = Task.WhenAll(step1Task, step2Task).ContinueWith((previousTask) => Step3());
//Task step3Task = Task.WhenAny(step1Task, step2Task).ContinueWith((previousTask) => Step3());
```

Worth trying each one in place of the active `ContinueWhenAny` call and comparing:

- **`ContinueWhenAll`**: `Step3` waits for *both* `Step1` and `Step2`, not just the first.
- **`Task.WhenAll(...).ContinueWith(...)`**: the modern `Task`-returning equivalent of `ContinueWhenAll`, same "wait for both" behavior, different (more composable, `await`-friendly) API surface.
- **`Task.WhenAny(...).ContinueWith(...)`**: the modern equivalent of the active `ContinueWhenAny` line, same "wait for whichever finishes first" behavior.

This is the same four-scenario comparison `Supplemental.03.TaskParallelLibrary` walks through explicitly (`Parallel.Invoke`, `ContinueWith`, `ContinueWhenAll`, `ContinueWhenAny`), condensed here into one method with its alternatives sitting right next to it in the same file, worth reading both projects together, this one for the minimal side-by-side comparison, that one for a fuller worked example with timing you can actually observe.
