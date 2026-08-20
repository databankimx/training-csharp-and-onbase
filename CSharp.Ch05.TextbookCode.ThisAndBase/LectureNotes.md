# Ch05 Textbook Code: This and Base

## What This Is

The best lab in this chapter for actually *seeing* constructor chaining order, no debugger, no stepping, just a textbox. Every `Person`/`Employee` constructor logs its own execution to a shared `Form1.Results` string, so the resulting textbox shows the exact sequence in which `this`/`base` chained constructors run.

No bugs found.

---

## Read the Output, Not Just the Code

Running `new Employee("Ed", "Eager", "IT")` produces output like this (indentation mirrors the class hierarchy):

```
Making Employee(Ed, Eager, IT)
  Person(Ed)
  Person(Ed, Able)
    Employee(Ed, Eager)
    Employee(Ed, Eager, IT)
```

Read bottom-up against the code and the chaining order becomes concrete instead of theoretical: `Employee(string, string, string)` calls `: this(firstName, lastName)` first, which calls `Employee(string, string)`, which calls `: base(firstName, lastName)` first, which calls `Person(string, string)`, which calls `: this(firstName)` first, which calls `Person(string)`. The innermost constructor (`Person(string)`) runs first, then each caller's own body runs only after the constructor it delegates to has fully finished. Four constructors, three of them chaining to another before doing their own work, and the textbox shows precisely which one actually executes when.

---

## Worth Comparing

This is the same `this`/`base` chaining concept covered in `CodeLabInvokingConstructors()` in the main `CSharp.Ch05.ImplementingClassHierarchies` lesson and in `CSharp.Ch05.TextbookCode.PersonHierarchy`, three different treatments of the same idea at increasing levels of chain depth. `PersonHierarchy` has one `base` call. This one has three constructors deep on `Employee` alone, and the growing indentation in the output makes the depth visually obvious as it runs.
