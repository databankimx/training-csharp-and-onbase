# ImplementingClassHierarchies

## Introduction

Inheritance, interfaces, and the standard .NET interfaces that show up constantly once you start building real class hierarchies: `IComparable`, `IComparer`, `IEquatable`, `ICloneable`, `IEnumerable`, and `IDisposable`.

---

## Terminology

- **Base class** (parent/superclass): the class other classes derive from.
- **Derived class** (child/subclass): inherits from a base class.
- **Descendant**: any class down the chain from a given class.
- **Ancestor**: any class up the chain from a given class.
- **Sibling**: classes sharing the same immediate parent.

Class inheritance is written as `class ChildClass : ParentClass`. A class can only inherit from one base class, but it can implement any number of interfaces.

---

## Code Lab: Invoking Constructors

```csharp
public class Person
{
    public Person(string firstName)
    {
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentOutOfRangeException(nameof(firstName), firstName, "FirstName must not be null or blank!");
        FirstName = firstName;
    }

    public Person(string firstName, string lastName) : this(firstName)
    {
        if (string.IsNullOrEmpty(lastName))
            throw new ArgumentOutOfRangeException(nameof(lastName), lastName, "LastName must not be null or blank!");
        LastName = lastName;
    }
}
```

`: this(...)` calls another constructor in the *same* class before the current one runs. The called constructor executes first, then the new constructor's own code runs.

```csharp
public class Employee : Person
{
    public Employee(string firstName, string lastName) : base(firstName, lastName)
    {
    }

    public Employee(string firstName, string lastName, string department) : base(firstName, lastName)
    {
        if (string.IsNullOrEmpty(department))
            throw new ArgumentOutOfRangeException(nameof(department), department, "Department must not be null or blank!");
        Department = department;
    }
}
```

`: base(...)` does the same thing, one level up, calling a constructor in the *parent* class. The base constructor's arguments must match one of its signatures, and it always runs before the derived constructor's own body.

```csharp
var bea = new Person("Bea");
var al = new Person("Al", "Able");
var carl = new Employee("Carl");
var ed = new Employee("Ed", "Eager", "IT");
```

---

## Code Lab: Implementing an Interface

```csharp
public interface IStudent
{
    List<Course> Courses { get; set; }
    void PrintGrades();
}

public class Student : Person, IStudent
{
    public List<Course> Courses { get; set; }

    public void PrintGrades()
    {
        foreach (var course in Courses)
        {
            Console.WriteLine($"{course.Name}: {course.LetterGrade} ({course.RawGrade})");
        }
    }
}
```

An interface is a contract, not an implementation. It declares members a class must provide, but doesn't provide any code itself, and its members don't take access modifiers.

```csharp
public class TeachingAssistant : Faculty, IStudent
{
    private readonly Student myStudent = new Student();

    public List<Course> Courses
    {
        get => myStudent.Courses;
        set => myStudent.Courses = value;
    }

    public void PrintGrades() => myStudent.PrintGrades();

    public string Credentials() => $"TA {FirstName} {LastName} has a {Degree} degree.";
}
```

This is the payoff for interfaces existing at all: a `TeachingAssistant` needs to be both a `Faculty` member and a `Student`. Since a class can only inherit from one base class, `TeachingAssistant` inherits from `Faculty` and implements `IStudent` instead, getting a student's behavior through composition (holding a private `Student` instance and delegating to it) rather than a second inheritance chain that C# doesn't allow.

```csharp
IStudent secondTa = new TeachingAssistant { FirstName = "Jake", ... };
secondTa.PrintGrades();                          // works, IStudent member
// secondTa.Credentials();                       // does NOT compile, not an IStudent member
Console.WriteLine(((TeachingAssistant)secondTa).Credentials()); // works, after casting back
```

You can't instantiate an interface, but you can declare a variable *as* one. Through that `IStudent` variable, only `IStudent`'s members are visible, even though the object underneath is fully a `TeachingAssistant`. Getting back to the `TeachingAssistant`-specific members requires an explicit cast.

---

## Code Lab: Comparing Cars (IComparable and IComparer)

```csharp
public class Car : IComparable
{
    public int CompareTo(object obj)
    {
        var other = (Car)obj;
        return string.Compare(Name, other.Name, StringComparison.CurrentCultureIgnoreCase);
    }
}
```

