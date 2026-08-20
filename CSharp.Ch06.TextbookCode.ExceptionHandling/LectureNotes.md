# Ch06 Textbook Code: Exception Handling

## What This Is

One button, six scenarios, one shared `ListBox` logging exactly what runs and in what order for each. Every scenario proves the same rule from a different angle: **`finally` always runs**, no matter how the `try`/`catch` above it exits.

No bugs found.

---

## Read the Output, Not Just the Code

Click "Test Finally" and read the list top to bottom:

```
NoException try
NoException finally
CaughtException try
CaughtException catch
CaughtException finally
UnCaughtException try
UnCaughtException finally
CatchThrowsException try
CatchThrowsException catch
CatchThrowsException finally
TryReturns try
TryReturns finally
CatchReturns try
CatchReturns catch
CatchReturns finally
```

Six scenarios, six different shapes of `try`/`catch`/`finally`, and `finally` appears in every single one, right before control actually leaves the method. Worth reading each scenario individually against this output:

- **`NoException`**: no exception thrown, `catch` never runs, `finally` still does.
- **`CaughtException`**: exception thrown and caught by a matching `catch`, `finally` runs after the `catch` block finishes.
- **`UnCaughtException`**: throws `ArgumentException`, but the local `catch (FormatException)` doesn't match it. `finally` still runs *before* the exception propagates out to the caller (`testFinallyButton_Click`'s own empty `catch` swallows it there). This is the one worth sitting with longest: `finally` runs even when the exception isn't caught anywhere inside this method at all.
- **`CatchThrowsException`**: the `catch` block itself throws a *new* exception (`FormatException`, different from the one it caught). `finally` still runs before that new exception propagates outward.
- **`TryReturns`**: `return` inside `try`. `finally` still runs before the method actually returns, this is exactly why `finally` is the right place for cleanup code, even an early `return` can't skip it.
- **`CatchReturns`**: same idea, `return` inside `catch` this time.

---

## Compare Against the Supplemental

`CSharp.Ch06.Supplemental.05.ExceptionHandling`'s own exception-handling content covers `try`/`catch`/`finally` too, but doesn't dedicate this much space specifically to `finally`'s "always runs" guarantee across this many different exit paths. Worth running both, this project is the deep, dedicated look at exactly that one property.
