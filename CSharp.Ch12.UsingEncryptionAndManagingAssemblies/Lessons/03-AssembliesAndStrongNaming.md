---
title: "Assembly Versions, Strong Naming, and the GAC"
chapter: 12
index: 3
dependencies: []
---

```csharp
using System;
using System.Reflection;

internal static class Program
{
    private static void Main()
    {
        // --- Assembly version ---
        // Comes from [assembly: AssemblyVersion(...)] in AssemblyInfo.cs.
        // The four-part version (Major.Minor.Build.Revision) is a .NET convention.
        // .NET doesn't enforce semantic versioning within it -- that's a team choice.
        var current = Assembly.GetExecutingAssembly();
        Console.WriteLine("=== Assembly Version ===");
        Console.WriteLine($"Name:      {current.GetName().Name}");
        Console.WriteLine($"Version:   {current.GetName().Version}");
        Console.WriteLine($"Full name: {current.FullName}");

        // --- Strong naming: side-by-side, this assembly vs. mscorlib ---
        // mscorlib is guaranteed to be strong-named on any .NET machine.
        // Its full name has four parts: Name, Version, Culture, PublicKeyToken.
        // The public key token is the last 8 bytes of the SHA-1 hash of the
        // signing public key. Microsoft's framework assemblies all share the
        // same token: b77a5c561934e089 -- that consistency lets the runtime
        // verify an assembly claiming to be from Microsoft actually is.
        var mscorlib = typeof(object).Assembly;
        Console.WriteLine("\n=== Strong Naming ===");
        Console.WriteLine($"This assembly:  {current.FullName}");
        Console.WriteLine($"  Token: {FormatToken(current.GetName().GetPublicKeyToken())}");
        Console.WriteLine($"mscorlib:       {mscorlib.FullName}");
        Console.WriteLine($"  Token: {FormatToken(mscorlib.GetName().GetPublicKeyToken())}");
        Console.WriteLine("Strong name = Name + Version + Culture + PublicKeyToken, all four together.");

        // --- GAC ---
        // GlobalAssemblyCache reflects which load path the runtime actually used.
        // Only strong-named assemblies can be in the GAC -- the runtime needs a
        // verifiable identity to safely share one assembly across applications.
        Console.WriteLine("\n=== Global Assembly Cache ===");
        Console.WriteLine($"This assembly loaded from GAC: {current.GlobalAssemblyCache}");
        Console.WriteLine($"mscorlib loaded from GAC:      {mscorlib.GlobalAssemblyCache}");
        Console.WriteLine("Application assemblies load from their own output folder (false).");
        Console.WriteLine("Framework assemblies are machine-wide shared (true).");

        // --- Binding redirects ---
        Console.WriteLine("\n=== Binding Redirects ===");
        Console.WriteLine("A <bindingRedirect> tells the runtime: \"when something asks for version X");
        Console.WriteLine("of this strong-named assembly, load version Y instead\" -- no recompile needed.");
        Console.WriteLine("This only works because the assembly is strong-named: a redirect targets a");
        Console.WriteLine("version number that's part of the assembly's full identity.");
        Console.WriteLine("NuGet writes these automatically when resolving dependency version conflicts.");
    }

    private static string FormatToken(byte[] token)
        => (token == null || token.Length == 0)
            ? "(none -- not strong-named)"
            : BitConverter.ToString(token).Replace("-", "").ToLowerInvariant();
}
```
