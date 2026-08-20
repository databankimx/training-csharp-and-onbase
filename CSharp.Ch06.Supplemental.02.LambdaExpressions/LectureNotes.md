# Chapter 6 Supplemental 02: Lambda Expressions

## What This Is

A focused lambda-expression lesson: expression lambdas with zero, one, and multiple parameters, a statement lambda (multi-line body), a real LINQ example (`Where` with an index parameter), and a "delegate evolution" demo showing the same call site written four different ways across C#'s history. No bugs found.

---

## The Lambda Operator (`=>`)

The chapter notes make a connection worth internalizing: `=>` isn't unique to lambdas. In an expression-bodied member, it does the same job:

```csharp
public override string ToString() => $"{fname} {lname}".Trim();

// is exactly equivalent to:

public override string ToString() { return $"{fname} {lname}".Trim(); }
```

Same operator, same idea in both places: separate "the thing being defined" from "what it evaluates to."

---

## `LambdaExamples()`: A Real LINQ `Where` With an Index

```csharp
string[] digits = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };

var shortDigits = digits.Where((digit, index) => digit.Length < index);
foreach (var sD in shortDigits) Console.WriteLine(sD);
```

This overload of `Where` passes both the element *and* its index into the lambda. Worth working through by hand once: `"zero"` is length 4 at index 0 (4 < 0? no), ... `"five"` is length 4 at index 5 (4 < 5? yes), and so on. The output ends up being `five, six, seven, eight, nine`, the point isn't the specific result, it's that a lambda passed to LINQ can use more than just the element itself.

---

## `DelegateEvolution()`: The Same Call, Four Ways

```csharp
private static void M(string s) { Console.WriteLine(s); }
```

```csharp
var testDelA = new TestDelegate(M);                                  // original syntax: explicit named method
TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); }; // C# 2.0: anonymous method
TestDelegate testDelC = (x) => { Console.WriteLine(x); };              // C# 3.0: lambda expression
TestDelegate testDelD = Console.WriteLine;                             // C# 6+: method group conversion
```

All four produce a `TestDelegate` that does the same thing when invoked. Reading this top to bottom is effectively a short history of how C# progressively shortened the syntax for "here's a method to call later", from an explicit `new TestDelegate(...)` wrapper down to just naming the method directly. Worth comparing directly against `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`, which covers named vs. anonymous from a different angle (instance/static binding, covariance) rather than this chronological one.
