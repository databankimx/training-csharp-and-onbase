# Reducing Complexity - Finding Primes

## What This Is

Ported from the `training-big-o-code-complexity` repo's `06_examples/primes` (all five "find all primes below max" approaches), plus a new Miller-Rabin implementation added for comprehensiveness (per direction, approved alongside Fibonacci's matrix exponentiation).

## What Changed From the Source

- **No static `max`-scoped globals or a shared mutable `count` field** - `count` is passed by `ref` into each function, matching the pattern used throughout this Algorithms series.
- **`EfficiencyReport()` and elapsed-time measurement** come from the shared `CSharp.Supplemental.Algorithms.Shared` project.
- **Every "find all primes" demo verifies its result count** against the well-documented number of primes below 100,000 (9,592).
- `internal class Program` → `internal static class Program`, matching the console-app convention used throughout this solution.

## Complexity Corrections

Two labels in the source material needed correcting - both explained in full in `Lesson.md`:

- **"Ok" and "Good"** (trial division up to √n) were labeled `O(n log n)`. The correct classification is **O(n^1.5)** (n√n) - a real complexity-class error, not just an imprecise label, since n^1.5 is asymptotically worse than n log n. The shared `EfficiencyReport`'s automatic classifier was updated with a dedicated n^1.5 bucket (between n log n and n²) specifically so this distinction is caught automatically going forward, not just fixed here as a one-off comment correction.
- **"Best"** (Sieve of Eratosthenes) was labeled simply `O(n)`. The textbook-correct complexity is **O(n log log n)** - close enough to O(n) in practice that the difference is nearly invisible for any realistic n, but worth stating precisely in a lesson specifically about Big-O notation.

"Worst" and "Bad" (both O(n²)) were already labeled correctly in the source material.

## Miller-Rabin

New implementation, not ported from anywhere. Uses `System.Numerics.BigInteger` (needs an explicit `<Reference Include="System.Numerics" />` in a net48 SDK-style project, same as `System.Configuration` needed elsewhere in this solution) to avoid any overflow concerns with the modular exponentiation involved, and to naturally support testing numbers far larger than `long` could represent - which is itself part of the point this demo is making. Tests a known Mersenne prime (2⁶¹ - 1) and a nearby composite number (2⁶¹ + 1), confirming the algorithm correctly identifies both.
