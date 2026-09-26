# Logging with Serilog

The second of three lessons comparing logging approaches side by side - the same small demo program (a prime-number sieve, plus a deliberately-thrown sample exception), implemented three different ways. See `CSharp.Supplemental.LoggingWithLog4Net`'s `Lesson.md` for how all three relate.

## What's Different From the Log4Net Version

Structurally, almost nothing - `HelperClasses/Logging.cs` still wraps a logging library behind the same string extension methods (`Trace`, `Debug`, `Info`, `Warn`, `Error`, `FatalError`) and the same `Exception.HandleException()` pattern, and `Program.cs` runs the identical demo. What's actually different:

- Configuration lives in `appsettings.json` (read via `Microsoft.Extensions.Configuration`) instead of a custom `App.config` section - JSON rather than XML, but the same idea: sinks (console, two rolling log files), minimum levels, and output formatting are all declared in one place, not scattered through code.
- Serilog's structured logging means format placeholders can be named (`"{i} is prime!"`) rather than positional (`"{0} is prime!"`) - both work, but named placeholders also become searchable properties on the log event itself if you're shipping logs somewhere that can query them.

## Try It Yourself

Run the project. `logs/debug-log-<date>.txt` and `logs/error-log-<date>.txt` land next to the executable, and everything also prints to the console at the same time - something log4net's setup in the previous lesson could also do, just via an extra `ConsoleAppender` entry rather than a second `WriteTo` block.
