# Chapter 5 - Implementing Class Hierarchies (Part 2: IEnumerable, IDisposable, and Operator Overloading)

Continuing from [Part 1](Lesson.md), which covered the hierarchy itself, constructor chaining, `IComparable`, `IComparer`, `IEquatable`, and `ICloneable`. This half covers the two most mechanically involved interfaces, plus operator overloading as a bonus.

---

### Mini-Program 10: IEnumerable -- What foreach Actually Is

`foreach` isn't magic. It's a compile-time transformation that calls `GetEnumerator()` on a collection and then loops on `MoveNext()` / `Current`. Any type that implements `IEnumerable<T>` gets `foreach` support, no matter what it actually stores underneath.

Build the two classes alongside `Main()`:

```csharp
public class TreeNode : IEnumerable<TreeNode>
{
    public int Depth { get; set; }
    public string Text { get; set; }
    public List<TreeNode> Children { get; set; } = new List<TreeNode>();

    public TreeNode(string text) { Text = text; }

    public TreeNode AddChild(string text)
    {
        var child = new TreeNode(text) { Depth = Depth + 1 };
        Children.Add(child);
        return child;
    }

    public List<TreeNode> Preorder()
    {
        var nodes = new List<TreeNode>();
        TraversePreorder(nodes);
        return nodes;
    }

    private void TraversePreorder(List<TreeNode> nodes)
    {
        nodes.Add(this);
        foreach (var child in Children) child.TraversePreorder(nodes);
    }

    public IEnumerator<TreeNode> GetEnumerator() => new TreeEnumerator(this);
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new TreeEnumerator(this);
}

public class TreeEnumerator : IEnumerator<TreeNode>
{
    private List<TreeNode> nodes;
    private int currentIndex;

    public TreeNode Current => GetCurrent();
    object System.Collections.IEnumerator.Current => GetCurrent();

    public bool MoveNext() { currentIndex++; return currentIndex < nodes.Count; }
    public void Reset() { currentIndex = -1; }

    public TreeEnumerator(TreeNode root) { nodes = root.Preorder(); Reset(); }

    private TreeNode GetCurrent()
    {
        if (currentIndex < 0 || currentIndex >= nodes.Count)
            throw new InvalidOperationException("Node index out of range!");
        return nodes[currentIndex];
    }

    ~TreeEnumerator() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool releaseManagedObjects)
    {
        if (!releaseManagedObjects) return;
        nodes = null;
    }
}
```

```csharp
var ceo = new TreeNode("CEO");
var vp1 = ceo.AddChild("VP of Engineering");
var vp2 = ceo.AddChild("VP of Marketing");
vp1.AddChild("Senior Engineer");
vp1.AddChild("Engineer");
vp2.AddChild("Marketing Manager");

// foreach works because TreeNode implements IEnumerable<TreeNode>
foreach (var node in ceo)
    Console.WriteLine(new string(' ', node.Depth * 2) + node.Text);

Console.WriteLine();

// The same iteration written out explicitly as what foreach compiles to:
using var enumerator = ceo.GetEnumerator();
while (enumerator.MoveNext())
    Console.WriteLine(new string(' ', enumerator.Current.Depth * 2) + enumerator.Current.Text);
```

Run it. Both blocks print the same org chart, indented by depth.

The two output blocks are literally the same iteration -- the first is `foreach` written as you'd normally write it; the second is what the compiler generates from that `foreach`. Being able to read the desugared form is useful the moment you're debugging something that "should work in foreach but doesn't," because now you know exactly where to look.

`Preorder()` flattens the whole tree into a `List<TreeNode>` up front (parent before children, depth-first), and `TreeEnumerator` just walks that flat list with an index. The tree structure (`Children`, `AddChild`) doesn't know anything about enumeration -- that's entirely `TreeEnumerator`'s concern. Clean separation.

One thing worth noting: `IEnumerator<T>` inherits `IDisposable`, which is why `TreeEnumerator` implements `Dispose()` even though this isn't the chapter's IDisposable example. Implementing one standard interface sometimes pulls in the obligations of another. The `using var enumerator` in the second block handles that disposal automatically.

