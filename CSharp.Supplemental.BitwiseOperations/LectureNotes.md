# Bitwise Operations

## What This Is

Ported from the standalone `training-bitwise-operations` repo - 19 numbered, progressive markdown lessons plus 11 small C# code examples, consolidated here into one project. Standalone in "Supplementary" per direction, since the closest chapter fit (the type system, Chapter 3) is too early in the curriculum for content this involved.

## Structural Changes From the Source

- **19 separate `.md` files → one `Lesson.md`.** The source repo's lessons are a single continuous narrative (integers → binary → negative numbers → overflow → one's/two's complement → truth tables → each operator in turn → real-world examples), not independent topics - reading them as one document top to bottom is how they were meant to be read. Numbered `##` headers preserve the original file boundaries and numbering.
- **11 separate example folders → one `Program.cs` with a menu.** The source repo's own `.sln` only actually wired up one of the eleven examples (`bitflags`) - the rest existed as loose folders with their own `.csproj`/`Program.cs` but were never part of a runnable solution. Rather than porting that same fragmentation forward (eleven separate projects for content this small), consolidated into one project with a numbered menu, each option matching one demo-bearing lesson. Lets someone pick just the one demo relevant to whatever part of `Lesson.md` they're reading, without needing to sit through the other ten.
- Lesson 14 ("Bit-Flags Redux") doesn't get its own menu option - the `ProductLicenses` enum in the Lesson 12 demo already includes the `Personal`/`Work`/`UltraDeluxe` composite values that lesson describes, so there was nothing further to add code-wise.

## Code Changes

- `internal class Program` → `internal static class Program` in every example, matching the console-app convention used throughout this solution.
- `catch (Exception? ex)` → `catch (Exception ex)` in every example - same reasoning as the `closures` port: a caught exception is never null, so the nullable annotation wasn't meaningful.
- Target framework: inherited `net48` default, no override needed (`FileAccess`, `Enum`, `StringBuilder`, everything used here has existed since the earliest .NET Framework versions).
