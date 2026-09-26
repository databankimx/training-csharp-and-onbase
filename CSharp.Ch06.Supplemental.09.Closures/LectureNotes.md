# Closures

## What This Is

Ported from the loose `closures` repo - already a complete, well-written C# example with a matching write-up (`closures.md`, now `Lesson.md`), covering closures conceptually and then comparing the same core example across Python, JavaScript, Rust, C++, and Java.

Placed as `CSharp.Ch06.Supplemental.09.Closures`, next in the numbered sequence alongside this chapter's other supplementals (`01.NamedVersusAnonymousDelegates` through `08.Assertions`) - closures are closely tied to the lambda/delegate material this chapter already covers, so this fits as a chapter supplemental rather than something standalone in "Supplementary."

## What Changed From the Original

- Target framework changed from the source repo's `net10.0` to this solution's inherited `net48` default, matching every other `CSharp.Ch06.Supplemental.*` project (none of them override the target framework).
- That meant dropping the `ImplicitUsings`/`Nullable` overrides the original had - added explicit `using System;` and `using System.Collections.Generic;` instead.
- `catch (Exception? ex)` had a nullable annotation on the caught exception, which isn't really meaningful (a caught exception is never null) - simplified to `catch (Exception ex)`.
- `internal class Program` → `internal static class Program`, matching the console-app convention used throughout this solution.
- Didn't add a `CSharp.SharedLibrary` reference, even though the sibling supplemental (`02.LambdaExpressions`) has one - this lesson doesn't use `DatabankException` or anything else from it, so there was nothing to reference.
