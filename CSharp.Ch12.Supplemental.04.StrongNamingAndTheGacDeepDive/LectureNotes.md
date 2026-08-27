# Chapter 12 Supplemental 04: Strong Naming and the GAC Deep Dive

## What This Is

The main lesson's `UnderstandingStrongNaming()` found this training set's own projects aren't strong-named, expected, since application projects rarely need to be. Rather than manufacturing a synthetic strong-named assembly for this lesson (a real `.snk`-based signing setup is genuinely a build-time/tooling concern, not something meaningfully demonstrated by generating one on the fly at runtime), this Supplemental inspects real, already strong-named assemblies guaranteed present on any .NET Framework machine: the framework's own core assemblies. Every comparison here is against genuine, verifiable data.

---

## A Real Side-by-Side: This Project's Assembly vs. `mscorlib`

```csharp
Assembly thisAssembly = Assembly.GetExecutingAssembly();
Assembly mscorlib = typeof(object).Assembly;
```

`mscorlib` (holding `System.Object`, `System.String`, and the rest of the truly foundational .NET Framework types) is strong-named; this project's own assembly isn't. Comparing `GetName().GetPublicKeyToken()` between the two makes the difference concrete rather than abstract: `mscorlib`'s token is a real, populated byte array, this project's is empty. That emptiness *is* what "not strong-named" means in practice, an assembly's full name still has four component slots (Name, Version, Culture, PublicKeyToken), a non-strong-named assembly simply leaves the last one blank.

---

## `Assembly.GlobalAssemblyCache`: Was This Actually Loaded From the GAC?

```csharp
bool fromGac = assembly.GlobalAssemblyCache;
```

This project's own assembly loads `false`, it sits in this project's own output folder, loaded from there directly, exactly what almost every application assembly does. `mscorlib` loads `true`, it's installed machine-wide and every .NET Framework application on the machine shares the exact same physical copy rather than each one bundling its own. This is the GAC in its most concrete, observable form: not a conceptual description, a real property reporting real, checkable state.

---

## Version Redirects: A Real Example Already in This Training Set

Worth going back and reading directly rather than taking on faith: `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`'s own `Web.config` contains a genuine `<bindingRedirect>`, put there to fix an actual `FileLoadException` hit during that project's migration. The specific problem: EntityFramework's *assembly* version stays frozen at `6.0.0.0` across every 6.x NuGet package release, while `Microsoft.Data.Services` (a different, related package) genuinely does version its assembly to track its package version. The `<bindingRedirect>` maps whatever *old* version something asks for to the version *actually installed*, without recompiling the code that made the original request.

Worth connecting the dots explicitly: this entire mechanism, redirecting one specific version to another without breaking anything else that depends on the assembly, only works *because* the assembly is strong-named in the first place. A `<bindingRedirect>` targets a specific `oldVersion`/`newVersion` range; a plain, non-strong-named `"MyLibrary.dll"` has no version number built into its identity at all for a redirect to meaningfully target.

---

## Why the Full Identity Matters: Tying It Back to Side-by-Side Versioning

A strong name's full identity, simple name + version + culture + public key token *together*, is exactly what lets two different versions of the same-named assembly coexist and be told apart, each caller loading the specific version it actually references rather than the runtime being forced to pick one version, machine-wide, for every application. This reframes something covered back in `CSharp.Ch09.TextbookCode.NorthwindsWCFDataService`'s own lecture notes as a deliberate design choice rather than an oversight: EntityFramework's assembly version staying frozen at `6.0.0.0` regardless of the NuGet package version means every EF6.x release shares one assembly identity, deliberately avoiding a painful side-by-side proliferation of near-identical assembly versions for what is, underneath, the same binary contract.
