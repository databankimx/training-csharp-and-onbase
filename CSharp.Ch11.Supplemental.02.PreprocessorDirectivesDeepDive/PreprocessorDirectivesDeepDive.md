# Preprocessor Directives Deep Dive

## Introduction

The main lesson showed `#if DEBUG` deciding what gets compiled. This lesson covers the rest: defining your own compile-time symbols, silencing a specific warning safely, and a genuinely useful related feature that automatically tells you where a line of code was called from.

---

## Defining Your Own Symbols

```csharp
#define MY_SYMBOL   // only this file sees it, and it must come before any real code
```

```xml
<!-- in the .csproj -->
<DefineConstants>$(DefineConstants);MY_SYMBOL</DefineConstants>
```

`#define` in a file only affects that one file, and has to appear right at the top, before any actual code. Defining a symbol in the `.csproj` instead makes every file in the project see it, usually the better choice unless you specifically want something scoped to just one file.

---

## `#region`: The One Directive That Changes Nothing

```csharp
#region Some Section
// code
#endregion
```

Every other directive covered in this training set changes what actually compiles or what warnings appear. `#region` doesn't, it's purely a code-folding aid for your editor. The compiled program is identical with or without it.

---

## Silencing One Specific Warning

```csharp
#pragma warning disable CS0219
int unused = 42;
#pragma warning restore CS0219
```

Sometimes you genuinely need to suppress a specific compiler warning for a specific bit of code. Do it as narrowly as possible, disable right before, restore right after, rather than turning a warning off for a whole file or project, where it'll also silence any *real* future mistakes the same warning would have caught.

---

## A Genuinely Useful Trick: Automatic Call-Site Info

```csharp
private static void Log(string message,
    [CallerFilePath] string filePath = "",
    [CallerLineNumber] int lineNumber = 0,
    [CallerMemberName] string memberName = "")
{
    Console.WriteLine($"[{filePath}:{lineNumber} in {memberName}()] {message}");
}
```

Call `Log("something happened")` from anywhere, and it automatically knows exactly which file, line, and method called it, without you ever typing that information yourself. Great for logging helpers, since the location info can never go stale the way a hand-typed comment could.

---

## Worth Knowing, But Not Demonstrated Here

- `#warning`/`#error` force a compiler warning or error at that line, every build, not something to trigger in a shared project on purpose.
- `#line` changes what file/line the compiler *reports*, used by tools that generate code.
- `#pragma checksum` helps debuggers verify source files match what was compiled.

All four affect tooling, not your running program, so there's nothing to watch happen at runtime.

---

## Try It Yourself

Run `UsingCallerInfoAttributes()` and look at the printed output, it names the exact file, line number, and method that called `Log()`, all filled in automatically by the compiler.
