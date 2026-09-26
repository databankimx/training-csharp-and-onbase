# Logging with Log4Net

One of three lessons comparing logging approaches side by side - the same small demo program (a prime-number sieve, plus a deliberately-thrown sample exception) implemented three different ways:

1. **This lesson** - log4net, configured via `App.config`
2. `CSharp.Supplemental.LoggingWithSerilog` - Serilog, hand-configured the same way this lesson's log4net setup is
3. `CSharp.Supplemental.LoggingWithDatabankLogging` - the real internal `Databank.Logging`/`Databank.Exceptions` NuGet packages, which do almost all of this for you

## Why This One Still Exists

DataBank's current policy is Serilog for all new logging work, on any target framework - log4net is only kept for existing legacy projects that already use it, largely because of a string of significant security vulnerabilities found in log4net in recent years. This lesson isn't showing you "the way we do it" - it's showing you what you'll actually encounter if you work on an older codebase that predates the Serilog move, so the pattern is recognizable rather than a surprise.

## What's Here

`HelperClasses/Logging.cs` wraps log4net's `ILog` behind simple string extension methods (`Trace`, `Info`, `Warn`, `Error`, `FatalError`) and an `Exception.HandleException()` that walks the full inner-exception chain. All of log4net's actual behavior - what gets logged where, at what level, in what format - lives in `App.config`'s `<log4net>` section: a console appender, and two rolling file appenders (one for everything DEBUG through WARN, one for ERROR and above).

## Try It Yourself

Run the project. Check `bin\Debug\logs\trace.log` and `bin\Debug\logs\error.log` afterward and compare what ended up in each - the level-range filter on the trace log and the threshold on the error log are what decide that split.
