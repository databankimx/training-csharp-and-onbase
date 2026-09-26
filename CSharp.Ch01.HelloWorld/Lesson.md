# Chapter 1 - Hello World

## Why Bother With Hello World

Every programming course on Earth opens with Hello World, and it's tempting to write that off as ceremony, the coding equivalent of a ribbon-cutting nobody asked for. It isn't. Printing one line of text to a screen is the smallest possible end-to-end proof that your entire toolchain actually works: the compiler found your source code, the build produced something runnable, the runtime loaded it, and the characters you typed came out the other end in the order you typed them.

That's four separate things that can each fail on their own, and if one of them is broken, you want to find out now, while the only thing at stake is a greeting, rather than three chapters from now while you're debugging a database call and genuinely can't tell whether the problem is your query or your entire installation.

This particular Hello World is unusually chatty for the genre. It does the traditional one-liner and then keeps going, using it as an excuse to quietly introduce about six things you'll use in every program you write from here on. It also contains a bug, on purpose. We'll get to that.

---

## Creating the Project

Before there's any code to write, there needs to be somewhere to write it. Two ways to get there, pick whichever matches your tools, and one honest warning either way: neither one hands you a finished project without a little extra work.

### A word on "Framework" versus "Core," since you're about to be asked to pick

You're about to see two things both calling themselves ".NET," and they are not the same thing wearing a different hat. **.NET Framework** is the older, Windows-only runtime, been around since 2002, and it's what this solution builds against almost everywhere (`net48`, meaning .NET Framework 4.8, set once for the whole solution and inherited by every project). **.NET** (what Microsoft called ".NET Core" for years before dropping the "Core") is the newer, actively-developed, cross-platform successor, and it's what a handful of the more modern, web-facing projects elsewhere in this solution target instead. Both are C#. Both compile with the same compiler, mostly. They are still, underneath, different runtimes with different capabilities, different installed SDKs, and, as you're about to find out, different default project file formats. Knowing the difference exists now saves you a confusing afternoon later, the first time you go looking for a class that exists in one and not the other.

This chapter, like most of the solution, targets .NET Framework 4.8.

### In Visual Studio

1. **File → New → Project.**
2. Search the template list for **Console App**, and from the results, specifically pick **Console App (.NET Framework)**. There's a plain **Console App** in that same list, and it's the wrong one here, it creates a modern .NET project rather than a Framework one. The two icons look almost identical. Read the subtitle.
3. Give it a name and a location, then **Next**.
4. You'll be asked for a **Framework** version. Choose **.NET Framework 4.8** (or **4.8.2**, if that's what you have installed) to match what this solution standardizes on. Then **Create**.
5. Visual Studio hands you a `Program.cs` already containing a `Console.WriteLine("Hello World!")`. Delete it. You're about to write your own, and typing it yourself is the entire point of this chapter.

Here's the part the wizard doesn't warn you about: even having correctly picked the Framework template, Visual Studio still generates an **old-style** `.csproj`, the older, considerably more verbose XML format that lists every source file individually and drags along an `AssemblyInfo.cs` you'll never look at again. It looks something like this, trimmed for mercy:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props" ... />
  <PropertyGroup>
    <Configuration Condition=" '$(Configuration)' == '' ">Debug</Configuration>
    <Platform Condition=" '$(Platform)' == '' ">AnyCPU</Platform>
    <ProjectGuid>{a-guid-nobody-will-ever-read}</ProjectGuid>
    <OutputType>Exe</OutputType>
    <RootNamespace>CSharp.Ch01.HelloWorld</RootNamespace>
    <AssemblyName>CSharp.Ch01.HelloWorld</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
    ...
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System" />
    <Reference Include="System.Core" />
    ...
  </ItemGroup>
  <ItemGroup>
    <Compile Include="Program.cs" />
    <Compile Include="Properties\AssemblyInfo.cs" />
  </ItemGroup>
  <Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets" />
</Project>
```

That is not what this solution's projects look like, and there is no checkbox anywhere in the wizard to fix it. Visual Studio simply doesn't offer "modern SDK-style project, targeting classic .NET Framework" as a combination you can create directly. So: convert it by hand, right now, before writing a single line of the actual lesson. Select everything in `.csproj` and replace it wholesale with:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <RootNamespace>CSharp.Ch01.HelloWorld</RootNamespace>
    <AssemblyName>CSharp.Ch01.HelloWorld</AssemblyName>
  </PropertyGroup>

</Project>
```

