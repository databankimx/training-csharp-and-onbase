---
title: "Operator Overloading on Car"
chapter: 5
index: 12
dependencies: []
---

```csharp
using System;

public class Car : IComparable
{
    public string  Make       { get; set; }
    public string  Model      { get; set; }
    public int     Year       { get; set; }
    public int     Horsepower { get; set; }
    public int     MaxMph     { get; set; }
    public decimal Price      { get; set; }
    public string  Name       => $"{Year} {Make} {Model}";

    public int CompareTo(object obj)
    {
        if (obj is null) return 1;
        if (!(obj is Car))
            throw new ArgumentException($"Cannot compare Car to [{obj.GetType().Name}]");
        return string.Compare(Name, ((Car)obj).Name, StringComparison.CurrentCultureIgnoreCase);
    }

    public override bool Equals(object obj) =>
        obj is Car other && CompareCars(this, other) == 0;

    public override int GetHashCode() =>
        Name?.ToUpperInvariant().GetHashCode() ?? 0;

    public static bool operator ==(Car left, Car right)  => CompareCars(left, right) == 0;
    public static bool operator !=(Car left, Car right)  => CompareCars(left, right) != 0;
    public static bool operator  <(Car left, Car right)  => CompareCars(left, right) < 0;
    public static bool operator <=(Car left, Car right)  => CompareCars(left, right) <= 0;
    public static bool operator  >(Car left, Car right)  => CompareCars(left, right) > 0;
    public static bool operator >=(Car left, Car right)  => CompareCars(left, right) >= 0;

    private static int CompareCars(Car left, Car right)
    {
        if (ReferenceEquals(left, right)) return 0;
        if (left is null) return -1;
        return left.CompareTo(right);
    }
}

internal static class Program
{
    private static void Main()
    {
        var car1 = new Car { Make = "BMW", Model = "M3", Year = 2023, MaxMph = 180, Horsepower = 503, Price = 75900m };
        var car2 = new Car { Make = "BMW", Model = "M3", Year = 2023, MaxMph = 180, Horsepower = 503, Price = 75900m };

        Console.WriteLine(car1 == car2);  // True
        Console.WriteLine(car1 != car2);  // False
        Console.WriteLine(car1 < car2);   // False -- same name, same position
    }
}
```
