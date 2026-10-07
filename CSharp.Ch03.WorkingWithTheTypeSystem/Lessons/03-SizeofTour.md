---
title: "A Tour of sizeof"
chapter: 3
index: 3
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int myInt = 5000;
        Console.WriteLine($"int: {myInt.GetType()}, {sizeof(int)} bytes");

        double myDouble = 5000.0;
        Console.WriteLine($"double: {myDouble.GetType()}, {sizeof(double)} bytes");

        byte myByte = 254;
        Console.WriteLine($"byte: {myByte.GetType()}, {sizeof(byte)} bytes");

        char myChar = 'r';
        Console.WriteLine($"char: {myChar.GetType()}, {sizeof(char)} bytes");

        decimal myDecimal = 20987.89756M;
        Console.WriteLine($"decimal: {myDecimal.GetType()}, {sizeof(decimal)} bytes");

        float myFloat = 254.09F;
        Console.WriteLine($"float: {myFloat.GetType()}, {sizeof(float)} bytes");

        long myLong = 2544567538754;
        Console.WriteLine($"long: {myLong.GetType()}, {sizeof(long)} bytes");

        short myShort = 3276;
        Console.WriteLine($"short: {myShort.GetType()}, {sizeof(short)} bytes");

        bool myBool = true;
        Console.WriteLine($"bool: {myBool.GetType()}, {sizeof(bool)} bytes");
    }
}
```
