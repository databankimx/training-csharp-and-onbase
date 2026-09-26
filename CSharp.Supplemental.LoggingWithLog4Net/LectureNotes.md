# Logging with Log4Net

## What This Is

Ported from the loose `logging-training` repo's `UsingLog4Net` project - one of a three-way logging comparison (see `Lesson.md` for how the three relate). Kept on log4net deliberately, unlike everything else built in this pass - the whole point of this lesson is showing the legacy approach, not modernizing it away.

## What Changed From the Original

- `internal class Program` → `internal static class Program`, matching the console-app convention used throughout this solution.
- One small bug fix in `HandleException`: the original used a direct cast (`(DatabankException)ex`) to check the exception's type while walking the inner-exception chain. That throws an `InvalidCastException` the moment the chain includes an exception that *isn't* a `DatabankException` - which is a completely normal thing to happen (an outer `DatabankException` wrapping an ordinary `IOException`, say). Changed to `ex as DatabankException` with a null check instead.
- Kept the original repo's local `DatabankException`/`ErrorCodes`/`ErrorCodeLookup` rather than swapping to `CSharp.SharedLibrary`'s simpler version, matching the same reasoning as `OnBase.Preprocessor` and `CSharp.Ch09.Supplemental.02.SqlInjection`'s local copies - this one classifies exceptions with `ExceptionType`/`ErrorType`, which the shared version doesn't have, and that classification is part of what this lesson is actually demonstrating.
- Target framework left at the inherited `net48` default (matches the original, no change needed).
