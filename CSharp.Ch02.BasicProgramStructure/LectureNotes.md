# Chapter 2: Basic Program Structure

## What This Chapter Is Actually About

Program flow. Every language has some way of saying "do this, then that," "do this only if," and "do this over and over," and this chapter walks through C#'s version of all three, plus enough operator trivia to make sense of the conditions those structures depend on. It's a long chapter because it's foundational, almost nothing after this point works without it.

---

## Statement vs. Expression

Worth nailing down before anything else, since the rest of this chapter uses both words constantly and they're easy to blur together.

A **statement** does a thing. It's an instruction, and it doesn't hand anything back to you.

```csharp
Console.WriteLine("Hello");
int counter = 0;
if (x > 5) { }
```

An **expression** evaluates to a value. You can put it on the right side of an `=`, pass it as an argument, or drop it inside a larger expression, because it produces something usable.

```csharp
2 + 2          // evaluates to 4
x > 5          // evaluates to a bool
expr1 = expr2  // evaluates to whatever expr2 is (see below)
```

The reason this distinction earns real estate at the top of the chapter instead of staying a footnote: `expr1 = expr2` is an expression, not just a statement, it evaluates to the value being assigned. That's exactly what makes the assignment-vs-equality gotcha later in this chapter possible. If assignment didn't evaluate to anything, writing `=` where you meant `==` inside a condition would just fail to compile instead of silently doing the wrong thing. C# lets it compile because `x = 5` is a perfectly good expression on its own, it's just probably not the expression you meant to write.

Most lines of code you'll write are statements built out of expressions, a statement wraps an expression and a semicolon around it, discarding whatever the expression evaluated to (or, in the case of `int counter = 0;`, keeping it by storing it in a variable). Keeping the two words straight makes the rest of this chapter, and every gotcha in it, easier to reason about.

---

## Statements and Blocks

A **statement** is one instruction, ended with a semicolon. A **block** is a group of statements wrapped in `{ }`. Put multiple simple statements inside a block, and you've got a **complex statement**, the `foreach` loop in `ComplexStatements()` is a complex statement made up of an `if` inside a loop, both of which are blocks in their own right.

```csharp
int counter;
counter = 0;
```

That's two simple statements: a declaration and an assignment. You can combine them (`int counter = 0;`), and past this lesson you'll basically always want to, declaring a variable without initializing it is asking for a "use of unassigned local variable" compile error the moment you try to read it before some code path sets it.

There's also the empty statement, a bare `;` sitting on its own line. It's legal and does nothing. You will basically never write one on purpose, it exists mostly as a footgun for a stray semicolon after an `if` condition:

```csharp
if (x == 5); // this semicolon ends the if statement right here
{
    // this block always runs, regardless of x
}
```

Worth burning into memory now, since it's an easy typo and the compiler won't warn you about it.

### A Style Note: Collection Expressions

`ComplexStatements()` declares its array the classic way:

```csharp
int[] numbers = { 5, 24, 36, 19, 45, 60, 78 };
```

Modern C# also lets you write that as a **collection expression**:

```csharp
int[] numbers = [5, 24, 36, 19, 45, 60, 78];
```

Same array, same values, just square brackets instead of curly braces. The old `{ }` style still compiles and still works fine, it's not deprecated or wrong, it's just no longer the syntax the compiler will nudge you toward by default. You'll see both forms throughout this codebase: `{ }` where a lesson is specifically preserving the classic style for comparison, `[ ]` everywhere else. Worth being able to read both, since older code you'll encounter on the job was written before `[ ]` existed as an option.

---

## Comparison and Logical Operators

### Relational Operators

`<`, `>`, `<=`, `>=`, `==`, `!=`. All six always return a `bool`. Nothing surprising here except the one that bites everyone eventually:

```csharp
Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
Console.WriteLine($"expr1 = expr2 ? {expr1 = expr2}");
```

The second line uses `=` (assignment), not `==` (equality), inside the interpolation. It compiles fine, because `expr1 = expr2` is itself an expression that evaluates to the assigned value, and `expr2` here is non-zero, so it prints as truthy-looking output. It also just quietly overwrote `expr1` with `expr2`'s value. This is exactly the kind of bug that hides in a large `if` condition for weeks before someone notices the variable it's "checking" keeps changing.

