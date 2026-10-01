# Chapter 12 Supplemental 04: Strong Naming and the GAC Deep Dive

## What This Is

The main lesson's `UnderstandingStrongNaming()` found that this project's own assembly isn't strong-named -- expected, since application projects rarely need to be. Rather than building a synthetic throwaway example (strong naming is a build-time tooling concern, not something meaningful to generate on the fly at runtime), this project instead inspects real, already-strong-named assemblies that are guaranteed to be present on any .NET Framework machine: the framework's own core assemblies. Every comparison is against genuine, verifiable data.

---

## How to Write This Program

Add a display helper to `Program.cs`:

```csharp
private static void PrintPublicKeyToken(Assembly assembly)
{
    byte[] token = assembly.GetName().GetPublicKeyToken();
    string display = (token == null || token.Length == 0)
        ? "(none -- not strong-named)"
        : BitConverter.ToString(token).Replace("-", "").ToLowerInvariant();
    Console.WriteLine($"  Public key token: {display}");
}
```

### Mini-Program 1: Strong-Named vs. Not Strong-Named, Side by Side

Clear `Main()` and write:

```csharp
Assembly thisAssembly = Assembly.GetExecutingAssembly();
Assembly mscorlib     = typeof(object).Assembly;

Console.WriteLine("THIS project's assembly:");
Console.WriteLine($"  Full name: {thisAssembly.FullName}");
PrintPublicKeyToken(thisAssembly);

Console.WriteLine("\nmscorlib (the core .NET Framework assembly, holds System.Object, System.String, etc.):");
Console.WriteLine($"  Full name: {mscorlib.FullName}");
PrintPublicKeyToken(mscorlib);

Console.WriteLine("\nmscorlib's full name has four parts: Name, Version, Culture, PublicKeyToken.");
Console.WriteLine("This project's has an empty PublicKeyToken.");
Console.WriteLine("That difference IS what \"strong-named\" means in practice: a full,");
Console.WriteLine("verifiable identity versus just a simple name.");

GenericFunctions.Pause();
```

Run it. Read mscorlib's full name carefully. The public key token -- the last eight bytes of the SHA-1 hash of the public key used to sign the assembly -- is what makes its identity globally unique rather than just "whatever dll happens to be named mscorlib."

Microsoft's framework assemblies all share the same public key token: `b77a5c561934e089`. That consistency is deliberate -- it lets the runtime verify that `System.Data` claiming to be from Microsoft actually is, not just that a file named `System.Data.dll` exists somewhere on the path.

### Mini-Program 2: GlobalAssemblyCache

Clear `Main()` and write:

```csharp
Assembly thisAssembly = Assembly.GetExecutingAssembly();
Assembly mscorlib     = typeof(object).Assembly;

Console.WriteLine($"THIS assembly loaded from GAC: {thisAssembly.GlobalAssemblyCache}");
Console.WriteLine($"mscorlib loaded from GAC:      {mscorlib.GlobalAssemblyCache}");

Console.WriteLine("\nThis project's assembly sits next to its own .exe, in its own output folder.");
Console.WriteLine("That's what most application assemblies do.");
Console.WriteLine("\nmscorlib and the rest of the .NET Framework's core assemblies are installed");
Console.WriteLine("machine-wide. Every .NET Framework application on this machine shares the");
Console.WriteLine("exact same physical copy rather than each one bundling its own.");

GenericFunctions.Pause();
```

Run it. `false` for this project, `true` for mscorlib.

`Assembly.GlobalAssemblyCache` is not just a convention flag -- it reflects which code path the runtime actually used to load the assembly. An assembly loaded from the GAC went through the GAC's identity verification; one loaded from a local path didn't. This is also why the GAC requires strong naming: without a verified identity, the runtime can't safely share one assembly across multiple applications.

### Mini-Program 3: Version Redirects

Clear `Main()` and write:

