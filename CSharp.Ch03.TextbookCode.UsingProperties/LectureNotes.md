# Ch03 Textbook Code: Using Properties

## What This Is

A small standalone lab: a `student` class with backing fields kept `private` and exposed through public properties, two of which (`Age`, `GPA`) validate the value on the way in instead of accepting anything handed to them.

Two things were fixed here:

1. **The `studentStudent` duplicated-word bug**, same pattern as `ValueTypePassing`, renamed to `student`. This one had actually leaked into a user-facing message too: the age validation printed `"StudentStudent age must be greater than 6"`, so the typo wasn't just sitting in code, it would have shown up on screen.
2. **`Main()` was completely empty.** The class was fully built, properties and validation and all, but nothing in the original download ever created one or called anything on it. Running the program did nothing. Added a short demo that creates a student, sets every property, prints the details, then deliberately sends an invalid `Age` and `GPA` through to show the validation actually rejecting them.

---

## Why This Class Bothers With Backing Fields At All

```csharp
private string firstName;
...
public string FirstName
{
    get { return firstName; }
    set { firstName = value; }
}
```

`FirstName` here is a plain pass-through, the getter and setter don't do anything beyond reading and writing `firstName` directly. You could reasonably ask why not just make `firstName` public and skip the property entirely. `Age` and `GPA` are the answer:

```csharp
public int Age
{
    get { return age; }
    set
    {
        if (value > 6)
        {
            age = value;
        }
        else
        {
            Console.WriteLine("Student age must be greater than 6");
        }
    }
}
```

A property's setter is a method, which means it can validate before it commits the value, a public field can't. `Age` and `GPA` reject values outside a sensible range instead of silently accepting them, `FirstName`, `LastName`, `MiddleInitial`, and `Program` don't have anything worth validating, so their setters are pure pass-throughs. The pattern is consistent even though the behavior differs: every property controls access to its backing field, some of them just don't have any rules to enforce yet. If a rule shows up later (say, `FirstName` shouldn't be empty), it can be added to that one setter without touching how any calling code uses the class.

One thing worth noticing in the demo: `firstStudent.Age = 3;` after the class is already built doesn't throw or crash, it just silently fails to update `age` and prints a warning. The old value stays exactly as it was. That's a validation design choice, not a compiler-enforced rule, and it's worth deciding for yourself whether "warn and ignore" or "throw an exception" is the better response when invalid data shows up, both are common, and you'll see both used elsewhere in this training set.
