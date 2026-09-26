# Sort Algorithms

## What This Is

Combines three sources: Selection/Bubble/Merge sort from the `training-big-o-code-complexity` repo, Insertion/Shell/Heap/Quick sort ported from the standalone `CSharp.SUpplemental.Sorting` project (old `developer-training-bb` solution), and Counting/Radix sort added new for comprehensiveness (per direction - "more is better" for this project).

## What Changed From the Sources

- **No static data file** - same reasoning as Search Algorithms. Every demo generates a shuffled array via `DataGenerator.GenerateShuffledArray(N, signedRange: true)`.
- **Every demo verifies its own result** (`IsSorted()`) before reporting efficiency - none of the source material did this, and it's a small, worthwhile addition: a sort that reports a beautiful O(n log n) but silently produced the wrong answer isn't actually a good outcome.
- **`EfficiencyReport()`** comes from the shared `CSharp.Supplemental.Algorithms.Shared` project rather than being copy-pasted per algorithm.
- **`ApplicationException` → more specific exceptions is a moot point here** - none of these implementations needed to throw at all once ported (the originals' `try`/`catch`/`throw new ApplicationException(...)` wrapping was mostly defensive boilerplate around code that can't actually fail given valid input), so it was simply dropped rather than replaced.
- **`internal class Program` → `internal static class Program`**, matching the console-app convention used throughout this solution.

## Complexity Corrections

Two labels in the source material needed correcting - see `Lesson.md` for the full explanation of each:

- **Shell Sort** was labeled simply "O(n²)" with no further context. That's a defensible worst-case label for this specific gap sequence, but stated alone it implies Shell Sort is no better than Bubble/Selection/Insertion, when empirically it's notably faster. `Lesson.md`'s table makes this distinction explicit rather than leaving the shared label to imply equivalent performance.
- **Quick Sort** was labeled "O(n*log(n))" with no mention of its worst case. That's the *average* case for random input; the actual worst case (triggered by this implementation's always-last-element pivot choice on already-sorted or reverse-sorted input) is O(n²). Both are now stated explicitly.

Everything else (Bubble/Selection/Insertion at O(n²), Merge/Heap at guaranteed O(n log n)) was already labeled correctly in the source material.

## New Algorithms

Counting Sort and Radix Sort are new implementations, not ported from anywhere - see `Lesson.md` for why they're worth including (the first two algorithms here that aren't comparison-based at all, which is what lets them beat the O(n log n) floor every comparison sort is bound by). Both fundamentally work on non-negative integers; since the shared data generator can produce a signed range, both offset the array to non-negative before sorting and restore the original values afterward.
