# Supplemental: Algorithms -- Recursion

## What This Is

Six ways to compute the nth Fibonacci number, ordered worst to best. The point isn't the Fibonacci result -- it's watching how the same mathematical problem can be solved at O(2ⁿ), O(n), O(log n), and O(1), and understanding why each approach sits where it does.

The demo uses N = 40. The naive recursive approach takes several seconds; every other approach is essentially instant. That contrast is the lesson.

---

## The Six Approaches

### 1. Recursive (naive) -- O(2ⁿ)

```csharp
private static int RunRecursive(int n, ref int count)
{
    count++;
    if (n < 2) return n;
    return RunRecursive(n - 1, ref count) + RunRecursive(n - 2, ref count);
}
```

Every call spawns two more calls, all the way down to the base cases. `f(40)` causes roughly 330 million recursive calls. Each distinct sub-problem (`f(5)`, `f(10)`, etc.) is recomputed from scratch every time it's needed -- there's no memory of anything already computed.

The tightest Big-O classification is O(φⁿ) where φ ≈ 1.618 (the golden ratio), but O(2ⁿ) is the standard description since both are exponential and φ < 2.

### 2. Recursive, Cached -- O(n)

Same recursive shape, but stores each computed value in a dictionary. Each distinct n is computed once; subsequent calls return the cached result immediately.

Still pays for recursive call overhead and dictionary lookups on every call. O(n) is the right Big-O class, but the constant factor is higher than the iterative approaches -- which is why it's ordered behind them despite sharing the class.

### 3. Iterative (array-based) -- O(n)

```csharp
var values = new int[n + 1];
values[0] = 0; values[1] = 1;
for (int i = 2; i <= n; i++)
    values[i] = values[i - 1] + values[i - 2];
return values[n];
```

Builds the full sequence from the bottom up, no recursion. O(n) time and O(n) space (the array).

### 4. Iterative (rolling variables) -- O(n), O(1) space

```csharp
int secondLast = 0, last = 1;
for (int i = 2; i <= n; i++)
{
    int result = last + secondLast;
    secondLast = last;
    last = result;
}
return last;
```

Same O(n) iterations as the array version, but only two variables instead of an n-element array. No allocation. The lowest-overhead O(n) approach.

### 5. Matrix Exponentiation -- O(log n)

Fibonacci numbers satisfy a matrix identity:

```
[F(n+1)  F(n)  ]   [1 1]ⁿ
[F(n)    F(n-1)] = [1 0]
```

Raising that 2x2 matrix to the nth power gives `F(n)`. Matrix multiplication for a fixed 2x2 size is O(1). Computing the nth power via exponentiation by squaring takes O(log n) multiplications -- the same "halve the problem each step" idea as binary search.

The implementation uses bit manipulation to iterate through the binary representation of the exponent, squaring the base matrix and multiplying it into the result only on set bits.

### 6. Formulaic (Binet's Formula) -- O(1)

```csharp
double phi = (1 + Math.Sqrt(5)) / 2;
double result = (Math.Pow(phi, n) - Math.Pow(1 - phi, n)) / Math.Sqrt(5);
return (int)Math.Round(result);
```

Computes `F(n)` directly from n with a fixed number of arithmetic operations regardless of n. Genuinely O(1).

The caveat: this uses floating-point math. `Math.Pow(phi, n)` loses precision as n grows. For n up to about 70 it rounds correctly; beyond that the floating-point error exceeds 0.5 and the rounded result is wrong. Every other approach here works with exact integers and has no such limit.

---

## Running the Demo

Run option 1 first and watch it take several seconds. Then run options 2-6 in sequence -- all essentially instant. The operation count in the report makes the exponential/linear/logarithmic/constant progression concrete.

All six verify their result against the known correct value for `f(40)` = 102,334,155.

---

## Takeaways

- O(2ⁿ) is catastrophic at any meaningful n. Never use naive recursion for overlapping subproblems.
- Caching (memoization) converts O(2ⁿ) to O(n) by ensuring each subproblem is solved once.
- Bottom-up iteration avoids call stack overhead and is usually faster than top-down recursion with caching.
- Rolling variables reduce O(n) memory to O(1) when only the last few values are needed.
- Mathematical structure can enable dramatically better complexity -- O(log n) via matrix exponentiation, O(1) via closed-form formula.
- O(1) formulas based on floating-point have precision limits. Know what they are before relying on them.
