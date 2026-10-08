---
title: "Strong Naming and the GAC - Real Assemblies, Side-By-Side, Binding Redirects"
chapter: 12
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
        // Rather than generating a synthetic example, this step inspects real,
        // already-strong-named assemblies guaranteed to be present on any .NET machine:
        // the framework's own core assemblies. Every comparison is against genuine data.

        var thisAssembly = Assembly.GetExecutingAssembly();
        var mscorlib     = typeof(object).Assembly;      // guaranteed strong-named
        var systemLinq   = typeof(System.Linq.Enumerable).Assembly;

        // --- Strong-named vs. not strong-named, side by side ---
        // A strong name's full identity: Name + Version + Culture + PublicKeyToken.
        // The public key token is the last 8 bytes of the SHA-1 hash of the signing
        // public key. Microsoft's framework assemblies all share the same token:
        // b77a5c561934e089 -- consistent across every Windows machine, letting the
        // runtime verify an assembly claiming to be from Microsoft actually is.
        Console.WriteLine("=== Strong-Named vs. Not Strong-Named ===");
        Console.WriteLine($"This assembly:  {thisAssembly.FullName}");
        Console.WriteLine($"  Token: {FormatToken(thisAssembly.GetName().GetPublicKeyToken())}");
        Console.WriteLine($"mscorlib:       {mscorlib.FullName}");
        Console.WriteLine($"  Token: {FormatToken(mscorlib.GetName().GetPublicKeyToken())}");
        Console.WriteLine($"System.Linq:    {systemLinq.FullName}");
        Console.WriteLine($"  Token: {FormatToken(systemLinq.GetName().GetPublicKeyToken())}");

        // --- GlobalAssemblyCache ---
        // Reflects which load path the runtime actually used -- not just a flag.
        // This is also why the GAC requires strong naming: without a verified identity
        // the runtime can't safely share one assembly across multiple applications.
        Console.WriteLine("\n=== GlobalAssemblyCache ===");
        Console.WriteLine($"This assembly from GAC: {thisAssembly.GlobalAssemblyCache}  <-- application assembly, own output folder");
        Console.WriteLine($"mscorlib from GAC:      {mscorlib.GlobalAssemblyCache}  <-- machine-wide shared");
        Console.WriteLine($"System.Linq from GAC:   {systemLinq.GlobalAssemblyCache}");

        // --- Side-by-side versioning ---
        // Two different versions of the same-named assembly can coexist in the GAC
        // because their FULL identities differ -- the version is part of the identity.
        // EntityFramework intentionally freezes its assembly version at 6.0.0.0 across
        // all EF6.x NuGet releases to avoid binding redirect noise in consuming projects.
        // Newtonsoft.Json versions its assembly to match its package -- historically
        // generating binding redirects across countless projects whenever it updates.
        Console.WriteLine("\n=== Side-By-Side Versioning ===");
        Console.WriteLine("Two versions of the same-named assembly coexist in the GAC because their");
        Console.WriteLine("FULL identities differ -- version is part of the identity.");
        Console.WriteLine("EntityFramework 6.x: assembly version frozen at 6.0.0.0 (deliberate).");
        Console.WriteLine("  Consuming projects rarely need binding redirects.");
        Console.WriteLine("Newtonsoft.Json: assembly version matches package version.");
        Console.WriteLine("  Consuming projects frequently see binding redirect conflicts.");
        Console.WriteLine("Both are valid design choices with real tradeoffs.");

        // --- Binding redirects ---
        // Only works because the assembly is strong-named -- a redirect targets a
        // version number that's baked into the full identity. A plain DLL filename has
        // no such version for a redirect to target.
        // NuGet writes these automatically when resolving dependency version conflicts.
        Console.WriteLine("\n=== Binding Redirects ===");
        Console.WriteLine("<bindingRedirect>: \"when something asks for version X, load version Y.\"");
        Console.WriteLine("No recompile needed -- runtime resolves at load time.");
        Console.WriteLine("Only applicable to strong-named assemblies.");
        Console.WriteLine("See CSharp.Ch09.TextbookCode.NorthwindsWCFDataService's Web.config for a");
        Console.WriteLine("real example in this training set.");
    }

    private static string FormatToken(byte[] token)
        => (token == null || token.Length == 0)
            ? "(none -- not strong-named)"
            : BitConverter.ToString(token).Replace("-", "").ToLowerInvariant();
}
```
