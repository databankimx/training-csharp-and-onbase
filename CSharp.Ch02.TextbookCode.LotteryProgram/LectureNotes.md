# Ch02 Textbook Code: Lottery Program

## What This Is

A small standalone lab: pick 6 unique-looking numbers from a range of 1–49, the way a lottery machine would. It's also the project with the built-in cautionary tale.

---

## The Bug That Was Here

The version downloaded straight from the textbook publisher looked like this:

```csharp
for (int limit = 0; limit < 49; limit++)
{
    for (int select = 0; select < 6; select++)
    {
        picked[select] = range[rnd.Next(49)];
    }
}
```

Read that outer `for (int limit = 0; limit < 49; limit++)` closely: `limit` is declared, incremented, and checked, but it's never actually used anywhere inside the loop body. The entire inner block, the part that actually picks the 6 numbers, runs 49 times in a row, and every pass except the very last one is thrown away. The output is still 6 valid numbers, so it *looks* correct, it's just doing roughly 49 times more work than it needs to, for no reason at all.

This is a good example of a bug that costs you performance and clarity without costing you correctness, the kind that's easy to miss in a code review because the program still "works." The fix removes the pointless outer loop entirely:

```csharp
for (int select = 0; select < 6; select++)
{
    picked[select] = range[rnd.Next(49)];
}
```

Same result, a fraction of the work, and no dead variable (`limit`) sitting there doing nothing.

## Worth Noticing While You're In Here

`picked[select] = range[rnd.Next(49)];` doesn't guard against picking the same number twice. Real lottery number generators dedupe, this one doesn't, and it's not fixed here on purpose, since deduping properly means either tracking what's already been picked or removing chosen numbers from the pool, both of which are better taught once you've covered collections beyond arrays. Worth keeping in the back of your mind as a "this looks done but isn't quite" example.
