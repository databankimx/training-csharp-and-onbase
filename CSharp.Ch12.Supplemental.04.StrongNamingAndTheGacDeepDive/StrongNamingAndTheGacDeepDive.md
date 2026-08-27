# Strong Naming and the GAC Deep Dive

## Introduction

This is the final lesson in the whole training set. It goes deeper on strong naming and the GAC by looking at real, already-present assemblies instead of trying to build a synthetic example, `mscorlib`, the core of .NET Framework itself, is strong-named and installed on every machine, no setup required.

---

## Seeing the Difference for Real

```csharp
Assembly thisAssembly = Assembly.GetExecutingAssembly();   // this project, NOT strong-named
Assembly mscorlib = typeof(object).Assembly;                 // core .NET Framework, IS strong-named
```

Comparing the two directly makes it concrete: `mscorlib`'s public key token is a real value. This project's is empty. That's the entire practical difference strong naming makes to an assembly's identity.

---

## Was It Actually Loaded From the GAC?

```csharp
assembly.GlobalAssemblyCache   // true or false
```

This project's own assembly: `false`, it loads from its own output folder like almost every application. `mscorlib`: `true`, it's shared machine-wide, every .NET Framework app uses the exact same installed copy.

---

## A Real Example of Version Redirects

Rather than a made-up scenario, go look at `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`'s actual `Web.config`. It contains a real `<bindingRedirect>`, added to fix a genuine error hit while building that project: EntityFramework's assembly version doesn't change between package updates the way you'd expect, so a redirect was needed to point old version requests at what's actually installed.

The key insight: that redirect only works *because* the assembly is strong-named. A plain DLL with no version baked into its identity gives a redirect nothing to target.

---

## Why This All Matters Together

Strong naming's full identity, name, version, culture, and public key token all together, is what lets two different versions of the same library sit side by side on one machine, each application loading the exact version it needs. That's not an edge case; it's the entire reason EntityFramework deliberately keeps its assembly version fixed across releases, so every 6.x version shares one identity instead of needing a redirect for every single update.

---

## Try It Yourself

Run `ComparingStrongNamedVsNotStrongNamed()` and look at the two public key tokens side by side. Then go open `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`'s `Web.config` and find the `<bindingRedirect>` for yourself, it's the same concept, in a real file, that already helped this training set actually work.

---

Thanks for following along through all twelve chapters. Good luck with your certification prep!
