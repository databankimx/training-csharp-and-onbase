## Understanding Big-O Complexity in Programming

---

> Measuring programming progress by lines of code is like measuring aircraft building progress by weight.<br>
> ~ Bill Gates

Big-O (or "order of") is a term used to describe the complexity of a programming algorithm in relation to the size of the input.

---

That's kind of a mouthful, so let's break it down.

* First of all, it's important to remember where an algorithm falls into problem solving.
    - The solution to a problem consists of three parts
        - Input
        : This is the incoming, unprocessed data
            - The size of this input can vary
            - We'll refer to the input size as ***n***
        - Process
        : This is how we transform the input into the output
            - Typically, we describe this in terms of an algorithm (a process with defined steps that takes us from the input to the output)
            - This algorithm is where we need to focus on complexity in relation to the size of ***n***
        - Output
        : This is the processed result of the program
<br>&nbsp;<br>
* Next, it's useful to explain what we mean by complexity
    - First of all, with big-O, we're referring to asymptotic complexity
        - This is the worst-case complexity as ***n*** grows very large
    - Complexity means how quickly the process grows as the input size ***n*** gets bigger.
        - For example, if we have an *n*-length unsorted array, it will require (in the worst case) separately examining *n* elements to search for a specific piece of information.
            - This means that the process is linear, which we express as order of *n* or O(*n*)
            - Meaning that the complexity grows at the same rate as the input
        - Other algorithms might take less time
            - If the same array is already sorted, then a binary search can be completed in log₂*n* operations, which we would describe as O(log *n*)
        - Or they might take much more time
            - Sorting the array in the first place almost always requires at least *n* * log₂*n* or O(*n* log *n*)
* Deciding where to reduce complexity in order to improve performance is a critical step in your design process.
    - Looking at the above example of an *n*-length unsorted array:
        - If we're only going to search it once (or very few times):
            - The total time cost to sort and search would be...
                - *n* * (log₂*n*)² for both the sort and one search
                - For *n* = 10,000 this would be around 1.7 million total operations
                - Since this is far more than the *n* operations it takes to search the unsorted array, in this instance, we'd probably choose not to sort the data.
        - On the other hand, if we're going to search the results many times after they're sorted<br>(say 10,000 searches)...
            - Then we have a cost of:
                - *n* * (log₂*n*)² * 2 operations for one sort followed by ten thousand searches
                - Or around 3.4 million total operations
                - Contrast that with 10,000 * n (around 100 million total operations) of repeated linear search, and the pre-sort cost is now very economical.
<br>&nbsp;<br>

---

Here is a list of the major Big-O complexities. Each one below has a matching, genuinely runnable demo in this project's `Program.cs` - the code samples here are simplified for reading; the real versions (with operation counting and timing) are one menu option away.

- O(1) → Constant Time<br>
    In O(1), it takes a constant time to run an algorithm, regardless of the size of the input. Examples include:
    - Setting the value of a variable
    - Performing a math operation
    - Accessing an array by index
    - **Try it**: menu option 1 (`GetFirstElement`)

- O(*n*) → Linear Time<br>
    In O(*n*), the run-time increases at the same pace as the input (or a constant multiple thereof). Examples include:
    - Traversing an array one time in such methods as `forEach`, `map`, `reduce`
    - Find() methods - keeping in mind that Big-O is worst case, even though built-in find methods frequently don't check every element, they are still O(*n*)
    - **Try it**: menu option 2 (`SumAllElements`)

- O(*n*²) → Quadratic Time<br>
    In O(*n*²), run-time increases with the square of the input. Examples include:
    - Some sort algorithms (see the dedicated Sort Algorithms project for Bubble, Selection, and Insertion Sort)
    - Nested loops traversing the same list
        - If traversing different lists, we have the less common notation O(*n* * *m*) or O(*nm*), which is similarly bad in terms of efficiency, but is subtly different
        - Nesting a third (or more) loop inside the second would be the less common O(*n*³) → Cubic Time, etc.
        - In general O(*n*ˣ), where *x* is 2 or more, is referred to as Polynomial Time
    - **Try it**: menu option 5 (`CountDuplicates`)

- O(log *n*) → Logarithmic Time<br>
    In O(log *n*), the running time grows in proportion to the logarithm (base 2) of the input size. Examples include:
    - Searching an ordered array by repeatedly checking the midpoint (see the dedicated Search Algorithms project for the full treatment)
    - Finding a value in a binary tree
    - Basically, any time the number of inputs to check is divided by 2 on each iteration, it is O(log *n*)
    - When an O(log *n*) step has to be repeated in relation to the input size (like in a merge sort), this becomes O(*n* log *n*) → Loglinear Time
    - **Try it**: menu option 3 (`BinarySearch`), and menu option 4 for the loglinear case (`MergeSort` - see the dedicated Sort Algorithms project for the rest of this family, including Heap and Quick Sort)

---

Here is a table showing the number of operations at *n* = the first few powers of 2 for each of the major Big-O orders:

>| log₂*n* | *n* | *n* * log₂*n* |  *n*² |     2*ⁿ*      |     *n*!     |
>|--------:|----:|--------------:|------:|--------------:|-------------:|
>|       1 |   2 |             2 |     4 |             4 |            2 |
>|       2 |   4 |             8 |    16 |            16 |           24 |
>|       3 |   8 |            24 |    64 |           256 |        40320 |
>|       4 |  16 |            64 |   256 |        65,536 |   ~ 2 × 10¹³ |
>|       5 |  32 |           160 | 1,024 | 4,294,967,296 | ~ 2.6 × 10³⁵ |

You can see how quickly less efficient algorithms can get out of hand - the Recursion project's naive Fibonacci approach (O(2ⁿ)) and the Sort Algorithms project's worst-case Quick Sort are both concrete, runnable examples of exactly this kind of blowup.

* The chart below illustrates the impact that the big-O of an algorithm can have on performance: `big_o_chart_only.png` (copied alongside this file - see `LectureNotes.md`).
