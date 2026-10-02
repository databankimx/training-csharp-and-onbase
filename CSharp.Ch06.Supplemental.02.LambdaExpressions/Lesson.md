# Chapter 6 Supplemental 02: Lambda Expressions

## What This Is

A focused lambda lesson in three parts: expression and statement lambdas from zero parameters up through multi-statement bodies, lambdas in LINQ with method vs. query syntax, and the same call site written four different ways across C#'s history.

---

## The Lambda Operator

`=>` is the lambda operator, and it's worth recognizing as the same thing in two different contexts:

```csharp
// In an expression-bodied member:
public override string ToString() => $"{FirstName} {LastName}".Trim();

// In a lambda:
Func<string, int> len = s => s.Length;
```

Both read as "evaluates to." Once you see `=>` as "evaluates to" rather than "lambda arrow," expression-bodied methods, properties, and lambdas all stop looking like separate features.

---

## How to Write This Program

### Mini-Program 1: Zero, One, and Multiple Parameters

```csharp
// Zero parameters -- empty () is required, unlike anonymous methods
Action note = () => Console.WriteLine("Parameterless lambda called.");
note();

// One parameter -- parentheses optional, type inferred
Action<string> greet = message => Console.WriteLine($"Hello, {message}!");
greet("world");

// Multiple parameters -- parentheses required, types inferred
Action<string, int> label = (text, n) => Console.WriteLine($"{n}: {text}");
label("first", 1);
label("second", 2);
```

Run it. Three lines of output.

Parameter types are always inferred from the target delegate type -- the compiler reads right to left, from `Action<string>` to `message`. That's also why a lambda can never be assigned to `var`: with no target type on the left, there's nothing to infer from.

The parentheses rule: optional with exactly one parameter, required with zero or more than one. Unlike anonymous methods, a lambda can't omit the parameter list entirely even when unused -- you write `(_, _)` or `(s, e)` even if you don't use either.

### Mini-Program 2: A Value-Returning Lambda

```csharp
Func<float, float> square = x => x * x;
Console.WriteLine($"2 squared is {square(2)}");
Console.WriteLine($"5 squared is {square(5)}");
```

Run it. `4` and `25`.

`Func`'s last type argument is the return type, everything before it is a parameter. `Func<float, float>` means "takes a `float`, returns a `float`." In an expression lambda, the expression *is* the return value -- no `return` keyword, and writing one is a syntax error.

`Action` vs. `Func` is the whole distinction: `Action` returns `void`, `Func` returns a value.

### Mini-Program 3: A Statement Lambda

```csharp
Func<float, int, float> power = (x, y) =>
{
    float z = x;
    for (int i = 1; i < y; i++) z *= x;
    return z;
};

Console.WriteLine($"2 to the 3 = {power(2, 3)}");
Console.WriteLine($"3 to the 4 = {power(3, 4)}");
```

Run it. `8` and `81`.

Braces mean the implicit return is gone and `return` becomes mandatory. Local variables declared inside the body (`z`, `i`) are created fresh on every invocation, exactly like a regular method.

`Func<float, int, float>` -- two parameters (`float` and `int`), return type `float`. When reading a `Func` signature, the *last* type argument is always the return type.

### Mini-Program 4: Lambdas in LINQ -- Method Syntax and Query Syntax

```csharp
string[] words = ["cherry", "apple", "blueberry"];

// Method syntax -- lambda passed to a LINQ extension method
int shortestLength = words.Min(w => w.Length);
Console.WriteLine($"Shortest: {shortestLength}");

// Query syntax -- compiles to the same thing underneath
var query = from w in words select w.Length;
int shortestLength2 = query.Min();
Console.WriteLine($"Shortest (query syntax): {shortestLength2}");
```

Run it. Both print `5` (`apple`).

Two syntaxes, identical IL. Query syntax is a surface convenience that the compiler rewrites into method calls. `query` is not a result -- it's a deferred `IEnumerable<int>`. Nothing executes until `.Min()` is called. That's LINQ's deferred execution, and it's a frequent source of surprise when a query is built in one place and enumerated somewhere else entirely.

### Mini-Program 5: The `Where` Overload That Supplies the Index

```csharp
string[] digits = ["zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine"];

var shortDigits = digits.Where((digit, index) => digit.Length < index);
foreach (var d in shortDigits)
    Console.WriteLine(d);
```

Run it. `five`, `six`, `seven`, `eight`, `nine`.

This overload of `Where` passes both the element and its position. The two-parameter lambda selects a *different overload* -- `Func<T, int, bool>` instead of `Func<T, bool>`. The lambda's shape is what picks which method is called, which is overload resolution driven by the lambda rather than the other way around. Working through it by hand is instructive: `"five"` has length 4 and sits at index 5, so `4 < 5` passes. Everything before it fails the same comparison.

### Mini-Program 6: Four Ways to Write the Same Delegate

```csharp
private delegate void TestDelegate(string s);

private static void M(string s) { Console.WriteLine(s); }
```

```csharp
// C# 1.0 -- explicit delegate constructor
var a = new TestDelegate(M);

// C# 2.0 -- anonymous method, parameter type written explicitly
TestDelegate b = delegate (string s) { Console.WriteLine(s); };

// C# 3.0 -- lambda expression
TestDelegate c = (x) => { Console.WriteLine(x); };

// Method group conversion -- cleanest when a method already exists
TestDelegate d = Console.WriteLine;

a("A"); b("B"); c("C"); d("D");
```

Run it. `A`, `B`, `C`, `D`.

Four syntaxes, identical behavior. Read top to bottom, it's a short history of C# progressively shortening the syntax for "here's a method to call later."

Details worth catching: `a` is the only one using `var` because `new TestDelegate(M)` states the type on the right; the others need `TestDelegate` on the left because the anonymous method, lambda, and method group have no type of their own. `b` must write `(string s)` in full -- anonymous methods don't infer parameter types. `c` uses braces even for a one-statement body; `x => Console.WriteLine(x)` would be shorter. `d` skips the wrapper entirely by converting the method group directly.

For new code: prefer `d` when a method already exists, and `c` without braces when writing inline. `a` and `b` are legacy syntax you'll read in older code rather than write yourself.

## Try It Yourself

Before running Mini-Program 5, predict which words pass `digit.Length < index`. Work through each element by hand: `"zero"` has length 4 at index 0 (4 < 0? no), `"one"` has length 3 at index 1 (3 < 1? no), and so on. Then run it and compare.

Then change the condition to `digit.Length <= index` and predict again before running. One extra word should qualify -- which one, and why?

---

## Takeaways

- `=>` means "evaluates to" -- same in lambdas and expression-bodied members.
- Expression lambda: one expression, implicit return. Statement lambda: braces, explicit `return`.
- Parentheses optional with one parameter, required with zero or more than one.
- Parameter types are inferred from the target delegate type -- lambdas can't be assigned to `var`.
- `Action` returns `void`. `Func`'s last type argument is the return type.
- LINQ query syntax compiles to method syntax with lambdas.
- LINQ queries are deferred -- nothing runs until enumerated.
- A lambda's parameter count and types select which overload gets called.
- Prefer method group conversion when a method exists; expression lambda when writing inline.
