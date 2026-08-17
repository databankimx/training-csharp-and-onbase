# Ch05 Textbook Code: IComparer Cars

## What This Is

The most interactive of the `Car`-sorting labs in this chapter: a `ComboBox` labeled "Sort By" with four options (`Name`, `Max MPH`, `Horsepower`, `Price`), and the `ListView` live-resorts every time you pick a different one. No debugger required, just click around.

No bugs found. Same real car data (sourced from thesupercars.org) as `IComparableCars` and the main lesson's `CodeLabComparingCars()`.

---

## Worth Noticing: A Separate Comparer, Not a Method on Car

```csharp
class CarComparer : IComparer<Car>
{
    public enum CompareField { Name, MaxMph, Horsepower, Price }
    public CompareField SortBy = CompareField.Name;

    public int Compare(Car x, Car y)
    {
        switch (SortBy)
        {
            case CompareField.Name: return x.Name.CompareTo(y.Name);
            case CompareField.MaxMph: return x.MaxMph.CompareTo(y.MaxMph);
            case CompareField.Horsepower: return x.Horsepower.CompareTo(y.Horsepower);
            case CompareField.Price: return x.Price.CompareTo(y.Price);
        }
        return x.Name.CompareTo(y.Name);
    }
}
```

Unlike `IComparableCars`, this `Car` class doesn't implement `IComparable<Car>` at all, it can't, since a single fixed `CompareTo()` can only ever sort one way. `IComparer<Car>` lives in a separate class instead, exactly because the sort criterion needs to change at runtime (whatever's selected in the combo box). `DisplayCars()` builds a fresh `CarComparer`, sets `SortBy` from the combo box's current text, and passes it to `Array.Sort(Cars, comparer)`, the overload that takes an external comparer instead of relying on the sorted type's own `CompareTo()`.

---

## Worth Noticing: The Reverse-After-Sort Trick

```csharp
Array.Sort(Cars, comparer);

// If we're not sorting by name, reverse the array.
if (sortByComboBox.Text != "Name") Array.Reverse(Cars);
```

`CarComparer.Compare()` always sorts ascending (lowest number first). That's exactly right for `Name` (alphabetical), but backwards for a "fastest cars" list sorted by `MaxMph`, `Horsepower`, or `Price`, you'd want the biggest numbers first. Rather than writing a second, descending comparer, this flips the whole array with `Array.Reverse()` after the fact. A small, pragmatic trick worth recognizing: reversing a sort is often simpler than writing the sort backwards.
