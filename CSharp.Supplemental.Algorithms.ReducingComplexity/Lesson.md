# Reducing Complexity - Finding Primes

Six approaches - five that progressively optimize the same task (find every prime below a maximum, worst to best), and a sixth (Miller-Rabin) that answers a fundamentally different question.

## The Five "Find All Primes Below N" Approaches

| # | Approach | Complexity | What changed from the one before it |
|---|---|---|---|
| 1 | Worst | O(n²) | Checks every possible factor from 2 up to the number itself |
| 2 | Bad | O(n²) | Stops at n/2 - no factor larger than half a number can ever divide it. Same complexity *class*, real constant-factor improvement |
| 3 | Ok | O(n^1.5) (n√n) | Stops at √n instead - a genuinely different, better complexity class, not just a smaller constant |
| 4 | Good | O(n^1.5) | Same √n bound as Ok, but skips even factors - same class as Ok, smaller constant |
| 5 | Best | O(n log log n) | A completely different strategy (Sieve of Eratosthenes) - cross off multiples instead of testing each number individually |

### A Complexity Correction: "Ok" and "Good"

The source material this was adapted from labeled both of these `O(n log n)`. That's not correct - trial division up to √n does roughly n·√n operations total, which is **O(n^1.5)** (also written O(n√n)), and n^1.5 is meaningfully worse than n log n (√n grows faster than log n does). The two can look deceptively similar at a glance, especially when the reasoning is written as "n · √n = O(n log n)" the way the source material had it - but that's not a valid simplification. `EfficiencyReport`'s automatic classifier (in the shared project) now has a dedicated bucket for n^1.5, sitting between n log n and n², specifically so this distinction shows up automatically rather than getting silently absorbed into whichever neighboring bucket happens to be closest.

### A Complexity Correction: "Best"

The source material labeled the Sieve of Eratosthenes as simply `O(n)`. The textbook-correct complexity is **O(n log log n)** - each prime found crosses off its own multiples, and summing that work across every prime up to n produces a log log n factor (a well-established result, not something worth re-deriving here). In practice this is nearly indistinguishable from O(n): log log n grows so slowly that log log 100,000 is only around 2.4, and log log of a number with a million digits is still under 5. Worth stating precisely anyway, in a lesson specifically about Big-O.

## Miller-Rabin: A Different Question

Every approach above answers "which numbers below N are prime" - it needs to check every candidate to answer that. Miller-Rabin answers a different question: "is this *one specific* number prime," without needing any information about any other number at all.

That distinction matters because it changes what's actually possible. This demo tests 2⁶¹ - 1 (a well-known, independently verifiable Mersenne prime) - a number that would take "Worst" or "Bad" an effectively unreachable amount of time to even approach via trial division, and that even "Best"'s sieve couldn't handle at all (it would need an array with more entries than there's memory to hold). Miller-Rabin tests it directly, in a small, fixed number of rounds, regardless of how large the number is.

The catch: Miller-Rabin is *probabilistic*, not deterministic. It can occasionally call a composite number "probably prime" (a false positive) - though never the reverse, a true prime is never mistakenly called composite. Each round, using a different randomly-chosen witness value, cuts the false-positive probability by at least 75%. This demo runs 20 rounds, making a false positive astronomically unlikely without ever being strictly impossible - a small, controllable, quantifiable chance of error, traded for the ability to test numbers no exhaustive method could ever reach in reasonable time. That trade-off is itself a form of "reducing complexity" - not a further-optimized version of the same algorithm, but a recognition that the problem being solved doesn't have to be the bigger one in the first place.

## Try It Yourself

Options 1-5 each verify their result against the well-documented count of primes below 100,000 (9,592). Option 6 tests both the known Mersenne prime and a nearby composite number, confirming Miller-Rabin correctly identifies each.
