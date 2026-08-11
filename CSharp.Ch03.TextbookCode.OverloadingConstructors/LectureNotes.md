# Ch03 Textbook Code: Overloading Constructors

## What This Is

A small standalone lab: one `student` class with three constructors, no arguments, first/last name only, or the full set including grade and school, showing what constructor overloading actually looks like when there's more than one reasonable way to build an object.

Two things were fixed:

1. **The `studentStudent` bug again**, this time it had spread furthest of anywhere in Chapter 3, the class, and all three variables in `Main()` (`studentStudent1`, `studentStudent2`, `studentStudent3`). Renamed to `student`, `student1`, `student2`, `student3`.
2. **`Main()` built three objects and printed nothing.** Each constructor overload ran, but with no output, there was no way to actually see what each one produced. Added a line after each construction printing every field, so the three overloads' different results are visible side by side.

---

## Why Overloading a Constructor Is Different From Overloading Any Other Method

```csharp
public student() { }

public student(string first, string last)
{
    firstName = first;
    lastName = last;
}

public student(string first, string last, int grade, string school)
{
    firstName = first;
    lastName = last;
    this.grade = grade;
    schoolName = school;
}
```

Same rule as overloading any method: each overload needs a distinct parameter signature, C# tells them apart by the number and types of arguments, not by name (constructors don't have names of their own to differentiate anyway, they're all just `student`). What makes this specific case worth calling out is that a constructor's whole job is initialization, so each overload here represents a genuinely different *default state* an object can start in, not just a different way of computing the same result. `student1` starts with every field at its type's default (`null` for the strings, `0` for `grade`), `student2` has a name but no grade or school yet, `student3` has everything. Printing all three side by side is what makes that difference concrete instead of theoretical.

Worth noticing the third constructor's `this.grade = grade;` line. `grade` is both the parameter name and the field name here, so `this.grade` is required to mean "the field," `grade` on its own inside that constructor body would refer to the parameter instead, shadowing the field entirely. `firstName` and `lastName` don't have this problem because their parameters are named `first` and `last`, no naming collision, no `this.` needed to disambiguate.
