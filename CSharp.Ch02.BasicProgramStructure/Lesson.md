# Chapter 2 - Basic Program Structure

## What This Chapter Teaches

Chapter 1 got a program to start. This one teaches it to do anything interesting once it has.

Every language, whatever else it does or doesn't have, needs three things: a way to say "do this, then do that," a way to say "do this only if," and a way to say "do this over and over." That's the entire subject of this chapter, program flow, plus the operators those three structures lean on to make their decisions. It's the longest chapter so far, and it earns the length: essentially nothing you write after this point works without it.

By the end, you'll have built, run, and broken (on purpose) `if`, `else if`, `switch`, all four loop shapes, and enough operator trivia to know exactly why `x = 5` inside a condition is legal C# and also usually a bug.

---

## Statement vs. Expression

Worth nailing down before anything else, since the rest of this lesson uses both words constantly.

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
expr1 = expr2  // evaluates to whatever expr2 is (see Mini-Program 4)
```

This distinction matters because `expr1 = expr2` is an expression, not just a statement -- it evaluates to the value being assigned. That's exactly what makes it possible to accidentally write `=` where you meant `==` inside a condition; C# doesn't stop you, because `x = 5` is a perfectly valid expression on its own.

Most lines you'll write are statements built out of expressions. A statement wraps an expression and a semicolon around it, either discarding the result or keeping it (as in `int counter = 0;`, which stores the expression's value in a variable).

---

## How to Write This Program

Seventeen topics in this chapter, each one below stands alone. Every "mini-program" is meant to be the entire contents of `Main()`, on its own, nothing else in the file. Clear out whatever was there before you start the next one. Build it, run it, see what happens, then move on, no method calls, no named helper methods, nothing carried over from the last topic. You haven't been introduced to functions yet, and none of what follows needs them.

(The finished version of this chapter, sitting in this project's own `Program.cs`, *does* organize all seventeen into named methods, one method per topic, all called in turn from a single `Main()`. That's a genuinely useful way to package a chapter's worth of demos into one runnable reference copy, and it's exactly the kind of thing methods are for, once you've met them. You haven't yet, so ignore that file's actual shape for now, it'll make a lot more sense in the chapter that covers methods properly. For the moment, one topic, one tiny program, repeat.)

### Mini-Program 1: Simple Statements

```csharp
int counter;
float distance;
string firstName;

counter = 0;
distance = 4.5f;
firstName = "Bill";

const string instructorName = "Scotty Mac";

Console.WriteLine($"counter = {counter}");
Console.WriteLine($"distance = {distance}");
Console.WriteLine($"firstName = {firstName}");
Console.WriteLine($"instructorName = {instructorName}");
```

Run it. You should see the four values printed and the program end.

A **statement** is one instruction. A **simple statement** ends with a semicolon and does one thing: declares a variable, or assigns one. Real code almost always combines the two (`int counter = 0;`), but they're split apart here on purpose so both categories are visible separately. Declaring a variable without initializing it, the way `counter`/`distance`/`firstName` are declared above, sets up a "use of unassigned local variable" compile error the instant some path reads one before something's written to it, C# tracks this for you and refuses to compile around it. That `4.5f` isn't decoration either: an undecorated `4.5` is a `double` literal, and C# won't silently narrow a `double` into a `float` assignment, the `f` suffix is what makes this a `float` literal in the first place.

`const` gets a quiet first appearance here too. A `const` value is fixed at compile time and can never be reassigned, anywhere, ever. That's different from `readonly` (set once, at construction), which is a Chapter 3 topic.

Now add one more line, right at the top, before anything else:

```csharp
;
```

Run it again. Nothing changes, because a bare semicolon on its own line is a legal statement, the empty statement, and it does exactly what it sounds like: nothing. You'll never write one on purpose. You may absolutely write one by accident, and this is the one line in the whole chapter worth memorizing rather than just reading:

```csharp
if (x == 5); // this semicolon ends the if statement right here
{
    // this block always runs, regardless of x
}
```

The `if` governs that stray `;` and nothing else. The block underneath, indented to look like it belongs to the `if`, is just an unconditional block that happens to run every single time. The compiler will not warn you. This is a one-character bug that looks impossible when you're staring directly at it, which is exactly why it's worth seeing once, deliberately, before you meet it by accident in a hundred-line file at 5pm on a Friday.

### Mini-Program 2: Complex Statements

```csharp
int[] numbers = { 5, 24, 36, 19, 45, 60, 78 };
int evenNums = 0;

