# Chapter 1: Hello World

## Why Bother With Hello World

Every language tutorial on Earth starts here, and it's not just tradition for tradition's sake. Printing a single line to the screen proves your whole toolchain works: the compiler compiles, the runtime runs, and the text you typed shows up where you expected it to. If Hello World fails, nothing else in this course is going to go better.

---

## The Anatomy of a Console App

### The Entry Point

```csharp
private static void Main(string[] args)
{
    ...
}
```

Every runnable .NET program needs exactly one entry point, a `Main` method the runtime knows to call first. By convention it lives in a class called `Program`, though nothing forces that name on you, the runtime doesn't care what the class is called, only that a `Main` method exists somewhere for it to find.

`args` is the array of anything typed after the program's name on the command line. Run `CSharp.Ch01.HelloWorld.exe Ada`, and `args[0]` holds `"Ada"`. Run it with nothing after the name, and `args` is an empty array, not null, empty, which matters in a moment.

### Curly Braces and Indentation

You'll notice everything inside a set of `{ }` is indented one level deeper than the brace that opened it. This isn't the compiler enforcing anything, C# doesn't care about whitespace the way Python does, it's purely for you and whoever reads your code after you. Indentation is how your eyes find the boundaries of a block without having to count braces.

---

## Three Ways to Put a Variable in a String

The example program greets you by name three separate times, using three different techniques that all produce the exact same output. This isn't padding, it's showing you the evolution of the same idea.

```csharp
// The classic way, going back to C's printf lineage
Console.WriteLine(string.Format("Hello {0}!", name));

// WriteLine can do the formatting itself, string.Format is redundant here
Console.WriteLine("Hello {0}!", name);

// String interpolation, the modern way, added in C# 6
Console.WriteLine($"Hello {name}!");
```

You'll see all three styles in the wild, sometimes in the same codebase, sometimes in the same file, so it's worth being able to read all of them even though interpolation is what you should reach for when writing new code. It's shorter, and it puts the variable right where it's used instead of making you count placeholder indices.

---

## Try, Catch, and Why We Always Wrap Main

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
        Console.WriteLine("\nDone!\n\nPress any key to exit!");
        Console.ReadKey();
    }
}
```

Exception handling gets its own full chapter later, but you're seeing the shape of it here on day one because it's a habit worth building early: wrap your entry point so an unhandled exception doesn't just vanish the console window before you've had a chance to read what went wrong.

The `while (ex != null)` loop is walking the chain of `InnerException` values. Exceptions can wrap other exceptions, a database call fails, gets caught and rethrown as a more specific application exception, which itself gets caught further up. Printing only the outermost exception can leave you staring at "an error occurred" with zero clue what actually broke. Walking the chain gets you the real story.

`finally` runs whether the `try` succeeded or the `catch` caught something, which is why it's the right place to keep the console window open. Without it, a console app can flash open and closed so fast you'd never read the output, exception or not.

One refinement worth calling out: wrapping the exit prompt in `if (!Debugger.IsAttached)`. When you run the app from Visual Studio with the debugger attached, VS already keeps the console window open for you after `Main` returns, so pressing a key to close it manually is redundant busywork you'd repeat every single debug session. Outside the debugger (double-clicking the .exe, running from a terminal), nothing holds the window open on its own, so the prompt is exactly the thing you still need there. Requires `using System.Diagnostics;` for the `Debugger` type.

---

## Regions

```csharp
#region Using Directives
using System;
#endregion
```

`#region` has zero effect on the compiled output, the compiler strips it out entirely. It exists purely so your IDE can collapse related blocks of code, which matters a lot more once a file has hundreds of lines instead of a hundred. You'll see it used throughout this training set to mark off copyright headers, using directives, and logical groupings within a class.

---

## Reading Input

```csharp
Console.WriteLine("Enter your name to continue...");
name = Console.ReadLine();
Console.WriteLine($"Hello {name}!");
```

`Console.ReadLine()` blocks, meaning the program stops and waits right there until the person at the keyboard types something and hits Enter. This is the simplest possible way to get interactive input, and also the reason console apps make good teaching tools: you can watch execution pause in real time.

---

## What's Actually Happening When You Run This

1. `Main` starts, `args` is whatever you passed on the command line.
2. `"Hello world!"` prints, unconditionally, the one line every tutorial promises you.
3. `Pause()` waits for any keypress, then clears the screen. This is purely a teaching aid, not something you'd ship in real software, nobody wants their production console app clearing itself.
4. `args[0]` is read. If you ran the program with no arguments, this throws an `IndexOutOfRangeException`, on purpose, so you can watch the catch block do its job.
5. Assuming an argument was provided, you get three identical-looking greetings, one per formatting style.
6. Another pause, then the program asks you to type your name directly and greets you with whatever you typed.
7. `finally` keeps the window open so you can actually read all of this before it closes.

Try running it once with a command-line argument and once without. Watching the exception path fire on purpose is a much better first exposure to `try`/`catch` than only ever seeing it work.
