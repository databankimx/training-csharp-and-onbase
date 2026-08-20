# Ch05 Textbook Code: Person Hierarchy

## What This Is

The smallest lab in this chapter: a `Person`/`Employee` pair, both defined directly inside `Form1.cs` (no separate model files), demonstrating `base` constructor chaining in its simplest possible form.

```csharp
private void Form1_Load(object sender, EventArgs e)
{
    Person person = new Person("Ann", "Archer");
    Employee employee = new Employee("Ben", "Baker", "Information Technology");
}
```

No bugs found. Pure reference code, nothing displayed anywhere, `person` and `employee` are created and then never used. Best viewed with a debugger breakpoint on the closing brace, inspecting both objects in the Locals window, rather than run straight through.

---

## Worth Noticing: The Whole Pattern in Two Constructors

```csharp
public class Person
{
    public Person(string firstName, string lastName)
    {
        if ((firstName == null) || (firstName.Length < 1))
            throw new ArgumentOutOfRangeException("firstName", firstName, "FirstName must not be null or blank.");
        if ((lastName == null) || (lastName.Length < 1))
            throw new ArgumentOutOfRangeException("lastName", lastName, "LastName must not be null or blank.");

        FirstName = firstName;
        LastName = lastName;
    }
}

public class Employee : Person
{
    public Employee(string firstName, string lastName, string departmentName)
        : base(firstName, lastName)
    {
        if ((departmentName == null) || (departmentName.Length < 1))
            throw new ArgumentOutOfRangeException("departmentName", departmentName, "DepartmentName must not be null or blank.");

        DepartmentName = departmentName;
    }
}
```

If the main lesson's version of this pattern (`CSharp.Ch05.ImplementingClassHierarchies`, `CodeLabInvokingConstructors()`) felt like it had a lot going on, this is the same idea distilled to its essentials: one base constructor that validates two fields, one derived constructor that calls `: base(...)` first and validates one more field after. Worth reading this version first if the concept didn't fully land the first time, then going back to the fuller example with the extra context this smaller one provides.
