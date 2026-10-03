# Supplemental: Algorithms -- Sort

## What This Is

Nine sorting algorithms ordered worst to best, each timed against the same 2,000-element shuffled array, with a visual walkthrough available by pressing `V` after any demo.

What changes from the Search project: search had two algorithms and one task. Sort has nine algorithms across four distinct complexity classes (O(n²), O(n log n), O(n + k), and O(n^1.5) for Shell Sort in practice), plus two non-comparison-based algorithms that break the O(n log n) lower bound for comparison sorts entirely. Reading the operation counts and elapsed times together shows where Big-O class differences dominate, and where implementation details matter more than asymptotic complexity.

---

## The Algorithms

### 1. Bubble Sort -- O(n²)

Bubble Sort repeatedly scans the array, swapping adjacent out-of-order pairs. With a "no swaps this pass" early exit, it genuinely finishes in O(n) on already-sorted input.

```csharp
private static int[] BubbleSort(int[] array, ref int count)
{
    var arr = (int[])array.Clone();
    for (int i = 0; i < arr.Length - 1; i++)
    {
        bool swapped = false;
        for (int j = 0; j < arr.Length - 1 - i; j++)
        {
            count++;
            if (arr[j] > arr[j + 1])
            {
                (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                swapped = true;
            }
        }
        if (!swapped) break; // already sorted - O(n) best case
    }
    return arr;
}
```

The outer loop shrinks by one each pass because the largest unsorted element "bubbles" to its final position on each pass. `swapped = false` on a full pass means no elements were out of order - the array is sorted, and we exit early.

Worst case: O(n²) comparisons and swaps. Usually the slowest O(n²) sort in practice because of the high swap count - every pass can produce up to n-1 swaps.

### 2. Selection Sort -- O(n²)

Selection Sort finds the minimum element in the unsorted portion and swaps it into its correct position. No early exit - always performs the full O(n²) comparisons regardless of input order.

```csharp
private static int[] SelectionSort(int[] array, ref int count)
{
    var arr = (int[])array.Clone();
    for (int i = 0; i < arr.Length - 1; i++)
    {
        int minIndex = i;
        for (int j = i + 1; j < arr.Length; j++)
        {
            count++;
            if (arr[j] < arr[minIndex])
                minIndex = j;
        }
        if (minIndex != i)
            (arr[i], arr[minIndex]) = (arr[minIndex], arr[i]);
    }
    return arr;
}
```

The inner loop always runs to the end - there's no early exit regardless of how sorted the input is. Each outer iteration does exactly one swap (or zero if the minimum is already in place).

Better than Bubble Sort specifically in one situation: fewer writes. Selection Sort does at most n-1 swaps total (one per outer iteration). On storage where writes are expensive (some flash memory), that matters. Otherwise generally considered the weakest of the three O(n²) sorts.

### 3. Insertion Sort -- O(n²), O(n) best case

Insertion Sort builds the sorted portion one element at a time, shifting elements right to make room for the incoming one.

```csharp
private static int[] InsertionSort(int[] array, ref int count)
{
    var arr = (int[])array.Clone();
    for (int i = 1; i < arr.Length; i++)
    {
        int key = arr[i];
        int j = i - 1;
        while (j >= 0 && arr[j] > key)
        {
            count++;
            arr[j + 1] = arr[j];
            j--;
        }
        count++;
        arr[j + 1] = key;
    }
    return arr;
}
```

`key` is the element being inserted into the already-sorted left portion. The `while` loop shifts elements one position right until it finds where `key` belongs. On already-sorted input the `while` condition is always false - O(n) total, one comparison per outer iteration.

Generally the best-performing O(n²) sort in practice: fewer comparisons than Bubble, fewer swaps than Bubble, and cache-friendly sequential access. Used inside hybrid algorithms (Timsort, Introsort) for small sub-arrays because its overhead is lower than the O(n log n) algorithms at small n.

### 4. Shell Sort -- O(n²) worst case, faster in practice

Shell Sort is a generalization of Insertion Sort. Instead of always comparing adjacent elements (gap = 1), it starts with a large gap and reduces it each pass, doing a final gap-1 pass at the end.

```csharp
private static int[] ShellSort(int[] array, ref int count)
{
    var arr = (int[])array.Clone();
    int gap = 1;
    while (gap < arr.Length / 3) gap = gap * 2 + 1; // powers-of-two gap sequence

    while (gap >= 1)
    {
        for (int i = gap; i < arr.Length; i++)
        {
            int key = arr[i];
            int j = i;
            while (j >= gap && arr[j - gap] > key)
            {
                count++;
                arr[j] = arr[j - gap];
                j -= gap;
            }
            count++;
            arr[j] = key;
        }
        gap /= 2;
    }
    return arr;
}
```

