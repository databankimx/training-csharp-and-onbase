# Search Algorithms

Two fundamentally different strategies for finding a value in an array - listed here worst to best, matching the convention used throughout this Algorithms series.

---

## Linear Search - O(n)

The straightforward approach: check every element in turn until the target is found (or the array runs out).

```
For every element 0 to n - 1
    If the element is what we're looking for, quit
    Else, continue to the next element
```

Imagine searching an 8-element unsorted array `[ 7 2 5 4 1 6 0 3 ]` for the number 3. In the worst case (the target is last, or absent entirely), that's 8 comparisons for 8 elements - the number of comparisons grows in direct proportion to the size of the input. That's what makes this **O(n)**: double the array, and you roughly double the worst-case number of comparisons.

**Try it**: menu option 1. The demo array is a shuffled range of 10,000 values, searched for a value guaranteed *not* to be present - forcing the true worst case, a full scan with no early exit.

---

## Binary Search - O(log n)

Requires a sorted array, but pays for that requirement with dramatically better performance: repeatedly check the midpoint, and eliminate half the remaining array each time.

```
(start) Select the midpoint of the array
    Compare the value there with the target
    If it's the target, quit
    Else if it's greater than the target, discard the upper half and go back to (start)
    Else, discard the lower half and go back to (start)
```

Searching a sorted 8-element array `[ 0 1 2 3 4 5 6 7 ]` for 3: check index 4 (value 4, too high, discard the upper half) → check index 1 (value 2, too low, discard the lower half) → check index 0 of what's left (value 3, found). Three comparisons for 8 elements.

That 3 isn't a coincidence: log₂8 = 3. Each comparison eliminates half of what's left, so the number of comparisons needed is the base-2 logarithm of the array size - hence **O(log n)**.

The practical difference is dramatic and grows with the data: for an array of 10,000 elements, log₂10,000 is about 14 - over 700 times fewer comparisons than a worst-case linear search of the same array. At a million elements, that gap widens to roughly 50,000 times fewer.

**Try it**: menu option 2. Same 10,000-value dataset as the linear search demo, sorted first (binary search requires it), searched for the same guaranteed-absent value to force the worst case.