```csharp
var car1 = new Car { Year = 2014, Make = "SSC Ultimate", Model = "Aero", MaxMph = 257, Horsepower = 1183, Price = 654400m };
var car2 = new Car { Year = 2014, Make = "SSC Ultimate", Model = "Aero", MaxMph = 257, Horsepower = 1183, Price = 654400m };

Console.WriteLine($"car1 == car2: {car1 == car2}");                     // true, Car overloads ==, see the Bonus section below
Console.WriteLine($"car1.CompareTo(car2): {car1.CompareTo(car2) == 0}"); // true
```

On a plain class, `==` defaults to reference comparison, two separately-constructed objects are never `==`, no matter how identical their data is, only `CompareTo` returning `0` reflects that they're equivalent by value. `Car` in this project overloads `==` itself (covered in the Bonus section), so it no longer follows the default rule, worth knowing which behavior you're looking at. `IComparable.CompareTo` is what `Array.Sort()` uses by default.

```csharp
public class CarComparer : IComparer<Car>
{
    public CompareField SortBy = CompareField.Name;

    public int Compare(Car x, Car y)
    {
        switch (SortBy)
        {
            case CompareField.MaxMph: return x.MaxMph.CompareTo(y.MaxMph);
            case CompareField.Horsepower: return x.Horsepower.CompareTo(y.Horsepower);
            case CompareField.Price: return x.Price.CompareTo(y.Price);
            default: return x.Name.CompareTo(y.Name);
        }
    }
}
```

`IComparer<T>` lives in a separate class instead of on the type being compared, and can support multiple sort criteria by switching `SortBy`. It's the more flexible option whenever "compare by what?" isn't a single fixed answer, and it's type-checked at the generic level rather than taking a bare `object`.

---

## Code Lab: Equating Persons (IEquatable)

```csharp
public class Person : IEquatable<Person>
{
    public bool Equals(Person other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(FirstName, other.FirstName, StringComparison.CurrentCultureIgnoreCase) &&
               string.Equals(LastName, other.LastName, StringComparison.CurrentCultureIgnoreCase);
    }
}
```

```csharp
var abeLincoln = new Person("Abe", "Lincoln");
var abrahamLincoln = new Person("Abe", "Lincoln");

Console.WriteLine($"abeLincoln == abrahamLincoln ? {abeLincoln == abrahamLincoln}");         // false
Console.WriteLine($"abeLincoln.Equals(abrahamLincoln) ? {abeLincoln.Equals(abrahamLincoln)}"); // true

var people = new List<Person> { abeLincoln };
people.Contains(abrahamLincoln); // true, thanks to IEquatable
```

Same pattern as `Car`: `==` checks reference identity, `Equals` (from `IEquatable`) checks value equality. `List<T>.Contains()` relies on `Equals` internally, so implementing `IEquatable` is what makes `Contains()` correctly recognize a value-equal object even though it's a different instance. It's a best practice to implement `IEquatable` on any type you'll store in a `List`, `Dictionary`, `Stack`, or `Queue`.

---

## Code Lab: Cloning Persons (ICloneable)

```csharp
var anne = ann;
anne.FirstName = "Anne";
// ann.FirstName is now "Anne" too, "anne" and "ann" are the same object
```

```csharp
public object Clone()
{
    return new Person
    {
        FirstName = FirstName,
        LastName = LastName,
        Manager = (Person)Manager?.Clone()
    };
}
```

```csharp
var robert = (Person)bob.Clone();
robert.FirstName = "Robert";
// bob.FirstName is unaffected, "robert" is a genuinely separate object
```

Plain assignment copies the reference, not the object. `Clone()` produces an independent copy. Notice `Manager` is cloned recursively too, `Manager?.Clone()`, not just copied as a reference, that's what makes this a **deep** clone rather than a shallow one, if it only copied `Manager` directly, the clone and original would still share the same `Manager` object underneath.

---

## Code Lab: The Org Chart (IEnumerable)

```csharp
public class TreeNode : IEnumerable<TreeNode>
{
    public List<TreeNode> Children { get; set; } = new List<TreeNode>();

    public TreeNode AddChild(string text)
    {
        var child = new TreeNode(text) { Depth = Depth + 1 };
        Children.Add(child);
        return child;
    }

    public IEnumerator<TreeNode> GetEnumerator() => new TreeEnumerator(this);
}
```

```csharp
public class TreeEnumerator : IEnumerator<TreeNode>
{
    public TreeNode Current => GetCurrent();

    public bool MoveNext()
    {
        currentIndex++;
        return currentIndex < nodes.Count;
    }

    public void Reset() => currentIndex = -1;

    public TreeEnumerator(TreeNode root)
    {
        nodes = root.Preorder(); // flattens the tree into a list, parent before children
        Reset();
    }
}
```

