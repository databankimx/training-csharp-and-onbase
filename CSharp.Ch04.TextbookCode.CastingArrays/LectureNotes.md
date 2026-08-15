# Ch04 Textbook Code: Casting Arrays

## What This Is

A WinForms lab, not a console app, unlike every other project in this training set. The entire demonstration lives inside `Form1_Load()`, and it deliberately has no `try`/`catch` anywhere. Run it under the debugger with F5, and watch it break exactly where the comments say it will.

This mirrors the exact same logic already ported into `CastingArrays()` in `CSharp.Ch04.UsingTypes`, side by side these two are worth comparing: the console version wraps the same failing casts in `try`/`catch` and prints what happened, this one just lets it happen and stops. Same lesson, two different ways of encountering it.

---

## Kept Exactly As Downloaded

Per policy for this lab specifically, everything except the project file format is untouched: the class names (`Person`, `Employee`, `Manager`), the field name (`number`), the typo in a comment ("arrat" instead of "array"), and critically, the absence of any exception handling. Running this without a debugger attached will crash the moment the form loads. That's not a defect to fix here, it's the intended way to experience this lesson: step through in Visual Studio, watch `managers = (Manager[])persons;` throw an `InvalidCastException`, inspect the call stack, understand exactly why.

---

## Why This One Stayed WinForms

Every other project in this training set is a console app, including a version of this exact demo. This one specifically was kept as WinForms rather than converted, since the goal for `TextbookCode.*` labs is fidelity to what a student would actually download from the publisher, not consistency with the rest of the curriculum. If you're looking for the console, output-visible version of this same lesson, that's `CastingArrays()` in `CSharp.Ch04.UsingTypes`.