foreach (int num in numbers)
{
    Console.WriteLine($"num = {num}");
    if (num % 2 == 0)
    {
        evenNums++;
    }
}

Console.WriteLine($"Found {evenNums} even number{(evenNums == 1 ? "" : "s")}");
```

Run it. You should see all seven numbers printed one at a time, then a count of how many were even.

A **block** is statements wrapped in `{ }`. Nest a block inside a control structure and you've built a **complex statement**, the `foreach` above, with an `if` nested inside it, is exactly that: two blocks, one containing the other.

Two small things worth a second look. First, `int[] numbers = { 5, 24, 36, 19, 45, 60, 78 };` uses the classic brace initializer. Modern C# also accepts `int[] numbers = [5, 24, 36, 19, 45, 60, 78];`, a **collection expression**, same array, square brackets instead of curly braces. Neither is deprecated. You'll see both across this codebase, braces where a lesson is deliberately preserving the older look, brackets everywhere else, and being able to read both matters more than picking a side, since plenty of code you'll inherit on the job predates the bracket syntax entirely.

Second, `{(evenNums == 1 ? "" : "s")}` sneaks the ternary operator into a string interpolation a couple of topics before it's formally introduced, just to make "1 even number" read correctly instead of "1 even numbers." Small, but it's the difference between output that looks finished and output that looks like a first draft.

### Mini-Program 3: Conditional Operators, Part One (Relational)

```csharp
const bool myConditionResult = false;
Console.WriteLine($"myConditionResult = {myConditionResult}");

