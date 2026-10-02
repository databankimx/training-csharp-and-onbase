# Supplemental: Algorithms -- Sort

## What This Is

Nine sorting algorithms ordered worst to best, each timed against the same 2,000-element shuffled array, with a visual walkthrough available by pressing `V` after any demo. Reading the operation counts and elapsed times together shows where Big-O class differences are the main driver and where implementation details matter more than asymptotic complexity.

---

## The Algorithms

### 1. Bubble Sort -- O(n²)

Repeatedly scans the array, swapping adjacent out-of-order pairs. With a "no swaps this pass" early exit, it genuinely finishes in O(n) on already-sorted input.

Worst case: O(n²) comparisons and swaps. Usually the slowest O(n²) sort in practice because of the high swap count -- every pass can produce up to n-1 swaps.

### 2. Selection Sort -- O(n²)

Finds the smallest remaining element and swaps it into position, one element per pass. No early exit -- always performs the full O(n²) comparisons regardless of input order.

Better than Bubble Sort specifically in one situation: fewer writes. Selection Sort does at most n-1 swaps total (one per pass). On storage where writes are expensive (some flash memory, for example), that matters. Otherwise it's generally considered the weakest of the three O(n²) sorts.

### 3. Insertion Sort -- O(n²), O(n) best case

Builds the sorted portion one element at a time, shifting elements right to make room for the new one. Like Bubble Sort, genuinely O(n) on already (or nearly) sorted input. Generally the best-performing O(n²) sort in practice: fewer comparisons than Bubble, fewer swaps than Bubble, and cache-friendly sequential access.

Used inside hybrid algorithms (Timsort, Introsort) for small sub-arrays because its overhead is lower than the O(n log n) algorithms for small n.

### 4. Shell Sort -- O(n²) worst case, faster in practice

Generalizes Insertion Sort by comparing elements a decreasing gap apart before a final gap-1 pass. This implementation uses a powers-of-two gap sequence, which has a known O(n²) worst case, but the practical constant factor is much smaller than the three algorithms above it. Often faster than the O(n log n) algorithms for moderate N.

### 5. Quick Sort -- O(n²) worst case, O(n log n) average

Picks a pivot (the last element in this implementation), partitions into "less than" and "greater than" piles, then recursively sorts each. On genuinely random input, the partition is well-balanced and the average case is O(n log n), typically the fastest sort in practice due to excellent cache locality.

The worst case -- O(n²) with maximally unbalanced partitions -- occurs on already-sorted (or reverse-sorted) input when the pivot is always the last element. A random or median-of-three pivot selection prevents this; this implementation uses the simple "last element" choice to keep the partitioning logic readable.

### 6. Merge Sort -- O(n log n), guaranteed

Recursively splits in half, sorts each half, merges. O(n log n) is guaranteed regardless of input order -- there's no unlucky pivot to worry about. The cost: O(n) extra memory for the merge step. Quick Sort and Heap Sort sort in place.

### 7. Heap Sort -- O(n log n), guaranteed, in-place

Builds a max-heap (largest element always at the root), then repeatedly swaps the root into its final position and re-heapifies what's left. Guaranteed O(n log n), in-place. Generally slower than Merge Sort and Quick Sort in practice due to poor cache locality -- heap operations jump around the array rather than working through it sequentially.

### 8. Counting Sort -- O(n + k)

Not comparison-based. Counts occurrences of each distinct value and reconstructs the sorted array from the counts. O(n + k) where k is the range of values. Faster than any comparison-based sort when k is small relative to n, but the counting array costs O(k) memory regardless of how many values are actually present. Impractical when the value range is much larger than the dataset.

### 9. Radix Sort -- O(d * (n + k))

Sorts by individual digit from least significant to most significant, using a stable counting pass for each. d is the number of digit positions; for 32-bit integers d is at most 10. In practice O(n) for integer data, at the cost of restricting what can be sorted (non-negative integers, or values that can be offset to non-negative).

---

## Reading the Results

Operation counts show the algorithm's structure. Elapsed times show real-world performance including cache behavior and implementation overhead. The two don't always agree: Shell Sort's operation count is often higher than Quick Sort's but its elapsed time can be lower at small N because of lower per-operation overhead.

The visual walkthrough (`V` after any demo) shows each step animated in a browser.

---

## Takeaways

- O(n²): Bubble, Selection, Insertion, Shell (worst case). Fine for small N or nearly-sorted data.
- O(n log n): Quick (average), Merge, Heap. The practical range for general sorting.
- O(n + k): Counting, Radix. Faster than comparison-based sorts when applicable, with restrictions.
- In-place vs. extra memory: Quick and Heap sort in place; Merge needs O(n) extra; Counting and Radix need O(k) or O(n).
- Guaranteed vs. average: Merge and Heap guarantee O(n log n); Quick Sort's O(n²) worst case requires a bad pivot selection to trigger.
- Insertion Sort is the practical winner for small N -- it's inside Timsort and Introsort for exactly that reason.
