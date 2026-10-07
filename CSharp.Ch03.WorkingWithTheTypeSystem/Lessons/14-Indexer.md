---
title: "An Indexer"
chapter: 3
index: 14
dependencies: []
---

```csharp
using System;

public class IpAddress
{
    private readonly int[] ip = new int[32];

    public int this[int index]
    {
        get => ip[index];
        set
        {
            if (value == 0 || value == 1) ip[index] = value;
            else throw new ArgumentException("Invalid value, must be 0 or 1", nameof(value));
        }
    }
}

internal static class Program
{
    private static void Main()
    {
        var myIp = new IpAddress();
        for (int i = 0; i < 32; i++)
        {
            myIp[i] = 0;
            Console.Write($"{myIp[i]} ");
        }
        Console.WriteLine();
    }
}
```
