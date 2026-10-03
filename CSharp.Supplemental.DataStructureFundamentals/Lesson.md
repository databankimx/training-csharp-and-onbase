# Supplemental: Data Structure Fundamentals

## What This Is

Five foundational data structures demonstrated side by side: arrays, linked lists, stacks, queues, and binary search trees. The point isn't the C# collection classes that wrap these structures - it's understanding how the structures themselves work and what that implies for performance, so the tradeoffs between `List<T>`, `LinkedList<T>`, `Stack<T>`, `Queue<T>`, and `SortedDictionary<K,V>` are something you understand rather than guess at.

This lesson is useful background before `CSharp.Supplemental.TrieExamples`, which builds on tree-structure concepts, and before the Algorithms series, which involves reasoning about array and collection operations at the Big-O level.

---

## How to Write This Program

Add `Models/LinkedListNode.cs` and `Models/TreeNode.cs` before starting the mini-programs. Each is a simple node definition - the lesson refers to them throughout.

```csharp
public class LinkedListNode
{
    public int Value { get; set; }
    public LinkedListNode Next { get; set; }
    public LinkedListNode(int value) { Value = value; }
}

public class TreeNode
{
    public int Value { get; set; }
    public TreeNode Left { get; set; }
    public TreeNode Right { get; set; }
    public TreeNode(int value) { Value = value; }
}
```

---

## Mini-Program 1: Arrays

Clear `Main()` and write:

```csharp
int[] numbers = [10, 20, 30, 40, 50];

Console.WriteLine("Index access -- O(1), no matter the array size:");
Console.WriteLine($"  numbers[2] = {numbers[2]}");

Console.WriteLine("\nContent search -- O(n), no shortcut without a sorted structure:");
int target = 40;
for (int i = 0; i < numbers.Length; i++)
{
    if (numbers[i] == target)
    {
        Console.WriteLine($"  Found {target} at index {i}");
        break;
    }
}

Console.WriteLine("\nArray.Resize allocates a new array and copies -- the 'ref' is honest about it:");
Array.Resize(ref numbers, 7);
numbers[5] = 60;
numbers[6] = 70;
Console.WriteLine($"  After resize: [{string.Join(", ", numbers)}]");

GenericFunctions.Pause();
```

Run it. Getting element `[2]` is O(1): the runtime calculates `baseAddress + (2 * elementSize)` and jumps there directly. Finding `40` by value is O(n) - there's no shortcut without knowing the position in advance.

`Array.Resize` takes `ref` because arrays cannot actually grow. It allocates a new array, copies everything over, and reassigns your variable. `List<T>` does the same thing internally using a doubling strategy, but hides the `ref` from you.

**Contiguous memory matters.** Because elements sit next to each other in memory, walking an array sequentially is extremely cache-friendly. The CPU prefetcher loads the next chunk of memory automatically. This is one of the reasons a plain `int[]` often outperforms more sophisticated structures for sequential access, even at large sizes.

---

## Mini-Program 2: Linked Lists

Clear `Main()` and write:

```csharp
// Build a small linked list manually: 10 -> 20 -> 30
LinkedListNode head = null;

void Prepend(int value)
{
    var node = new LinkedListNode(value) { Next = head };
    head = node;
}

void PrintList()
{
    var current = head;
    var parts = new List<string>();
    while (current != null) { parts.Add(current.Value.ToString()); current = current.Next; }
    Console.WriteLine($"  [{string.Join(" -> ", parts)}]");
}

Prepend(30);
Prepend(20);
Prepend(10);
Console.WriteLine("Built list (each Prepend was O(1)):");
PrintList();

Console.WriteLine("\nPrepend 5 -- O(1), no shifting:");
Prepend(5);
PrintList();

Console.WriteLine("\nReach position 3 -- O(n), must follow Next references:");
var current = head;
int steps = 0;
while (steps < 3) { current = current.Next; steps++; }
Console.WriteLine($"  Value at position 3: {current.Value} (took {steps} Next hops)");

GenericFunctions.Pause();
```

Run it. Inserting at the front is O(1): create a new node, point it at the current head, update the head reference. No elements shift. Compare that with inserting at the front of an array, which requires shifting every existing element one position right - O(n).