```csharp
Console.WriteLine("A <bindingRedirect> in App.config tells the .NET Framework runtime:");
Console.WriteLine("  \"When something asks for version X of this strong-named assembly,");
Console.WriteLine("  load version Y instead\" -- without recompiling anything.");
Console.WriteLine();
Console.WriteLine("This training set has a real example worth reading directly:");
Console.WriteLine("  CSharp.Ch09.TextbookCode.NorthwindsWCFDataService's own Web.config");
Console.WriteLine();
Console.WriteLine("That project hit a FileLoadException because EntityFramework's assembly");
Console.WriteLine("version stays frozen at 6.0.0.0 for every 6.x NuGet release, while");
Console.WriteLine("Microsoft.Data.Services genuinely does version its assembly to match its");
Console.WriteLine("package version. The fix was a <bindingRedirect> mapping the old version");
Console.WriteLine("range to the version actually installed.");
Console.WriteLine();
Console.WriteLine("Worth connecting the dots: this entire redirect mechanism only works because");
Console.WriteLine("the assembly is strong-named in the first place. A plain \"MyLibrary.dll\"");
Console.WriteLine("has no version built into its identity for a binding redirect to target.");

GenericFunctions.Pause();
```

The example in the training set is real. If you have access to that project's `Web.config`, it's worth opening alongside this lesson.

A binding redirect is also how NuGet resolves dependency conflicts: when two packages depend on different versions of a shared library, NuGet writes a binding redirect to the app's config file, redirecting all requests to a single installed version. The mechanism is mundane but saves an enormous amount of "DLL hell" that was common before it existed.

### Mini-Program 4: Why Side-By-Side Versioning Matters

Clear `Main()` and write:

```csharp
Console.WriteLine("Side-by-side versioning: two DIFFERENT versions of the SAME-NAMED assembly");
Console.WriteLine("can both be installed and loaded simultaneously. Each application gets the");
Console.WriteLine("specific version it actually references.");
Console.WriteLine();
Console.WriteLine("This only works because a strong name's full identity is:");
Console.WriteLine("  Name + Version + Culture + PublicKeyToken -- ALL together.");
Console.WriteLine("Two assemblies with the same simple name but different versions have genuinely");
Console.WriteLine("DIFFERENT full identities, letting the runtime load the correct one per caller.");
Console.WriteLine();
Console.WriteLine("EntityFramework's assembly version staying frozen at 6.0.0.0 (from");
Console.WriteLine("UnderstandingVersionRedirects above) is a deliberate design choice, not an");
Console.WriteLine("oversight: every EF6.x NuGet release shares ONE assembly identity, avoiding");
Console.WriteLine("a proliferation of near-identical assembly versions for what's really the");
Console.WriteLine("same binary contract. Contrast this against, say, Newtonsoft.Json, which DOES");
Console.WriteLine("version its assembly to match its package -- and has generated binding redirect");
Console.WriteLine("headaches across projects for years as a result.");

GenericFunctions.Pause();
```

Run it. The EntityFramework / Newtonsoft.Json comparison is concrete and real -- both are in this training set's dependencies.

The tension between "version the assembly to match the package" and "keep the assembly version frozen for stability" is a real tradeoff library authors navigate. Freezing the version means binding redirects are rarely needed but the assembly version stops conveying meaningful information about breaking changes. Versioning to match the package keeps the version meaningful but generates redirect noise in consuming projects whenever a package version updates.

---

## Takeaways

- A strong name's full identity is `Name + Version + Culture + PublicKeyToken` -- all four together.
- The public key token is the last eight bytes of the SHA-1 hash of the signing public key. It's globally unique per key pair.
- Microsoft's framework assemblies share the same public key token: `b77a5c561934e089`.
- `Assembly.GlobalAssemblyCache` reflects which load path the runtime actually used.
- The GAC requires strong naming so the runtime can verify identity before sharing an assembly across applications.
- A `<bindingRedirect>` only works against strong-named assemblies -- a simple name has no version to redirect.
- Side-by-side versioning works because different versions have different full identities, not just different file names.
- Keeping an assembly version frozen is a deliberate design choice to avoid binding redirect noise in consuming projects.