### Logical and Bitwise Operators

`&&` and `||` are the logical operators you'll use constantly, and both **short-circuit**: `&&` skips evaluating the right side if the left side is already `false`, `||` skips it if the left side is already `true`. This matters beyond performance, it's a common pattern to rely on the short-circuit to avoid a crash:

```csharp
if (obj != null && obj.SomeProperty == 5)
```

If `obj` is null, `&&` never evaluates `obj.SomeProperty`, so there's no null-reference exception. Swap `&&` for the bitwise `&` and this guarantee disappears, `&` always evaluates both sides.

Speaking of which: `&`, `|`, and `^` are the bitwise operators. On `bool` operands they behave like their logical cousins minus the short-circuit; on integer operands they compare bit-by-bit and return an integer, not a `bool`. `ConditionalOperators()` demonstrates both readings side by side.

The `~` bitwise complement has its own gotcha:

```csharp
Console.WriteLine($"~expr1 = {Convert.ToString((byte)~expr1, 2).PadLeft(8, '0')} = {(byte)~expr1}");
```

`~` on a `byte` doesn't return a `byte`. It promotes to a signed 32-bit `int` first, complements all 32 bits, and hands you back a much larger (and possibly negative) number than you expected unless you explicitly cast back down. The `(byte)` cast here isn't decoration, it's load-bearing.

### Truth Tables

Worth having these memorized rather than re-deriving them every time:

**Negation (NOT)**

| `!x` | Result |
|---|---|
| `false` | `true` |
| `true` | `false` |

**Conjunction (AND)**, both logical `&&` and bitwise `&` follow this table, the only difference is short-circuiting

| x | y | `x && y` |
|---|---|---|
| `true` | `true` | `true` |
| `false` | `true` | `false` |
| `true` | `false` | `false` |
| `false` | `false` | `false` |

**Disjunction (OR)**

| x | y | `x \|\| y` |
|---|---|---|
| `true` | `true` | `true` |
| `false` | `true` | `true` |
| `true` | `false` | `true` |
| `false` | `false` | `false` |

Bitwise operators on integers follow the same shape per-bit, but return bits (0/1), not `bool`. `^` (XOR, exclusive-or) is bitwise-only, there's no logical `^^`, it returns `1` when exactly one of the two bits is `1`, and `0` when they match:

| b1 | b2 | `b1 ^ b2` |
|---|---|---|
| `1` | `0` | `1` |
| `0` | `1` | `1` |
| `1` | `1` | `0` |
| `0` | `0` | `0` |

### There's No Logical XOR, But You Can Build One

C# has `&&`, `||`, and bitwise `^`, but no logical `^^`. If you need "exactly one of these two conditions is true" using booleans instead of bits, any of these three expressions are equivalent:

```csharp
(expr1 || expr2) && !(expr1 && expr2)
(expr1 || expr2) && (!expr1 || !expr2)
(expr1 && !expr2) || (!expr1 && expr2)   // parentheses not required here, && binds tighter than ||
```

All three read as some variation of "at least one is true, but not both." Worth recognizing this pattern when you see it, since none of them announce themselves as "this is XOR" the way `^` does for bits.

### The Ternary Operator

```csharp
string result = expr1 > expr2 ? "" : "not ";
```

Shorthand for an `if`/`else` that only exists to assign one of two values to a variable. `ConditionalOperators()` shows the expanded `if`/`else` right next to it in a comment, worth comparing the two side by side once so the shorthand stops looking like magic.

---

## Branching: if, else, and switch

`if`/`else`/`else if` need no real introduction, but one style note worth internalizing early:

```csharp
if (true) Console.WriteLine("This statement still executes.");
```

A single statement after an `if` doesn't strictly need braces. `IfThenElse()` shows this once and then deliberately never does it again, sticking to braces for the rest of the lesson. Braceless `if` bodies are exactly one careless edit away from a bug, someone adds a second line later, assumes it's part of the `if`, and it silently isn't. Always brace it.