The cost is position access. Reaching position 3 means following 3 `Next` references from the head. There's no index arithmetic equivalent. For random access by position, a linked list is strictly worse than an array.

---

## Mini-Program 3: Stacks

Clear `Main()` and write:

```csharp
// Hand-rolled stack backed by a linked list.
// Real code would use System.Collections.Generic.Stack<T>.
LinkedListNode stackTop = null;

void Push(int value)
{
    stackTop = new LinkedListNode(value) { Next = stackTop };
}

int Pop()
{
    if (stackTop == null) throw new InvalidOperationException("Stack is empty.");
    int value = stackTop.Value;
    stackTop = stackTop.Next;
    return value;
}

int Peek() => stackTop?.Value ?? throw new InvalidOperationException("Stack is empty.");

Push(10);
Push(20);
Push(30);
Console.WriteLine($"Pushed 10, 20, 30. Top is: {Peek()}");

Console.WriteLine($"\nPop: {Pop()}");
Console.WriteLine($"Pop: {Pop()}");
Console.WriteLine($"Top after two pops: {Peek()}");

Console.WriteLine("\nStack is LIFO -- last in, first out.");
Console.WriteLine("Push and Pop are both O(1) -- no searching, no shifting.");
Console.WriteLine("Real usage: call stack, undo history, balanced-brackets checking,");
Console.WriteLine("depth-first traversal without recursion.");

GenericFunctions.Pause();
```

Run it. A stack is a restricted linked list: you can only add and remove from one end (the top). Both operations are O(1) because they only touch the head node.

The restriction is the point. Stacks appear wherever the most recently added item needs to be the first one retrieved: method call frames (the call stack), undo/redo history, expression evaluation, and iterative depth-first traversal where you'd otherwise need recursion.

`System.Collections.Generic.Stack<T>` is the production version. It's backed by an array rather than a linked list (better cache locality), but the observable behavior is identical.

---

## Mini-Program 4: Queues

Clear `Main()` and write:

```csharp
// Hand-rolled queue backed by two linked list pointers.
// Enqueue at the tail, dequeue from the head -- both O(1).
// Real code would use System.Collections.Generic.Queue<T>.
LinkedListNode queueHead = null;
LinkedListNode queueTail = null;

void Enqueue(int value)
{
    var node = new LinkedListNode(value);
    if (queueTail == null) { queueHead = queueTail = node; return; }
    queueTail.Next = node;
    queueTail = node;
}

int Dequeue()
{
    if (queueHead == null) throw new InvalidOperationException("Queue is empty.");
    int value = queueHead.Value;
    queueHead = queueHead.Next;
    if (queueHead == null) queueTail = null;
    return value;
}

Enqueue(10);
Enqueue(20);
Enqueue(30);
Console.WriteLine("Enqueued 10, 20, 30 (front is 10).");

Console.WriteLine($"\nDequeue: {Dequeue()}");
Console.WriteLine($"Dequeue: {Dequeue()}");
Console.WriteLine($"Remaining front: {queueHead.Value}");

Console.WriteLine("\nQueue is FIFO -- first in, first out.");
Console.WriteLine("Enqueue and Dequeue are both O(1).");
Console.WriteLine("Real usage: task queues, print spoolers, BFS traversal,");
Console.WriteLine("message buffers, anything where arrival order must be preserved.");

GenericFunctions.Pause();
```

Run it. Where a stack serves the most-recent item first (LIFO), a queue serves the oldest item first (FIFO). The key implementation detail: tracking both `head` and `tail` pointers lets both Enqueue (at the tail) and Dequeue (from the head) be O(1). Without the tail pointer, finding the end to enqueue would require walking the whole list - O(n).

`System.Collections.Generic.Queue<T>` is backed by a circular array for better cache performance, but the observable FIFO behavior is identical.

**Stack vs. Queue - the practical distinction.** Both restrict where you can add and remove. The difference is which end serves for removal: stack removes from the same end you insert (LIFO), queue removes from the opposite end (FIFO). The right choice depends entirely on whether arrival order should be preserved.

---

