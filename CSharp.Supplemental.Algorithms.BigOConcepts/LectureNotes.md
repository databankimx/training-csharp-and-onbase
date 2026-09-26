# Big-O Complexity - General Concept

## What This Is

Ported from the `training-big-o-code-complexity` repo's `00_training_docs/00_understanding_big_o.md` - the concept introduction the rest of this Algorithms series builds on. This is the entry point for the whole series: Search, Sort, Recursion, and Reducing Complexity each go deep on one or two of the complexity classes introduced here.

## What Changed From the Source

- **Added a genuinely runnable demo for each complexity class**, rather than just the static code snippets the source material had on the page - `GetFirstElement` (O(1)), `SumAllElements` (O(n)), `BinarySearch` (O(log n)), `MergeSort` (O(n log n)), and `CountDuplicates` (O(n²)), each using the same `EfficiencyReport`/`DataGenerator`/`Stopwatch` pattern as every other project in this series.
- The source's `PrintAllElements` example was renamed `SumAllElements` here - printing 2,000 lines to the console on every run isn't a great demo experience; summing accomplishes the same "visit every element once" point while actually returning something worth checking.
- Cross-references added pointing to the dedicated Search, Sort, and Recursion projects wherever this lesson's simplified examples overlap with their fuller treatment.
- `big_o_chart_only.png` (~986 KB) wasn't copied through directly, for the same reason `CSharp.Supplemental.TrieExamples`' word list wasn't - a binary file that size isn't safe to push through text-editing tools. Copy it manually:
  ```
  Copy-Item "C:\Development\training-big-o-code-complexity\00_training_docs\_resources\big_o_chart_only.png" "C:\Development\training\developer-training\CSharp.Supplemental.Algorithms.BigOConcepts\big_o_chart_only.png"
  ```

## Complexity Check

All classifications in the source material's concept introduction are correct as stated. The corrections found elsewhere in this series (Sort's Shell/Quick Sort labels, Primes' "Ok"/"Good"/"Best" labels) are specific to those projects' own more detailed material, not this overview.
