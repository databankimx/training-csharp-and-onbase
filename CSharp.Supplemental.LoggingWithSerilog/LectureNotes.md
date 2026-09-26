# Logging with Serilog

## What This Is

Ported from the loose `logging-training` repo's `UsingSerilog` project - one of a three-way logging comparison (see `CSharp.Supplemental.LoggingWithLog4Net`'s `Lesson.md` for how the three relate).

## What Changed From the Original

- **Target framework changed from `net8.0` to `net48`**, per direction - keeps all three lessons in this comparison on the same framework, so the only variable being demonstrated between them is the logging library itself, not also a framework difference. Required explicitly overriding `ImplicitUsings` and `Nullable` back to `enable` (this solution's `Directory.Build.props` disables both by default) - the source code relies on nullable annotations (`bool[]?`, `Exception?`) and implicit usings throughout, kept as-is rather than rewritten to the wider solution's more explicit style.
- `internal class Program` → `internal static class Program`, matching the console-app convention used throughout this solution.
- Same `HandleException` fix as the log4net version: a direct cast (`(DatabankException)ex`) that would throw partway through an inner-exception chain if a non-`DatabankException` showed up in it, changed to `ex as DatabankException` with a null check.
- Kept the original's local `DatabankException`/`ErrorCodes`/`ErrorCodeLookup`, same reasoning as the log4net lesson.
- Didn't need `Microsoft.Extensions.Hosting` - the original repo's README lists it as a requirement, but the actual code only ever uses `Microsoft.Extensions.Configuration`'s `ConfigurationBuilder` directly. Left it out rather than adding an unused dependency.
