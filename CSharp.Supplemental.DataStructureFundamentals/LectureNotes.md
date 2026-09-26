# Data Structure Fundamentals

## What This Is

A new lesson, not a port - came out of a side discussion while building `CSharp.Supplemental.TrieExamples`. A Trie assumes some familiarity with tree structures (branching nodes, traversal) that nothing else in this solution currently covers explicitly, so this exists as a short prerequisite: arrays, linked lists, and trees, covered just deeply enough to give Trie (and the array-heavy search/sort material still to come from the Big-O consolidation) a shared foundation to point back to, rather than each one re-explaining the basics inline.

Deliberately kept short and mechanical - what each structure *is* and the trade-off it makes (index-based O(1) access vs. no-shifting insertion vs. branching/traversal), not a performance deep-dive. That's what the upcoming Big-O material is for.

## Worth Checking Before Calling This Fully Placed

`CSharp.Ch04.UsingTypes` already has a `CastingArrays` textbook lab, which suggests arrays get at least some treatment in that chapter already - I haven't read that lecture's actual content, so I don't know how much this overlaps. Placed in "Supplementary Lessons" for now rather than folded into Chapter 4, since linked lists and trees don't have a chapter home either and splitting this lesson across two locations seemed worse than a little redundancy, if there turns out to be any.

## Cross-References Added

`CSharp.Supplemental.TrieExamples`'s `Lesson.md` now points back to this lesson for readers who want the tree-structure background first.
