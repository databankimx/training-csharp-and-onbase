# Chapter 5 - Implementing Class Hierarchies (Part 1: Inheritance and Interfaces)

## What This Chapter Is Actually About

Up to now, every type you've written has stood alone. This chapter is about types that relate to each other: one class inheriting state and behavior from another, and types declaring capabilities they promise to support. It also introduces the handful of standard .NET interfaces -- `IComparable`, `IComparer`, `IEquatable`, `ICloneable`, `IEnumerable`, `IDisposable` -- that you'll encounter constantly in real C# code.

Part 1 covers the hierarchy itself, constructor chaining, and the comparison and equality interfaces. [Part 2](Lesson-Part2-Enumerable-and-Disposable.md) picks up with `IEnumerable`, `IDisposable`, and the operator overloading bonus.

---

## Some Vocabulary the Chapter Leans On

Before the code, four terms that show up constantly:

- **Base class** (parent/superclass): the class being inherited from.
- **Derived class** (child/subclass): the one doing the inheriting.
- **Descendant**: any class down the chain from a given class, immediate children or further.
- **Ancestor**: any class up the chain, immediate parent or further.

`Person -> Employee -> Faculty -> TeachingAssistant` is the spine of this chapter's hierarchy. `TeachingAssistant` is a descendant of `Person`; `Faculty` is its ancestor; and `TeachingAssistant`/`Student` are siblings in the loose sense that both eventually trace back to `Person`, even though they take different paths to get there.

---

## How to Write This Program

This chapter's programs involve building types, not just using them, so the mini-program approach shifts slightly. Each section below asks you to build one type alongside `Main()`, run a small demo, and keep building. The types you write along the way stack on top of each other as the chapter progresses, so unlike Chapters 2 and 3, you're not clearing out `Main()` between topics -- you're extending it.

### Mini-Program 1: A Base Class With Constructor Chaining

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public Person Manager { get; set; }

    public Person() { }

    public Person(string firstName)
    {
        if (string.IsNullOrEmpty(firstName))
            throw new ArgumentOutOfRangeException(nameof(firstName), "FirstName must not be null or blank!");
        FirstName = firstName;
    }

    public Person(string firstName, string lastName) : this(firstName)
    {
        if (string.IsNullOrEmpty(lastName))
            throw new ArgumentOutOfRangeException(nameof(lastName), "LastName must not be null or blank!");
        LastName = lastName;
    }

    public string FullName(bool lastFirst = false) =>
        lastFirst ? $"{LastName}, {FirstName}" : $"{FirstName} {LastName}";
}
```

```csharp
var person = new Person("Ada", "Lovelace");
Console.WriteLine(person.FullName());
Console.WriteLine(person.FullName(lastFirst: true));

try
{
    var bad = new Person(""); // throws
}
catch (ArgumentOutOfRangeException ex)
{
    Console.WriteLine(ex.Message);
}
```

Run it. `Ada Lovelace`, then `Lovelace, Ada`, then the exception message.

The `: this(firstName)` on the two-argument constructor is **constructor chaining** -- it calls the one-argument constructor before running its own body, so `firstName` is validated exactly once rather than duplicated in every constructor that takes it. The called constructor always runs to completion *before* the calling constructor's body starts. Chain it a few levels and you can guarantee a specific sequence of initialization regardless of which public constructor a caller picks.

A class can only inherit from a **single** base class -- that's not an oversight, it's deliberate, and it's why interfaces exist. You can implement any number of interfaces on top of that single base class, and that combination covers most of what people want from multiple inheritance without the ambiguity that made multiple-class inheritance a mess in other languages.

### Mini-Program 2: A Derived Class With `base()`

```csharp
public class Employee : Person
{
    public string Department { get; set; }

    public Employee() { }

    public Employee(string firstName) : base(firstName) { }

    public Employee(string firstName, string lastName) : base(firstName, lastName) { }

    public Employee(string firstName, string lastName, string department) : base(firstName, lastName)
    {
        if (string.IsNullOrEmpty(department))
            throw new ArgumentOutOfRangeException(nameof(department), "Department must not be null or blank!");
        Department = department;
    }
}
```

```csharp
var employee = new Employee("Grace", "Hopper", "Engineering");
Console.WriteLine($"{employee.FullName()} - {employee.Department}");

