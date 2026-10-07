---
title: "Assembly - Inspecting Metadata"
chapter: 8
index: 1
dependencies: []
---

```csharp
using System;
using System.Reflection;

internal static class Program
{
    private static void Main()
    {
        // GetExecutingAssembly() -- the assembly this code is running from
        var asm = Assembly.GetExecutingAssembly();

        Console.WriteLine($"FullName:             {asm.FullName}");
        Console.WriteLine($"ImageRuntimeVersion: {asm.ImageRuntimeVersion}");
        Console.WriteLine($"Location:            {asm.Location}");
        Console.WriteLine();

        // GetTypes() -- all types, including internal ones
        // GetExportedTypes() -- public types only
        Console.WriteLine($"GetTypes() count:         {asm.GetTypes().Length}");
        Console.WriteLine($"GetExportedTypes() count: {asm.GetExportedTypes().Length}");
        Console.WriteLine();

        // Referenced assemblies -- everything this assembly depends on
        Console.WriteLine("Referenced assemblies:");
        foreach (var r in asm.GetReferencedAssemblies())
            Console.WriteLine($"  {r.Name}");
        Console.WriteLine();

        // Assembly.Load() by simple name -- finds in GAC and standard probing paths
        var mscorlib = Assembly.Load("mscorlib");
        Console.WriteLine($"mscorlib FullName: {mscorlib.FullName}");
        Console.WriteLine($"mscorlib GAC: {mscorlib.GlobalAssemblyCache}");
        Console.WriteLine();

        // typeof() vs instance.GetType() -- the key distinction
        // typeof(T) is compile-time: reflects the declared type
        // instance.GetType() is runtime: reflects the actual type
        object o = 42;
        Console.WriteLine($"typeof(int).Name:    {typeof(int).Name}");
        Console.WriteLine($"(42).GetType().Name: {o.GetType().Name}");
        Console.WriteLine();
        Console.WriteLine("Note: 'int' is a C# alias -- the CLR only knows System.Int32.");
    }
}
```
