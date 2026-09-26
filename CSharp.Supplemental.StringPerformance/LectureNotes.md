# String Performance

## What This Is

Adapted from the loose `C# String Performance` repo. Original `Program.cs` only demonstrated one of the five methods its own write-up (`StringPerformance.md`, now `Lesson.md`) actually lists under "Methods Considered" (`string.Equals`, `Compare`, `IndexOf`, `StartsWith`, `EndsWith`). Expanded here to demonstrate all five with the same before/after pattern the original used for `Equals` - explicit `StringComparison.OrdinalIgnoreCase` next to the version without it, so the difference in outcome is visible rather than just described.

## What Changed From the Original

- The original's `Uri url = new("https://learn.microsoft.com/");` uses target-typed `new`, which needs C# 9 or later. This solution's default target framework (`net48`, from `Directory.Build.props`) doesn't enable that language version by default. Rather than overriding the target framework or language version just for one line, rewrote it the traditional way (`new Uri(...)`) - functionally identical, no special-casing needed.
- Top-level statements (no `internal static class Program`) kept deliberately, matching the original source's own style. Every other console lesson in this solution uses the explicit-class style; this is a fine, idiomatic modern C# alternative worth having represented at least once.
- Registered with `LessonRunner` - this one needs no arguments and is meant to just run and be watched, unlike `OnBase.Preprocessor`.
