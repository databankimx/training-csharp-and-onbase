# Chapter 6 Supplemental 04: Multicast Delegates

## What This Is

The canonical multicast-delegate example (this is essentially Microsoft's own documentation sample for the feature), combining two delegates with `+`, then removing one with `-`. No bugs found.

---

## Combining and Subtracting

```csharp
internal delegate void CustomDel(string s);
...
CustomDel hiDel = Hello;
CustomDel byeDel = Goodbye;

CustomDel multiDel = hiDel + byeDel;
CustomDel multiMinusHiDel = multiDel - hiDel;
```

```csharp
Console.WriteLine("Invoking delegate hiDel:");        hiDel("A");            // Hello, A!
Console.WriteLine("Invoking delegate byeDel:");        byeDel("B");           // Goodbye, B!
Console.WriteLine("Invoking delegate multiDel:");      multiDel("C");         // Hello, C!  Goodbye, C!
Console.WriteLine("Invoking delegate multiMinusHiDel:"); multiMinusHiDel("D"); // Goodbye, D!
```

`multiDel("C")` calls **both** `Hello` and `Goodbye`, in the order they were combined, that's what "multicast" means, a single delegate variable holding an ordered list of methods, all invoked in sequence on one call. `multiDel - hiDel` produces a new delegate whose invocation list has `Hello` removed, leaving only `Goodbye`.

---

## Worth Noticing: The Commented-Out `Action<string>` Alternative

```csharp
// In this example, you can omit the custom delegate if you 
// want to and use Action<string> instead.
//Action<string> hiDel, byeDel, multiDel, multiMinusHiDel;
```

`CustomDel` didn't need to be a custom-declared delegate type at all, `Action<string>` (a built-in generic delegate matching "takes a string, returns nothing") would work identically here. The custom delegate exists mainly for clarity in this teaching example. In your own code, prefer the built-in `Func<...>`/`Action<...>` types unless you have a specific reason to declare your own, less boilerplate, and it's immediately recognizable to anyone else reading the code.

---

## Worth Noticing: `// ReSharper disable once DelegateSubtraction`

Delegate subtraction (`multiDel - hiDel`) is a real, supported C# feature, but ReSharper and similar code nannies flag it by default because it has a genuine sharp edge worth knowing about: if a multicast delegate's invocation list contains a method more than once, `-` only removes the *last* occurrence, and results can be surprising when combined with delegates built from different sources (lambdas, closures over different variables that happen to look identical). It works correctly and predictably here, two distinct named methods, no duplicates, this ReSharper suppression is a reasonable, deliberate acknowledgment of that rather than something accidentally left in.

---

## Compare Against Supplemental 01

`CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`'s `CombineDelegates()` method covers the same `+`/`-` mechanics as a quick preview before moving on to other topics. This project is the fuller, dedicated treatment, worth running both back to back if the concept didn't fully land the first time.