`switch` exists for the specific case of comparing one variable against several possible values, it gets unwieldy fast as a chain of `else if`. Two things worth knowing beyond the basic syntax: cases can stack to share one result block (`case 0: case 1: /* shared code */ break;`), and forgetting a `break` at the end of a case falls through into the next one, which is rarely what you want and is exactly why `default` exists as a catch-all.

---

## Loops

Four flavors, each suited to a different shape of problem:

- **`for`**: you know how many times you want to repeat something, or you're iterating with an index. `for (int i = 1; i <= 10; i++)`.
- **`foreach`**: you have a collection and want to touch every item in it, without caring about index bookkeeping. `foreach (int num in numbers)`.
- **`while`**: you want to loop until some condition becomes true, and you don't know in advance how many iterations that'll take.
- **`do while`**: identical to `while`, except the condition is checked *after* the body runs instead of before, guaranteeing at least one execution even if the condition starts out false.

```csharp
do
{
    Console.WriteLine("Note: Even though I made the condition false, this loop ran once.");
} while (false);
```

That's the whole point of `do while` in one example: the condition is already `false` and the loop still runs once, because the check happens at the bottom, not the top.

### The Infinite Loop Gotcha

Every loop needs two things: a condition that can become false, and something inside the loop that actually pushes it toward that outcome. Miss either one and you've got an infinite loop:

```csharp
// Because this iterates down (-1, -2, etc.), i will *always* be less than 10, and the loop never ends
for (int i = 0; i <= 10; i--)
{
    Console.WriteLine(i);
}
```

`i--` moves the wrong direction relative to the `<= 10` condition, so the condition never flips false. This is a classic copy-paste bug, someone reuses a `for` loop template and forgets to flip `++` to `--` (or vice versa) to match the new bounds.

---

## The Code Labs

Three small standalone exercises, tucked into their own methods so each can be studied in isolation:

- **`CodeLabUseOfBool`**: the absolute minimum boolean example, one comparison, one assignment.
- **`CodeLabUsingIfStatements`**: single conditions, compound conditions with `&&`, and nested `if` blocks, three ways of combining logic that build directly on each other.
- **`CodeLabLotteryProgram`**: picks 6 numbers from a 49-number range using `Random` and a `for` loop, a nice small example of loops and arrays working together.
- **`CodeLabAverageGrades`**: sums an array with `foreach`, then divides, with a defensive check (`if (gradeCount == 0) total = gradeCount = 1;`) to avoid a divide-by-zero if the array were ever empty.
- **`CodeLabForLoops`**: runs through counting up, counting down, counting by twos, counting by multiples of five, then repeats the same idea with `foreach`, `while`, and `do while`, so you see all four loop types solve variations of the same "count to something" problem back to back.

---

## Bonus: Arithmetic Operators, Precedence, and Increment/Decrement

Not part of the textbook's core lesson, but useful enough to earn a permanent spot in this chapter.

### Compound Assignment

`+=`, `-=`, `*=`, `/=` all do "perform this operation, then assign the result back to the same variable" in one step. `c += 5;` is shorthand for `c = c + 5;`, nothing more exotic than that.

### Precedence

```csharp
Console.WriteLine($"2 + 2 * 2 = {2 + 2 * 2}");        // 6, multiplication first
Console.WriteLine($"(2 + 2) * 2 = {(2 + 2) * 2}");    // 8, parentheses override precedence
```

Multiplication and division happen before addition and subtraction, same order of operations you learned in grade school. Parentheses are how you override that when the math you actually want disagrees with the default order.

### Prefix vs. Postfix

```csharp
Console.WriteLine($"a = {++a}");  // prefix: increments first, then uses the new value
Console.WriteLine($"a = {a++}");  // postfix: uses the current value, then increments
```

Both increment `a`. The difference is only visible when the increment happens *inside* an expression that also uses the value: prefix hands you the value after the change, postfix hands you the value before it. Outside of an expression, `a++;` and `++a;` on their own line behave identically, the difference only shows up when something else is reading the value in the same statement.

One place this genuinely doesn't matter: a `for` loop's iterator step. `for (int i = 0; i < 5; i++)` and `for (int i = 0; i < 5; ++i)` produce identical output, because the iterator step runs after the loop body executes, not inside an expression that consumes its value. `BonusIncrementAndDecrement()` proves this by running both versions and showing they match.
