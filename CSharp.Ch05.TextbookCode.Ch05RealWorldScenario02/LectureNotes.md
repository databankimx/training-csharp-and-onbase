# Ch05 Textbook Code: Shape Resources, Part 2 (Real-World Scenario 2)

## What This Is

The second half of the "Shape Resources" real-world scenario, this one demonstrates a class implementing both `IDisposable` and `IComparable<Shape>` at once.

```csharp
class Shape : IDisposable, IComparable<Shape>
{
    public Brush FillBrush { get; set; }
    public Pen OutlinePen { get; set; }

    public void Dispose()
    {
        if (IsDisposed) return;
        FillBrush.Dispose();
        OutlinePen.Dispose();
        IsDisposed = true;
    }

    public int CompareTo(Shape other)
    {
        throw new NotImplementedException();
    }
}
```

Unlike `Ch05RealWorldScenario01`, there's nothing to run here. `Form1` is a completely blank window with no logic at all, `Shape.cs` exists purely as a reference class, not a working demo. No bugs found.

---

## Worth Noticing

`FillBrush` and `OutlinePen` are `System.Drawing.Brush`/`Pen`, both of which wrap unmanaged GDI+ resources under the hood. This is a good example of exactly the kind of type `IDisposable` exists for, `Shape.Dispose()` doesn't do anything exotic, it just makes sure the two GDI+ objects it owns get released instead of waiting on the garbage collector to eventually get around to it.

`CompareTo()` is stubbed to throw `NotImplementedException`, the original download declares `IComparable<Shape>` on the class without ever providing a body for `CompareTo()`, which doesn't compile, an interface member has to be implemented by something. The stub exists purely to satisfy the compiler, there's no comparison logic here to study, if you wanted a working example of `IComparable`, `Car` in the main `CSharp.Ch05.ImplementingClassHierarchies` lesson is the one to look at instead.
