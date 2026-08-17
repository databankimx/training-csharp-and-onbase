# Ch05 Textbook Code: IComparable Cars

## What This Is

An interactive WinForms lab, no debugger required: a `Car` class implementing generic `IComparable<Car>` by `Name`, displayed via two side-by-side `ListView` controls, unsorted on the left, `Array.Sort()`'d on the right, all four columns (`Name`, `Max MPH`, `Horsepower`, `Price`) visible at once.

No bugs found. This is the same real car data (sourced from thesupercars.org) used in `CodeLabComparingCars()` in `CSharp.Ch05.ImplementingClassHierarchies`, this is likely where that data originally came from.

---

## Worth Noticing: Two Versions of CompareTo, Side by Side

```csharp
class Car : IComparable<Car>
{
    // Non-generic version.
    // Compare Cars alphabetically by Name.
    //public int CompareTo(object obj)
    //{
    //    if (!(obj is Car))
    //        throw new ArgumentException("Object is not a Car");
    //
    //    Car other = obj as Car;
    //    return Name.CompareTo(other.Name);
    //}

    // Generic version.
    // Compare Cars alphabetically by Name.
    public int CompareTo(Car other)
    {
        return this.Name.CompareTo(other.Name);
    }
}
```

The commented-out block is the non-generic `IComparable.CompareTo(object)` shape, kept right next to the active generic `IComparable<Car>.CompareTo(Car)` version. Worth reading both side by side: the non-generic version needs a runtime type check (`is Car`) and a cast before it can do anything, since `object` could be literally anything. The generic version skips both, the compiler already guarantees `other` is a `Car`. That's the concrete payoff of using the generic interface, less defensive code, and a type mismatch becomes a compile error instead of something you find out about at runtime.
