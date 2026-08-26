# Chapter 11 Supplemental 02: Preprocessor Directives Deep Dive

## What This Is

The main lesson showed `#if DEBUG`/`#else`/`#endif` deciding what gets compiled based on build configuration. This Supplemental covers the rest: defining your own symbols two different ways, `#pragma warning` for narrowly silencing a specific warning, a few directives worth knowing about but not safely demonstrable live (they'd break the build on purpose), and a closely-related, genuinely useful, actually runtime-observable feature, caller info attributes.

---

## Two Ways to Define a Symbol

```csharp
#define FILE_SCOPED_DEMO   // must appear before any real code token in the file
```

```xml
<!-- .csproj -->
<DefineConstants>$(DefineConstants);TRAINING_BUILD</DefineConstants>
```

`#define` inside a `.cs` file only affects **that one file**, and it has a real, easy-to-miss placement rule: it must appear before any actual code token in the file, comments and other preprocessor directives are fine before it, but a `using` directive is not. That's why it sits immediately after this file's copyright comment block rather than down with the code it affects. `<DefineConstants>` in the `.csproj`, by contrast, defines a symbol every single file in that project sees, the more common, more maintainable choice for anything beyond a single, deliberately narrow file-local flag.

---

## `#region`/`#endregion`: The One Directive With Zero Compiled Effect

```csharp
#region An Example
Console.WriteLine("...");
#endregion
```

Worth calling out specifically because every other directive in this file changes *something*, what compiles, which warnings fire, how errors are reported. `#region`/`#endregion` change none of that. They exist purely for an editor's code-folding/navigation UI, the compiled program is byte-for-byte identical whether they're present or not.

---

## `#pragma warning`: Silence Narrowly, Restore Immediately

```csharp
#pragma warning disable CS0219
int intentionallyUnused = 42;
#pragma warning restore CS0219
```

Without this, the line above generates `CS0219` ("The variable ... is assigned but its value is never used"). `#pragma warning disable <number>` turns a specific warning off starting at that line; `restore` turns it back on. Worth using exactly this narrowly, disable immediately before the code that needs it, restore immediately after, rather than disabling a warning number for an entire file or (worse) an entire project. A warning silenced project-wide stays silenced for every *future* occurrence too, including a genuine mistake that same warning would have caught months from now.

---

## Caller Info Attributes: Genuinely "Predefined Compiler Constants," Just Not Preprocessor Directives

```csharp
private static void Log(string message,
    [CallerFilePath] string filePath = "",
    [CallerLineNumber] int lineNumber = 0,
    [CallerMemberName] string memberName = "")
{
    Console.WriteLine($"[{Path.GetFileName(filePath)}:{lineNumber} in {memberName}()] {message}");
}
```

Not a preprocessor directive at all, technically, but close enough in spirit to belong right alongside them: `[CallerFilePath]`/`[CallerLineNumber]`/`[CallerMemberName]` tell the *compiler* to fill in the calling file's path, line number, and containing member's name automatically at every call site, the caller never passes them explicitly (and, using these attributes, structurally can't, they're optional parameters the compiler itself supplies). This is the genuine, modern C# answer to what `__FILE__`/`__LINE__` textual macros do in other languages, a real, typed language feature rather than text substitution before compilation. Extremely useful for exactly what's demonstrated here: a logging helper that reports where it was actually called from, without the caller ever having to type that information out by hand (and risk it going stale if the code later moves).

---

## Worth Knowing, Not Demonstrated Live

- **`#warning "message"`** forces a compiler warning at that exact line, every single build.
- **`#error "message"`** forces a compiler error at that exact line, stops the build entirely.
- **`#line 200 "Other.cs"`** makes the compiler report subsequent lines as if they came from line 200 of a different file, used by code generators so an error in *generated* code points back to the *original* source that produced it, not the generated file the person writing code never actually sees.
- **`#pragma checksum`** embeds a checksum for a source file, used by debuggers to verify the source shown matches what was actually compiled.

None of these four are demonstrated live in this project. `#warning`/`#error` would break this shared training solution's build on purpose, every time anyone builds it, clearly not appropriate here. `#line`/`#pragma checksum` have no runtime-observable effect worth demonstrating at all, their entire effect is on tooling (the compiler's own error reporting, a debugger's source mapping), not on the running program's behavior.