byte expr1 = 1;
Console.WriteLine($"expr1 = {expr1}");
byte expr2 = 2;
Console.WriteLine($"expr2 = {expr2}");
Console.WriteLine($"expr1 < expr2 ? {expr1 < expr2}");
Console.WriteLine($"expr1 > expr2 ? {expr1 > expr2}");
Console.WriteLine($"expr1 <= expr2 ? {expr1 <= expr2}");
Console.WriteLine($"expr1 >= expr2 ? {expr1 >= expr2}");
Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
Console.WriteLine($"expr1 != expr2 ? {expr1 != expr2}");
```

Run it. `<`, `>`, `<=`, `>=`, `==`, `!=`. All six always return a `bool`, no surprises yet, which is exactly what makes the next mini-program land.

### Mini-Program 4: Conditional Operators, Part Two (The Gotcha)

Same as Mini-Program 3, with two lines added right after the relational block:

```csharp
Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
Console.WriteLine($"expr1 = expr2 ? {expr1 = expr2}");
Console.WriteLine($"expr1 = {expr1}");
Console.WriteLine($"expr2 = {expr2}");
```

Run it, and read that third-from-last line slowly. One equals sign. It compiles cleanly, because `expr1 = expr2` isn't just an assignment statement, it's an *expression*, and expressions evaluate to a value, in this case the value being assigned. So it prints something that looks plausible, and it also just quietly overwrote `expr1` with whatever `expr2` held. That final `WriteLine` exists purely to prove the damage.

This is exactly why "statement versus expression" earns real estate at the front of this chapter rather than staying a footnote. If assignment didn't evaluate to anything, writing `=` where you meant `==` would simply refuse to compile. Instead it compiles, and quietly changes your data. C# does protect you in the single most common case, inside an `if` condition the expression must resolve to `bool`, so `if (x = 5)` fails outright because `5` is an `int`, not a `bool`. But `if (someBool = true)` compiles perfectly and is true forever, and that version is the one that actually shows up in production code, because nobody's staring at it expecting a bug.

### Mini-Program 5: Conditional Operators, Part Three (Bitwise)

```csharp
byte expr1 = 15; // Binary 00001111
byte expr2 = 10; // Binary 00001010
Console.WriteLine($"expr1 = {Convert.ToString(expr1, 2).PadLeft(8, '0')} = {expr1}");
Console.WriteLine($"expr2 = {Convert.ToString(expr2, 2).PadLeft(8, '0')} = {expr2}");
Console.WriteLine($"expr1 & expr2 = {Convert.ToString(expr1 & expr2, 2).PadLeft(8, '0')} = {expr1 & expr2}");
Console.WriteLine($"expr1 | expr2 = {Convert.ToString(expr1 | expr2, 2).PadLeft(8, '0')} = {expr1 | expr2}");
Console.WriteLine($"expr1 ^ expr2 = {Convert.ToString(expr1 ^ expr2, 2).PadLeft(8, '0')} = {expr1 ^ expr2}");
Console.WriteLine($"~expr1 = {Convert.ToString((byte)~expr1, 2).PadLeft(8, '0')} = {(byte)~expr1}");
Console.WriteLine($"~expr2 = {Convert.ToString((byte)~expr2, 2).PadLeft(8, '0')} = {(byte)~expr2}");
```

Run it. `&`, `|`, and `^` compare integers bit by bit and hand back an integer, not a `bool`. On `bool` operands they behave like their logical cousins `&&`/`||`, minus one crucial thing: no short-circuiting. `&&` skips evaluating its right-hand side entirely if the left side is already `false`, which is the mechanism behind the most common null guard you'll ever write:

```csharp
if (obj != null && obj.SomeProperty == 5)
```

If `obj` is `null`, `obj.SomeProperty` is never touched, `&&` never gets that far. Swap in the bitwise `&` and that protection is gone, `&` always evaluates both sides regardless.

`Convert.ToString(value, 2).PadLeft(8, '0')` is worth stealing outright, it's the standard .NET Framework idiom for printing a number in binary, and `PadLeft` restores the leading zeros `ToString` otherwise drops.

The `~` lines have their own trap: apply `~` to a `byte` and you don't get a `byte` back. C# promotes the operand to a signed 32-bit `int` first, complements all 32 bits, and hands you back a number nowhere near what you expected unless you cast back down explicitly. That `(byte)` cast is load-bearing, not decoration, remove it and the output changes completely.

**Logical and bitwise operator quick reference:**

| Operator | Meaning |
|---|---|
| `&` | Bitwise AND |
| `\|` | Bitwise OR |
| `^` | Bitwise Exclusive OR (XOR) |
| `!` | Logical Negation (NOT) |
| `~` | Bitwise Complement |
| `&&` | Logical AND (short-circuits) |
| `\|\|` | Logical OR (short-circuits) |

**Truth tables for reference:**

Negation (NOT): `!true` = `false`, `!false` = `true`

Conjunction (AND): only `true && true` produces `true` -- both sides must be true.

Disjunction (OR): only `false || false` produces `false` -- at least one side must be true.

There's no logical XOR operator in C# -- no `^^` to match `&&`/`||`. If you need "exactly one of these is true" with booleans rather than bits, any of these three are equivalent:

```csharp
(expr1 || expr2) && !(expr1 && expr2)
(expr1 || expr2) && (!expr1 || !expr2)
(expr1 && !expr2) || (!expr1 && expr2)   // parentheses unnecessary, && binds tighter than ||
```

None of them announce themselves as XOR the way bitwise `^` does, which is exactly why it's worth recognizing the shape on sight.

### Mini-Program 6: The Ternary Operator

```csharp
byte expr1 = 15;
byte expr2 = 10;
string result = expr1 > expr2 ? "" : "not ";
Console.WriteLine($"{expr1} is {result}greater than {expr2}");
```

Run it. `condition ? valueIfTrue : valueIfFalse`. It's shorthand for exactly this:

```csharp
string result;
if (expr1 > expr2)
{
    result = "";
}
else
{
    result = "not ";
}
```

Six lines collapsed into one, and that's its entire job description: choosing between two *values*, not choosing between two branches of real work. The moment either arm of a ternary needs more than a single expression, go back to `if`/`else`.

### Mini-Program 7: Use of Bool

```csharp
bool result;
result = 2 == 2;
Console.WriteLine($"result = {result}");
```

Run it. `2 == 2` isn't something that only works inside an `if`. It's an expression, it evaluates to `true`, and `true` can be stored in a variable exactly like any other value. `result` also gets declared without initialization first, on purpose, same reasoning as Mini-Program 1: local variables demand explicit assignment before they're read, and C# enforces it at compile time rather than leaving it to hope.

### Mini-Program 8: If, Else, and Else If

```csharp
int x = 1;
int y = 2;

