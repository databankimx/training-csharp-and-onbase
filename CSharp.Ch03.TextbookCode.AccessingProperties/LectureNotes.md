# Ch03 Textbook Code: Accessing Properties

## What This Is

The same `student` class as `UsingProperties`, this lab focuses on the calling side: creating an instance and setting each property from outside the class, `myStudent.Age = 15;`, `myStudent.GPA = 3.5;`, and so on, then reading them back through `displayDetails()`.

Same `studentStudent` duplicated-word bug as its sibling lab, renamed to `student` here too, including the age validation message that had the typo baked into user-facing text (`"StudentStudent age must be greater than 6"` → `"Student age must be greater than 6"`). Unlike `UsingProperties`, this one's `Main()` already exercised the class, nothing needed adding.

---

## Worth Noticing

This lab and `UsingProperties` define the exact same class. The difference between them is entirely about what each one is teaching: `UsingProperties` is about building a class whose properties encapsulate and validate their backing fields, `AccessingProperties` is about what it looks like to actually *use* that class once it exists, setting each property one at a time from calling code and seeing which assignments stick.

```csharp
var myStudent = new student("Tom", "Thumb");
myStudent.MiddleInitial = 'R';
myStudent.Age = 15;
myStudent.GPA = 3.5;
myStudent.displayDetails();
```

Every one of those property assignments reads exactly like setting a public field would, `myStudent.Age = 15;` doesn't look any different from what it would look like if `Age` were a bare public `int`. That's the point of properties: the validation logic inside `Age`'s setter runs invisibly from the caller's perspective. Try changing `myStudent.Age = 15;` to `myStudent.Age = 3;` and rerunning, the assignment "succeeds" syntactically, but the validation message prints and the age never actually changes, all without the calling code doing anything differently.