Worth knowing for later: C#'s `yield return` generates the entire `TreeEnumerator` class for you at compile time -- all of `MoveNext()`, `Current`, `Reset()`, and the state machine that makes it work. Writing `TreeEnumerator` by hand is the point of the exercise, so you know what `yield` is doing for you, rather than treating it as unexplained magic.

### Mini-Program 11: IDisposable -- Deterministic Cleanup

The GC is non-deterministic: an eligible object can sit in memory for a while before it's actually collected. A file handle or database connection that's "eventually cleaned up when the GC gets around to it" is a resource leak in practice, even though nothing technically leaked. `IDisposable` is how you get cleanup on your own schedule instead.

```csharp
public class DisposableClass : IDisposable
{
    public string Name { get; set; } = "";
    private bool resourcesAreFreed;

    public void Dispose() => FreeResources(true);

    ~DisposableClass() => FreeResources(false);

    private void FreeResources(bool freeManagedResources)
    {
        if (resourcesAreFreed) return;

        Console.WriteLine($"{Name}: FreeResources");
        GC.SuppressFinalize(this);
        resourcesAreFreed = true;

        Console.WriteLine($"{Name}: Dispose of unmanaged resources");
        // Unmanaged resources would be freed here regardless of which path we came in on

        if (!freeManagedResources) return;

        Console.WriteLine($"{Name}: Dispose of managed resources");
        // Managed resources only freed when called from Dispose(), not the finalizer
    }
}
```

```csharp
// Path 1: explicit Dispose()
var alan = new DisposableClass { Name = "Alan" };
alan.Dispose();
alan.Dispose(); // safe to call twice - resourcesAreFreed guards against it

// Path 2: left for the GC to finalize (you won't see this message during the program's run)
var betty = new DisposableClass { Name = "Betty" };

// Path 3: using block -- disposes even if an exception is thrown
using (var charles = new DisposableClass { Name = "Charles" })
{
    Console.WriteLine($"{charles.Name}: inside using block");
}

Console.WriteLine("End of program");
```

Run it. You'll see Alan's cleanup messages, then Charles's, then "End of program". Betty's messages appear after the program exits (if at all) -- that's the finalizer running non-deterministically, which is exactly the point the demo exists to make.

The `bool freeManagedResources` parameter is the key idea. Called from `Dispose()` -- explicit, deterministic -- you free everything, managed and unmanaged. Called from the finalizer -- non-deterministic, GC-triggered -- you only free unmanaged resources. The managed ones are handled by the GC anyway, and they might already be finalized themselves by the time your finalizer runs. Touching an already-finalized managed object from a finalizer is undefined behavior, which is exactly what the parameter guards against.

`GC.SuppressFinalize(this)` inside `FreeResources` tells the GC: "don't run the finalizer for this one, the cleanup's already done." Objects with finalizers survive at least one extra GC generation even after they become unreachable, so suppressing it when cleanup is already done is a real performance improvement, not ceremony.

The `resourcesAreFreed` guard makes `Dispose()` safe to call more than once. It must be safe to call more than once -- a `using` block plus an explicit call is a completely normal pattern, and a `Dispose()` that throws or double-frees on a second call would be a nasty surprise.

Three paths, three behaviors. The `using` form is the one to default to in real code: it disposes even when an exception is thrown in the middle of the block, which a manual call at the end of a method does not.

---

## Bonus: Operator Overloading on Car

`Car` from Part 1 only implemented `IComparable`. Adding equality and comparison operators on top of that gives the type the same natural syntax you'd expect from a built-in numeric type.

This fits cleanly with the hierarchy lesson because it builds entirely on what `IComparable.CompareTo` already provides -- you're not adding new comparison logic, just exposing it through additional operators. Open `Car.cs` in the project and read the Bonus Methods and Bonus Operator Overloads regions alongside this section:

