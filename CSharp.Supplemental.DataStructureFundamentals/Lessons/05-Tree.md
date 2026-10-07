---
title: "Binary Search Tree - In-Order Traversal"
chapter: 0
index: 5
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("--- Binary Search Tree ---");

        TreeNode root = null;
        foreach (int value in new[] { 50, 30, 70, 20, 40, 60, 80 })
            root = Insert(root, value);

        Console.Write("In-order traversal (left, node, right) visits every value in sorted order: ");
        PrintInOrder(root);
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("Key properties:");
        Console.WriteLine("  - Smaller values go left, larger values go right");
        Console.WriteLine("  - In-order traversal produces sorted output without a separate sort step");
        Console.WriteLine("  - Search, insert, delete: O(log n) average on a balanced tree");
        Console.WriteLine("  - Degrades to O(n) on a sorted input (tree becomes a linked list)");
        Console.WriteLine("  - BCL equivalent: SortedDictionary<K,V> uses a red-black tree (self-balancing)");
    }

    private static TreeNode Insert(TreeNode node, int value)
    {
        if (node == null) return new TreeNode(value);
        if (value < node.Value) node.Left  = Insert(node.Left,  value);
        else                    node.Right = Insert(node.Right, value);
        return node;
    }

    private static void PrintInOrder(TreeNode node)
    {
        if (node == null) return;
        PrintInOrder(node.Left);
        Console.Write($"{node.Value} ");
        PrintInOrder(node.Right);
    }
}

internal sealed class TreeNode
{
    public int Value { get; }
    public TreeNode Left  { get; set; }
    public TreeNode Right { get; set; }
    public TreeNode(int value) { Value = value; }
}
```
