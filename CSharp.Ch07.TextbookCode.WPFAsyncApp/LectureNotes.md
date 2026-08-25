# Ch07 Textbook Code: WPF Async App

## What This Is

The async/await companion to `CSharp.Ch07.TextbookCode.WpfApp`'s `BackgroundWorker` demo, same one-button layout, but running two I/O simulations with `async`/`await` instead. Unlike its sibling, **neither file here carries the "Warning! unedited code" header**, this looks like it may be Scott's own written content (or a lightly-adapted extension) rather than a raw publisher download, possibly built specifically to give the `WpfApp` demo a direct `async`/`await` point of comparison. Treated with the same `TextbookCode.*` structural conventions regardless, since that's how it's organized in the source archive.

No bugs found.

---

## The Interesting Part: Two `await`s That Still Only Take ~2 Seconds Total

```csharp
private async Task GetDataAsync()
{
    var task1 = Utils.CommonFunctions.ReadDataFromIOAsync();
    var task2 = Utils.CommonFunctions.ReadDataFromIOAsync();

    // await Task.WhenAll(task1, task2);

    lblResult.Content = await task1;
    lblResult2.Content = await task2;
}
```

Each `ReadDataFromIOAsync()` call (2 seconds each) starts running the moment it's called, `Task.Run()` inside it kicks off immediately, not when awaited. Both `task1` and `task2` are created back to back, *before* either `await` runs, so they're both already in flight concurrently by the time the first `await task1` line executes. By the time `task1` finishes (~2 seconds), `task2` has been running the whole time too and is already done (or nearly done), so `await task2` returns almost instantly afterward. Total elapsed time ends up close to 2 seconds, not 4, even though the code reads as two sequential `await` statements.

This is worth sitting with, since it's a common point of confusion: **the concurrency comes from when the tasks are *started*, not from how they're *awaited*.** Writing `await task1; await task2;` looks sequential, but it only behaves that way if `task2` wasn't already started before `task1`'s `await`. Compare against a version that awaited immediately after each creation (`var r1 = await ReadDataFromIOAsync(); var r2 = await ReadDataFromIOAsync();`), that genuinely would take ~4 seconds, since the second call wouldn't even start until the first had already finished.

The commented-out `await Task.WhenAll(task1, task2);` shows the more idiomatic way to express the same "wait for both" intent explicitly, worth trying in place of the two separate `await` lines and confirming the timing comes out the same either way.

---

## Worth Noticing: An Unused `using System.Data.SqlClient;`

```csharp
using System.Data.SqlClient;
```

Nothing in this file touches SQL at all. A harmless leftover, but worth noting as a small, concrete clue supporting the theory that this file was adapted or assembled from something else (possibly a different code sample that did use `SqlClient`) rather than being a from-scratch textbook download. Preserved as found.

---

## Worth Noticing: The Same Namespace Normalization Applied Here Too

Same situation as `CSharp.Ch07.TextbookCode.WpfApp`: `App.xaml`/`App.xaml.cs` originally used `CSharp.Ch07.TextbookCode.WPFAsyncApp` while `MainWindow.xaml`/`MainWindow.xaml.cs` used `WPFAsyncApp`. Normalized to `WPFAsyncApp` throughout, matching the same established-precedent reasoning documented in `WpfApp`'s lecture notes.
