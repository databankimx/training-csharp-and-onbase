# Supplemental: Algorithms -- Big-O Concepts

## What This Is

Five complexity classes introduced through small, runnable examples. This project is the entry point for the Algorithms series -- the four dedicated projects (Search, Sort, Recursion, Reducing Complexity) each go deep on one class; this one introduces what the classes actually mean, side by side, with an operation counter and a timer on each.

---

## How to Use This Project

Menu-driven. Pick a complexity class, run the demo, and read the operation count and elapsed time the `EfficiencyReport` prints. Then come back and try a different one. The demo size is `N = 2,000` -- large enough to see real differences, small enough that even O(n²) finishes quickly.

---

## The Five Classes

### O(1) -- Constant Time

```csharp
private static int GetFirstElement(int[] array, ref int count)
{
    count++;
    return array[0]; // One operation, always
}
```

The array size doesn't affect how long this takes. Whether `array` has 10 elements or 10 million, index `[0]` is calculated as `baseAddress + 0` and returned in one step. The operation count in the report is always `1`.

### O(n) -- Linear Time

```csharp
private static long SumAllElements(int[] array, ref int count)
{
    long sum = 0;
    foreach (int value in array)
    {
        count++;
        sum += value;
    }
    return sum;
}
```

Every element is visited exactly once. Double the array size, double the work. The operation count equals `N`.

### O(log n) -- Logarithmic Time

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

Each comparison halves the remaining search space. With 2,000 elements, the worst case is about 11 comparisons (log₂ 2,000 ≈ 11). With 2,000,000 elements, it's about 21. The operation count grows logarithmically -- very slowly relative to N.

Binary search requires the array to be sorted first. The `Search Algorithms` project covers this in full, including timing both algorithms against each other.

### O(n log n) -- Loglinear Time

```csharp
private static int[] MergeSort(int[] array, ref int count)
{
    if (array.Length <= 1) return array;
    int mid = array.Length / 2;
    int[] left  = MergeSort(array.Take(mid).ToArray(), ref count);
    int[] right = MergeSort(array.Skip(mid).ToArray(), ref count);
    // ... merge left and right
}
```

An O(log n) step (halving the array into sub-arrays) repeated O(n) times (every element participates in the merges). The operation count grows faster than linear but far slower than quadratic. This is the complexity class of the efficient comparison-based sorts -- Merge Sort, Heap Sort, and Quick Sort (on average). The `Sort Algorithms` project covers all nine.

### O(n²) -- Quadratic Time

```csharp
private static int[] CountDuplicates(int[] array, ref int count)
{
    var result = new int[array.Length];
    for (int i = 0; i < array.Length; i++)
    {
        for (int j = 0; j < array.Length; j++)
        {
            count++;
            if (j != i && array[j] == array[i]) result[i]++;
        }
    }
    return result;
}
```

A loop inside a loop, both running to `n`. The operation count is `n²`. With 11 elements (the demo uses a small fixed array here to keep the output readable) that's 121 operations. With 2,000 it would be 4,000,000. With 10,000 it would be 100,000,000. This is why O(n²) algorithms are fine for small inputs and genuinely painful for large ones.

---

## Reading the Efficiency Report

Each demo prints three numbers:

- **N**: the input size
- **Operations**: how many times the counting line inside the algorithm incremented
- **Elapsed**: wall-clock time for the algorithm itself

The operation count is more informative than elapsed time for understanding complexity -- elapsed time includes OS scheduling noise, JIT compilation effects, and cache behavior. The count shows the algorithm's structure directly.

---

## Takeaways

- O(1): fixed work regardless of input size. Array index access, hash table lookup.
- O(n): one pass through the input. Summing, scanning, linear search.
- O(log n): halving the problem each step. Binary search, balanced tree operations.
- O(n log n): the efficient sort class. Merge Sort, Heap Sort, Quick Sort average.
- O(n²): nested loops over the same data. Bubble Sort, naive duplicate counting. Fine for small N, painful for large.
- Big-O describes the growth rate, not the absolute cost. An O(n) algorithm with a large constant can be slower than an O(n²) algorithm at small N.
