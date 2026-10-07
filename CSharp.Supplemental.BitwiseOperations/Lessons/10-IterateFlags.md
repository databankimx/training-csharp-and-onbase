---
title: "Iterating a Bit-Flag with Shifts"
chapter: 0
index: 10
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
#pragma warning disable S2245 // Not a security-sensitive use of randomness
        int settings = new Random().Next(0, 15);
#pragma warning restore S2245

        Console.WriteLine($"Settings value: {settings}");
        Console.WriteLine("Set flags:");

        int current = 1;
        int copy    = settings;
        while (copy > 0)
        {
            if ((copy & 1) == 1)
                Console.WriteLine($"  {Enum.GetName(typeof(Settings), current)}");
            copy    >>= 1;
            current <<= 1;
        }

        Console.WriteLine();
        Console.WriteLine("Technique: check the lowest bit with (value & 1), then shift right");
        Console.WriteLine("to expose the next bit. Track the flag value by shifting left in parallel.");
    }
}

[Flags]
internal enum Settings
{
    DebugMode       = 1,
    InteractiveMode = 2,
    RememberSettings = 4,
    Optimize        = 8,
}
```
