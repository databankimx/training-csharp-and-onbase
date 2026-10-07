---
title: "Three Ways to Greet by Name"
chapter: 1
index: 5
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

            // args[0] throws IndexOutOfRangeException if no argument was supplied --
            // intentional, so you can watch the catch block fire on the first run
            string name = args[0];

            Console.WriteLine(string.Format("Hello {0}!", name));
            Console.WriteLine("Hello {0}!", name);
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
