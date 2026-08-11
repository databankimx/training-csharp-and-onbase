# Ch02 Textbook Code: Working With For Loops

## What This Is

A small standalone lab: the same "count to something" problem solved seven different ways, `for` counting up, counting down, counting by twos, counting by multiples of five, then `foreach` over an integer array, `foreach` over a string array, and finally `while` and `do-while` covering the same ground again.

No functional bugs in the original download this time, only the usual project-structure updates plus one cosmetic fix: a console message that read `"foeach over an array of integers"` now says `"foreach"`.

---

## Why Bother Showing the Same Thing Seven Times

Because the point isn't any single loop, it's noticing what stays the same and what changes between them. All seven blocks produce a sequence of numbers or items, one line at a time. What changes is only how the sequence is generated and how the loop knows when to stop:

```csharp
for (int i = 0; i < 10; i++)          // increments by 1
for (int i = 10; i > 0; i--)          // decrements by 1
for (int i = 0; i < 10; i += 2)       // increments by 2
for (int i = 5; i < 1000; i *= 5)     // multiplies, doesn't just add
```

That last one is worth lingering on. Every earlier example changes `i` by *adding* a fixed amount each pass. `i *= 5` changes it by *multiplying*, which is a different kind of progression entirely, 5, 25, 125, 625, then stops because 3125 no longer satisfies `i < 1000`. The loop syntax doesn't care what kind of arithmetic sits in that third clause, `i++`, `i--`, `i += 2`, `i *= 5` are all just expressions that get evaluated once per pass. If you can write it as a statement, you can put it there.

The two `foreach` examples exist to make one thing obvious by contrast: `foreach` doesn't know or care about counting at all, there's no index, no increment, no condition to write. You hand it a collection, it hands you back one item at a time until the collection runs out. That's the entire trade you're making by choosing `foreach` over `for`, you give up control over the index in exchange for not having to think about it.

`while` and `do-while` close it out by solving the exact same "count to 10" problem as the first `for` loop, so you can see all three condition-driven loop types (`for`, `while`, `do-while`) arrive at an identical result through different control structures. If you're ever unsure which loop to reach for, this file is a reasonable one to reread, it's the same problem asked seven different ways on purpose.