if (true) Console.WriteLine("This statement still executes.");

if (x < y)
{
    Console.WriteLine($"{x} is less than {y}");
}

x = 3;

if (x > y)
{
    Console.WriteLine($"{x} is greater than {y}");
}
else
{
    Console.WriteLine($"{x} is not greater than {y}");
}

x = 2;

if (x < y)
{
    Console.WriteLine($"{x} is less than {y}");
}
else if (x > y)
{
    Console.WriteLine($"{x} is greater than {y}");
}
else
{
    Console.WriteLine($"{x} is equal to {y}");
}
```

Run it. `x` gets reassigned between each check on purpose, so all three branch shapes actually fire as you watch, rather than you reading about a branch that never executed.

That first line, `if (true) Console.WriteLine(...)` with no braces, is shown exactly once and never again. Braces around a single-statement `if` body aren't strictly required, they're optional right up until someone adds a second line underneath later, indents it to match, and assumes it's part of the condition when it silently isn't. That's not a hypothetical, it's a real, well-documented shape of production bug. Type the braces anyway, always, even when the compiler says you don't have to.

One more thing worth noticing on the way past: `else if` isn't a keyword. It's an `else` whose one governed statement happens to be another `if`. That's the entire mechanism that lets you chain as many of them as you want, and it's why the final `else` in a chain always binds to the nearest preceding `if`, not the first one.

### Mini-Program 9: Nested If Statements

```csharp
int first = 2;
int second = 0;

if (first == 2)
{
    Console.WriteLine("The if statement evaluated to true");
}
Console.WriteLine("This line outputs regardless of the if condition");

if (first == 2 && second == 0)
{
    Console.WriteLine("The if statement evaluated to true");
}
Console.WriteLine("This line outputs regardless of the if condition");

if (first == 2)
{
    if (second == 0)
    {
        Console.WriteLine("Both outer and inner conditions are true.");
    }
    Console.WriteLine("Outer condition is true, inner may be true.");
}
Console.WriteLine("This line outputs regardless of the if condition");
```

Run it. Watch that last block closely. The line `"Outer condition is true, inner may be true."` belongs to the *outer* `if`, it runs whenever `first == 2`, entirely regardless of `second`. Flatten this into `if (first == 2 && second == 0)` and there's simply nowhere left to put that line, the flattened version can only run code when *both* conditions hold. Nesting exists precisely so you have somewhere to put work that depends on the outer condition alone. If you find yourself nesting and there's nothing that belongs in that outer-only space, that's the signal to flatten it back down.

### Mini-Program 10: Switch Statements

```csharp
string condition = "Hello";
Console.WriteLine($"condition = {condition}");

switch (condition)
{
    case "Good Morning":
        Console.WriteLine("Good morning to you!");
        break;
    case "Hello":
        Console.WriteLine("Hello to you too.");
        break;
    case "Good Evening":
        Console.WriteLine("Have a wonderful evening!");
        break;
    default:
        Console.WriteLine("Good bye...");
        break;
}