## Mini-Program 5: Binary Search Trees

Clear `Main()` and write:

```csharp
TreeNode root = null;

TreeNode Insert(TreeNode node, int value)
{
    if (node == null) return new TreeNode(value);
    if (value < node.Value) node.Left  = Insert(node.Left,  value);
    else                    node.Right = Insert(node.Right, value);
    return node;
}

bool Search(TreeNode node, int value)
{
    if (node == null) return false;
    if (value == node.Value) return true;
    return value < node.Value
        ? Search(node.Left,  value)
        : Search(node.Right, value);
}

void InOrder(TreeNode node, List<int> result)
{
    if (node == null) return;
    InOrder(node.Left, result);
    result.Add(node.Value);
    InOrder(node.Right, result);
}

int[] values = [50, 30, 70, 20, 40, 60, 80];
foreach (int v in values) root = Insert(root, v);
Console.WriteLine($"Inserted: [{string.Join(", ", values)}]");

var sorted = new List<int>();
InOrder(root, sorted);
Console.WriteLine($"In-order traversal (always sorted): [{string.Join(", ", sorted)}]");

Console.WriteLine($"\nSearch(40): {Search(root, 40)}");
Console.WriteLine($"Search(99): {Search(root, 99)}");
Console.WriteLine("\nBalanced BST: O(log n) search AND O(log n) insert.");
Console.WriteLine("Inserting sorted values (1,2,3...) degenerates the tree to a linked list -- O(n).");
Console.WriteLine("SortedDictionary<K,V> uses a self-balancing red-black tree to prevent this.");

GenericFunctions.Pause();
```

Run it. The tree maintains one invariant: everything in the left subtree is smaller than the node's value; everything in the right is larger. This lets `Search` eliminate half the remaining nodes at each step - O(log n) in a balanced tree.

**In-order traversal** visits the left subtree, then the node, then the right subtree - which visits every node in sorted order without any separate sorting step. Inserting `[50, 30, 70, 20, 40, 60, 80]` and running in-order prints `[20, 30, 40, 50, 60, 70, 80]`.

**The balance problem.** Insert values in already-sorted order (1, 2, 3, 4...) into this naive tree and every node has only a right child - the tree degenerates into a linked list and search becomes O(n). Self-balancing variants (AVL trees, red-black trees) restructure the tree on insertion to prevent this. `SortedDictionary<TKey, TValue>` in .NET is backed by a red-black tree.

---

## Summary: Five Structures at a Glance

| | Array | Linked List | Stack | Queue | Balanced BST |
|---|---|---|---|---|---|
| Index access | O(1) | O(n) | N/A | N/A | N/A |
| Insert at front | O(n) | O(1) | O(1) Push | N/A | O(log n) |
| Insert at back | O(1) amortized | O(n)* | N/A | O(1) Enqueue | O(log n) |
| Remove | O(n) shift | O(1) front | O(1) Pop | O(1) Dequeue | O(log n) |
| Search by value | O(n) | O(n) | O(n) | O(n) | O(log n) |
| Memory layout | Contiguous | Scattered | Scattered | Scattered | Scattered |
| .NET equivalent | `T[]`, `List<T>` | `LinkedList<T>` | `Stack<T>` | `Queue<T>` | `SortedDictionary<K,V>` |

*O(1) with a tail pointer, as shown in the Queue demo.

---

## Takeaways

- Array index access is O(1). Finding a value by content is O(n) - no shortcut without a sorted structure.
- Linked list front-insertion is O(1). Position access is O(n). No index arithmetic is possible.
- A stack is a restricted linked list: O(1) Push and Pop, LIFO order. Use it when the most recent item is always next.
- A queue is also O(1) Enqueue and Dequeue (with a tail pointer), FIFO order. Use it when arrival order must be preserved.
- A balanced BST gives O(log n) for both search and insertion - the best of both worlds for ordered data.
- `List<T>` is an array under the hood. `Stack<T>` and `Queue<T>` are array-backed too, for cache performance. `SortedDictionary<K,V>` is a red-black tree.
- Contiguous memory (arrays) is cache-friendly. Scattered memory (linked lists, trees) is not.
