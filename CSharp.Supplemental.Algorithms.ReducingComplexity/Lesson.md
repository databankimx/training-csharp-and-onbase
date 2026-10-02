# Supplemental: Algorithms -- Reducing Complexity

## What This Is

Five progressively-optimized approaches to finding every prime below 100,000, plus a sixth that answers a genuinely different question. Each approach is timed against the same target, and the operation counts and elapsed times together show where complexity class changes dominate, where constant-factor improvements matter even within the same class, and where the right answer is recognizing you're solving a smaller problem.

---

## The Five Approaches (Worst to Best)

### 1. Worst -- O(n²)

For every candidate number up to the maximum, try dividing it by every integer from 2 up to the candidate minus 1. Checks factors that could never possibly divide the number -- anything above half the candidate, for a start -- purely because nothing has ruled them out.

At 100,000, this takes a noticeable several seconds.

### 2. Bad -- O(n²)

Same structure, but stops checking factors at `n / 2`. No factor larger than half a number can divide it evenly, so all those checks in "Worst" were wasted. Same O(n²) complexity class -- dividing the inner bound by a constant 2 doesn't change the asymptotic class -- but a real, measurable constant-factor improvement.

Still O(n²). Still slow. But faster than Worst.

### 3. Ok -- O(n^1.5)

Stops checking factors at `√n` instead of `n / 2`. If a number has any factor larger than its square root, it must also have a corresponding factor *smaller* than its square root, so checking past `√n` can never find something the earlier checks didn't catch. This is a genuinely different complexity class -- roughly n * √n operations total, which is O(n^1.5) -- noticeably faster than O(n²) at 100,000.

Worth knowing: O(n^1.5) and O(n log n) can look superficially similar at small n, but they're different classes. O(n log n) grows slower. The source material this was adapted from mislabeled this approach as O(n log n) -- the code comments note the correction.

### 4. Good -- O(n^1.5), smaller constant

Same √n bound as "Ok," but skips even factors after handling 2 as a special case. Any even factor above 2 would require the number to have an even divisor, which is impossible for the odd candidates being checked. Same complexity class as "Ok," but roughly half the inner-loop iterations in practice.

### 5. Best -- O(n log log n) -- Sieve of Eratosthenes

Completely different strategy. Instead of testing each number independently, assume everything is prime, then cross off every multiple of each prime found so far:

```csharp
for (int n = 2; n <= max; n++)
{
    if (isComposite[n]) continue;
    primes.Add(n);
    for (long multiple = (long)n * n; multiple <= max; multiple += n)
        isComposite[multiple] = true;
}
```

The inner loop starts at `n * n` -- every smaller multiple of `n` was already crossed off by a smaller prime. The total work across all crossing-off passes is O(n log log n) -- not simply O(n) as the source material claimed, though log log n grows so slowly that the difference is practically invisible at any n you'd run. (log log 100,000 ≈ 2.4.)

Run Worst, then Best, and look at the elapsed time difference. Both produce the same 9,592 primes. The source code comments document the complexity corrections.

---

## The Sixth Approach: Miller-Rabin -- A Different Question

```
2⁶¹ - 1 = 2,305,843,009,213,693,951
```

This is a known Mersenne prime -- a prime too large for any of the five approaches above to reach by exhaustive checking. Even the Sieve would need an array with more entries than there is memory on the machine.

Miller-Rabin answers the question "is this one specific number prime" without finding any other numbers. It tests a candidate against several "witness" values, each of which proves the candidate composite (if it is) or fails to disprove it (if it's probably prime). Running 20 rounds reduces the false-positive probability below 1 in 10^12 -- not zero, but astronomically unlikely.

The key point: reducing complexity sometimes means recognizing you're solving a smaller problem. Miller-Rabin doesn't find all primes up to N; it answers one specific primality question. That more focused question has a much more tractable solution.

---

## Takeaways

- Constant-factor improvements within the same complexity class are real and measurable, even if they don't change the Big-O label.
- Moving from O(n²) to O(n^1.5) by checking only up to √n is a genuine complexity class reduction, not just a constant factor.
- The Sieve of Eratosthenes is O(n log log n) -- not O(n), though log log n grows so slowly the difference rarely matters in practice.
- Reducing complexity isn't always about optimizing the same algorithm. Sometimes it means recognizing that a different, smaller question can be answered instead.
- Miller-Rabin is probabilistic: it can report "probably prime" for a composite (a false positive), but never "composite" for a true prime. Each round reduces the false-positive probability by at least 75%.
