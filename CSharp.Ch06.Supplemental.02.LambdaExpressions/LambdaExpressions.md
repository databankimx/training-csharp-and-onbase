# Lambda Expressions

## Introduction

A lambda expression is the modern way to write inline, passable code, the preferred replacement for the older anonymous-method syntax, and central to how LINQ queries actually work under the hood.

---

## The Lambda Operator

```csharp
(parameters) => expression-or-block
```

`=>` separates the input parameters from the body. It shows up in one other place too, expression-bodied members:

```csharp
public override string ToString() => $"{fname} {lname}".Trim();
```

is exactly equivalent to:

```csharp
public override string ToString() { return $"{fname} {lname}".Trim(); }
```

---

## Expression Lambdas

A single statement, no braces needed:

```csharp
Action note = () => Console.WriteLine("Executed a parameterless expression lambda...");
note();

Action<string> noteWithParameter = message => Console.WriteLine(message);
noteWithParameter("Executed an expression lambda with a parameter...");

Action<string, int> noteWithMultipleParameters = (message, number) => Console.WriteLine($"{number}: {message}");
noteWithMultipleParameters("Executed an expression lambda with multiple parameters...", 3);

Func<float, float> square = x => x * x;
Console.WriteLine(square(2));   // 4
```

Zero parameters need empty parentheses (`()`), exactly one parameter can drop the parentheses (`message =>`), and any lambda that returns a value can be assigned to a `Func<...>` rather than an `Action<...>`.

---

## Statement Lambdas

The same idea, but with a full block body (multiple statements, braces, an explicit `return`):

```csharp
Func<float, int, float> xToTheY = (x, y) =>
{
    float z = x;
    for (int i = 1; i < y; i++) z *= x;
    return z;
};

Console.WriteLine(xToTheY(2, 3));   // 8
```

---

## Lambdas in LINQ

```csharp
string[] words = { "cherry", "apple", "blueberry" };
int shortestWordLength = words.Min(w => w.Length);
```

That's method syntax. LINQ also offers query syntax for the equivalent operation:

```csharp
var query = from w in words select w.Length;
int shortestWordLength2 = query.Min();
```

Both produce the same result, LINQ's query syntax is itself translated into method calls with lambdas under the hood.

Some LINQ methods pass extra information into the lambda, `Where` has an overload that includes the element's index:

```csharp
string[] digits = { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
var shortDigits = digits.Where((digit, index) => digit.Length < index);
```

This keeps only the words whose length is less than their position in the array, `"five"` (length 4) at index 5 qualifies, `"zero"` (length 4) at index 0 does not.

---

## How We Got Here: Delegate Syntax Over Time

```csharp
private static void M(string s) { Console.WriteLine(s); }
private delegate void TestDelegate(string s);
```

```csharp
var testDelA = new TestDelegate(M);                                     // classic: explicit delegate wrapping a named method
TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); };  // C# 2.0: anonymous method
TestDelegate testDelC = (x) => { Console.WriteLine(x); };               // C# 3.0: lambda expression
TestDelegate testDelD = Console.WriteLine;                              // C# 6+: method group conversion
```

All four lines create a working `TestDelegate`. Each one is simply a shorter way to say the same thing than the version before it.

---

## Try It Yourself

Predict the output of `digits.Where((digit, index) => digit.Length < index)` before running it. Then try changing the condition (for example, `digit.Length <= index`) and predict again before running.
