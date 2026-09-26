#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

namespace CSharp.Supplemental.DataStructureFundamentals;

#region Using Directives
using System;
#endregion

internal static class Program
{
    #region Main
    private static void Main()
    {
        DemonstrateArray();
        Pause();

        DemonstrateLinkedList();
        Pause();

        DemonstrateTree();
    }
    #endregion

    #region Arrays
    // Fixed size, contiguous in memory, accessed by index. Getting element [i] is O(1) -
    // the runtime jumps straight there by calculating an offset, no searching involved.
    // Finding a specific VALUE (rather than a known index) is O(n) in the worst case,
    // though - there's no way to know where it is without checking.
    private static void DemonstrateArray()
    {
        Console.WriteLine("--- Array ---");

        int[] numbers = [10, 20, 30, 40, 50];

        Console.WriteLine($"Size is fixed at creation: {numbers.Length} elements");
        Console.WriteLine($"Access by index is instant: numbers[2] = {numbers[2]}");

        // Finding a value (as opposed to a known index) means checking each element in turn
        int target = 40;
        int foundAt = -1;
        for (int i = 0; i < numbers.Length; i++)
        {
            if (numbers[i] != target) continue;
            foundAt = i;
            break;
        }
        Console.WriteLine($"Finding value {target} took checking up to index {foundAt} - a linear search");
    }
    #endregion

    #region Linked Lists
    // No fixed size, and nodes don't need to sit next to each other in memory - each one
    // just holds a reference to the next. Inserting at the front is O(1), no shifting
    // anything else - contrast that with an array, where inserting at the front means
    // moving every other element over by one. The cost: there's no "jump straight to
    // element 2" the way array indexing allows - reaching any node means walking the
    // chain from the front, one reference at a time.
    private static void DemonstrateLinkedList()
    {
        Console.WriteLine("--- Linked List ---");

        LinkedListNode head = null;

        // Inserting at the head is just a couple of reference reassignments - O(1),
        // regardless of how many nodes are already in the list
        foreach (int value in new[] { 30, 20, 10 })
        {
            var node = new LinkedListNode(value) { Next = head };
            head = node;
            Console.WriteLine($"Inserted {value} at the head");
        }

        Console.Write("Traversing the list (has to walk it node by node): ");
        var current = head;
        while (current != null)
        {
            Console.Write($"{current.Value} ");
            current = current.Next;
        }
        Console.WriteLine();
    }
    #endregion

    #region Trees
    // Nodes branch instead of chaining linearly - each one can point to up to two
    // children here (a binary tree). This example keeps the "smaller values go left,
    // larger values go right" ordering rule of a binary SEARCH tree specifically, which
    // is what makes an in-order traversal (left, then this node, then right) visit every
    // value in sorted order without any separate sorting step.
    private static void DemonstrateTree()
    {
        Console.WriteLine("--- Tree ---");

        TreeNode root = null;
        foreach (int value in new[] { 50, 30, 70, 20, 40, 60, 80 })
        {
            root = Insert(root, value);
        }

        Console.Write("In-order traversal (left, node, right) visits every value in sorted order: ");
        PrintInOrder(root);
        Console.WriteLine();
    }

    // Recursive insertion into a binary search tree
    private static TreeNode Insert(TreeNode node, int value)
    {
        if (node == null) return new TreeNode(value);

        if (value < node.Value)
            node.Left = Insert(node.Left, value);
        else
            node.Right = Insert(node.Right, value);

        return node;
    }

    // Recursive in-order traversal of a binary search tree
    private static void PrintInOrder(TreeNode node)
    {
        if (node == null) return;
        PrintInOrder(node.Left);
        Console.Write($"{node.Value} ");
        PrintInOrder(node.Right);
    }
    #endregion

    #region Helper Functions
    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        Console.WriteLine();
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
