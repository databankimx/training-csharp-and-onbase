# LessonRunner

## What This Is

Not a lesson. A menu-driven launcher for the rest of the training solution: pick a chapter, pick a lesson within it, watch it run, land back on the same lesson menu when it finishes so the next one is a single keypress away.

---

## How It Works

Each lesson is run with `dotnet run --project <path>`, launched as a child process that shares this console window rather than opening a new one. That choice over a direct project reference is deliberate: `LessonRunner` never needs a new reference (and rebuild) added every time a chapter grows another project, it only needs to know the lesson's folder name, and `dotnet run` builds the lesson first if it's out of date.

`FindSolutionRoot()` walks up from wherever the `.exe` actually is looking for `DataBank.DeveloperTraining.sln`, so lesson paths resolve correctly no matter how `LessonRunner` itself was launched, `dotnet run`, F5 in Visual Studio, or double-clicking the built `.exe` directly.

---

## Adding a New Chapter or Lesson

Everything lives in one place: `BuildCatalog()` in `Program.cs`. Each `Chapter` holds a title and an ordered list of `Lesson`s, each `Lesson` just needs a display name for the menu and the project's folder name (which doubles as its `.csproj` base name, every project in this solution keeps those identical on purpose).

Order matters here in a way alphabetical sorting wouldn't get right: lessons are listed in the order they should be taught, not the order their folder names happen to sort. `CSharp.Ch02.TextbookCode.LotteryProgram` comes before `CSharp.Ch02.TextbookCode.AverageGrades` in the menu because that's the order `BasicProgramStructure` actually references them in, alphabetically it'd be the other way around.

---

## Why Plain Classes Instead of Records

`Lesson` and `Chapter` are ordinary classes with `get`-only properties set in the constructor, not C# 9 `record` types, even though records would read a little shorter. Records rely on `init`-only property accessors, which the compiler implements using a marker type, `System.Runtime.CompilerServices.IsExternalInit`, that only exists in the BCL starting with .NET 5. Since this project targets `net48`, that type isn't available, and the project would fail to compile with a fairly confusing error unless a polyfill for that marker type was added. Plain classes with a constructor sidestep the issue entirely and match how every other model class in this solution (`Student`, `DatabankException`, and so on) is already written.
