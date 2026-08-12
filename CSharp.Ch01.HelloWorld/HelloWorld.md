# HelloWorld

## Introduction

Every language tutorial starts here for a reason. Printing a single line to the screen proves your whole toolchain works, the compiler compiles, the runtime runs, and the text you typed shows up where you expected it to.

---

## The Entry Point

```csharp
internal static class Program
{
    private static void Main(string[] args)
    {
        ...
    }
}
```

Every runnable .NET program needs exactly one entry point, a `Main` method the runtime knows to call first. By convention it lives in a class called `Program`, though nothing forces that name on you.

`args` is the array of anything typed after the program's name on the command line. Run the program with `Ada` as an argument, and `args[0]` holds `"Ada"`. Run it with nothing after the name, and `args` is an empty array, not null.

---

## Wrapping Main in try/catch/finally

```csharp
try
{
    ...
}
catch (Exception ex)
{
    while (ex != null)
    {
        Console.WriteLine(ex);
        ex = ex.InnerException;
    }
}
finally
{
    if (!Debugger.IsAttached)
    {
        Console.WriteLine($"\nDone!\n\nPress any key to exit!");
        Console.ReadKey();
    }
}
```

Exception handling gets its own full chapter later, but the shape of it starts here. Wrapping the entry point means an unhandled exception doesn't just vanish the console window before you've had a chance to read what went wrong.

The `while (ex != null)` loop walks the chain of `InnerException` values. Exceptions can wrap other exceptions, printing only the outermost one can leave you staring at a generic error message with no clue what actually broke. Walking the chain gets you the real story.

`finally` runs whether the `try` succeeded or the `catch` caught something, which is why it's the right place to keep the console window open. The `Debugger.IsAttached` check means you don't have to press a key every time you stop debugging from Visual Studio, the debugger already keeps the window open for you there.

---

## Three Ways to Put a Variable in a String

```csharp
Console.WriteLine("Hello world!");

string name = args[0];

// The classic way to embed a variable value in a string is using string.Format
Console.WriteLine(string.Format("Hello {0}!", name));

// The WriteLine() method can interpolate formatting without needing "string.Format"
Console.WriteLine("Hello {0}!", name);

// In newer versions of C#, we can accomplish the same thing using string interpolation
Console.WriteLine($"Hello {name}!");
```

Three different techniques, all producing the exact same output.

- `string.Format("Hello {0}!", name)` is the classic way, going back to C's `printf` lineage.
- `Console.WriteLine("Hello {0}!", name)` skips `string.Format` entirely, `WriteLine` can do the formatting itself.
- `$"Hello {name}!"` is string interpolation, added in C# 6. Shorter, and it puts the variable right where it's used instead of making you count placeholder indices.

You'll see all three styles in real code, so it's worth being able to read all of them, even though interpolation is what you should reach for when writing new code.

---

## Reading Input

```csharp
Console.WriteLine("Enter your name to continue...");
name = Console.ReadLine();
Console.WriteLine($"Hello {name}!");
```

`Console.ReadLine()` blocks, meaning the program stops and waits right there until someone types something and hits Enter. This is the simplest possible way to get interactive input.

---

## Regions

```csharp
#region Directives
using System;
using System.Diagnostics;
#endregion
```

`#region` has zero effect on the compiled output, the compiler strips it out entirely. It exists purely so your IDE can collapse related blocks of code, which matters more once a file has hundreds of lines instead of a hundred.

---

## A Helper Method

```csharp
private static void Pause()
{
    Console.WriteLine($"\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}
```

Executable code doesn't need to live inside `Main` itself. Here, `Pause()` is a separate method that `Main` calls between sections, waiting for a keypress and clearing the screen so each part of the demo starts fresh. Splitting logic into named methods like this, rather than writing one enormous `Main`, is a habit worth building from day one.
