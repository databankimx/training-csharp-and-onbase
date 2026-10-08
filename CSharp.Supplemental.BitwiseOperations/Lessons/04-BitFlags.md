---
title: "Bit-Flags with AND"
chapter: 0
index: 4
dependencies: []
---

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        byte licenses = (byte)ProductLicenses.Work;
        Console.WriteLine($"License value: {licenses} ({ProductLicenses.Work})");
        Console.WriteLine("Included features:");

        foreach (ProductLicenses flag in Enum.GetValues(typeof(ProductLicenses)))
        {
            if (flag == ProductLicenses.None) continue;
            // AND the license value against each flag - if the result equals the flag,
            // that bit is set and the feature is included.
            if ((licenses & (byte)flag) == (byte)flag)
                Console.WriteLine($"  {flag} = {(byte)flag}");
        }

        Console.WriteLine();
        Console.WriteLine($"Has WordProcessing: {HasFlag(licenses, (byte)ProductLicenses.WordProcessing)}");
        Console.WriteLine($"Has Publishing:     {HasFlag(licenses, (byte)ProductLicenses.Publishing)}");
    }

    private static bool HasFlag(byte value, byte flag) => (value & flag) == flag;
}

[Flags]
internal enum ProductLicenses : byte
{
    None              = 0b0000_0000,
    WordProcessing    = 0b0000_0001,
    Spreadsheets      = 0b0000_0010,
    Presentations     = 0b0000_0100,
    EmailClient       = 0b0000_1000,
    Notebook          = 0b0001_0000,
    Collaboration     = 0b0010_0000,
    ProjectManagement = 0b0100_0000,
    Publishing        = 0b1000_0000,
    Personal          = WordProcessing | EmailClient,
    Work              = Personal | Spreadsheets | Presentations | Notebook | Collaboration,
    UltraDeluxe       = Work | ProjectManagement | Publishing,
}
```
