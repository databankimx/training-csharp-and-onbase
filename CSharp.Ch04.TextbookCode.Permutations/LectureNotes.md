# Ch04 Textbook Code: Permutations

## What This Is

The interactive WinForms original of the concatenation-vs-`StringBuilder` timing demo already ported into `UsingStringBuilder()` in `CSharp.Ch04.UsingTypes`. Type a letter count into the `NumericUpDown`, hit Go, and watch both approaches generate every permutation and report how long each one took.

---

## The Bug That Was Here

Same off-by-one bug already found and fixed in `CSharp.Ch04.UsingTypes`:

```csharp
private long Factorial(long number)
{
    long result = 1;
    for (int i = 2; i < number; i++) result *= i;
    return result;
}
```

`i < number` stops one short, so `Factorial(8)` returned `5040` (that's `7!`) instead of the correct `40320`. The permutation-generating recursion itself was never affected, it correctly builds all `40320` permutations regardless, only the `# Permutations` readout displayed the wrong count. Fixed to `i <= number`, matching the fix already applied in the console version.

---

## Worth Actually Using This One

Unlike `CastingArrays` or `CloneArray`, this lab is genuinely meant to be interacted with, no debugger required. Try running it with 8 or 9 letters and watch the `Concatenation` and `StringBuilder` timing fields directly, the console version prints the same numbers, but watching them update live in a form after clicking a button makes the performance gap land differently than reading it off a scrolled-past console line.
