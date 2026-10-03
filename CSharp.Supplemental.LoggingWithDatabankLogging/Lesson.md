# Supplemental: Logging With Databank.Logging

## What This Is

The same prime-sieve demo as the other two logging projects, now using the internal `Databank.Logging` and `Databank.Models` NuGet packages. No local `Logging` class, no local `DatabankException` - those come from the packages.

The point of placing this third in the series is that the call sites are identical to the hand-rolled versions. Once you've seen how the `Logging` wrapper and `HandleException` are built in log4net and Serilog, this version shows what the same code looks like when that groundwork is already provided for you.

---

## The Difference From the Other Two Projects

```csharp
// LoggingWithLog4Net and LoggingWithSerilog:
using CSharp.Supplemental.LoggingWithLog4Net.HelperClasses;
using CSharp.Supplemental.LoggingWithLog4Net.Models;

// LoggingWithDatabankLogging:
using Databank.Extensions;
using Databank.Models;
```

That's the entire difference at the top of `Program.cs`. The `Main()` method is otherwise the same code.

---

## What the Packages Provide

**`Databank.Extensions`** - the `Logging` static class and all string/exception extension methods:

```csharp
Logging.Info("Program Starting...");
message.Trace();
message.Debug();  // one extra level compared to the hand-rolled wrappers
message.Info();
message.Warn();
message.Error();
message.FatalError();
ex.HandleException();
```

**`Databank.Models`** - `DatabankException` with `ExceptionType`, `ErrorType`, and `IsFatal` classification:

```csharp
throw new DatabankException("Sample error for log testing!");
```

`HandleException` recognizes `DatabankException` and includes the classification fields in the log entry automatically.

---

## Running It

Build and run. Two date-stamped log files appear under the output directory's `logs/` folder and open automatically when the program exits. The behavior is identical to the other two projects - the difference is entirely in where the logging infrastructure came from.

---

## The Practical Takeaway

Logging infrastructure that's used across many projects belongs in a shared package, not copy-pasted into each project. When the underlying logger changes (from log4net to Serilog, for example, as has happened in this solution's history), the call sites in every consuming project stay untouched. Only the package changes.

This is the same pattern as `CSharp.SharedLibrary` in this solution: utilities and helpers that would otherwise be duplicated live in one place, and the consuming projects reference the package rather than copying the code.

---

## Summary: Where the Infrastructure Comes From

| | LoggingWithLog4Net | LoggingWithSerilog | LoggingWithDatabankLogging |
|---|---|---|---|
| `Logging` class | Local, hand-rolled | Local, hand-rolled | `Databank.Extensions` package |
| `DatabankException` | Local copy | Local copy | `Databank.Models` package |
| Underlying logger | log4net | Serilog | Serilog (via package) |
| Call-site `using` | Local namespace | Local namespace | `Databank.Extensions`, `Databank.Models` |

---

## Takeaways

- The call sites are identical across all three logging projects. The `using` directives are the only visible difference.
- Logging infrastructure that spans multiple projects belongs in a package, not copy-pasted per project.
- When the underlying logger changes, consuming projects are unaffected - only the package is updated.