// An Employee IS-A Person: widening assignment needs no cast
Person person = employee;
Console.WriteLine(person.GetType().Name); // still Employee at runtime
```

Run it. The name and department, then `Employee`.

`: base(firstName, lastName)` calls the two-argument `Person` constructor before `Employee`'s own body runs. That means `firstName`/`lastName` validation lives in `Person` exactly once, and every `Employee` constructor that chains up to it gets the validation for free. The three-argument `Employee` constructor only validates `department` -- the rest was already handled before its body started.

The last two lines demonstrate something that's easy to take for granted: assigning an `Employee` to a `Person` variable compiles without a cast, because every `Employee` IS-A `Person`, always, by definition of inheritance. What changes is which members the compiler will let you access through the `person` variable -- only `Person`'s members, even though the object is genuinely still an `Employee` at runtime. `person.Department` won't compile, but `person.GetType().Name` returns `"Employee"` because the *object* never changed, only the lens you're viewing it through did.

### Mini-Program 3: A Deeper Hierarchy

```csharp
public enum Degree { Associate, Bachelor, Master, Doctorate }

public class Faculty : Employee
{
    public Degree Degree { get; set; }

    public Faculty() { }

    public Faculty(string firstName, string lastName, Degree degree) : base(firstName, lastName)
    {
        Degree = degree;
    }
}
```

```csharp
var faculty = new Faculty("Charles", "Babbage", Degree.Doctorate);
Console.WriteLine($"{faculty.FullName()} -- {faculty.Department ?? "no department"}, {faculty.Degree}");
```

Run it. `Charles Babbage -- no department, Doctorate`.

`Faculty : Employee : Person` -- three levels deep. `Faculty` inherits everything `Employee` has (including everything `Employee` inherited from `Person`), which is why `faculty.FullName()` works even though `Faculty` itself has no idea `FullName` exists. Also notice `faculty.Department` is `null` here -- the `Faculty(string, string, Degree)` constructor chains to the two-argument `Employee` constructor, not the three-argument one, so `Department` was never set. `null` is the default for a string property, and the `?? "no department"` handles it cleanly.

### Mini-Program 4: An Interface

```csharp
public class Course
{
    public string Name { get; set; }
    public int RawGrade { get; set; }
    public string LetterGrade => RawGrade switch
    {
        >= 90 => "A", >= 80 => "B", >= 70 => "C", >= 60 => "D", _ => "F"
    };
}

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
            Console.WriteLine($"{course.Name}: {course.LetterGrade} ({course.RawGrade})");
    }
}
```

```csharp
var student = new Student
{
    FirstName = "Alan",
    LastName = "Turing",
    Courses =
    [
        new Course { Name = "Computer Science", RawGrade = 98 },
        new Course { Name = "Mathematics", RawGrade = 95 }
    ]
};
Console.WriteLine(student.FullName());
student.PrintGrades();
```

Run it. The name, then two courses with letter grades.

`Student : Person, IStudent` -- one base class (after the colon, first), then one interface (separated by a comma). An interface is a **contract**: any class implementing `IStudent` promises to provide `Courses` and `PrintGrades()`. It doesn't know or care how -- that's entirely up to the implementing class.

The practical difference between an interface and an abstract class:

| | `interface` | `abstract class` |
|---|---|---|
| How many can a class use? | Any number | Exactly one |
| Can hold state (fields)? | No (default implementations only in C# 8+) | Yes |
| Constructors? | No | Yes |
| Expresses | A capability ("can do this") | An identity ("is a kind of this") |

`IStudent` is a capability -- something a type can do, independently of what it is. `Person` is an identity. That's the distinction driving which mechanism to reach for.

### Mini-Program 5: A Type That Implements Both Base Class and Interface

```csharp
public class TeachingAssistant : Faculty, IStudent
{
    // Composition: TA has a Student identity rather than inheriting it
    // (can't inherit from both Faculty and Student, since you only get one base class)
    private readonly Student myStudent = new Student();

    public string Credentials() =>
        $"TA {FirstName} {LastName} has a {Degree} degree.";

    // IStudent delegated to the internal Student instance
    public List<Course> Courses
    {
        get => myStudent.Courses;
        set => myStudent.Courses = value;
    }

    public void PrintGrades() => myStudent.PrintGrades();
}
```

```csharp
var ta = new TeachingAssistant
{
    FirstName = "Linus",
    LastName = "Torvalds",
    Degree = Degree.Master,
    Courses = [new Course { Name = "Operating Systems", RawGrade = 99 }]
};
Console.WriteLine(ta.Credentials());
ta.PrintGrades();
```

Run it. The credentials line, then the course.

`TeachingAssistant` is both a `Faculty` member (through inheritance) and a student (through `IStudent`). It can't inherit from both `Faculty` and `Student` -- you only get one base class -- so it **composes** a `Student` as a private field and delegates the `IStudent` implementation to it. This is the "has a" relationship from last chapter: a TA *has a* student identity rather than *is a* student in the inheritance sense.

The composition approach has a bonus: if `Student`'s implementation of `PrintGrades()` changes, `TeachingAssistant` gets the update for free, because it's not duplicating the logic -- it's calling through to the real thing.

### Mini-Program 6: IComparable

```csharp
public class Car : IComparable
{
    public string Make { get; set; }
    public string Model { get; set; }
    public int Year { get; set; }
    public int Horsepower { get; set; }
    public int MaxMph { get; set; }
    public decimal Price { get; set; }
    public string Name => $"{Year} {Make} {Model}";

