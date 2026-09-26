# Logging with Databank.Logging

The third of three lessons comparing logging approaches side by side - the same small demo program (a prime-number sieve, plus a deliberately-thrown sample exception). See `CSharp.Supplemental.LoggingWithLog4Net`'s `Lesson.md` for how all three relate.

> **This lesson requires DataBank's internal NuGet feed.** `Databank.Logging` and `Databank.Exceptions` are published to DataBank's own GitHub Enterprise package feed, not nuget.org - restoring this project requires a DataBank GHE account with access to that feed, configured in your own NuGet settings. This project won't restore or build outside DataBank's network/account access.

## What's Different Here

The previous two lessons each had their own `HelperClasses/Logging.cs` and `Models/DatabankException.cs` - roughly 150 lines of code you'd have to write (and maintain, and keep consistent with everyone else on the team) before you could log a single message. This lesson has neither. `using Databank.Extensions;` and `using Databank.Models;` bring in everything: the same `Trace`/`Debug`/`Info`/`Warn`/`Error`/`FatalError` string extension methods, the same `Exception.HandleException()` pattern, and a `DatabankException` with everything the other two lessons' hand-rolled versions had (`ExceptionType`, `ErrorType`) plus more (`IsFatal`, a `Description` property, a purpose-built `ToString()`).

Configuration is `serilog.json` - Serilog under the hood, same as the previous lesson, just packaged so you don't have to wire it up yourself. It even auto-initializes: nothing in `Program.cs` calls `Logging.Initialize()` at all. The library probes for `serilog.json` in the output directory the first time you call any logging method, and falls back to a sensible default rolling-file logger if it can't find one.

## Try It Yourself

Run the project - the demo itself is identical to the other two lessons, so the output (and the `logs/` folder it produces) should look the same. What's worth actually looking at is `Program.cs` itself, next to the same file in the other two lessons - the demo logic is the same length, but there's no `HelperClasses` folder and no local `Models/DatabankException.cs` to go with it.
