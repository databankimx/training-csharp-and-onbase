# Chapter 6 Supplemental 02: Lambda Expressions

## What This Is

A focused lambda lesson in three parts:

1. `ExpressionAndStatementLambdas()` — expression lambdas with zero, one, and multiple parameters; a value-returning lambda; a statement lambda
2. `LambdaExamples()` — lambdas in LINQ, including method syntax vs. query syntax and a `Where` overload that supplies the index
3. `DelegateEvolution()` — the same call site written four ways across C#'s history

No bugs found.

---

## The Lambda Operator (`=>`)

The chapter notes make a connection worth internalizing: `=>` isn't unique to lambdas. In an expression-bodied member it does the same job.

```csharp
public override string ToString() => $"{fname} {lname}".Trim();

// is exactly equivalent to:

public override string ToString() { return $"{fname} {lname}".Trim(); }
```

Same operator, same idea in both places: separate "the thing being defined" from "what it evaluates to." Once you read `=>` as "evaluates to" rather than "lambda arrow," expression-bodied properties, methods, constructors, and lambdas all stop looking like separate features.

The two syntactic forms:

```csharp
([parameters]) => expression;              // expression lambda — one statement, implicit return
([parameters]) => { series of statements } // statement lambda — braces, explicit return
```

The chapter notes also flag that `async` applies here (`async () => { ... }`), which Chapter 7 picks up.

---

## `ExpressionAndStatementLambdas()`

### No parameters

```csharp
Action note = () => Console.WriteLine("1: Executed a parameterless expression lambda...");
note();
```

`Action` takes nothing and returns nothing, so the parameter list is an empty `()`. It cannot be omitted — unlike anonymous methods, a lambda always needs a parameter list, even an empty one.

### One parameter

```csharp
Action<string> noteWithParameter = message => Console.WriteLine(message);
noteWithParameter("2: Executed an expression lambda with a parameter...");
```

Two things are implicit here. The parentheses are dropped, which is legal only with exactly one parameter. And `message` has no declared type — the compiler infers `string` from `Action<string>`. That inference flows *from the delegate type into the lambda*, which is why a lambda can never be assigned to `var`: with no target type, there's nothing to infer from.

### Multiple parameters

```csharp
Action<string, int> noteWithMultipleParameters = (message, number) => Console.WriteLine($"{number}: {message}");
noteWithMultipleParameters("Executed an expression lambda with multiple parameters...", 3);
```

Parentheses come back as soon as there's more than one parameter. Both types are still inferred, positionally, from `Action<string, int>`.

### Returning a value

```csharp
Func<float, float> square = x => x * x;
float num = 2;
Console.WriteLine($"{num} squared is {square(num)}");
```

`Func<float, float>` — last type argument is the return type, everything before it is a parameter. Note there's no `return` keyword: in an expression lambda, the expression *is* the return value. Writing `x => return x * x;` is a syntax error.

The `Action` vs. `Func` choice is the whole distinction: `Action` returns `void`, `Func` returns a value. That's it.

### Statement lambda

```csharp
Func<float, int, float> xToTheY = (x, y) =>
{
	float z = x;
	for (int i = 1; i < y; i++) z *= x;
	return z;
};
int exp = 3;
Console.WriteLine($"{num} to the power {exp} = {xToTheY(num, exp)}");
```

The comment in the source is precise: a statement lambda "differs from an expression lambda only in having multiple statements in a block." Braces mean the implicit return is gone, so `return z;` becomes mandatory.

Note `Func<float, int, float>` — parameters `float` and `int`, return `float`. Reading `Func` correctly is a matter of remembering that the *last* argument is always the return type.

Also note that `z` and `i` are declared inside the lambda body. They're ordinary local variables scoped to that body, created fresh on every invocation.

---

## `LambdaExamples()`: Lambdas in LINQ

### Method syntax vs. query syntax

```csharp
string[] words = ["cherry", "apple", "blueberry"];

int shortestWordLength = words.Min(w => w.Length);
Console.WriteLine(shortestWordLength);

var query = from w in words select w.Length;
int shortestWordLength2 = query.Min();
Console.WriteLine(shortestWordLength2);
```

Both print `5` (`apple`). Two syntaxes, one result.

