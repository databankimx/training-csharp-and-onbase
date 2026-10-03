# Supplemental: Logging With Serilog

## What This Is

The same demo as `LoggingWithLog4Net` - prime sieve, five severity levels, exception handling - rewritten on top of Serilog. The `Logging` wrapper and `DatabankException` are built locally again; the underlying logger is Serilog with two file sinks.

What changes from log4net: the configuration moves from XML to code, and Serilog adds **structured logging** - capturing the message template and its arguments separately rather than formatting them into a single string before writing.

---

## The Setup

Serilog is configured in code (using the fluent `LoggerConfiguration` API) reading sink paths from `appsettings.json`:

```json
{
  "Serilog": {
    "LogPath": "logs/debug-log-{Date}.txt",
    "ErrorPath": "logs/error-log-{Date}.txt"
  }
}
```

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.File(logPath,    restrictedToMinimumLevel: LogEventLevel.Verbose)
    .WriteTo.File(errorPath,  restrictedToMinimumLevel: LogEventLevel.Error)
    .CreateLogger();
```

Unlike log4net's XML configuration, Serilog's setup is ordinary C# - readable, refactorable, and testable without parsing config files.

---

## The `Logging` Class

Same public API as the log4net version - static methods and string extension methods at five severity levels, plus `HandleException`:

```csharp
Logging.Info("Program Starting...");
"Sample log entry".Trace();
"Sample log entry".Warn();
ex.HandleException();
```

Internally, each method calls the corresponding Serilog method on `Log.Logger`. The `HandleException` extension walks the inner exception chain the same way the log4net version does.

---

## Structured Logging -- The Serilog Difference

log4net's `InfoFormat("{0} is prime!", 7)` produces the string `"7 is prime!"` and stores that string. Nothing separates the template from the value.

Serilog's template syntax uses named placeholders:

```csharp
Logging.Info("{i} is prime!", i);
```

The template `"{i} is prime!"` and the value `7` are stored separately in Serilog's `LogEvent`. A JSON sink can write:

```json
{ "MessageTemplate": "{i} is prime!", "Properties": { "i": 7 } }
```

This makes log data queryable after the fact - a log aggregation tool can filter on `Properties.i > 5` without parsing free-text strings. For simple file-based logging the difference is invisible, but it becomes significant when logs are shipped to a centralized store like Seq or Elasticsearch.

---

## Serilog Severity Levels

| Level | Method |
|---|---|
| Verbose / Trace | `.Trace()` |
| Debug | (not exposed in this wrapper) |
| Information | `.Info()` |
| Warning | `.Warn()` |
| Error | `.Error()` |
| Fatal | `.FatalError()` |

Serilog's own level name is `Verbose`; this project aliases it as `Trace` in the wrapper to match the rest of the solution.

---

## Running It

Build and run. Two date-stamped log files appear under `logs/` in the current working directory. The program opens them automatically when it exits.

---

## Summary: log4net vs. Serilog

| | log4net | Serilog |
|---|---|---|
| Configuration | XML (`App.config`) | Code (`LoggerConfiguration`) |
| Message format | String concatenated before write | Template + named properties stored separately |
| Queryable after the fact | Only by string parsing | Yes, via structured sinks (Seq, Elasticsearch) |
| Call-site syntax | Same `Logging` wrapper API | Same `Logging` wrapper API |

---

## Takeaways

- Serilog is configured in code with the fluent `LoggerConfiguration` API; no XML config needed.
- Structured logging captures template and arguments separately, making logs queryable as structured data.
- Named placeholders (`{i}`) rather than positional ones (`{0}`) - the name becomes a property in the stored event.
- The public API matches the log4net wrapper - call sites look identical regardless of which logger is underneath.
- See `LoggingWithLog4Net` for the XML-configured equivalent, and `LoggingWithDatabankLogging` for the fully packaged version.
