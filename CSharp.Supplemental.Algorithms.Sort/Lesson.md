# Sort Algorithms

Nine sorting algorithms - the three from the Big-O repo (Selection, Bubble, Merge), four ported from a standalone sorting project (Insertion, Shell, Heap, Quick), and two added for comprehensiveness (Counting, Radix). Listed worst to best, per convention - though "worst to best" needs a word of explanation here, since several of these share the same Big-O *class* while performing very differently in practice.

## Why This Order

| # | Algorithm | Worst case | Best/average case | Why it's ordered here |
|---|---|---|---|---|
| 1 | Bubble | O(n²) | O(n) if already sorted | No early shortcut beyond full-pass detection; historically the "worst" O(n²) sort in practice due to how many swaps it performs |
| 2 | Selection | O(n²) | O(n²), always | Always does the full comparison count regardless of input order - no best-case improvement at all, but fewer swaps than Bubble |
| 3 | Insertion | O(n²) | O(n) if nearly sorted | Same asymptotic class as the two above, but the lowest overhead per comparison and the most adaptive to partially-sorted input - generally considered the best O(n²) sort in practice |
| 4 | Shell | O(n²) (this gap sequence) | Notably better in practice | Still O(n²) worst case with the power-of-two gap sequence used here, but meaningfully faster empirically than a "true" O(n²) sort - a deliberate middle step between the O(n²) family and the O(n log n) family |
| 5 | Quick | O(n²) (bad pivot luck) | O(n log n) | Average case matches Merge/Heap, but this implementation's always-pick-the-last-element pivot strategy degrades to O(n²) on already-sorted or reverse-sorted input - see the note below |
| 6 | Merge | O(n log n), guaranteed | O(n log n), guaranteed | No pivot choice to go wrong; costs O(n) extra memory for the merge step |
| 7 | Heap | O(n log n), guaranteed | O(n log n), guaranteed | Same guarantee as Merge, in-place instead of needing extra memory, but generally worse cache locality in practice |
| 8 | Counting | O(n + k) | O(n + k) | Beats every comparison-based sort above when the value range (k) is small relative to n - but doesn't compare elements at all, so it only works for this specific kind of data |
| 9 | Radix | O(d·(n + k)) | O(d·(n + k)) | Same non-comparison approach as Counting, extended to work by digit - effectively O(n) for fixed-width integers, since the digit count (d) is a small constant |

## A Note on Quick Sort's Pivot

This implementation always picks the last element of whatever it's currently partitioning as the pivot. That's simple to follow, but it's a well-known trap: feed it an already-sorted (or reverse-sorted) array, and every partition is maximally unbalanced, degrading to O(n²) - the exact same worst case as the algorithms it's usually faster than. A common real-world fix is picking a random element, or the median of the first/middle/last elements, as the pivot instead - not implemented here, to keep the partitioning logic itself the focus, but worth knowing if you ever see Quick Sort perform surprisingly badly on real data.

## Counting Sort and Radix Sort Aren't Comparison Sorts

Every algorithm above Counting Sort in this list works by comparing pairs of elements - and there's a well-known theoretical floor for that whole category: no comparison-based sort can do better than O(n log n) in the general case. Counting Sort and Radix Sort sidestep that floor entirely by never comparing elements to each other at all - they count occurrences (or digits) instead. That's what makes O(n) sorting *possible* for the right kind of data, and also exactly why it doesn't generalize: both need the values being sorted to be integers (or things that map cleanly to integers) with a reasonably bounded range, not arbitrary comparable objects.

## Try It Yourself

Each menu option generates a fresh shuffled array (every integer in a signed range, so the same dataset shape is used across all nine algorithms), sorts it, and verifies the result is genuinely sorted before printing the efficiency report - a good habit for any sort implementation, not just here.
