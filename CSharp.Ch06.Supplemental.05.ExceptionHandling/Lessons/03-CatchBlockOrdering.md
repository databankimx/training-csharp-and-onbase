---
title: "Catch Block Ordering - Most Specific First"
chapter: 6
index: 3
dependencies: []
---

```csharp
using System;
using System.IO;

internal static class Program
{
    private static void Main()
    {
        // Catch blocks are checked top to bottom -- first match wins.
        // Most specific type must come before less specific types.
        // The compiler prevents the obvious case (base before derived), but not every ordering mistake.
        try
        {
            File.Open(@"C:\InvalidDirectory\InvalidFile.txt", FileMode.Append);
        }
        catch (DirectoryNotFoundException)
        {
            // Fires here -- the directory doesn't exist, so .NET never checks for the file
            Console.WriteLine("Caught: DirectoryNotFoundException");
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Caught: FileNotFoundException");
        }
        catch (IOException)
        {
            Console.WriteLine("Caught: IOException (base of both above)");
        }
        catch (Exception)
        {
            // If this were first, every exception would match it and nothing below would run
            Console.WriteLine("Caught: Exception");
        }

        Console.WriteLine("\nRule: catch the narrowest type you can actually respond to.");
        Console.WriteLine("A catch block you can't meaningfully handle is usually better left unwritten.");
    }
}
```
