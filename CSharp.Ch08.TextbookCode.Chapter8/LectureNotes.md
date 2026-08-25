# Ch08 Textbook Code: Chapter 8

## What This Is

The textbook's own combined Chapter 8 sample project, covering `Assembly`/`Type` reflection, custom attributes, the CodeDOM, and lambda expressions, all in one download. Unlike most `TextbookCode.*` labs in this training set, this one isn't organized as a single runnable demo, it's presented as sixteen numbered, individually-explorable code snippets (`Chapter8CodeBlock1` through `Chapter8CodeBlock16`), plus two entirely separate, self-contained example classes. No functional bugs found in anything that's actually reachable; several structural quirks are worth understanding before diving in.

---

## `Main()` Is Genuinely Empty, On Purpose

```csharp
static void Main(string[] args)
{
}
```

`Program.Main()`, the actual entry point (`<StartupObject>Chapter8.Program</StartupObject>` in the `.csproj` pins it there explicitly), does nothing at all. None of the sixteen `Chapter8CodeBlockN()` methods are ever called automatically. This matches the textbook's own presentation style: each block corresponds to a numbered code listing in the book's text, meant to be read, or manually invoked one at a time (temporarily adding a call inside `Main()`, or running under a debugger and calling a method directly from the Immediate/Watch window), not run end to end as a program. Running this project via `LessonRunner` will show a blank console that starts and exits instantly, that's expected, not broken, the actual value here is in reading the sixteen blocks directly.

---

## Two More `Main()` Methods, Neither One Reachable

`CodeDOMExample.cs` and `LambdaExpressionExample.cs` each declare their **own** `static void Main(string[] args)`. Having multiple classes with their own `Main()` in one project is legal C#, as long as the `.csproj` disambiguates which one is the real entry point, which it does (`Chapter8.Program`). The other two are just ordinary static methods that happen to be named `Main`, never invoked by anything. Both are fully self-contained and genuinely worth reading on their own:

- **`LambdaExpressionExample.Main()`**: a compact tour of the exact same delegate → anonymous method → lambda progression covered at length in Chapter 6, worth a quick read as a review, calls `Console.ReadLine()` at the end (another sign it was meant to be run in isolation, not as part of a larger program).
- **`CodeDOMExample.Main()`**: builds a `Calculator` class via CodeDOM (fields, properties with getters/setters, a `Divide()` method with a real `if`/`else`, an `Exponent()` method calling `Math.Pow()`), broadly the same structure the main lesson's own `Calculator`-flavored CodeDOM examples build, worth comparing against `CSharp.Ch08.Supplemental.03.CodeDomCompileAndRun`'s fuller version.

`CodeDOMExample.Main()` does have a real limitation worth knowing about, even though it's unreachable: it writes the generated source to a hardcoded path, `c:\CodeDom\Calculator.cs`, and that folder isn't created automatically. If you ever want to actually run this method (by temporarily changing `StartupObject`, or copy-pasting its body elsewhere), you'll need to create `C:\CodeDom\` first, or it throws `DirectoryNotFoundException`. Left exactly as downloaded rather than fixed, since this code never runs as part of the actual program.

---

## `Person.GetPerson()`: Also Unreachable, Also Requires Infrastructure That Isn't There

```csharp
public bool GetPerson(int personId)
{
    SqlConnection cn = new SqlConnection("Server=(local);Database=Reflection;Trusted_Connection=True;");
    cn.Open();
    ...
}
```

`Person.GetPerson()` opens a literal SQL Server connection to a local `Reflection` database that doesn't exist anywhere in this training environment. Nothing calls this method (Program's `Main()` never touches `Person` at all), so it's inert, exactly like `CodeDOMExample`'s hardcoded path. It exists here purely to demonstrate `ReflectionExample.LoadClassFromSQLDataReader()`, a genuinely well-designed reflection pattern, worth reading `ReflectionExample.cs` on its own: it reads a `SqlDataReader`'s column names, checks each one against `Person`'s `[DataMapping(...)]` attributes (an `AllowMultiple = true` attribute, see `CSharp.Ch08.Supplemental.01.CustomAttributes` for a deeper look at that feature) to find the right property to map it to, falling back to a same-named property when no explicit mapping exists, then sets each property's value via reflection. This is a real, compact example of exactly the kind of "generic property mapper" pattern `CSharp.Ch08.Supplemental.02.DynamicInvocation`'s `PropertyMapper` also demonstrates, worth comparing the two.

---

## Worth Reading Block by Block

Some highlights among the sixteen numbered blocks, cross-referenced against the main lesson and Supplementals:

- **Blocks 1–5**: `Assembly` metadata, `GetTypes()`, `GetModules()`, `CreateInstance()`, `GetReferencedAssemblies()`, direct precedent for the main lesson's own Assembly section.
- **Block 8**: `GetConstructors()` on `System.Data.DataTable`, a real BCL type rather than a custom class, worth comparing against the main lesson's `Person`-based version.
- **Block 9**: `GetEnumNames()`, `GetEnumValues()`, **and** `GetEnumName(int)` (singular, by underlying value), the main lesson only demonstrated the first two.
- **Blocks 10–12**: `GetFields()`/`GetProperties()` with a full five-flag `BindingFlags` combination (`Public | Instance | Static | NonPublic | FlattenHierarchy`), worth comparing against the narrower flag combinations used elsewhere in this chapter's content.
- **Blocks 13–14**: two different ways to call a method dynamically, `MethodInfo.Invoke()` (the same technique used throughout this chapter) versus `Type.InvokeMember()` (an older, slightly different API that looks up and invokes in one step), worth reading side by side.
- **Block 15**: reads the assembly-level custom attributes (like `AssemblyTitle`, `AssemblyDescription`) off every assembly the current one references, a genuinely different angle on "reading custom attributes" than anything else in this chapter, since it targets assembly-level attributes rather than class-level ones.
