# Ch03 Textbook Code: Using Enums

## What This Is

A small standalone lab: define a `Months` enum, then use `Enum.GetName()` to look up a name from a value, and `Enum.GetValues()` to loop over every underlying value in the enum.

No functional bugs, arrived clean.

---

## Worth Noticing

```csharp
private enum Months
{
    Jan = 1, Feb, Mar, Apr, May, Jun, Jul, Aug, Sept,
    Oct, Nov, Dec
}
```

Only `Jan` gets an explicit value (`1`). Every member after it, `Feb` through `Dec`, picks up the next integer automatically, so `Feb` is `2`, `Mar` is `3`, and so on through `Dec` at `12`. That's why `Enum.GetName(typeof(Months), 8)` returns `Aug`, the 8th position lines up with the 8th month because `Jan` started the count at 1 instead of the default 0.

This enum also uses `Sept` for September rather than the three-letter abbreviation the rest of the months use. Not a bug, `Sep` would collide visually with nothing here, it's just an inconsistent abbreviation choice in the original download. Worth a glance if you're comparing this enum against the one in `CSharp.Ch03.WorkingWithTheTypeSystem`, which spells it `Sep` for consistency with the other three-letter names.
