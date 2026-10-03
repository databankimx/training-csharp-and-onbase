# Supplemental: Logging With log4net

## What This Is

A working log4net integration using the same `Logging` helper class pattern the rest of this solution uses. The demo runs a Sieve of Eratosthenes to find primes up to 20, logging every step, then opens the resulting log files automatically when it finishes.

This project builds a local `Logging` class and a local `DatabankException` from scratch. Compare it to `LoggingWithDatabankLogging`, which replaces all of that with two NuGet package imports. See `LoggingWithSerilog` for the structured-logging equivalent of the same setup.

---

## The Setup

log4net is configured through `App.config`. The `<log4net>` section defines two file appenders - one for trace-level output (`logs/trace.log`), one for errors and above (`logs/error.log`) - and assigns them to the root logger:

```xml
<log4net>
  <appender name="TraceAppender" type="log4net.Appender.RollingFileAppender">
    <file value=".\logs\trace.log" />
    <appendToFile value="false" />
    <rollingStyle value="Size" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date [%thread] %-5level %logger - %message%newline" />
    </layout>
  </appender>
  ...
</log4net>
```

`XmlConfigurator.Configure()` in the `Logging` static constructor reads this section and wires up the appenders. This happens once, the first time anything references `Logging`.

---

## The `Logging` Class

`Logging` wraps a log4net `ILog` instance and exposes the five severity levels as both static methods and string extension methods:

```csharp
// Static call
Logging.Info("Program Starting...");

// Extension method call -- same underlying logger
string message = "Sample log entry";
message.Trace();
message.Info();
message.Warn();
message.Error();
message.FatalError();
```

The extension methods let any `string` be logged directly without constructing a log message object. Both forms accept format arguments:

```csharp
Logging.Info("{0} is prime!", 7);
```

`HandleException` is an extension method on `Exception` that walks the inner exception chain and logs each level:

```csharp
try
{
    throw new DatabankException("Sample error for log testing!");
}
catch (Exception ex)
{
    ex.HandleException();
}
```

`DatabankException` adds `ExceptionType` and `ErrorType` classification. `HandleException` includes those fields in the log entry when the caught exception is a `DatabankException`, and falls back gracefully for any other exception type.

---

## log4net Severity Levels

From lowest to highest:

| Level | Method | When to use |
|---|---|---|
| DEBUG / TRACE | `.Trace()` | Detailed diagnostic information - disabled in production |
| INFO | `.Info()` | Normal operational events |
| WARN | `.Warn()` | Something unexpected that isn't an error |
| ERROR | `.Error()` | A failure in a specific operation |
| FATAL | `.FatalError()` | Application-level failure |

log4net's own level name is `DEBUG`; this project aliases it as `Trace` in the `Logging` wrapper to match the naming convention the rest of the solution uses.

---

## Running It

Build and run. Two log files appear under the output directory's `logs/` folder and open automatically when the program exits. The trace log contains every step including the sieve internals; the error log contains only the deliberately-thrown `DatabankException`.

The `OpenLogFile` helper uses `Process.Start` with `UseShellExecute = true` to open each log in whatever application is associated with `.log` files, falling back to Notepad if no association exists.

---

## Summary: The Three Logging Projects

| | LoggingWithLog4Net | LoggingWithSerilog | LoggingWithDatabankLogging |
|---|---|---|---|
| Configuration | XML (`App.config`) | Code (`LoggerConfiguration`) | Package-provided (`serilog.json`) |
| Logging class | Hand-rolled locally | Hand-rolled locally | From `Databank.Extensions` package |
| DatabankException | Local copy | Local copy | From `Databank.Models` package |
| Output format | Plain text | Structured (named properties) | Same as Serilog |
| Log levels | DEBUG/INFO/WARN/ERROR/FATAL | Verbose/Information/Warning/Error/Fatal | Same as Serilog |

---

## Takeaways

- log4net is configured in `App.config`; `XmlConfigurator.Configure()` reads it once on first use.
- Severity levels control which messages reach which appenders - the error appender ignores DEBUG and INFO; the trace appender captures everything.
- The `Logging` wrapper provides static methods and string extension methods for consistent call-site syntax.
- `HandleException` walks the inner exception chain, logging each level with full type and stack trace information.
- See `LoggingWithSerilog` for structured logging with JSON sinks, and `LoggingWithDatabankLogging` for the fully packaged version.