> Note: If you changed the project name or namespace, update those values here to match. The rest of the XML is boilerplate that doesn't need to change.

Notice there's no `<TargetFrameworkVersion>` in there at all. This solution sets that once, for every project at once, in a `Directory.Build.props` file at the solution root, so an individual project's `.csproj` only needs to override it if that one project genuinely needs something different. Then delete `Properties\AssemblyInfo.cs` outright, an SDK-style project generates that information at build time instead of keeping a checked-in file around for it, and delete `packages.config` too if Visual Studio gave you one. What's left is a folder with a `.csproj` and a `Program.cs` in it, and nothing else, which is exactly what every other project in this solution looks like.

If you build this in a separate solution, you'll need the `<TargetFrameworkVersion>` like this:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <RootNamespace>CSharp.Ch01.HelloWorld</RootNamespace>
    <AssemblyName>CSharp.Ch01.HelloWorld</AssemblyName>
    <TargetFrameworkVersion>v4.8</TargetFrameworkVersion>
  </PropertyGroup>
</Project>
```

### In VS Code

VS Code doesn't come with a project wizard baked in, it leans on the `dotnet` CLI instead, which is worth learning anyway since it's the same tool running quietly underneath Visual Studio's UI, and it turns out to be the easier of the two paths for exactly the problem above.

1. Open a terminal in the folder where you want the project to live.
2. Run:
   ```pwsh
   dotnet new console -n CSharp.Ch01.HelloWorld -f net48
   ```
   `dotnet new console` scaffolds a runnable console app; `-n` names both the folder and the project; `-f net48` targets .NET Framework 4.8 directly. Unlike the Visual Studio wizard, this produces a correct, minimal, SDK-style `.csproj` on the first try, no conversion required. Occasionally the command line really is the more polished tool.
3. Open the resulting folder in VS Code (`code CSharp.Ch01.HelloWorld`, or File → Open Folder). If this is the first time you've opened a C# project in VS Code, it'll offer to install the C# extension. Accept that offer, you want it.
4. Same deal as Visual Studio: `Program.cs` already has a starter `Console.WriteLine` in it. Delete it.

Either route gets you to the same place, eventually: an empty method waiting for instructions, and a `.csproj` file that's almost suspiciously simple.

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <RootNamespace>CSharp.Ch01.HelloWorld</RootNamespace>
    <AssemblyName>CSharp.Ch01.HelloWorld</AssemblyName>
  </PropertyGroup>

</Project>
```

`Sdk="Microsoft.NET.Sdk"` at the top brings in a mountain of default behavior on your behalf, including "compile every `.cs` file sitting in this folder." Add a file, it gets built, no XML editing required, unlike the considerably more verbose format you may have just finished converting away from. `OutputType=Exe` is the one line doing real, load-bearing work here: it's what tells the build to produce a runnable `.exe` with a console window attached, rather than a `.dll` that only exists to be called from somewhere else.

---

## How to Write This Program

### Step 1: Find the entry point

```csharp
internal static class Program
{
    private static void Main(string[] args)
    {

    }
}
```

Every runnable .NET program needs exactly one entry point, a method named `Main` that the runtime goes looking for and calls first. By convention it lives inside a class called `Program`, though nothing about the runtime actually cares what that class is called, it's hunting for the method, not the container. You could rename the class `Aardvark` and the program would run exactly the same. You should not do this, but you could.

`internal` means nothing outside this one assembly has any business calling into it. `static` means the class can't be instantiated, you will never write `new Program()`, and marking it `static` gets the compiler to enforce that rather than leaving it as a polite suggestion nobody follows.

`args` is whatever got typed after the program's name on the command line, already split into pieces for you. Run the program as `CSharp.Ch01.HelloWorld.exe Ada`, and `args[0]` holds `"Ada"`. Run it with nothing after the name, and `args` is an empty array, length zero. Not `null`. That distinction is about to matter a great deal.

### Step 2: Say hello

```csharp
Console.WriteLine("Hello world!");
```

