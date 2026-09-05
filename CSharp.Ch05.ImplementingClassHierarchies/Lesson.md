# Chapter 5: Implementing Class Hierarchies

## What This Chapter Is Actually About

Inheritance, interfaces, and the handful of standard .NET interfaces (`IComparable`, `IComparer`, `IEquatable`, `ICloneable`, `IEnumerable`, `IDisposable`) that show up constantly once you start writing real class hierarchies.

Chapter 5 has four projects worth running, three of which are supplemental. This document covers the main one. The original model set traced clean — every bug documented here was actually introduced (and then fixed) by the bonus operator-overloading pass on `Car`, not present in the textbook download itself.

---

## Terminology, Because the Book Leans On It

- **Base class** (parent/superclass): the class other classes derive from.
- **Derived class** (child/subclass): inherits from a base class.
- **Descendant**: any class down the chain from a given class, immediate children or further.
- **Ancestor**: any class up the chain, immediate parent or further.
- **Sibling**: classes sharing the same immediate parent.

`Person → Employee → Faculty → TeachingAssistant` is the spine of this chapter's hierarchy. `TeachingAssistant` is a descendant of `Person`, `Faculty` is its ancestor, and `TeachingAssistant`/`Student` are siblings in the loose sense that both eventually trace back to `Person` — though `Student` inherits directly from `Person` while `TeachingAssistant` goes through `Faculty` first.

---

## Constructor Chaining: `this()` and `base()`

```csharp
public Person(string firstName) { ... }

public Person(string firstName, string lastName) : this(firstName)
{
	// firstName validation already happened in the constructor above
	...
}
```

`: this(...)` calls another constructor *in the same class* before running the current one's body. `Employee`'s constructors do the equivalent thing one level up:

```csharp
public Employee(string firstName, string lastName) : base(firstName, lastName)
{
}
```

`: base(...)` calls a constructor *in the parent class*. Either way, the called constructor runs to completion first, then the calling constructor's own body executes. This is why `Employee(string, string, string)` (with department) doesn't need to re-validate `firstName`/`lastName` — `Person`'s constructor already did that work before `Employee`'s body even starts.

The ordering matters and is worth committing to memory: base constructors run **before** derived ones. A field initialized in `Employee` is not yet set while `Person`'s constructor is running, which is why calling a virtual method from a base constructor is a well-known trap — the override runs against a not-yet-initialized derived object.

A class can only inherit from one base class (`ChildClass : ParentClass`), but as this chapter demonstrates repeatedly, it can implement any number of interfaces. That's the practical answer to "why interfaces at all" — they're how C# gets the useful part of multiple inheritance without the ambiguity that killed it in other languages.

---

## Interfaces vs. Abstract Classes

`IStudent` and the `Student`/`Faculty`/`TeachingAssistant` set demonstrate the interface side. Quick decision table:

