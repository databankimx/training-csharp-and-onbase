---
title: "Console Input"
chapter: 1
index: 6
cumulative: true
defaultArgs: "Scott"
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private static void Pause()
    {
        Console.WriteLine("##LESSON_PAUSE##");
        Console.ReadLine(); // blocks until Continue is clicked in the runner
        Console.WriteLine("##LESSON_CLEAR##");
    }

    private static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Hello world!");
            Pause();

            string name = args[0];

            Console.WriteLine(string.Format("Hello {0}!", name));
            Console.WriteLine("Hello {0}!", name);
            Console.WriteLine($"Hello {name}!");
            Pause();

            // ReadLine blocks until the user presses Enter
            Console.WriteLine("Enter your name to continue...");
            name = Console.ReadLine();
            Console.WriteLine($"Hello {name}!");
            Pause();
        }
        catch (Exception ex)
        {
            while (ex != null)
            {
                Console.WriteLine(ex);
                ex = ex.InnerException;
            }
        }
        finally
        {
            if (!Debugger.IsAttached)
            {
                Console.WriteLine("\nDone!");
            }
        }
    }
}
```
