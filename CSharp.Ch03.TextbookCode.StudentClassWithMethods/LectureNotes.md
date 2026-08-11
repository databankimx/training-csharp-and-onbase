# Ch03 Textbook Code: Student Class With Methods

## What This Is

The same `Student` class as the previous lab, with two methods added: `concatenateName()`, which builds a full name from the two name fields, and `displayName()`, which calls `concatenateName()` and prints the result.

No functional bugs. Casing (`firstName`, `concatenateName`, `displayName`, and so on) is left as originally downloaded rather than converted to PascalCase, same policy as `StudentClass`: `TextbookCode.*` projects preserve the original naming even where it doesn't match our usual standard.

---

## Worth Noticing

`displayName()` doesn't build the greeting itself, it delegates to `concatenateName()` and just prints whatever comes back:

```csharp
public void displayName()
{
    string name = concatenateName();
    Console.WriteLine(name);
}
```

That's a small but worthwhile habit to notice: `concatenateName()` is reusable on its own (something else could call it and get the name back as a string to use elsewhere), while `displayName()` exists purely to combine "get the name" with "print it." Splitting those two responsibilities into separate methods, one that returns a value and one that does something with it, is what makes each piece independently useful instead of bundling everything into a single method that only knows how to print.
