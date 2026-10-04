---
title: "Value vs Reference - Side by Side"
chapter: 3
index: 17
dependencies: []
---

```csharp
using System;

public struct ValueCoordinates
{
    public int X;
    public int Y;

    public ValueCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class ReferenceCoordinates
{
    public int X { get; set; }
    public int Y { get; set; }

    public ReferenceCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
}

internal static class Program
{
    // Passing a struct by value -- changes don't escape the method
    private static void MoveXAxis(ValueCoordinates coords, int distance = 1)
    {
        coords.X += distance;
    }

    // Passing a struct by ref -- changes DO escape the method
    private static void MoveXAxis(ref ValueCoordinates coords, int distance = 1)
    {
        coords.X += distance;
    }

    // Passing a class -- always reference semantics, changes escape the method
    private static void MoveXAxis(ReferenceCoordinates coords, int distance = 1)
    {
        coords.X += distance;
    }

    private static void Main()
    {
        var valueCoords = new ValueCoordinates(0, 0);
        MoveXAxis(valueCoords);
        Console.WriteLine($"{valueCoords.X},{valueCoords.Y}"); // 0,0 -- unchanged

        MoveXAxis(ref valueCoords);
        Console.WriteLine($"{valueCoords.X},{valueCoords.Y}"); // 1,0 -- changed

        var refCoords = new ReferenceCoordinates(0, 0);
        MoveXAxis(refCoords);
        Console.WriteLine($"{refCoords.X},{refCoords.Y}"); // 1,0 -- changed (reference semantics)
    }
}
```
