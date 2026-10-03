# Supplemental: Algorithms -- Search

## What This Is

Two search algorithms - Linear Search and Binary Search - each timed against the same 10,000-element array, with the target deliberately chosen to be absent (forcing the true worst case), and a visual walkthrough available by pressing `V` after either demo.

What changes from the Big-O Concepts project: this one makes O(n) vs. O(log n) tangible through a single concrete task - finding a value in an array - rather than across five different operations. The same task, same data, same target, two fundamentally different approaches. The operation counts tell the story more clearly than any diagram.

---

## Linear Search -- O(n)

```csharp
private static int LinearSearch(int[] array, int target, ref int count)
{
    for (int i = 0; i < array.Length; i++)
    {
        count++;
        if (array[i] == target) return i;
    }
    return -1;
}
```

Visits every element in sequence. Stops at the first match. In the worst case - the target is at the very end, or absent - it visits every element. That worst case is O(n).

The demo searches for a value known not to be present (`target = N`), which forces the full n comparisons every time. This makes the timing stable and the operation count unambiguous.

**When to use it.** When the data is unsorted, or when you're searching a small collection where the overhead of sorting first would outweigh the benefit of binary search. Also when you're searching for multiple criteria or a non-comparable predicate - "find the first element where this function returns true" is linear search by definition.

---

## Binary Search -- O(log n)

```csharp
private static int BinarySearch(int[] array, int target, int low, int high, ref int count)
{
    if (high < low) return -1;
    count++;
    int mid = low + (high - low) / 2;
    if (array[mid] == target) return mid;
    if (array[mid] < target) return BinarySearch(array, target, mid + 1, high, ref count);
    return BinarySearch(array, target, low, mid - 1, ref count);
}
```

The algorithm:
1. If the range is empty, the target isn't here.
2. Check the midpoint.
3. If it matches, done.
4. If the target is larger than the midpoint, search the upper half.
5. If smaller, search the lower half.

Each comparison eliminates half the remaining elements. With 10,000 elements, the worst case is about 14 comparisons (log₂ 10,000 ≈ 13.3). The operation count in the report makes this concrete.

**The prerequisite.** Binary search requires a sorted array. The demo uses `Array.Sort` to sort the data before searching. In a real application, the cost of sorting is part of the equation: if you're searching the same dataset many times, sorting once and binary-searching every subsequent query pays off quickly. For a single search of an unsorted dataset, linear search is often faster overall because it avoids the sort cost.

**`mid = low + (high - low) / 2` instead of `(low + high) / 2`.** The naive midpoint calculation overflows when `low` and `high` are both large integers. The safe form subtracts first to keep the intermediate value within range. This is a well-known bug in many binary search implementations - Java's standard library had it for decades.

---

## The Visual Walkthrough

After either demo, pressing `V` opens a browser-based visualization that steps through the algorithm element by element. Run the algorithm first so the operation count is in front of you, then press `V` to watch the same process animate.

---

## Summary: Two Algorithms, Same Task

| | Linear Search | Binary Search |
|---|---|---|
| Complexity | O(n) | O(log n) |
| Requires sorted data | No | Yes |
| Operations at N=10,000 | ~10,000 | ~14 |
| Best for | Unsorted data, small N, predicates | Sorted data, repeated searches |

Run both and read the operation counts side by side. With N = 10,000: linear search performs ~10,000 operations; binary search performs ~14. That gap, not elapsed time, is where the complexity difference lives.

---

## Takeaways

- Linear search: O(n), no sorting required. The right choice for unsorted data, small datasets, or predicate-based searches.
- Binary search: O(log n), requires sorted data. Dramatically fewer comparisons - about 14 instead of 10,000 at N = 10,000.
- Sorting costs O(n log n). If you search once, sort-then-binary-search is slower than plain linear. If you search many times, sort once and binary-search every time.
- `mid = low + (high - low) / 2`, not `(low + high) / 2` - avoids integer overflow for large indices.