`IEnumerable`/`IEnumerator` are what `foreach` actually compiles down to. `TreeNode.Preorder()` walks the whole tree once, depth-first, parent before children, and flattens it into a plain list, `TreeEnumerator` just walks that list with an index. The tree structure itself doesn't need to know anything about enumeration order, that's entirely `TreeEnumerator`'s responsibility.

```csharp
var president = new TreeNode("President");
var sales = president.AddChild("VP Sales");
var domestic = sales.AddChild("Domestic Sales");
// ... more nodes ...

using (var enumerator = president.GetEnumerator())
{
    while (enumerator.MoveNext())
    {
        string spacer = new string(' ', 4 * enumerator.Current.Depth);
        Console.WriteLine($"{spacer}{enumerator.Current.Text}");
    }
}
```

---

## Implementing IDisposable

```csharp
public class DisposableClass : IDisposable
{
    private bool resourcesAreFreed;

    public void Dispose()
    {
        FreeResources(true);
    }

    ~DisposableClass()
    {
        FreeResources(false);
    }

    private void FreeResources(bool freeManagedResources)
    {
        if (resourcesAreFreed) return;

        GC.SuppressFinalize(this);
        resourcesAreFreed = true;

        // free unmanaged resources here, always

        if (!freeManagedResources) return;

        // free managed resources here, only when called explicitly via Dispose()
    }
}
```

`IDisposable` exists to release resources **deterministically**, on your own schedule, rather than waiting for the garbage collector.

**How garbage collection actually works, briefly:**
1. An object becomes eligible for collection once nothing reachable still references it.
2. Collection runs periodically, not immediately, an eligible object can sit in memory a while before actually being reclaimed.
3. When it runs, the GC marks everything unreachable, walks live references marking what's actually reachable, checks the still-unreachable objects for finalizers and calls any it finds, then reclaims the memory.
4. Finalization is **non-deterministic**, you can't predict exactly when a given object's finalizer will run.

The `bool` parameter distinguishes the two paths: called from `Dispose()` (explicit, you control the timing), free everything, managed and unmanaged. Called from the finalizer `~DisposableClass()` (GC-triggered, non-deterministic), only free unmanaged resources. `GC.SuppressFinalize(this)` tells the GC "skip the finalizer, this object is already cleaned up," avoiding redundant work.

```csharp
var alan = new DisposableClass { Name = "Alan" };
alan.Dispose(); // explicit, deterministic

var betty = new DisposableClass { Name = "Betty" }; // left for the GC to finalize eventually

using (var charles = new DisposableClass { Name = "Charles" })
{
    charles.Name = "Chuck";
} // Dispose() called implicitly here, when the using block ends
```

Three disposal paths, side by side: explicit, GC-finalized, and `using`-block-implicit.

---

## Bonus: Operator Overloading on Car

`Car` implements `IComparable`, which gets you `CompareTo()`, but C# also lets you overload the actual comparison operators directly on a type, so callers can write `car1 < car2` instead of `car1.CompareTo(car2) < 0`.

```csharp
public static bool operator ==(Car left, Car right)
{
    return CompareCars(left, right) == 0;
}

public static bool operator <(Car left, Car right)
{
    return CompareCars(left, right) < 0;
}

public override bool Equals(object obj)
{
    return obj is Car other && CompareCars(this, other) == 0;
}

public override int GetHashCode()
{
    return Name?.ToUpperInvariant().GetHashCode() ?? 0;
}
```

`Car` now supports `==`, `!=`, `<`, `<=`, `>`, `>=`, `Equals()`, and `GetHashCode()`, all built on the same `CompareTo()` logic already in place. Two things worth knowing:

- **`GetHashCode()` has to change whenever `Equals()` does.** Two objects that are `Equal` must return the same hash code, otherwise the type breaks the moment it's used as a `Dictionary` key or stored in a `HashSet`.
- **`CompareTo(null)` follows standard .NET convention**: `null` is treated as sorting before any non-null instance, so comparing against `null` returns a positive number instead of throwing. Every operator here goes through a private `CompareCars()` helper on top of that, which adds the one thing `CompareTo()` still can't handle on its own, you can't call an instance method on a `null` reference at all, so `CompareCars()` checks for a `null` left-hand `Car` before ever calling `CompareTo()`.

One consequence worth noticing: earlier in this lesson, `car1 == car2` was used to demonstrate that a plain class defaults to reference equality. Once `Car` gets these operator overloads, that's no longer true for `Car` specifically, `car1 == car2` now returns `true` for two cars with equivalent data. Both facts are correct, they're just about two different states of the same class.
