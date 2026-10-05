---
title: "IDisposable - Deterministic Cleanup"
chapter: 5
index: 11
dependencies: []
---

```csharp
using System;

public class DisposableClass : IDisposable
{
    public string Name { get; set; } = "";
    private bool _resourcesAreFreed;

    public void Dispose() => FreeResources(true);

    ~DisposableClass() => FreeResources(false);

    private void FreeResources(bool freeManagedResources)
    {
        if (_resourcesAreFreed) return;

        Console.WriteLine($"{Name}: FreeResources");
        GC.SuppressFinalize(this);
        _resourcesAreFreed = true;

        Console.WriteLine($"{Name}: Dispose of unmanaged resources");

        if (!freeManagedResources) return;

        Console.WriteLine($"{Name}: Dispose of managed resources");
    }
}

internal static class Program
{
    private static void Main()
    {
        // Path 1: explicit Dispose()
        var alan = new DisposableClass { Name = "Alan" };
        alan.Dispose();
        alan.Dispose(); // safe to call twice

        // Path 2: left for the GC (Betty's messages appear after program exit, if at all)
        var betty = new DisposableClass { Name = "Betty" };

        // Path 3: using block -- disposes even if an exception is thrown
        using (var charles = new DisposableClass { Name = "Charles" })
        {
            Console.WriteLine($"{charles.Name}: inside using block");
        }

        Console.WriteLine("End of program");
    }
}
```
