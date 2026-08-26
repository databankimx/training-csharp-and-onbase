# Trace Listeners

## Introduction

`Trace.WriteLine()` doesn't write anywhere by default, it writes through "listeners" you register. This lesson covers the built-in listeners, writing your own, using several at once, and filtering output by how serious it is.

---

## Sending Trace Output to a File

```csharp
var fileListener = new TextWriterTraceListener(logPath);
Trace.Listeners.Add(fileListener);
Trace.WriteLine("This goes to the file now.");
Trace.Flush();
```

Once a `TextWriterTraceListener` is registered, every `Trace.WriteLine()` writes to that file. `Trace.Flush()` makes sure anything buffered actually reaches disk.

---

## Writing Your Own Listener

```csharp
public class TimestampedTraceListener : TraceListener
{
    public override void Write(string message) { ... }
    public override void WriteLine(string message) { ... }
}
```

Every built-in listener, console, file, event log, is built the same way you'd build your own: override `Write()` and `WriteLine()`. This lesson's custom listener adds a timestamp to every line automatically.

---

## More Than One Listener at a Time

```csharp
Trace.Listeners.Add(consoleListener);
Trace.Listeners.Add(fileListener);
Trace.WriteLine("One call, two destinations.");
```

You can register multiple listeners at once, one `Trace.WriteLine()` call reaches all of them. Useful for logging to the console during development and to a file at the same time, with no extra code.

---

## Nested, Readable Output

```csharp
Trace.WriteLine("Starting...");
Trace.Indent();
Trace.WriteLine("A sub-step...");
Trace.Unindent();
```

`Trace.Indent()`/`Unindent()` make nested output actually look nested, great for logging something with real structure, like a recursive process or nested steps.

---

## Filtering by Severity

```csharp
var mySwitch = new TraceSwitch("Demo", "") { Level = TraceLevel.Warning };
Trace.WriteLineIf(mySwitch.TraceWarning, "This prints.");
Trace.WriteLineIf(mySwitch.TraceInfo, "This doesn't.");
```

A `TraceSwitch` lets you set how detailed your logging should be, `Error`, `Warning`, `Info`, or `Verbose`, and messages below that level get skipped automatically. Real applications usually configure this through a settings file, so you can turn up the detail temporarily to investigate a problem in production without redeploying anything.

---

## Try It Yourself

Run `UsingTraceIndentation()` and watch how the indentation in the console output visually matches the nested structure of the operations being logged.