var r = new Random();
int number = r.Next(0, 9);
switch (number)
{
    case 0:
    case 1:
        Console.WriteLine($"Number [{number}] could be binary, octal, or decimal.");
        break;
    case 2:
    case 3:
    case 4:
    case 5:
    case 6:
    case 7:
        Console.WriteLine($"Number [{number}] could be octal or decimal.");
        break;
    default:
        Console.WriteLine($"Number [{number}] must be decimal.");
        break;
}
```

Run it a few times in a row, the random number means the second `switch` won't say the same thing twice.

When you're comparing one variable against several possible values, a chain of `else if` gets unwieldy fast, `switch` exists for exactly that job. Two prohibitions worth memorizing rather than rediscovering the hard way: don't reach for `switch` when your branching depends on more than one variable, and don't reach for it to compare complex data types. Both of those still want `if`/`else if`.

C# allows switching on a string, which C and C++ don't, and it's genuinely useful, with one sharp edge: the comparison is ordinal and case-sensitive. `"hello"` here would fall straight through to `default`.

The second `switch` groups digits by a real mathematical fact rather than an arbitrary split: 0 and 1 are valid in base 2, base 8, and base 10 alike; 2 through 7 are valid in base 8 and base 10 but not base 2; 8 and 9 only work in base 10. The stacked `case 0: case 1:` syntax is what makes that grouping possible, an empty case falls straight through into the next one, sharing its body. The instant you put even one statement under `case 0:` on its own, it needs its own `break`, C# requires every non-empty case to end with an explicit jump, `break`, `return`, or `goto case`. Forget one and you get a compile error, not a silent bug, which is a meaningfully stricter rule than C or C++ enforce, and one of the rare cases where the compiler is doing you an active favor rather than just getting in the way.

`default` is optional. Leave it out, and a value that matches nothing simply skips the whole `switch` without complaint. Include it whenever "none of the above" is a case actually worth handling, which is more often than it first appears.

### Mini-Program 11: The `for` Loop, Plus the Lottery Numbers

```csharp
for (int i = 1; i <= 10; i++)
{
    Console.WriteLine($"i = {i}");
}

int[] range = new int[49];
int[] picked = new int[6];
Random rnd = new();

for (int i = 0; i < 49; i++)
{
    range[i] = i + 1;
}

for (int select = 0; select < 6; select++)
{
    picked[select] = range[rnd.Next(49)];
}

Console.WriteLine("Your lotto numbers are:");
for (int j = 0; j < 6; j++)
{
    Console.Write(" " + picked[j] + " ");
}
Console.WriteLine();
```

Run it. A `for` loop has three parts separated by semicolons: an initializer that runs once, a condition checked before every single pass including the first, and an iterator that runs after each pass through the body. The first loop here starts at `1` and uses `<=`, printing `1` through `10`. The more common idiom starts at `0` and uses `<`, which produces the same ten iterations, both are fine, mixing them up carelessly is where off-by-one errors are born.

Every loop needs two things: a condition that *can* become false, and something inside the loop that actually pushes it toward that outcome. Miss either one and you've built an infinite loop, and the miss is rarely as obvious as it sounds:

```csharp
// Because this iterates down (-1, -2, etc.), i will *always* be less than 10, and the loop never ends
for (int i = 0; i <= 10; i--)
{
    Console.WriteLine(i);
}
```

This has a perfectly good condition, `i <= 10` is entirely capable of becoming false, and an iterator moving the wrong direction relative to it. It compiles without a single warning. Technically it's not infinite, `i` eventually underflows past `int.MinValue` and wraps around to a positive number, which takes on the order of four billion iterations, close enough to infinite for anyone who has to wait for it.

Now the lottery numbers. `new int[49]` gives you 49 zeros, not 49 empty slots, array elements of a value type are zero-initialized the instant the array exists, unlike the local variables from Mini-Program 1 that demand explicit assignment before they're read. The `+ 1` in the fill loop exists purely to map array indices (0 through 48) onto lottery numbers (1 through 49). `Random rnd = new();` is a target-typed `new` expression, the compiler infers `Random` from the declared type on the left, so you don't repeat it on the right. `rnd.Next(49)` with one argument returns `0` through `48`, exclusive of the upper bound, which is exactly right here since it's indexing straight into `range`.

There's a bug still sitting in this code, and it's worth knowing it's there rather than assuming it's airtight: nothing removes a number from `range` once it's drawn, so this can pick the same number twice. A real lottery draw is without replacement. Fixing it properly is a genuinely worthwhile exercise on its own, look up either a swap-and-shrink approach or a full Fisher-Yates shuffle.

### Mini-Program 12: The `foreach` Loop, Plus Average Grades

```csharp
int[] numbers = [5, 10, 15, 20];
foreach (int number in numbers)
{
    Console.WriteLine($"number / 5 = {number / 5}");
}