The large initial gaps move elements long distances quickly, reducing the disorder before the final O(n²) gap-1 pass has to deal with it. This powers-of-two gap sequence has a known O(n²) worst case, but the practical constant factor is much smaller than the three algorithms above it. Often faster than O(n log n) algorithms for moderate N.

### 5. Quick Sort -- O(n²) worst case, O(n log n) average

Quick Sort picks a pivot, partitions everything smaller to the left and everything larger to the right, then recursively sorts both sides.

```csharp
private static void QuickSort(int[] arr, int low, int high, ref int count)
{
    if (low >= high) return;
    int pivotIndex = Partition(arr, low, high, ref count);
    QuickSort(arr, low, pivotIndex - 1, ref count);
    QuickSort(arr, pivotIndex + 1, high, ref count);
}

private static int Partition(int[] arr, int low, int high, ref int count)
{
    int pivot = arr[high]; // last element as pivot
    int i = low - 1;
    for (int j = low; j < high; j++)
    {
        count++;
        if (arr[j] <= pivot)
        {
            i++;
            (arr[i], arr[j]) = (arr[j], arr[i]);
        }
    }
    (arr[i + 1], arr[high]) = (arr[high], arr[i + 1]);
    return i + 1;
}
```

`Partition` walks the array, keeping elements smaller than the pivot on the left. When it finishes, the pivot is in its final sorted position. Everything to its left is smaller; everything to its right is larger. Both sides are then sorted recursively.

On genuinely random input the partition is well-balanced and the average case is O(n log n) - typically the fastest sort in practice due to excellent cache locality. The worst case - O(n²) - occurs on already-sorted input when the pivot is always the last element and one side of the partition is always empty. A random or median-of-three pivot selection prevents this.

### 6. Merge Sort -- O(n log n), guaranteed

Merge Sort recursively splits the array in half, sorts each half, then merges them back together in order.

```csharp
private static int[] MergeSort(int[] array, ref int count)
{
    if (array.Length <= 1) return array;
    int mid = array.Length / 2;
    int[] left  = MergeSort(array[..mid], ref count);
    int[] right = MergeSort(array[mid..], ref count);
    return Merge(left, right, ref count);
}

private static int[] Merge(int[] left, int[] right, ref int count)
{
    var result = new int[left.Length + right.Length];
    int i = 0, j = 0, k = 0;
    while (i < left.Length && j < right.Length)
    {
        count++;
        if (left[i] <= right[j]) result[k++] = left[i++];
        else                      result[k++] = right[j++];
    }
    while (i < left.Length)  result[k++] = left[i++];
    while (j < right.Length) result[k++] = right[j++];
    return result;
}
```

The `Merge` step compares the front elements of each sorted half and picks the smaller one, repeating until one side is exhausted, then appending the remainder of the other. O(n log n) is guaranteed regardless of input order - there's no unlucky pivot. The cost: O(n) extra memory for the merge step, allocating a new array at each level of recursion.

### 7. Heap Sort -- O(n log n), guaranteed, in-place

Heap Sort builds a max-heap (largest element always at the root), then repeatedly extracts the maximum and places it at the end of the sorted region.

```csharp
private static int[] HeapSort(int[] array, ref int count)
{
    var arr = (int[])array.Clone();
    int n = arr.Length;

    // Build max-heap
    for (int i = n / 2 - 1; i >= 0; i--)
        Heapify(arr, n, i, ref count);

    // Extract max repeatedly
    for (int i = n - 1; i > 0; i--)
    {
        (arr[0], arr[i]) = (arr[i], arr[0]); // swap max to end
        Heapify(arr, i, 0, ref count);        // restore heap for remaining elements
    }
    return arr;
}

private static void Heapify(int[] arr, int n, int root, ref int count)
{
    int largest = root;
    int left = 2 * root + 1;
    int right = 2 * root + 2;
    count++;
    if (left  < n && arr[left]  > arr[largest]) largest = left;
    if (right < n && arr[right] > arr[largest]) largest = right;
    if (largest != root)
    {
        (arr[root], arr[largest]) = (arr[largest], arr[root]);
        Heapify(arr, n, largest, ref count);
    }
}
```

A heap is a complete binary tree stored as an array: for a node at index `i`, its left child is at `2i+1` and its right child is at `2i+2`. Guaranteed O(n log n), in-place. Generally slower than Merge Sort and Quick Sort in practice due to poor cache locality - heap operations jump around the array rather than working through it sequentially.

