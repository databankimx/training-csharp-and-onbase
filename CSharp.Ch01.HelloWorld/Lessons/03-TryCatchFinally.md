---
title: "try/catch/finally"
chapter: 1
index: 3
cumulative: true
dependencies: []
---

```csharp
using System;
using System.Diagnostics;

internal static class Program
{
    private static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("Hello world!");
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
                Console.WriteLine("\nDone!\n\nPress any key to exit!");
                Console.ReadKey();
            }
        }
    }
}
```
