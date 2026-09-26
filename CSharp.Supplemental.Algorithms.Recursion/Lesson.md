# Recursion - Computing Fibonacci Numbers

Six ways to compute the nth Fibonacci number, listed worst to best. Three of these (Recursive Cached, Iterative array-based, Iterative rolling-variable) share the same O(n) Big-O class but differ meaningfully in overhead - see the ordering rationale below.

## Why This Order

| # | Approach | Complexity | Why it's ordered here |
|---|---|---|---|
| 1 | Recursive (naive) | O(2ⁿ) | Recomputes the same values over and over - f(5) alone recomputes f(3) twice, f(2) three times, and so on |
| 2 | Recursive, Cached | O(n) | Remembers every value already computed, so each distinct n is only ever calculated once - but still pays for recursive call overhead and dictionary lookups on every call |
| 3 | Iterative (array-based) | O(n) | No recursion at all, but allocates and fills a full array of every value from 0 to n along the way |
| 4 | Iterative (rolling variables) | O(n) | Same iteration count as the array-based version, but only ever tracks the two most recent values - O(1) memory instead of O(n), no allocation at all |
| 5 | Matrix Exponentiation | O(log n) | The first approach here that's asymptotically better than every O(n) one above it |
| 6 | Formulaic (Binet's formula) | O(1) | A fixed number of arithmetic operations, regardless of how large n is |

## Matrix Exponentiation

Fibonacci numbers satisfy a neat identity:

```
[F(n+1)  F(n)  ]   [1 1]ⁿ
[F(n)    F(n-1)] = [1 0]
```

Raising that 2×2 matrix to the nth power gives F(n) directly. Multiplying two fixed-size 2×2 matrices is always the same handful of operations (O(1)), and *exponentiation by squaring* - repeatedly squaring the base matrix and multiplying it into a running result only on the exponent's set bits - computes the nth power in O(log n) matrix multiplications instead of n. It's the same "halve the remaining problem each step" idea Binary Search uses, just applied to an exponent instead of an array.

This isn't in the original source material this project was ported from - added for comprehensiveness, since it's a genuinely different (and asymptotically better) approach than any of the five iteration/recursion-based ones, without being the "just skip straight to the formula" jump that Binet's formula represents.

## Formulaic (Binet's Formula)

f(n) = (φⁿ - (1-φ)ⁿ) / √5, where φ (the golden ratio) = (1 + √5) / 2. Genuinely O(1) - the same handful of arithmetic operations no matter how large n is. Worth knowing: this relies on floating-point math, and loses precision at a large enough n, unlike every integer-only approach above it (including matrix exponentiation, which stays exact throughout).

## Try It Yourself

Every menu option computes f(40) and checks the result against a known-correct value before reporting efficiency - a good habit whenever multiple independent implementations are all supposed to agree.