int[] arrGrades = [78, 89, 90, 76, 98, 65];
int total = 0;
int gradeCount = 0;
double average;

foreach (int grade in arrGrades)
{
    total += grade;
    gradeCount++;
}

if (gradeCount == 0) total = gradeCount = 1;

average = (double)total / gradeCount;
Console.WriteLine($"Average grade = {average}");
```

Run it. `foreach` asks the collection for an enumerator and keeps going until the enumerator says stop, no index, no bounds check, no chance of an off-by-one mistake. The tradeoff: the iteration variable is read-only, you can't use `foreach` to modify elements in place, and modifying the collection itself mid-`foreach` throws `InvalidOperationException`. Reach for `for` with an explicit index, or build a new collection, if you actually need to change things as you go.

The grades half is the accumulator pattern, one variable collecting a running total, one counting iterations, both initialized before the loop starts because you can't add to something that doesn't exist yet.

The cast placement here is the entire lesson. `(double)total / gradeCount` casts `total` to `double` first, which makes the division itself a `double`-by-`int` operation, and C# promotes `gradeCount` to match, giving you genuine floating-point division. Compare that to `(double)(total / gradeCount)`, same characters rearranged, wildly different result: those parentheses force the integer division to happen *first*, truncating the answer, and only then widen the already-wrong result to `double`. 496 divided by 6 becomes 82 instead of 82.666, and the `(double)(...)` version is arguably worse than not casting at all, because `82.0` looks precise while being flatly incorrect. You only ever need *one* operand to be floating point, the other one gets promoted automatically to match.

The `if (gradeCount == 0) total = gradeCount = 1;` guard is worth keeping as a habit. Integer division by zero throws `DivideByZeroException` outright; floating-point division by zero doesn't throw at all, it quietly returns `Infinity` or `NaN`, neither of which is likely what you want printed to a screen. `gradeCount` is technically redundant here, `arrGrades.Length` would give you the same number without a loop, it's written longhand because the pattern generalizes: the moment you're iterating something that doesn't expose a `Length`, a stream, a data reader, counting as you go is the only option you have.

### Mini-Program 13: While and Do While

```csharp
int num = 0;
var r = new Random();
while (num != 10)
{
    num = r.Next(0, 11);
    Console.WriteLine($"num = {num}");
}

