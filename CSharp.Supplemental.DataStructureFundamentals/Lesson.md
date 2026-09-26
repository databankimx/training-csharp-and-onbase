# Data Structure Fundamentals

A short, deliberately brief look at three foundational ways to hold a collection of values: arrays, linked lists, and trees. This isn't about performance measurement (see the upcoming Big-O/complexity material for that) - just what each structure actually *is*, and the trade-off each one makes.

## Array

Fixed size, values sit contiguously in memory, accessed by index. `numbers[2]` jumps straight to that element - no searching required, regardless of how large the array is. Finding a specific *value* (rather than a known index) is a different story: with nothing telling you where it is, checking every element in turn (a linear search) is the only option, in the worst case.

## Linked List

No fixed size. Instead of sitting next to each other in memory, each node just holds a reference to the next one. Inserting at the front costs almost nothing - a couple of reference reassignments, regardless of how many nodes already exist - compare that to an array, where inserting at the front means shifting every other element over by one. The trade-off: there's no equivalent of `numbers[2]` here. Reaching any given node means walking the chain from the front, one reference at a time.

## Tree

Nodes branch instead of chaining in a straight line - each one can point to multiple children. The example here is a *binary search* tree specifically: values smaller than a node go left, larger go right. That ordering rule is what makes an in-order traversal (visit the left subtree, then this node, then the right subtree) produce every value in sorted order, with no separate sorting step needed.

This is the same underlying idea `CSharp.Supplemental.TrieExamples` builds on - a Trie is a tree too, just with up to 26 children per node (one per letter) instead of 2, and no ordering rule between them. If a tree's branching structure feels unfamiliar, this lesson is worth working through first.

## Try It Yourself

Run the project - it builds a small array, linked list, and tree in turn, printing what each operation actually does along the way. Try changing the values being inserted into the tree and predicting what the in-order traversal will print before running it again.
