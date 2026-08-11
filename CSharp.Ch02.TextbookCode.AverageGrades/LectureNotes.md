# Ch02 Textbook Code: Average Grades

## What This Is

A small standalone lab: sum an array of grades with `foreach`, divide by the count, print the average.

---

## The Bug That Was Here

```csharp
average = total / gradeCount;
```

`total` and `gradeCount` are both `int`. In C#, dividing an `int` by an `int` performs **integer division**, the result is truncated to a whole number before it's ever handed off to anything else, regardless of what type you're about to assign it to. It doesn't matter that `average` is declared as `double`, by the time the division happens, the decimal portion is already gone.

With this data set, `total` is 496 and `gradeCount` is 6. `496 / 6` is `82.666...`, but integer division gives you `82`, and that's what got assigned to `average`. The `double` variable faithfully stores `82`, it just never had a chance to be anything more precise, the precision was thrown away one step earlier.

The fix is a cast on one of the two operands, forcing the division itself to happen in floating point:

```csharp
average = (double)total / gradeCount;
```

Casting `total` to `double` before the `/` means C# now sees a `double` divided by an `int`, promotes the whole expression to floating-point division, and `average` ends up with the actual `82.6666666666667` you'd expect.

## Why This One Is Sneaky

Unlike the lottery program's dead loop, this bug doesn't announce itself. The program runs without error, prints a number that's in the right ballpark, and the average even happens to look plausible on casual inspection, 82 versus 82.67 isn't an alarming difference at a glance. That's exactly what makes integer division bugs dangerous in real code: they don't crash, they don't throw, they just quietly hand back a slightly wrong answer that someone downstream trusts.

The rule worth internalizing: division only produces a fractional result if at least one operand is already a floating-point type at the moment the division happens. Declaring the *destination* variable as `double` doesn't retroactively change how the division itself was computed.
