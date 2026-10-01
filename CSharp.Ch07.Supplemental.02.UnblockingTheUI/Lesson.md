# Chapter 7 Supplemental 02: Unblocking the UI

## What This Is

Two buttons. Both trigger the same 15-second `Thread.Sleep`. One freezes the window so completely that you can't move it. The other keeps the window fully responsive for the entire 15 seconds.

Everything else in Chapter 7 asks you to read numbers and infer what happened. This one you feel. Click both buttons before reading further.

---

## How to Write This Program

This is a WinForms project. Add a new Windows Forms App (.NET Framework), target net48, and convert the `.csproj` to SDK-style as usual.

### Step 1: The Form Shell

In the designer, add two buttons to `UiUnblockingForm`:

- `BtnBlock` -- Text: "Run Process Blocking the UI Thread"
- `BtnUnblock` -- Text: "Run Process Unblocking the UI Thread"

Add a constant and a helper to the code-behind:

```csharp
private const int SecondsToSleep = 15;

private static void Nap()
{
    Thread.Sleep(1000 * SecondsToSleep);
}
```

Build and run. The form opens. Nothing interesting yet, but the plumbing is intact.

### Mini-Program 1: The Blocking Button

Double-click `BtnBlock` in the designer and fill in the handler:

```csharp
private void BtnBlock_Click(object sender, EventArgs e)
{
    Nap();
    MessageBox.Show(@"BLOCKED - All Done!", @"Work Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
}
```

Run it and click the button. Try to move the window. Resize it. Click anything. The window is completely frozen for 15 seconds, and Windows will probably grey it out and slap "(Not Responding)" on the title bar like a disappointed parent.

There is nothing wrong with this code in any way a compiler or code review checklist would catch. Four lines, no threading, no shared state, no exceptions. It is also completely unacceptable in a real application, and that's the point. **UI responsiveness is a correctness property that no static analysis will flag for you.**

WinForms runs a message loop on the UI thread. Every user action -- mouse move, click, resize, repaint request -- arrives as a Windows message that the loop dequeues and dispatches to your event handlers. `BtnBlock_Click` is one of those dispatched handlers. While it runs, the loop doesn't loop. Messages pile up. Nothing repaints. After a few seconds of an unpumped queue, Windows assumes the process has died and adds the "(Not Responding)" badge.

Note this is not about `Thread.Sleep` specifically. A tight calculation loop, a synchronous database call, or a synchronous HTTP request produces the identical freeze. Anything that occupies the UI thread blocks the message loop. `Thread.Sleep` is just the most honest way to demonstrate it.

### Mini-Program 2: The Non-Blocking Button

Double-click `BtnUnblock` in the designer and add the handler and its delegates:

```csharp
private void BtnUnblock_Click(object sender, EventArgs e)
{
    var worker = new BackgroundWorker();
    worker.DoWork += OnDoWork;
    worker.RunWorkerCompleted += AfterDoWork;
    if (!worker.IsBusy) worker.RunWorkerAsync();
}

private static void OnDoWork(object sender, DoWorkEventArgs e)
{
    Nap();
}

private static void AfterDoWork(object sender, RunWorkerCompletedEventArgs e)
{
    MessageBox.Show(@"UNBLOCKED - All Done!", @"Work Complete", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
}
```

Run it and click the button. Drag the window. Resize it. Click Block if you want. The window stays fully interactive for the full 15 seconds.

`BtnUnblock_Click` returns almost immediately -- `RunWorkerAsync()` queues the work and hands control straight back, so the message loop resumes within microseconds. The 15-second nap happens on a pool thread where nobody is waiting on it.

Note the same `Nap()` method is called in both cases. The work is identical. Only the thread it runs on differs. That's the cleanest possible isolation of the one variable being demonstrated.

`BackgroundWorker` wraps thread creation behind two events:

| Event | Runs on |
|---|---|
| `DoWork` | a background (pool) thread |
| `RunWorkerCompleted` | the **UI thread**, automatically |

That automatic return to the UI thread is the detail worth paying attention to. `AfterDoWork` calls `MessageBox.Show()` directly with no `Invoke()` or `BeginInvoke()`. With raw `Thread` or `ThreadPool`, touching a UI control from a background thread throws `InvalidOperationException: Cross-thread operation not valid`. `BackgroundWorker` handles the marshaling so you don't have to.

The `if (!worker.IsBusy)` guard prevents calling `RunWorkerAsync()` on an already-running worker, which throws. Since a fresh `BackgroundWorker` is created on every click here it's technically redundant -- but it's the correct habit for the more common case where the worker is a reused field rather than a fresh local.

---

## Worth Knowing: There Is More to BackgroundWorker Than This

This demo uses the minimum viable subset. The full API also includes:

- **`ReportProgress` / `ProgressChanged`** -- set `WorkerReportsProgress = true`, call `ReportProgress(int)` from `DoWork`, update a progress bar in the handler (automatically marshaled to the UI thread).
- **`CancelAsync` / `CancellationPending`** -- set `WorkerSupportsCancellation = true`, poll `CancellationPending` inside `DoWork`. Cancellation is cooperative -- nothing forcibly stops the thread.
- **`e.Result` / `e.Error`** -- assign a result in `DoWork`, read it in `RunWorkerCompleted`. Exceptions thrown in `DoWork` are captured into `e.Error`. Always check it -- reading `e.Result` when `e.Error` is set rethrows the exception, and ignoring `e.Error` entirely swallows the failure silently. This demo's handler skips the check because `Thread.Sleep` cannot throw, not because ignoring errors is fine.

## Worth Knowing: This Is the Historical Option

`BackgroundWorker` predates the Task Parallel Library and `async`/`await`, both covered in the next two supplementals. In new code you'd write:

```csharp
private async void BtnUnblock_Click(object sender, EventArgs e)
{
    await Task.Run(() => Nap());
    MessageBox.Show(@"UNBLOCKED - All Done!", ...);
}
```

Same behavior, straight-line control flow, no event wiring. `BackgroundWorker` is still worth understanding -- it appears throughout existing WinForms codebases, and its two-event structure (work here, completion there, marshaling handled for you) is exactly what `await` automates. Understanding it explicitly makes what `await` does implicitly considerably less mysterious.

---

## Takeaways

- A GUI has exactly one thread allowed to touch its controls.
- Blocking that thread stops the message loop, which stops repainting, input, and everything else.
- The cause is occupying the thread -- not sleeping specifically.
- Code can be completely correct and still unacceptable because it blocks the UI.
- `BackgroundWorker.DoWork` runs on a background thread; `RunWorkerCompleted` is automatically marshaled back to the UI thread.
- Without that marshaling, touching a control from a background thread throws a cross-thread exception.
- Always check `e.Error` in `RunWorkerCompleted`. Exceptions in `DoWork` are captured, not propagated.
- `BackgroundWorker` cancellation is cooperative: your code must poll `CancellationPending`.
- `async`/`await` supersedes it in new code but does structurally the same thing.
