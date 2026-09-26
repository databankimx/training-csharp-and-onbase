# Search Algorithms

## What This Is

Ported from the `training-big-o-code-complexity` repo's `01_linear_search` and `02_binary_search` (both the write-ups, combined into one `Lesson.md`, and the C# implementations).

## What Changed From the Original

- **No static data file.** The source read from a shared `_data/unordered_integers.txt`/`ordered_integers.txt` (~52 KB each). Both demos now generate a shuffled array at runtime via the shared `DataGenerator.GenerateShuffledArray()`, and search for a value guaranteed to be absent (`N`, one past the generated `0..N-1` range) to force the genuine worst case on every run, rather than depending on whatever happened to be in a static file.
- **The `count` field is no longer static shared state** - each demo method owns its own local counter, passed by `ref` into the search function, rather than a class-level field both demos would otherwise share.
- **`EfficiencyReport()` extracted to the shared `CSharp.Supplemental.Algorithms.Shared` project**, used by every project in this Algorithms series rather than copy-pasted into each one.
- **Binary search's midpoint calculation** changed from `(int)Math.Floor((high + low) / 2d)` to `low + (high - low) / 2` - the original can overflow `int` for a large enough array (if `high + low` exceeds `int.MaxValue`), the second form can't. Not a practical concern at this demo's array size, but worth fixing while already rewriting the method.
- `internal class Program` → `internal static class Program`, matching the console-app convention used throughout this solution.

## Complexity Check

Both classifications in the source material are correct as stated - Linear Search is O(n), Binary Search is O(log n). No corrections needed here.
