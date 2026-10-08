---
title: "Bitwise OR"
chapter: 0
index: 5
dependencies: []
---

```csharp
using System;
using System.IO;

internal static class Program
{
    private static void Main()
    {
        int a = 0b10011100; // 156
        int b = 0b00110100; //  52
        Console.WriteLine($"  156 | 52 = {a | b}");
        Console.WriteLine($"  Binary:  10011100");
        Console.WriteLine($"         | 00110100");
        Console.WriteLine($"         = 10111100  (188)");
        Console.WriteLine();
        Console.WriteLine("OR outputs 1 where EITHER input is 1.");
        Console.WriteLine();

        // Practical example: combining FileAccess flags
        bool needToWrite  = true;
        FileAccess perms  = FileAccess.Read;
        Console.WriteLine($"Initial permissions: {perms} = {(int)perms}");
        if (needToWrite) perms |= FileAccess.Write;
        Console.WriteLine($"After adding Write:  {perms} = {(int)perms}");
        Console.WriteLine();
        Console.WriteLine("Common uses: combining flags, setting specific bits, building permission masks.");
    }
}
```