    public int CompareTo(object obj)
    {
        if (obj is null) return 1; // null sorts before any non-null instance
        if (!(obj is Car))
            throw new ArgumentException($"Cannot compare Car to [{obj.GetType().Name}]");
        return string.Compare(Name, ((Car)obj).Name, StringComparison.CurrentCultureIgnoreCase);
    }
}
```

```csharp
var cars = new[]
{
    new Car { Make = "Tesla",   Model = "Model S",  Year = 2023, MaxMph = 155, Horsepower = 670, Price = 74990m },
    new Car { Make = "Ferrari", Model = "Roma",     Year = 2023, MaxMph = 199, Horsepower = 612, Price = 222000m },
    new Car { Make = "BMW",     Model = "M3",       Year = 2023, MaxMph = 180, Horsepower = 503, Price = 75900m },
    new Car { Make = "Porsche", Model = "911 GT3",  Year = 2023, MaxMph = 184, Horsepower = 502, Price = 161100m },
};

Array.Sort(cars);
foreach (var car in cars)
    Console.WriteLine($"{car.Name,-35} {car.MaxMph,3} mph  {car.Price:C}");
```

Run it. Four cars, alphabetically by `Name` (which is `Year Make Model`), because that's what `CompareTo` sorts on.

`IComparable.CompareTo` returns a negative number when `this` sorts before `obj`, zero when they're equivalent, and positive when `this` sorts after. `Array.Sort` uses exactly that to decide the order. The return-value contract is easy to get backwards the first time -- remember: negative means "I come first."

Notice `CompareTo(null)` returns `1`, not a throw. That's the documented .NET convention: `null` sorts before any non-null instance, and `IComparable.CompareTo` should never throw for a null argument. A non-null, wrong-type argument genuinely is an error and throws `ArgumentException`.

### Mini-Program 7: IComparer

```csharp
public class CarComparer : IComparer<Car>
{
    public enum CompareField { Name, MaxMph, Horsepower, Price }
    public CompareField SortBy = CompareField.Name;

    public int Compare(Car x, Car y)
    {
        return SortBy switch
        {
            CompareField.MaxMph      => x.MaxMph.CompareTo(y.MaxMph),
            CompareField.Horsepower  => x.Horsepower.CompareTo(y.Horsepower),
            CompareField.Price       => x.Price.CompareTo(y.Price),
            _                        => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase)
        };
    }
}
```

```csharp
var comparer = new CarComparer { SortBy = CarComparer.CompareField.Price };
Array.Sort(cars, comparer);
foreach (var car in cars)
    Console.WriteLine($"{car.Name,-35} {car.Price:C}");

comparer.SortBy = CarComparer.CompareField.MaxMph;
Array.Sort(cars, comparer);
foreach (var car in cars)
    Console.WriteLine($"{car.Name,-35} {car.MaxMph} mph");
```

Run it. Cars sorted by price, then again sorted by max speed.

`IComparable` gave `Car` one fixed sort: alphabetically by name, baked right into the class. `IComparer<Car>` lives in a *separate* class and can offer multiple sort criteria, one per enum value. The `IComparer<Car>` is also type-safe: `Compare(Car x, Car y)` only accepts `Car` instances, while `IComparable.CompareTo(object obj)` takes a bare `object` that you have to check yourself.

The rule of thumb: reach for `IComparable` when "compare two of these" has one obvious, fixed meaning. Reach for `IComparer<T>` when the answer to "compare by what?" legitimately changes.

### Mini-Program 8: IEquatable, and Why `==` Isn't Enough

```csharp
public class PersonWithEquality : IEquatable<PersonWithEquality>
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public PersonWithEquality(string first, string last)
    {
        FirstName = first; LastName = last;
    }

    public bool Equals(PersonWithEquality other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(FirstName, other.FirstName, StringComparison.CurrentCultureIgnoreCase) &&
               string.Equals(LastName, other.LastName, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object obj) => Equals(obj as PersonWithEquality);

    public override int GetHashCode() =>
        HashCode.Combine(
            FirstName?.ToUpperInvariant(),
            LastName?.ToUpperInvariant());
}
```

```csharp
var abe = new PersonWithEquality("Abraham", "Lincoln");
var lincoln = new PersonWithEquality("Abraham", "Lincoln");

