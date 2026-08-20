# Ch05 Textbook Code: University Classes

## What This Is

The best interface-implementation lesson in this chapter: four different `TeachingAssistant` classes, each implementing `IStudent` a different way, side by side in one file.

```csharp
public class TeachingAssistant : Faculty, IStudent
{
    public List<string> Courses { get; set; }
    public void PrintGrades() { Console.WriteLine("TeachingAssistant.PrintGrades"); }
}

// Delegate IStudent to a Student object.
public class TeachingAssistant2 : Faculty, IStudent
{
    private Student MyStudent = new Student();
    public List<string> Courses { get => MyStudent.Courses; set => MyStudent.Courses = value; }
    public void PrintGrades() { MyStudent.PrintGrades(); }
}

// Implicit implementation.
public class TeachingAssistant3 : Faculty, IStudent { ... }

// Explicit implementation.
public class TeachingAssistant4 : Faculty, IStudent
{
    List<string> IStudent.Courses { get; set; }
    void IStudent.PrintGrades() { Console.WriteLine("TeachingAssistant4.IStudent.PrintGrades"); }
}
```

No bugs found. `Faculty`, `Employee`, `Staff`, `Student`, `Person` are also all defined here, a small self-contained hierarchy just for this demo.

---

## The Real Point: Explicit Interface Implementation

```csharp
TeachingAssistant4 ta = new TeachingAssistant4();

// The following causes a design time error for the
// TeachingAssistant4 class but not the TeachingAssistant class.
//ta.PrintGrades();

// The following does work.
IStudent student = ta;
student.PrintGrades();
```

`TeachingAssistant4.PrintGrades()` is written as `void IStudent.PrintGrades()`, an **explicit** interface implementation. That syntax means the method is only accessible through an `IStudent`-typed reference, `ta.PrintGrades()` genuinely does not compile (try uncommenting that line and see for yourself), even though `ta` is unquestionably a `TeachingAssistant4` and `TeachingAssistant4` unquestionably implements `IStudent`. Assign the same object to an `IStudent` variable first, and the identical method call works fine. This is a real, correct language feature being demonstrated accurately, not a bug, and the inline comment already explains it correctly.

Worth understanding why you'd ever want this: explicit implementation is how you resolve a name collision when a class needs to implement two interfaces (or an interface and its own method) that both declare a member with the same name, or when you want a member to be part of a type's contract without cluttering its ordinary public surface. `TeachingAssistant3` (implicit) and `TeachingAssistant4` (explicit) sitting right next to each other, doing the same job two different ways, is the clearest possible way to see the distinction.

---

## Worth Comparing: Four Ways to Satisfy One Interface

Reading all four `TeachingAssistant*` classes in sequence tells a small story:

1. **`TeachingAssistant`**: implement the interface directly, the straightforward default.
2. **`TeachingAssistant2`**: delegate to a private `Student` field, composition instead of duplicating logic (the same pattern `Ch05.Supplemental.ImplementingClassHierarchies`'s `Contact`/`Person`/`Address` hierarchy uses, worth comparing).
3. **`TeachingAssistant3`**: implicit implementation, stubbed with `NotImplementedException` for the parts not needed for this demo.
4. **`TeachingAssistant4`**: explicit implementation, the one actually exercised in `Form1_Load()`.

---

## Worth Knowing: A Namespace Mismatch, Left As-Is

Same note as `CSharp.Ch05.TextbookCode.TreeEnumerator`: every `.cs` file here declares `namespace UniversityClasses`, not `namespace CSharp.Ch05.TextbookCode.UniversityClasses`. Kept exactly as downloaded per the "unedited code" policy, the `.csproj`'s `RootNamespace`/`AssemblyName` still follow this solution's naming convention without needing to touch the `.cs` files' own namespace declarations.
