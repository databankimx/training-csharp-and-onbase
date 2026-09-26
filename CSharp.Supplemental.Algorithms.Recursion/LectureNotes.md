# Recursion - Computing Fibonacci Numbers

## What This Is

Ported from the `training-big-o-code-complexity` repo's `06_examples/fibonacci` (Recursive, Recursive Cached, Iterative, Iterative Non-Cached, Formulaic), plus a new Matrix Exponentiation approach added for comprehensiveness (per direction - approved alongside Miller-Rabin for the Primes project).

This absorbs the standalone `CSharp.Supplemental.Recursion` project from the old `developer-training-bb` solution as well - that project's four approaches (naive recursive, memoized recursive, array-based iterative, rolling-variable iterative) are conceptually identical to four of this project's five source approaches, so there was nothing distinct left to port from it separately.

## What Changed From the Source

- **No `fibo_fcn` delegate array with reflection-based naming** (`alg.Method.Name`) - each approach is its own named method, called directly from the menu, matching the pattern used in Search and Sort.
- **`EfficiencyReport()` and elapsed-time measurement** come from the shared `CSharp.Supplemental.Algorithms.Shared` project.
- **Every demo verifies its result** against a known-correct `f(40)` value before reporting efficiency, rather than just trusting the output.
- **Formulaic's `phi` changed from `float` to `double`** - `float` has meaningfully less precision, and this project's whole point is comparing approaches accurately; using `double` keeps the formulaic result correct at `n = 40` with more headroom before precision loss becomes visible.
- **Formulaic's divisor simplified from `(phi - (1 - phi))` to `Math.Sqrt(5)`** - these are mathematically identical (`phi - (1 - phi) = 2·phi - 1 = √5`, since `phi = (1 + √5) / 2`), just written in the more immediately recognizable textbook form of Binet's formula.
- `internal class Program` → `internal static class Program`, matching the console-app convention used throughout this solution.

## Complexity Check

All five complexity classifications in the source material are correct as stated - Recursive at O(2ⁿ), the three O(n) approaches, and Formulaic at O(1). No corrections needed for the ported approaches. Matrix Exponentiation's O(log n) classification (new to this project) is verified in `Lesson.md`.
