# Supplemental: Data Structure Fundamentals

## What This Is

Three foundational data structures demonstrated side by side: arrays, linked lists, and binary search trees. The point isn't the C# collection classes that wrap these structures -- it's understanding how the structures themselves work and what that implies for performance, so the tradeoffs between `List<T>`, `LinkedList<T>`, and a sorted tree are something you understand rather than guess at.

---

## Arrays

A fixed-size, contiguous block of memory. Elements are stored adjacent to each other, which is what makes index access fast.

```csharp
int[] numbers = [10, 20, 30, 40, 50];

int third = numbers[2]; // O(1) - direct jump by offset, no searching
```

Getting element `[i]` is O(1): the runtime calculates `baseAddress + (i * elementSize)` and jumps there directly. The array's size doesn't change how long that calculation takes.

Finding a value by content rather than by index is different. Without knowing which position holds `40`, there's no shortcut -- the array has to be checked element by element:

```csharp
int target = 40;
for (int i = 0; i < numbers.Length; i++)
{
    if (numbers[i] == target) { /* found at i */ break; }
}
```

That's O(n) in the worst case -- the target might be at the very end, or absent entirely.

**The fixed-size constraint.** An array is declared with a specific length and never grows. Adding an element beyond that length means allocating a new, larger array and copying everything over. `List<T>` hides this behind a doubling strategy -- it over-allocates, tracks how many slots are used, and copies to a new array only when the capacity is exhausted. The underlying mechanism is still arrays.

**Contiguous memory matters.** Because elements sit next to each other in memory, walking an array sequentially is extremely cache-friendly. The CPU prefetcher loads the next chunk of memory automatically. This is one of the reasons a plain `int[]` often outperforms more sophisticated structures for sequential access, even at large sizes.

---

## Linked Lists

A chain of nodes, each holding a value and a reference to the next node. Nodes don't need to be adjacent in memory.

```csharp
public class LinkedListNode
{
    public int Value { get; set; }
    public LinkedListNode Next { get; set; }
}
```

Inserting at the front is O(1): create a new node, point it at the current head, update the head reference. No elements shift. Compare this with inserting at the front of an array, which requires shifting every existing element one position to the right -- O(n).

```csharp
var node = new LinkedListNode(value) { Next = head };
head = node;
```

The cost: there's no index arithmetic. Reaching the 50th node means following 49 `Next` references from the head, one at a time -- O(n). There's no "jump to position 49" equivalent. For random access by position, a linked list is strictly worse than an array.

**When to use which.** If you need fast access by position and rarely insert in the middle, an array (or `List<T>`) is the right tool. If you need to insert or remove at arbitrary positions frequently and don't need position-based access, a linked list is better. In practice, `List<T>` covers most cases because its amortized append is O(1) and position access is O(1), and its cache-friendly layout often wins even when big-O alone suggests otherwise.

---

## Binary Search Trees

A tree where each node can have up to two children, and the structure maintains an ordering rule: all values in the left subtree are less than the node's value, all values in the right subtree are greater.

```csharp
public class TreeNode
{
    public int Value { get; set; }
    public TreeNode Left { get; set; }
    public TreeNode Right { get; set; }
}
```

Insertion follows the rule recursively: go left if the new value is smaller, right if larger, until an empty slot is found:

```csharp
private static TreeNode Insert(TreeNode node, int value)
{
    if (node == null) return new TreeNode(value);
    if (value < node.Value) node.Left = Insert(node.Left, value);
    else node.Right = Insert(node.Right, value);
    return node;
}
```

An **in-order traversal** -- left subtree, then the node, then right subtree -- visits every value in sorted order automatically, with no separate sorting step:

```csharp
private static void PrintInOrder(TreeNode node)
{
    if (node == null) return;
    PrintInOrder(node.Left);
    Console.Write(node.Value + " ");
    PrintInOrder(node.Right);
}
```

Run it with `[50, 30, 70, 20, 40, 60, 80]` inserted in that order, and the in-order traversal prints `20 30 40 50 60 70 80`.

**Search complexity.** In a balanced tree, each comparison eliminates half the remaining nodes -- O(log n), the same as binary search on a sorted array. The difference is that insertion is also O(log n) on a balanced tree, whereas inserting into a sorted array is O(n) because elements have to shift.

The "balanced" qualifier matters. If values are inserted in already-sorted order into a naive binary search tree (1, 2, 3, 4, 5...), the tree degenerates into a linked list -- every node has only a right child, and search becomes O(n). Self-balancing variants (AVL trees, red-black trees) prevent this automatically at the cost of more complex insertion logic. `SortedDictionary<TKey, TValue>` in .NET is backed by a red-black tree.

---

## Takeaways

- Array index access is O(1). Finding a value by content is O(n) -- there's no shortcut without a sorted structure.
- Linked list front-insertion is O(1). Position access is O(n). No index arithmetic is possible.
- A balanced binary search tree gives O(log n) for both search and insertion.
- Contiguous memory (arrays) is cache-friendly. Scattered memory (linked lists, trees with heap-allocated nodes) is not.
- `List<T>` is an array under the hood -- amortized O(1) append, O(1) index access, O(n) insert-at-position.
- `SortedDictionary<K, V>` is a red-black tree -- O(log n) for all operations, always sorted.
