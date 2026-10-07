---
title: "IComparer"
chapter: 5
index: 7
dependencies: []
---

```csharp
using System;
using System.Collections.Generic;

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
}

public class CarComparer : IComparer<Car>
{
    public enum CompareField { Name, MaxMph, Horsepower, Price }
    public CompareField SortBy = CompareField.Name;

    public int Compare(Car x, Car y) => SortBy switch
    {
        CompareField.MaxMph     => x.MaxMph.CompareTo(y.MaxMph),
        CompareField.Horsepower => x.Horsepower.CompareTo(y.Horsepower),
        CompareField.Price      => x.Price.CompareTo(y.Price),
        _                       => string.Compare(x.Name, y.Name, StringComparison.CurrentCultureIgnoreCase)
    };
}

internal static class Program
{
    private static void Main()
    {
        var cars = new[]
        {
            new Car { Make = "Tesla",   Model = "Model S", Year = 2023, MaxMph = 155, Horsepower = 670, Price = 74990m },
            new Car { Make = "Ferrari", Model = "Roma",    Year = 2023, MaxMph = 199, Horsepower = 612, Price = 222000m },
            new Car { Make = "BMW",     Model = "M3",      Year = 2023, MaxMph = 180, Horsepower = 503, Price = 75900m },
            new Car { Make = "Porsche", Model = "911 GT3", Year = 2023, MaxMph = 184, Horsepower = 502, Price = 161100m },
        };

        var comparer = new CarComparer { SortBy = CarComparer.CompareField.Price };
        Array.Sort(cars, comparer);
        foreach (var car in cars)
            Console.WriteLine($"{car.Name,-35} {car.Price:C}");

        Console.WriteLine();

        comparer.SortBy = CarComparer.CompareField.MaxMph;
        Array.Sort(cars, comparer);
        foreach (var car in cars)
            Console.WriteLine($"{car.Name,-35} {car.MaxMph} mph");
    }
}
```