do
{
    Console.WriteLine("Note: Even though I made the condition false, this loop ran once.");
} while (false);
```

Run it. Use `while` or `do while` when you need to loop until some condition occurs and you're not the one controlling that condition from outside the loop, exactly the shape of "keep rolling until you hit a 10." Two details worth catching: `r.Next(0, 11)` uses an exclusive upper bound, so this actually produces `0` through `10`, write `Next(0, 10)` instead and this loop genuinely never terminates, since 10 would never come up. And the number of iterations here is fundamentally unknowable in advance, which is the entire justification for reaching for `while` over `for` in the first place.

`while` checks its condition before the body runs. `do while` checks after, which is the whole reason it exists: `do { ... } while (false);` has a condition that's already false, and the body still runs, exactly once, because the check happens at the bottom rather than the top. Reach for `do while` specifically when the first pass has to happen unconditionally, prompting someone before you have anything to validate yet is the textbook example. Note the semicolon after `while (false)`, it's required, and this is the one place in C# where the `while` keyword is followed by one.

### Mini-Program 14: A Catalog of For Loop Shapes

```csharp
for (int i = 0; i < 10; i++) Console.WriteLine($"i = {i}");     // count up by one
for (int i = 10; i > 0; i--) Console.WriteLine($"i = {i}");     // count down by one
for (int i = 0; i < 10; i += 2) Console.WriteLine($"i = {i}");  // count up by two
for (int i = 5; i < 1000; i *= 5) Console.WriteLine($"i = {i}"); // count up by multiples of five
```

Run it. Four variations on the same underlying idea, deliberately repetitive. The counting-up and counting-down loops are near mirror images with a small, easy-to-miss asymmetry: one excludes its upper bound, the other includes its start and excludes zero, exactly the kind of asymmetry off-by-one errors live in.

The multiply-by-5 loop makes a point the syntax alone doesn't: the iterator clause is arbitrary code, it can add, multiply, or do anything else you write there, and the compiler makes no attempt to verify it actually makes progress toward the exit condition. Change that loop's initializer to `int i = 0` and `i *= 5` leaves `i` at zero, forever, with no warning at compile time.

### Mini-Program 15: Arithmetic Operators

```csharp
int a = 4;
int b = 2;
Console.WriteLine($"a = {a} and b = {b}");

int c = +1;
Console.WriteLine($"c = {c}");
int d = -1;
Console.WriteLine($"d = {d}");

c = a + b;
Console.WriteLine($"{a} + {b} = {c}");
c = a - b;
Console.WriteLine($"{a} - {b} = {c}");
c = a * b;
Console.WriteLine($"{a} * {b} = {c}");
c = a / b;
Console.WriteLine($"{a} / {b} = {c}");

c = 5;
Console.Write($"{c} += 5 yields ");
c += 5;
Console.WriteLine(c);
Console.Write($"{c} -= 5 yields ");
c -= 5;
Console.WriteLine(c);
Console.Write($"{c} *= 2 yields ");
c *= 2;
Console.WriteLine(c);
Console.Write($"{c} /= 2 yields ");
c /= 2;
Console.WriteLine(c);

d = c % b;
Console.WriteLine($"{c} % {b} = {d}");
```

Run it. Unary `+` is a real operator that genuinely does nothing, `+1` is simply `1`. It exists for symmetry with unary `-` and because a custom type can technically overload it, you will essentially never type it on purpose. Integer division truncating toward zero is the thing actually worth remembering here: `a / b` above is `4 / 2`, a clean `2`, but try `5 / 2` and you get `2` back, not `2.5`, not `3`, no rounding, just truncation.

`Console.Write` (no `Line`) before each compound-assignment example puts the before-value and after-value on the same output line, a small formatting trick genuinely worth stealing. `c += 5` is shorthand for `c = c + 5`, every arithmetic operator has a compound form, and so do the bitwise ones (`&=`, `|=`, `^=`, `<<=`, `>>=`).

Modulus, `%`, returns the remainder after division, the same operator that made `num % 2 == 0` work as an even-number test back in Mini-Program 2. Watch it with negative operands: `-7 % 3` in C# is `-1`, not `2`, the result takes the sign of the dividend, and if you need a mathematically positive modulus you have to adjust for it by hand.

### Mini-Program 16: Precedence

```csharp
Console.WriteLine($"2 + 2 * 2 = {2 + 2 * 2}");
Console.WriteLine($"(2 + 2) * 2 = {(2 + 2) * 2}");
```

Run it. Multiplication and division happen before addition and subtraction, the same order of operations you learned in grade school. Parentheses, processed inner to outer and then left to right, are how you override that whenever the math you actually want disagrees with the default order. The practical advice isn't to memorize the full precedence table, it's to reach for parentheses the instant a reader might have to stop and think about order, they cost nothing at runtime and are dramatically cheaper than the debugging session the ambiguity would otherwise cost.

### Mini-Program 17: Increment and Decrement

```csharp
int a = 0;
Console.WriteLine($"a = {a}");