`Min(w => w.Length)` is **method syntax** — a lambda passed directly to a LINQ extension method as a selector. The `from ... select` form is **query syntax**, which the compiler rewrites into method calls before it ever reaches IL. Query syntax is a surface convenience; underneath, it's lambdas either way.

Worth noticing that `query` is not a result — it's a deferred `IEnumerable<int>`. Nothing executes until `.Min()` is called on it. That's LINQ's deferred execution, and it's a frequent source of surprise when a query is built in one place and enumerated somewhere else entirely.

The collection expression syntax `["cherry", "apple", "blueberry"]` is a modern C# 12 shorthand for `new string[] { ... }` — unrelated to lambdas, but present in the source.

### `Where` with an index

```csharp
string[] digits = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];

var shortDigits = digits.Where((digit, index) => digit.Length < index);
foreach (var sD in shortDigits)
{
	Console.WriteLine(sD);
}
```

This overload of `Where` passes both the element *and* its position into the lambda. Worth working through by hand once:

| index | digit | length | `length < index`? |
|---|---|---|---|
| 0 | zero | 4 | no |
| 1 | one | 3 | no |
| 2 | two | 3 | no |
| 3 | three | 5 | no |
| 4 | four | 4 | no |
| 5 | five | 4 | **yes** |
| 6 | six | 3 | **yes** |
| 7 | seven | 5 | **yes** |
| 8 | eight | 5 | **yes** |
| 9 | nine | 4 | **yes** |

Output: `five, six, seven, eight, nine`.

The specific result isn't the point. The point is that the two-parameter lambda selects a *different overload* of `Where` — `Func<T, int, bool>` instead of `Func<T, bool>`. The lambda's shape is what picks which method gets called, which is overload resolution driven by the lambda rather than the other way around.

---

## `DelegateEvolution()`: The Same Call, Four Ways

```csharp
private delegate void TestDelegate(string s);

private static void M(string s)
{
	Console.WriteLine(s);
}
```

```csharp
var testDelA = new TestDelegate(M);                                    // original: explicit constructor + named method
TestDelegate testDelB = delegate (string s) { Console.WriteLine(s); }; // C# 2.0: anonymous method
TestDelegate testDelC = (x) => { Console.WriteLine(x); };              // C# 3.0: lambda expression
TestDelegate testDelD = Console.WriteLine;                             // method group conversion
```

All four produce a `TestDelegate` that does the same thing when invoked. Read top to bottom, it's a short history of C# progressively shortening the syntax for "here's a method to call later" — from an explicit `new TestDelegate(...)` wrapper down to simply naming the method.

Details worth catching:

- **A** is the only one that needs `var`, because `new TestDelegate(M)` states the type on the right. The others declare `TestDelegate` on the left because the anonymous method, lambda, and method group have no type of their own — they need a target type to convert to.
- **B** must write `(string s)` in full. Anonymous methods don't infer parameter types.
- **C** uses braces even though the body is one statement, so it's technically a statement lambda. `x => Console.WriteLine(x)` would be shorter and equivalent. Also note `(x)` — the parentheses are optional for a single parameter, so `x =>` works too.
- **D** skips the wrapper entirely. `Console.WriteLine` is a method group, converted directly to `TestDelegate` because the `WriteLine(string)` overload matches. This is the same mechanism seen in Supplemental 01.

For new code, prefer D when a method already exists, and C (without braces) when it doesn't. A and B are legacy syntax you'll encounter reading older codebases rather than write yourself.

Compare this against `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`, which covers named vs. anonymous from a different angle — instance/static binding and variance — rather than this chronological one.

---

## Takeaways

- `=>` means "evaluates to," in lambdas and expression-bodied members alike.
- Expression lambda: one expression, implicit return, no braces. Statement lambda: braces, explicit `return`.
- Parentheses around parameters are optional only for exactly one parameter.
- Parameter types are inferred from the target delegate type, which is why a lambda can't be assigned to `var`.
- `Action` returns `void`; `Func`'s last type argument is the return type.
- LINQ query syntax compiles down to method syntax with lambdas — same thing, different surface.
- LINQ queries are deferred; nothing runs until enumerated.
- A lambda's shape can select which overload gets called, as with `Where`'s index overload.
- Prefer method group conversion when a suitable method exists, and an expression lambda when it doesn't.