This needs `using System;` at the top of the file to find `Console` at all, since this solution deliberately doesn't auto-inject that for you the way a brand-new project template would. Nothing hidden, nothing implicit, every type you use gets an explicit `using` directive naming exactly where it came from.

### Step 3: Build and run it, right now, before adding another line

Don't wait until the file is finished. One line of working code deserves to actually run before you pile anything else on top of it.

In Visual Studio, press **F5** (or **Ctrl+F5**, which skips attaching the debugger). In VS Code or a terminal, `dotnet run` from inside the project folder does the same thing. Either way, a console window opens, `Hello world!` appears in it, and the whole toolchain you just spent a section setting up has now proven itself: the compiler found your code, the build produced something runnable, the runtime loaded it, and your text came out the other end.

Congratulations! You're now a programmer! That's not just a glib taunt. You created an instruction to the computer, compiled it, and executed it in your operating system. At its core, that's all being a developer is. Everything else is details.

### Step 4: Wrap the whole thing in try/catch/finally

```csharp
try
{
    Console.WriteLine("Hello world!");
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

Exception handling gets its own full chapter later, but the shape of it starts right here, because it's a habit worth building on day one rather than retrofitting once you've got years of bad habits to unlearn. An exception that escapes `Main` entirely is unhandled, the runtime terminates the process, and on Windows the console window slams shut before you've had a chance to read a single word of what went wrong. Your error message technically did print. For about four milliseconds, into a window that no longer exists.

Each of the three blocks makes a different promise. `try` runs the code you're protecting. `catch` runs only if something in there threw. `finally` runs regardless, success or failure, which is exactly why it's the right place to keep the window open long enough to actually read.

The `while (ex != null)` loop deserves a second look, because it's doing more work than its four lines suggest. Exceptions nest: code catches something low-level and rethrows a more meaningful exception around it, tucking the original away as `InnerException`. Do that a couple of layers deep and printing only the outermost exception leaves you staring at "an error occurred," with the actual cause sitting two links further down a chain you never walked. This loop walks it, printing each exception in turn until it runs out of inner ones to unwrap.

One more small refinement, the `if (!Debugger.IsAttached)` around the exit prompt. Run this from Visual Studio with the debugger attached, and VS already keeps the console window open for you after `Main` returns, so pressing a key to close it yourself is redundant busywork you'd otherwise repeat every single debugging session for the rest of your career. Outside the debugger, double-clicking the `.exe`, running it from a terminal, nothing else is holding that window open, so the prompt is exactly what you need there instead. One binary, two situations, the right behavior in both. This needs `using System.Diagnostics;` for `Debugger`, easy to forget, and the error you get if you do, *"The name 'Debugger' does not exist in the current context,"* is one you'll see roughly ten thousand more times in your career. It almost always means a missing `using`, not missing code.

### Step 5: Split logic into its own method

```csharp
private static void Pause()
{
    Console.WriteLine("\nPress any key to continue...");
    Console.ReadKey();
    Console.Clear();
}
```

Executable code doesn't have to live inside `Main`. `Pause()` waits for a single keypress, no Enter required (that's `ReadKey` rather than `ReadLine`), then clears the screen so the next section of the demo starts fresh. Call it from inside the `try` block wherever you want a breather:

```csharp
Console.WriteLine("Hello world!");
Pause();
```

Notice this method has no `try`/`catch` of its own, and that's deliberate rather than an oversight. If `Pause()` threw, the exception wouldn't stop there, it propagates up the call stack to whoever called it, `Main`, which does have a `catch` and handles it there. This is why you don't need to wrap every single method defensively: catch an exception where you can actually do something about it, and let anything you can't handle rise to a caller who might be able to. A method that can't meaningfully recover from a failure isn't being lazy by not catching it, it's being correct.

### Step 6: Greet someone by name, three different ways

```csharp
string name = args[0];

// The classic way to embed a variable value in a string is using string.Format
Console.WriteLine(string.Format("Hello {0}!", name));

// The WriteLine() method can interpolate formatting without needing "string.Format"
Console.WriteLine("Hello {0}!", name);