```csharp
public override bool Equals(object obj)
{
    return obj is Car other && CompareCars(this, other) == 0;
}

public override int GetHashCode()
{
    return Name?.ToUpperInvariant().GetHashCode() ?? 0;
}

public static bool operator ==(Car left, Car right) => CompareCars(left, right) == 0;
public static bool operator !=(Car left, Car right) => CompareCars(left, right) != 0;
public static bool operator  <(Car left, Car right) => CompareCars(left, right) < 0;
public static bool operator <=(Car left, Car right) => CompareCars(left, right) <= 0;
public static bool operator  >(Car left, Car right) => CompareCars(left, right) > 0;
public static bool operator >=(Car left, Car right) => CompareCars(left, right) >= 0;

// Null-safe comparison: a null Car sorts before any non-null instance.
// Can't call an instance method on a null reference, so this helper is needed.
private static int CompareCars(Car left, Car right)
{
    if (ReferenceEquals(left, right)) return 0;
    if (left is null) return -1;
    return left.CompareTo(right);
}
```

```csharp
var car1 = new Car { Make = "BMW", Model = "M3", Year = 2023, MaxMph = 180, Horsepower = 503, Price = 75900m };
var car2 = new Car { Make = "BMW", Model = "M3", Year = 2023, MaxMph = 180, Horsepower = 503, Price = 75900m };

Console.WriteLine(car1 == car2);     // True
Console.WriteLine(car1 != car2);     // False
Console.WriteLine(car1 < car2);      // False -- same name, same position
```

Run it. `True`, `False`, `False`.

Three things worth internalizing from this pattern:

**`GetHashCode()` is not optional once you override `Equals()`.** Two objects that are `Equals` must always return the same hash code, or the type silently breaks as a `Dictionary` key or `HashSet` member. The implementation here hashes `Name` case-insensitively, matching the case-insensitive comparison `CompareTo` already uses.

**Operators must be defined in matching pairs.** You cannot define `==` without `!=`, or `<` without `>`. The compiler enforces this -- try defining only `==` and it will tell you exactly what's missing.

**Operator overloading is excellent for types that genuinely behave like values** -- money amounts, coordinates, measurements -- and actively harmful for types that don't. If a reader would have to check your source code to know what `<` means for your type, don't define `<` for your type.

---

## Skipped on Purpose

The book's "Shape Resources" real-world example isn't covered in lecture, though `Ellipse` and `Circle` are included in the project for reference. `Circle : Ellipse`, with a constructor that validates width equals height, is a compact example of a derived class adding validation on top of an inherited constructor. It's also a quiet illustration of the Liskov Substitution Principle problem: a `Circle` that IS-A `Ellipse` breaks the moment someone sets width and height independently through the base type. Real inheritance hierarchies hit this more than textbooks admit.

---

## Chapter Takeaways

- One base class, unlimited interfaces. `: base(...)` calls the parent constructor; `: this(...)` calls another in the same class. Base constructors run before derived ones, always.
- Interface for "can do"; abstract class for "is a kind of." When you're unsure, say the sentence out loud.
- `IComparable` is one fixed sort on the type; `IComparer<T>` is many sorts, defined outside it.
- `IEquatable<T>` makes `Contains`, `Dictionary`, `HashSet` work correctly. Once you override `Equals`, you must override `GetHashCode` -- no exceptions.
- `Clone()` is shallow unless you deliberately make it deep. `ICloneable` doesn't tell callers which one they're getting.
- `IEnumerable`/`IEnumerator` is what `foreach` compiles to. `yield return` writes `TreeEnumerator` for you.
- GC is non-deterministic. `IDisposable` is how you get deterministic cleanup. Prefer `using`.
- Override `Equals` and you must override `GetHashCode`. This bears repeating.

---

## Also in Chapter 5

Three supplemental projects accompany this one, each documented separately in its own `Lesson.md`:

- `CSharp.Ch05.Supplemental.ImplementingClassHierarchies` -- a plain, ordinary class hierarchy for an address book contact. No exam material, just what the unglamorous version looks like.
- `CSharp.Ch05.Supplemental.Cloning` -- shallow vs. deep cloning slowed way down, with `ReferenceEquals` making the difference observable at each step.
- `CSharp.Ch05.Supplemental.ConfigurationClasses` -- custom `ConfigurationSection`/`ConfigurationElement`/`ConfigurationElementCollection` classes for OnBase settings. The most directly job-applicable project in Chapter 5.
