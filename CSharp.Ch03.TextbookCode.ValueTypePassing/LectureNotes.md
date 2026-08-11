# Ch03 Textbook Code: Value Type Passing

## What This Is

A small standalone lab: pass an `int` two different ways into methods, then pass a class instance the same way, to see value-type and reference-type parameter passing side by side in one place.

The original download named the student class `studentStudent` and its variable `firstStudentStudent`, a duplicated word, not an intentional name, almost certainly left over from a rename that only got half-applied. Fixed to `student` and `firstStudent`. Everything else keeps its original lowercase casing, per policy, this is a naming defect fix, not a casing change.

---

## The Two Halves of This Lab

### Value Types: `sum()` and `changeValues()`

```csharp
changeValues(num1, num2);
Console.WriteLine(num1);  // outputs 2, unchanged
Console.WriteLine(num2);  // outputs 3, unchanged
```

`changeValues()` decrements `value1` and adds 5 to `value2`, but those are its own local copies. `int` is a value type, so passing `num1` and `num2` into the method hands over copies of the values, not the originals. Whatever `changeValues()` does to its parameters stays inside `changeValues()`.

### Reference Types: `changeName()`

```csharp
changeName(firstStudent);
Console.WriteLine(firstStudent.firstName);  // "George" now, changed
```

Same calling pattern, opposite result. `student` is a class, a reference type, so `firstStudent` holds a reference to the object, and passing it into `changeName()` hands over a copy of that reference, both the caller's `firstStudent` and the method's `refValue` point at the exact same object in memory. `refValue.firstName = "George";` mutates the one object both variables are looking at, so the change is visible back in `Main()` after the call returns.

Running `changeValues()` and `changeName()` back to back, with nearly identical calling syntax and opposite outcomes, is the whole point of this lab. The parameter-passing mechanics look the same from the call site either way, `changeValues(num1, num2)` and `changeName(firstStudent)` read almost identically, but what actually gets copied, a value versus a reference, determines whether the caller sees the change afterward.
