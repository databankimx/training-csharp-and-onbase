---
title: "Pause Method"
chapter: 1
index: 4
cumulative: true
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