### 8. Counting Sort -- O(n + k)

Counting Sort counts how many times each value appears, then reconstructs the sorted output from those counts. It never compares elements against each other.

```csharp
private static int[] CountingSort(int[] array, ref int count)
{
    if (array.Length == 0) return array;
    int min = array.Min(), max = array.Max();
    int range = max - min + 1;
    var counts = new int[range];

    foreach (int val in array) { counts[val - min]++; count++; }

    var result = new int[array.Length];
    int idx = 0;
    for (int i = 0; i < range; i++)
        while (counts[i]-- > 0) { result[idx++] = i + min; count++; }

    return result;
}
```

O(n + k) where k is the value range (`max - min`). Faster than any comparison-based sort when k is small relative to n, but the counting array costs O(k) memory regardless of how many values are actually present. Impractical when the value range is much larger than the dataset.

### 9. Radix Sort -- O(d * (n + k))

Radix Sort sorts by individual digit position, from least significant to most significant, using a stable counting pass for each digit.

```csharp
private static int[] RadixSort(int[] array, ref int count)
{
    if (array.Length == 0) return array;
    int offset = -array.Min(); // shift to non-negative
    var arr = array.Select(x => x + offset).ToArray();
    int max = arr.Max();

    for (int exp = 1; max / exp > 0; exp *= 10)
        arr = CountingPassByDigit(arr, exp, ref count);

    return arr.Select(x => x - offset).ToArray(); // restore original values
}

private static int[] CountingPassByDigit(int[] arr, int exp, ref int count)
{
    var output = new int[arr.Length];
    var digitCounts = new int[10];

    foreach (int val in arr) { digitCounts[(val / exp) % 10]++; count++; }
    for (int i = 1; i < 10; i++) digitCounts[i] += digitCounts[i - 1];
    for (int i = arr.Length - 1; i >= 0; i--) { output[--digitCounts[(arr[i] / exp) % 10]] = arr[i]; count++; }
    return output;
}
```

d is the number of digit positions; for 32-bit integers d is at most 10. In practice O(n) for integer data - log₁₀(max) grows so slowly it's nearly constant. Requires values to be non-negative, so any negative values are offset before sorting and restored afterward.

The counting pass iterates backward through the input to maintain stability - elements with the same digit maintain their relative order from the previous pass. This stability is what makes each pass build correctly on the previous one.

---

## Reading the Results

Operation counts show the algorithm's structure. Elapsed times show real-world performance including cache behavior and implementation overhead. The two don't always agree: Shell Sort's operation count is often higher than Quick Sort's but its elapsed time can be lower at small N because of lower per-operation overhead.

The visual walkthrough (`V` after any demo) shows each step animated in a browser.

---

## Summary: Nine Algorithms at a Glance

| Algorithm | Complexity | In-place? | Guaranteed? | Notes |
|---|---|---|---|---|
| Bubble Sort | O(n²) | Yes | - | O(n) best case |
| Selection Sort | O(n²) | Yes | - | Minimum swaps |
| Insertion Sort | O(n²) | Yes | - | O(n) best case; best O(n²) in practice |
| Shell Sort | O(n²) worst | Yes | - | Faster in practice than Big-O implies |
| Quick Sort | O(n log n) avg | Yes | No (O(n²) worst) | Fastest in practice on random data |
| Merge Sort | O(n log n) | No (O(n) extra) | Yes | Stable; predictable |
| Heap Sort | O(n log n) | Yes | Yes | Poor cache locality |
| Counting Sort | O(n + k) | No | Yes | Requires bounded integers |
| Radix Sort | O(d(n + k)) | No | Yes | Requires non-negative integers (offset applied) |

---

## Takeaways

- O(n²): Bubble, Selection, Insertion, Shell (worst case). Fine for small N or nearly-sorted data.
- O(n log n): Quick (average), Merge, Heap. The practical range for general sorting.
- O(n + k): Counting, Radix. Faster than comparison-based sorts when applicable, with restrictions.
- In-place vs. extra memory: Quick and Heap sort in place; Merge needs O(n) extra; Counting and Radix need O(k) or O(n).
- Guaranteed vs. average: Merge and Heap guarantee O(n log n); Quick Sort's O(n²) worst case requires a bad pivot selection.
- Insertion Sort is the practical winner for small N - it's inside Timsort and Introsort for exactly that reason.
- Radix and Counting Sort bypass the O(n log n) comparison-sort lower bound by never comparing elements directly.
