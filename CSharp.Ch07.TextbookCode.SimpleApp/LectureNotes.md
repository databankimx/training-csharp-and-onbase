# Ch07 Textbook Code: Simple App

## What This Is

The direct source `CSharp.Ch07.MultithreadingAndAsynchronousProcessing`'s (the main lesson's) `RunSequential()`/`RunWithThreads()`/`RunInThreadPool()`/`RunInThreadPoolWithEvents()` methods were adapted from, confirmed by depending on `CSharp.Ch07.TextbookCode.Utils`, which is in turn the direct source `CSharp.SharedLibrary.HelperClasses.Ch07SharedFunctions` was adapted from.

`Main()` only calls `RunInThreadPool()`, the deliberately-incomplete version (the `// TODO: We will need a way to indicate that the thread pool thread finished the execution` comment says as much directly), the other three methods (`RunSequencial`, `RunWithThreads`, `RunInThreadPoolWithEvents`) are defined but never called, available to swap in manually for exploration.

No bugs found, this is the intentional "before" state of a progressive example, not a mistake.

---

## Worth Comparing: The Menu-Driven Version vs. This One

The main lesson wraps this same set of four approaches in an interactive menu, letting you pick any of the four and see the elapsed time for each. This project is the simpler, original shape: one method call, no menu, swap which method `Main()` calls by hand to explore the others. Worth reading both, this one shows the techniques in their most stripped-down form, the main lesson shows them wrapped in a UI that makes comparing all four side by side easier.

---

## Worth Noticing: `RunSequencial`, Not `RunSequential`

Preserved exactly as downloaded, including the misspelling in the method name itself. A small, harmless reminder that "unedited textbook code" means genuinely unedited, right down to typos in identifiers that still compile and run correctly regardless.