Console.WriteLine(abe == lincoln);        // False - reference equality, different objects
Console.WriteLine(abe.Equals(lincoln));   // True  - value equality via IEquatable

var people = new List<PersonWithEquality> { abe };
Console.WriteLine(people.Contains(lincoln)); // True - List.Contains uses Equals()
```

Run it. `False`, then `True`, then `True`.

`==` on a class compares object identity by default -- two different `new PersonWithEquality(...)` objects are never `==` unless you override the operator explicitly. `Equals()` is what `List<T>.Contains()`, `Dictionary<TKey,TValue>`, `Stack<T>`, and `Queue<T>` all use internally, so implementing `IEquatable<T>` is the thing that actually makes those collections find your objects correctly.

`GetHashCode()` is not optional once you override `Equals()`. Two objects that are `Equals` must always return the same hash code, or the type silently breaks as a `Dictionary` key or `HashSet` member -- lookups can fail to find an entry that's genuinely there, with no error anywhere in the chain to point at. Note that `HashCode.Combine` is a .NET Framework 4.6.1+ API, available in this project's `net48` target without any additional packages.

### Mini-Program 9: ICloneable -- Three Implementations, Side by Side

```csharp
public class PersonCloneable : ICloneable
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public PersonCloneable Manager { get; set; }

    public object Clone()
    {
        // Option 1 -- hand-written shallow clone:
        // return new PersonCloneable { FirstName = FirstName, LastName = LastName, Manager = Manager };

        // Option 2 -- MemberwiseClone() shallow clone (same result as Option 1, less code):
        // return MemberwiseClone();

        // Option 3 -- deep clone (Manager is recursively cloned, not shared):
        return new PersonCloneable
        {
            FirstName = FirstName,
            LastName = LastName,
            Manager = (PersonCloneable)Manager?.Clone()
        };
    }
}
```

```csharp
var boss = new PersonCloneable { FirstName = "Ada", LastName = "Lovelace" };
var bob = new PersonCloneable { FirstName = "Bob", LastName = "Smith", Manager = boss };

// Plain assignment -- no copy at all
var anne = bob;
anne.FirstName = "Anne";
Console.WriteLine(bob.FirstName); // "Anne" -- anne and bob are the same object

// Clone -- genuinely independent copy
var robert = (PersonCloneable)bob.Clone();
robert.FirstName = "Robert";
Console.WriteLine(bob.FirstName); // still "Anne" -- robert is a separate object

// Deep clone means Manager is also independent
robert.Manager.FirstName = "Changed";
Console.WriteLine(bob.Manager.FirstName); // still "Ada" -- deep clone, not shared
```

Run it. `Anne`, then `Anne`, then `Ada`.

Plain assignment (`anne = bob`) copies the reference, not the object -- same as every reference type in C#. `Clone()` is how you get an actual independent copy.

The three commented options inside `Clone()` are worth reading side by side: Option 1 and Option 2 both produce a **shallow** clone -- a new outer object but shared child references. Under Option 2, `bob` and `robert` would share the same `Manager` object, so mutating `robert.Manager.FirstName` would also change `bob.Manager.FirstName`. Option 3, the deep clone, recursively calls `Clone()` on `Manager` too, so each person owns their own independent copy of their manager.

One caveat on `ICloneable` itself: its `Clone()` returns `object`, forcing a cast on every call site, and the interface contract never specifies whether the result is shallow or deep. Microsoft's own guidance is that it's a poorly specified interface and explicitly named methods like `ShallowClone()`/`DeepClone()` are cleaner in new code. It's covered here because you'll encounter it constantly in existing code and it's on the exam.

---

## Interfaces vs. Abstract Classes (The Summary Table)

Worth having in one place before moving on:

| | `interface` | `abstract class` |
|---|---|---|
| How many can a class use? | Any number | Exactly one |
| Can hold state (fields)? | No (default implementations only, C# 8+) | Yes |
| Can provide implementation? | Default implementations only | Yes, freely |
| Constructors? | No | Yes |
| Expresses | A capability ("can do this") | An identity ("is a kind of this") |

"Is a" points toward inheritance. "Can do" points toward an interface. When you're not sure which one you're dealing with, try saying the sentence out loud -- "a TeachingAssistant **is a** Faculty member" (inherit) vs. "a TeachingAssistant **can be** a student" (interface).

---

Continue to [Part 2](Lesson-Part2-Enumerable-and-Disposable.md) for `IEnumerable`, `IDisposable`, operator overloading, and `GC.SuppressFinalize`.