a = a + 1;
Console.WriteLine($"a = {a}");
a += 1;
Console.WriteLine($"a = {a}");
a++;
Console.WriteLine($"a = {a}");
++a;
Console.WriteLine($"a = {a}");

a = a - 1;
Console.WriteLine($"a = {a}");
a -= 1;
Console.WriteLine($"a = {a}");
a--;
Console.WriteLine($"a = {a}");
--a;
Console.WriteLine($"a = {a}");

Console.WriteLine("Prefix");
Console.WriteLine($"a = {++a}");
Console.WriteLine($"a = {a}");

Console.WriteLine("Postfix");
Console.WriteLine($"a = {a++}");
Console.WriteLine($"a = {a}");

Console.WriteLine($"{Environment.NewLine}Using postfix in a for loop iterator...");
for (int i = 0; i < 5; i++)
{
    Console.Write($"{(i > 0 ? ", " : "")}{i}");
}

Console.WriteLine($"{Environment.NewLine}Using prefix in a for loop iterator...");
for (int i = 0; i < 5; ++i)
{
    Console.Write($"{(i > 0 ? ", " : "")}{i}");
}
```

Run it. The first eight lines all change `a` identically, `a = a + 1`, `a += 1`, `a++`, and `++a` are four spellings of the same operation, and as standalone statements they're genuinely interchangeable. The difference only becomes visible the moment the increment happens *inside* an expression that also reads the value. Prefix, `++a`, increments first and hands back the new value. Postfix, `a++`, hands back the current value and increments afterward. Same variable, same eventual result, different value captured by whatever's reading it in that exact statement.

There's exactly one place this genuinely doesn't matter: a `for` loop's own iterator clause. `for (int i = 0; i < 5; i++)` and `for (int i = 0; i < 5; ++i)` produce identical output, because the iterator step runs after the loop body executes on its own line, never inside an expression that's consuming the value in the same breath. The two loops at the end prove exactly that by running both versions and printing the same sequence twice.

---

## Seeing It All Together

Seventeen small, disposable programs, each one gone the moment you moved to the next. That's the right way to learn each idea in isolation, but it's also not how you'd want to actually ship a chapter's worth of runnable demos, nobody wants to retype `Main()` from scratch every time they want to revisit the `switch` example.

This project's own `Program.cs` is that shipped version. It takes every mini-program above, wraps each one in its own named method (`SimpleStatements()`, `ConditionalOperators()`, `UsingLoops()`, and so on), and calls them all in turn from a single `Main()`, pausing between each so you can read the output before the next one clears the screen. That's a preview of a tool you don't have yet, breaking code into named, callable pieces, which gets its own proper treatment in a later chapter. For now, it's fine to open that file, recognize the code you just wrote scattered across it, and not fully understand *why* it's organized that way. You will.

---

## Run It Yourself

- **Trigger the semicolon bug on purpose.** Write `if (false); { Console.WriteLine("gotcha"); }` somewhere and run it. Then explain out loud, to somebody else if you can, why it printed anyway.
- **Break the average deliberately.** Change Mini-Program 12's cast to `(double)(total / gradeCount)` and watch the answer become `82` instead of `82.666666...`, worse than an obviously wrong answer, because it looks plausible.
- **Hang the `while` loop.** Change `r.Next(0, 11)` to `r.Next(0, 10)` in Mini-Program 13 and understand exactly why it never finishes.
- **Fix the lottery duplicates.** Make the draw happen without replacement. Look up Fisher-Yates afterward and compare it to whatever you came up with on your own.
- **Rewrite the stacked `switch` as `if`/`else if`.** Count the resulting lines, then decide honestly which version you'd rather maintain in six months.
- **Prove compound assignment's implicit cast.** Try `byte b = 10; b += 300;` next to `byte b = 10; b = b + 300;` side by side, and explain why only one of the two compiles.
