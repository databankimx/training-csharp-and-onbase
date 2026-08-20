# Task Parallel Library

## Introduction

The Task Parallel Library (TPL) introduces `Task` as a unit of asynchronous work. It's built on top of the thread pool, but hides most of the manual thread management, letting you focus on *what* work needs to happen and how pieces of it depend on each other, rather than on threads directly.

---

## `Task` and `Task<TResult>`

```csharp
Task task = Task.Run(() => Step(1));                          // no return value
Task<double> resultTask = Task.Run(Ch07SharedFunctions.DoIntensiveCalculations);  // returns a double
```

Use plain `Task` when the work doesn't need to hand anything back. Use `Task<TResult>` when it does, `TResult` is whatever type the work produces.

```csharp
double result = resultTask.Result;
```

Reading `.Result` blocks until the task finishes, then gives you its return value. This is important: if you read `.Result` before checking that the task is actually done, you'll wait for it right there, which is exactly what makes it safe to use for getting a correct final answer.

---

## A Wrong Result, Then a Correct One

```csharp
// Wrong: doesn't wait for the tasks to finish
var tasks = new Task[NumberOfIterations];
for (int i = 0; i < NumberOfIterations; i++)
    tasks[i] = Task.Run(() => result += Ch07SharedFunctions.DoIntensiveCalculations());

Console.WriteLine($"Result: {result}");   // likely 0, or incomplete
```

```csharp
// Correct: Task<double>.Result implicitly waits
var tasks = new Task<double>[NumberOfIterations];
for (int i = 0; i < NumberOfIterations; i++)
    tasks[i] = Task.Run(Ch07SharedFunctions.DoIntensiveCalculations);

foreach (var task in tasks) result += task.Result;
```

The first version queues up 32 tasks and immediately tries to print a result, before any of those tasks have necessarily finished. The second version has each task return its own value and only combines them by reading `.Result`, which waits for each one. Same idea as `Ch07SharedFunctions`' thread pool example from the main lesson, one level up.

---

## `Parallel.For`

```csharp
Parallel.For(0, NumberOfIterations, i => result += Ch07SharedFunctions.DoIntensiveCalculations());
```

`Parallel.For` runs loop iterations across multiple threads and *does* wait for all of them to finish before returning. But this particular example still produces a wrong result, and for a different reason than the `Task` example above: multiple iterations updating the same `result` variable at the same time is a **race condition**. `+=` isn't a single atomic step, it's read the current value, add to it, write the new value back, and two threads can interleave those steps in a way that loses an update.

```csharp
Parallel.For(0, NumberOfIterations,
    () => 0d,                                                                       // each thread starts its own running total at 0
    (i, state, interimResult) => interimResult + Ch07SharedFunctions.DoIntensiveCalculations(),  // accumulate locally
    (lastInterimResult) => result += lastInterimResult                              // combine once per thread, at the end
);
```

This three-delegate overload gives every participating thread its own private accumulator. No thread ever touches another thread's running total mid-calculation, only the final combine step touches the shared `result`, and only once per thread rather than once per iteration. This is the general pattern for safely parallelizing an accumulation: keep the frequent work thread-local, merge shared state only at the end.

---

## Keeping UI Updates on the UI Thread

```csharp
// Throws when clicked, not on the UI thread
Task.Factory.StartNew(() => UpdateLabel("BtnCannot"));

// Works correctly
Task.Factory.StartNew(() => UpdateLabel("BtnCan"), CancellationToken.None, TaskCreationOptions.None,
    TaskScheduler.FromCurrentSynchronizationContext());
```

`TaskScheduler.FromCurrentSynchronizationContext()` captures the UI thread's context and tells the task to run there specifically, instead of on an arbitrary thread pool thread. Click both buttons in the demo form, one throws a real cross-thread exception (shown in a message box), the other updates the label correctly.

---

## Task Continuations

A continuation is a task that starts only once another task (or set of tasks) finishes. This lesson runs the same three steps four different ways:

```csharp
// All independent, run at the same time
Parallel.Invoke(() => Step(1), () => Step(2), () => Step(3));

// Step 3 depends on Step 1 only
Task task1 = Task.Run(() => Step(1));
Task task2 = Task.Run(() => Step(2));
Task task3 = task1.ContinueWith(antecedent => Step(3));
Task.WaitAll(task2, task3);

// Step 3 depends on both Step 1 and Step 2
task3 = Task.Factory.ContinueWhenAll(new[] { task1, task2 }, antecedent => Step(3));
task3.Wait();

// Step 3 depends on whichever of Step 1 / Step 2 finishes first
task3 = Task.Factory.ContinueWhenAny(new[] { task1, task2 }, antecedent => Step(3));
Task.WaitAll(task1, task2, task3);
```

Same three pieces of work every time, only the dependency relationship between them changes. The total time each scenario takes is a direct consequence of that relationship, all-independent finishes fastest, depends-on-both finishes slowest (since it has to wait for whichever prerequisite takes longer).

---

## Try It Yourself

Run the project section by section and read the console output carefully. For the continuation scenarios specifically, predict roughly how long each one should take before running it (each `Step()` sleeps for 2 seconds), then check your prediction against the actual output.
