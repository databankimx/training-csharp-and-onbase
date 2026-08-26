# Ch11 Textbook Code: Write to Event Log

## What This Is

A small, genuinely runnable, interactive WinForms demo of the Windows Event Log, matching the main lesson's `LoggingToEventLog()`, but as a real form: type a source, log name, event ID, and message, click "Write," and the entry is written for real. No bugs found here, the code is clean and matches the exact same `EventLog.SourceExists()` / `EventLog.CreateEventSource()` / `EventLog.WriteEntry()` pattern already covered in the main lesson.

---

## The Same Permission Boundary as `PerformanceCounter`

```csharp
if (!EventLog.SourceExists(source))
    EventLog.CreateEventSource(source, log);

EventLog.WriteEntry(source, message, EventLogEntryType.Information, id);
```

Worth reconnecting to `CSharp.Ch11.Supplemental.04.PerformanceCountersAndProfiling`'s `CreatingACustomPerformanceCounter()`: this is the exact same permission shape, creating a *new* source the first time needs administrator privileges, writing to a source that already exists doesn't. Unlike that Supplemental's version, this form doesn't wrap the call in a `try`/`catch`, run it without administrator rights on a source that doesn't exist yet and it throws an unhandled `SecurityException` rather than failing gracefully. Worth noticing as a real contrast: the Supplemental's more defensive version and this raw textbook version demonstrate the identical underlying API, with and without the error handling a production application would actually want.

---

## Try It Yourself

Run this form (as Administrator, the first time, so `EventLog.CreateEventSource()` succeeds), click "Write," then open Windows Event Viewer (`eventvwr.msc`) → Windows Logs → Application, and find the entry, source "OrderMaker," ID 1001, exactly what the form's own default field values describe.
