# Supplemental: Algorithms -- Shared

## What This Is

A class library shared by all five algorithm projects (BigOConcepts, Search, Sort, Recursion, ReducingComplexity). Not a runnable project -- a dependency. No Lesson.md is needed for consumers of the library, but the three classes here are worth understanding since their output appears in every algorithm demo.

---

## `DataGenerator`

Generates test data for the algorithm demos:

```csharp
int[] array = DataGenerator.GenerateShuffledArray(N);
int[] array = DataGenerator.GenerateShuffledArray(N, signedRange: true);
```

Produces an array of N integers in a shuffled order. Without `signedRange`, values run 0 to N-1. With `signedRange: true`, the range includes negative values -- used by the Sort project since some algorithms (Counting Sort, Radix Sort) need special handling for signed input.

Using a pre-seeded, deterministic shuffle would produce the same input every run, which makes timing results more reproducible. This implementation uses `System.Random` with a random seed, so results vary slightly between runs -- acceptable for a training demo, worth noting if you ever adapt this pattern for benchmarking.

## `EfficiencyReport`

Prints the operation count and elapsed time for each demo:

```csharp
EfficiencyReport.Print("BinarySearch", N, count, timer.Elapsed);
```

Output format:
```
BinarySearch: N=10,000 | Operations=14 | Elapsed=0.12ms
```

The operation count comes from a `ref int count` parameter threaded through every algorithm -- each comparison or key operation increments it. This is the number the Big-O class describes; the elapsed time adds real-world context including JIT effects and cache behavior.

## `Visualization`

Opens the appropriate browser-based visualization when `V` is pressed:

```csharp
Visualization.Open("binary-search");
Visualization.OpenSieve();
```

Uses `Process.Start` with `UseShellExecute = true` to open `player.html` or `sieve.html` from the `CSharp.Supplemental.Algorithms.Visualizations` project directory, resolved relative to the calling assembly's location.