// In newer versions of C#, we can accomplish the same thing using string interpolation
Console.WriteLine($"Hello {name}!");
```

Three lines, byte-identical output, and that's the point rather than an oversight. `string.Format("Hello {0}!", name)` is the oldest of the three, tracing its lineage back through Java to C's `printf`. `{0}` is a placeholder referring to the first argument after the format string, and nothing checks at compile time that your placeholder indices line up with what you actually supplied, misnumber one and you get a `FormatException` at runtime for a mistake the compiler cheerfully waved through. `Console.WriteLine("Hello {0}!", name)` skips the `string.Format` wrapper entirely, since `WriteLine` already has an overload that does the same formatting itself, same placeholders, same failure mode, fewer characters typed. String interpolation, `$"Hello {name}!"`, added in C# 6, puts the variable right where it's used instead of making you count placeholder positions, and it's checked at compile time, misspell `name` as `nmae` and the build fails immediately rather than misbehaving at runtime.

You'll meet all three styles in real code, sometimes in the same file, so being able to read all of them is a reading skill worth having. Writing new code is a style rule, though, and the rule is: reach for interpolation.

Now, about that `args[0]` line specifically. Read it again. If you run this program with no command-line argument at all, which is precisely what happens the first time you hit F5 without configuring anything, `args` is an empty array, and asking an empty array for element zero throws `IndexOutOfRangeException`, immediately, no ceremony. This is intentional. You're about to watch that `catch` block you wrote in Step 4 do actual work, on your very first run, rather than reading about exception handling in the abstract and taking it on faith. Run the program without an argument and watch it happen. Then supply one and watch it not happen. Both runs teach you something the other one can't.

(A real, shipped version of this line would guard against the empty case, `args.Length > 0 ? args[0] : "world"` is the shortest fix. This one doesn't, on purpose, for exactly one run, so you can see what an unguarded assumption actually costs.)

### Step 7: Take input directly

```csharp
Console.WriteLine("Enter your name to continue...");
name = Console.ReadLine();
Console.WriteLine($"Hello {name}!");
```

`Console.ReadLine()` blocks, meaning execution stops dead on that line until a person sits down and types something, then presses Enter. It's the simplest possible way to get interactive input, and it's reassigning `name` here rather than redeclaring it, same variable, new value, a plain assignment.

### Step 8: Fold the boilerplate into `#region` blocks, if you like

```csharp
#region Using Directives
using System;
using System.Diagnostics;
#endregion
```

`#region` has exactly zero effect on the compiled output. The compiler notices it, does nothing with it, and produces identical IL whether it's there or not. Its entire purpose is letting your editor collapse a labeled block down to one line, which barely matters at a hundred lines and matters quite a lot once a file runs to a few thousand. This is a genuine style preference, not a rule, some teams ban regions outright on the theory that folding a mess doesn't clean it up. This solution uses them consistently as house style. Take it or leave it in your own code, but recognize it when you see it, because you'll see it in every project from here on.

---

## Run It Yourself

Run it twice, once with a command-line argument and once without, and compare. In Visual Studio, set the argument via the project's Debug properties (right-click the project → Properties → Debug); from a terminal, it's just `CSharp.Ch01.HelloWorld.exe Ada` versus `CSharp.Ch01.HelloWorld.exe` with nothing after it. Watching the `catch` block fire on demand, then not fire, teaches you more about exception handling in thirty seconds than several paragraphs about it ever could.

A few things worth trying once that's done:

- **Make it safe.** Replace `string name = args[0];` with something that defaults to `"world"` when no argument was supplied, and confirm the crash is gone.
- **Greet everyone, not just the first name.** `args` is an array. Nothing's stopping you from looping over all of it.
- **Break the classic style on purpose.** Change one `string.Format` call to reference `{1}` when only one argument exists. It compiles fine, then throws at runtime. Try the same mistake with interpolation instead and notice you never even get as far as running it, the build itself refuses.
- **Nest an exception and watch the chain walk.** Inside the `try`, throw one exception from inside a `catch` for another, passing the first as the new one's inner exception, and watch `while (ex != null)` print both in order.

The habits this chapter is quietly instilling: wrap your entry point, split logic into named methods, catch things only where you can actually do something about them, and print the whole exception chain rather than just the top of it, show up again and again for the rest of this course. Chapter 1 is small on purpose. What it teaches isn't.
