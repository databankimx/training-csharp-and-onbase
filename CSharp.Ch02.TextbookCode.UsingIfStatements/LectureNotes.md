# Ch02 Textbook Code: Using If Statements

## What This Is

A small standalone lab: three variations on `if`, building on each other, a single condition, a compound condition with `&&`, and a nested `if` inside another `if`.

Unlike its two siblings in this chapter (`LotteryProgram` had a dead outer loop, `AverageGrades` had a silent integer-division bug), this one arrived clean. No logic to fix here, only the usual project-structure and standards updates, SDK-style project, namespace alignment, standard exception handling, and so on.

---

## Why the Three Examples Are Ordered the Way They Are

```csharp
if (first == 2)
{
    Console.WriteLine("The if statement evaluated to true");
}
```

```csharp
if (first == 2 && second == 0)
{
    Console.WriteLine("The if statement evaluated to true");
}
```

```csharp
if (first == 2)
{
    if (second == 0)
    {
        Console.WriteLine("Both outer and inner conditions are true.");
    }
    Console.WriteLine("Outer condition is true, inner may be true.");
}
```

All three examples check exactly the same underlying facts (`first == 2` and `second == 0`), just structured three different ways, which makes them a useful set to run side by side. The single condition and the `&&` version produce the same "both are true" result through different means, and the nested version shows what `&&` is actually doing under the hood: an outer check that gates whether the inner check even runs at all.

That last point is worth sitting with. `if (first == 2 && second == 0)` and the nested version aren't just two ways of writing the same thing stylistically, the nested version makes visible what `&&`'s short-circuit behavior hides: if `first != 2`, the `second == 0` check in the nested version never executes, same as it wouldn't in the `&&` version. Seeing it spelled out as two separate `if` blocks is what makes short-circuiting click for a lot of people before they ever have to reason about it in a single compound condition.