| | `interface` | `abstract class` |
|---|---|---|
| How many can a class use? | Any number | Exactly one |
| Can hold state (fields)? | No | Yes |
| Can provide implementation? | Default implementations only (C# 8+) | Yes, freely |
| Constructors? | No | Yes |
| Expresses | "can do this" (a capability) | "is a kind of this" (an identity) |

The rule of thumb: if the shared thing is *behavior a type can perform*, use an interface. If it's *shared state and partial implementation across a family of related types*, use an abstract class. `IStudent` is a capability; `Person` is an identity.

---

## IComparable vs. IComparer

```csharp
public class Car : IComparable
{
	public int CompareTo(object obj) { ... }
}
```

`IComparable.CompareTo` lives on the class itself — one fixed way to compare two instances (here, alphabetically by `Name`). It's what `Array.Sort()` uses by default.

```csharp
public class CarComparer : IComparer<Car>
{
	public CompareField SortBy = CompareField.Name;
	public int Compare(Car x, Car y) { ... }
}
```

`IComparer<T>` lives in a *separate* class, and can support multiple sort criteria (`CarComparer.SortBy` switches between `Name`, `MaxMph`, `Horsepower`, `Price`). This is the more flexible option when "compare by what?" isn't a single fixed answer, and it's type-checked at the generic parameter level rather than taking a bare `object` the way `IComparable.CompareTo` does.

The return-value contract for both is the same, and it's easy to get backwards:

| Return | Meaning |
|---|---|
| Negative | `this` (or `x`) sorts **before** the other |
| Zero | Equivalent for ordering purposes |
| Positive | `this` (or `x`) sorts **after** the other |

`CodeLabComparingCars()` demonstrates why `==` doesn't help *by default*: two `Car` instances with identical property values still return `false` from `==` on a plain class, since neither `Car` nor `object` defines value-based equality for it to use. Only `CompareTo` (returning `0` for "equivalent") reflects the actual data. `Car` in this project goes a step further and overloads `==` itself (see the bonus section below), so by the time you run this lab, `car1 == car2` prints `True`, not `False` — that override is what changed it.

---

## IEquatable

```csharp
public class Person : IEquatable<Person>, ICloneable
{
	public bool Equals(Person other) { ... }
}
```

Same shape of gotcha as `Car`: `abeLincoln == abrahamLincoln` returns `false` (reference comparison, two different objects), while `abeLincoln.Equals(abrahamLincoln)` returns `true` (value comparison via `IEquatable`, matching first and last name).

The practical payoff is that framework types lean on `Equals` internally. `List<Person>.Contains()` correctly finds a value-equal `Person` because `IEquatable` is implemented, not because of any magic in `List<T>` itself. It's a best practice to implement `IEquatable` on any type you plan to store in a `List`, `Dictionary`, `Stack`, or `Queue`.

`IEquatable<T>` is also the generic, allocation-free version of `object.Equals(object)` — no boxing for value types, no cast for reference types. That's why the framework prefers it when it's available.

---

## ICloneable, and Shallow vs. Deep Clones

```csharp
var anne = ann;
anne.FirstName = "Anne";
// ann.FirstName is now "Anne" too - "anne" and "ann" point at the same object

var robert = (Person)bob.Clone();
robert.FirstName = "Robert";
// bob.FirstName is unaffected - "robert" is a separate object
```

Plain assignment (`anne = ann`) copies the reference, not the object — exactly the reference-type behavior covered back in Chapter 3. `Clone()` is how you actually get an independent copy. `Person.Clone()` goes one step further than a shallow copy:

```csharp
return new Person
{
	FirstName = FirstName,
	LastName = LastName,
	Manager = (Person)Manager?.Clone()
};
```

A shallow clone (the commented-out `MemberwiseClone()` alternative left in the file) would copy the `Manager` reference as-is, so the clone and the original would still share the same `Manager` object. This version recursively clones `Manager` too — a genuine deep clone.

Worth reading both commented-out alternatives in `Person.cs` alongside the active implementation. Seeing the three options side by side (raw property copy, `MemberwiseClone()`, recursive deep clone) makes the distinction concrete instead of abstract.

One caveat on `ICloneable` itself: it returns `object`, so every call needs a cast, and the interface never specifies whether the clone is shallow or deep. Microsoft's own guidance is that it's a poorly specified interface and you're generally better off with a strongly-typed `Clone()` method of your own design. It's covered here because it's on the exam and you'll encounter it in older code, not because it's a pattern to reach for in new work.

---

## Custom IEnumerable: the Org Chart

```csharp
public class TreeNode : IEnumerable<TreeNode>
{
	public IEnumerator<TreeNode> GetEnumerator() => new TreeEnumerator(this);
}

public class TreeEnumerator : IEnumerator<TreeNode>
{
	public TreeNode Current => GetCurrent();
	public bool MoveNext() { currentIndex++; return currentIndex < nodes.Count; }
	public void Reset() { currentIndex = -1; }
}
```

This is what makes `foreach` (or, as `CodeLabOrgChart()` shows explicitly, a manual `while (enumerator.MoveNext())` loop) work on a `TreeNode` at all. `IEnumerable`/`IEnumerator` are the actual mechanism `foreach` compiles down to — there is no special `foreach` magic in the language beyond "call `GetEnumerator()`, then loop on `MoveNext()` reading `Current`."

`TreeNode.Preorder()` flattens the whole tree into a `List<TreeNode>` up front (a preorder traversal — parent before children, depth-first), and `TreeEnumerator` just walks that flattened list with an index. The tree structure itself (`Children`, `AddChild()`) doesn't need to know anything about enumeration; that's entirely `TreeEnumerator`'s job.

Note that `IEnumerator<T>` inherits `IDisposable`, which is why `TreeEnumerator` implements `Dispose()` even though it isn't the chapter's `IDisposable` example. Implementing one standard interface sometimes drags in the obligations of another.

Worth knowing for later: C#'s `yield return` generates this entire enumerator class for you at compile time. Writing `TreeEnumerator` by hand is the point of the exercise — it shows you what the compiler is doing when you write `yield`, so `yield` stops being magic.

---

## IDisposable and Garbage Collection

The chapter separates `IDisposable` out from the other interfaces because it addresses a different problem: releasing resources *deterministically*, on your own schedule, rather than waiting for the garbage collector.

**How GC actually works, briefly:**

1. An object becomes eligible for collection once nothing reachable still references it.
2. Collection happens periodically, not immediately — an eligible object can sit in memory for a while before it's actually reclaimed.
3. When the GC runs: it marks everything as unreachable, then walks live references marking what's actually reachable, then checks the unreachable set for finalizers (calling any it finds), then reclaims the memory.
4. Finalization is **non-deterministic**. You cannot predict exactly when a given object's finalizer will run.

That last point is the whole reason `IDisposable` exists. A file handle, a database connection, or a COM reference held until "whenever the GC gets around to it" is a resource leak in practice, even though the memory is technically managed.

```csharp
public void Dispose()
{
	Dispose(true);
	GC.SuppressFinalize(this);
}

~DisposableClass()
{
	Dispose(false);
}

protected void Dispose(bool releaseManagedObjects)
{
	if (disposed) return;
	// free unmanaged resources here, always
	if (!releaseManagedObjects) return;
	// free managed resources here, only when called from Dispose(), not the finalizer
}
```

The `bool` parameter is the key idea. Called from `Dispose()` (explicit, deterministic cleanup), you free everything, managed and unmanaged. Called from the finalizer (non-deterministic, GC-triggered), you only free unmanaged resources — the managed ones are handled by the GC anyway, and might already be finalized themselves by the time your finalizer runs. Touching an already-finalized managed object from a finalizer is undefined behavior, which is exactly what the flag prevents.

`GC.SuppressFinalize(this)` in `Dispose()` tells the GC "don't bother calling the finalizer, I already did the cleanup it would have done," which avoids redundant work. It also matters for performance: objects with finalizers survive at least one extra GC generation, so suppressing the finalizer lets the object be reclaimed on the first pass.

The `disposed` guard flag isn't decorative either. `Dispose()` must be safe to call more than once, because a `using` block plus an explicit call is a completely normal thing to happen.

`CodeLabDispose()` shows all three disposal paths side by side: `alan.Dispose()` explicitly, `betty` left for the GC to finalize eventually (which is why you won't see her cleanup message during the program's normal run), and `charles` cleaned up implicitly by a `using` block. The `using` form is the one to default to — it disposes even if an exception is thrown, which a manual call at the end of a method does not.

---

## Skipped on Purpose

The book's "Shape Resources" real-world example isn't covered in lecture, though `Ellipse` and `Circle` are included in this project for reference. `Circle : Ellipse` has a constructor that validates width equals height — a nice small example of a derived class adding validation on top of an inherited constructor.

It's also a quiet illustration of the Liskov Substitution Principle problem. A `Circle` that "is-a" `Ellipse` breaks the moment someone sets width and height independently through the base type. Real inheritance hierarchies hit this more often than textbooks admit.

---

## Bonus: Operator Overloading on Car

`Car` only implemented `IComparable` originally. The Bonus Methods region adds `Equals()`, `GetHashCode()`, `==`, `!=`, `<`, `<=`, `>`, and `>=`, all built on top of the existing `CompareTo()` logic, so a plain reference type quietly grows the same natural comparison syntax you'd expect from a built-in numeric type.

```csharp
public override bool Equals(object obj)
{
	return obj is Car other && CompareCars(this, other) == 0;
}

public static bool operator ==(Car left, Car right)
{
	return CompareCars(left, right) == 0;
}

public static bool operator <(Car left, Car right)
{
	return CompareCars(left, right) < 0;
}
```

Three things worth knowing about this addition:

- **`GetHashCode()` is not optional once you override `Equals()`.** Two objects that are `Equal` must return the same hash code, or the type silently breaks the moment it's used as a `Dictionary` key or stored in a `HashSet` — lookups can fail to find an entry that's genuinely there. The implementation here hashes `Name` case-insensitively, matching the case-insensitive comparison `CompareTo` already uses. Note also that operators must be declared in matching pairs: you cannot define `==` without `!=`, or `<` without `>`. The compiler enforces it.

- **Writing these operators surfaced a real bug in `CompareTo(object obj)` itself**, which has since been fixed. The original textbook code checked `!(obj is Car)` without checking for `null` first. Since `null is Car` is always `false`, that branch was already `true` for a `null` argument, and it then dereferenced `obj.GetType().Name` on that same `null` — a `NullReferenceException` instead of a sensible result. The fix follows documented .NET convention: `IComparable.CompareTo` should never throw for a `null` argument, and `null` sorts before any non-null instance, so `CompareTo(null)` now returns `1` instead of crashing. A genuine type mismatch (a non-null object that isn't a `Car`) is still correctly an error and still throws `ArgumentException`; only the missing `null` case was added. `CompareCars()` still guards against a `null` *left-hand* `Car` before calling `CompareTo()` — that's an unavoidable language-level constraint (you can't call an instance method on a `null` reference), not a workaround for the now-fixed bug.

- **This is why `car1 == car2` now prints `True`** in `CodeLabComparingCars()`, where the lesson originally used that exact comparison to demonstrate reference-type `==` returning `false` by default. Both things remain true: a plain class defaults to reference equality, and `Car` specifically no longer does, because of this addition. Worth keeping straight which one you're demonstrating if you reuse this pattern elsewhere.

The broader lesson on operator overloading: it's excellent for types that genuinely behave like values (money, coordinates, measurements) and actively harmful for types that don't. If a reader would have to check your source to know what `<` means for your type, don't define `<` for your type.

---

## Chapter Takeaways

- One base class, unlimited interfaces. Base constructors run before derived ones.
- Interface for "can do," abstract class for "is a."
- `IComparable` is one fixed sort on the type; `IComparer<T>` is many sorts, defined outside it.
- `IEquatable<T>` is what makes `Contains`, `Dictionary`, and `HashSet` behave correctly for your type.
- `Clone()` is shallow unless you deliberately make it deep. `ICloneable` doesn't tell the caller which one it is.
- `IEnumerable`/`IEnumerator` is what `foreach` actually compiles to, and what `yield return` generates for you.
- GC is non-deterministic; `IDisposable` is how you get deterministic cleanup. Prefer `using`.
- Override `Equals` and you must override `GetHashCode`. No exceptions.

---

## Also in Chapter 5

Three supplemental projects accompany this one and are documented separately:

- `CSharp.Ch05.Supplemental.ImplementingClassHierarchies`
- `CSharp.Ch05.Supplemental.ConfigurationClasses`
- `CSharp.Ch05.Supplemental.Cloning`
