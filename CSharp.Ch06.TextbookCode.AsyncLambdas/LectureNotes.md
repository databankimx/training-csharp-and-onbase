# Ch06 Textbook Code: Async Lambdas

## What This Is

A single button whose `Click` handler is an `async` lambda expression, the fifth delegate-syntax variant this chapter shows off (after named methods, anonymous methods, expression lambdas, and statement lambdas), this one specifically demonstrating that lambdas can be `async` too.

No bugs found.

---

## Click It Several Times in a Row

```csharp
runAsyncButton.Click += async (button, buttonArgs) =>
{
    int trial = ++Trials;
    statusLabel.Text = "Running trial " + trial.ToString() + "...";
    await DoSomethingAsync();
    statusLabel.Text = "Done with trial " + trial.ToString();
};

async Task DoSomethingAsync()
{
    await Task.Delay(3000);
}
```

Click "Run Async" once, and after 3 seconds the status bar reads "Done with trial 1". But click it three times in quick succession, and you'll see the status bar update through overlapping trials, "Running trial 2..." appearing while trial 1 is still in flight, rather than trial 2 waiting for trial 1 to finish. That's the entire lesson made visible: `await` doesn't block the UI thread while it waits, it yields control back so the form stays responsive (you can click the button again immediately) and so multiple `await`ed operations can be in flight at once.

---

## Worth Noticing: This Is a Preview, Not the Full Story

This project doesn't explain *how* `async`/`await` actually works under the hood (the state machine the compiler generates, the synchronization context that gets captured and resumed on, why `Task.Delay` doesn't block a thread the way `Thread.Sleep` would). That's intentional, Chapter 7 covers multithreading and asynchronous processing in full. This project's job is narrower: showing that the lambda syntax you already know from earlier in this chapter (`(params) => expression`) extends naturally to `async (params) => { ... await ... }`, one more shape in the same family, before the deeper material arrives.
