# BasicProgramStructure

## Introduction

Program flow: how to say "do this, then that," "do this only if," and "do this over and over" in C#. Along the way you'll pick up the operators those structures depend on.

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
expr1 = expr2  // evaluates to whatever expr2 is (see below)
```

This distinction matters because `expr1 = expr2` is an expression, not just a statement, it evaluates to the value being assigned. That's exactly what makes it possible to accidentally write `=` where you meant `==` inside a condition, C# doesn't stop you, because `x = 5` is a perfectly good expression on its own.

Most lines of code you'll write are statements built out of expressions, a statement wraps an expression and a semicolon around it, either discarding the result or keeping it (as in `int counter = 0;`, which stores the expression's value in a variable).

---

## Statements and Blocks

A **statement** is one instruction, ended with a semicolon.

```csharp
int counter;
float distance;
string firstName;

counter = 0;
distance = 4.5f;
firstName = "Bill";

const string instructorName = "Scott McLean";
```

A **block** is a group of statements wrapped in `{ }`. Put multiple simple statements inside a block and you've got a **complex statement**:

```csharp
int[] numbers = [5, 24, 36, 19, 45, 60, 78];
int evenNums = 0;

foreach (int num in numbers)
{
    Console.WriteLine(num);
    if (num % 2 == 0)
    {
        evenNums++;
    }
}

Console.WriteLine($"Found {evenNums} even number{(evenNums == 1 ? "" : "s")}");
```

The `foreach` loop above is a complex statement, an `if` inside a loop, both blocks in their own right.

There's also the empty statement, a bare `;` sitting on its own line. It's legal and does nothing, and it exists mostly as a footgun for a stray semicolon after an `if` condition:

```csharp
if (x == 5); // this semicolon ends the if statement right here
{
    // this block always runs, regardless of x
}
```

---

## Comparison and Logical Operators

### Relational Operators

```csharp
byte expr1 = 1;
byte expr2 = 2;
Console.WriteLine($"expr1 < expr2 ? {expr1 < expr2}");
Console.WriteLine($"expr1 > expr2 ? {expr1 > expr2}");
Console.WriteLine($"expr1 <= expr2 ? {expr1 <= expr2}");
Console.WriteLine($"expr1 >= expr2 ? {expr1 >= expr2}");
Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
Console.WriteLine($"expr1 != expr2 ? {expr1 != expr2}");
```

`<`, `>`, `<=`, `>=`, `==`, `!=`. All six always return a `bool`.

Watch the difference between `=` and `==` closely:

```csharp
Console.WriteLine($"expr1 == expr2 ? {expr1 == expr2}");
Console.WriteLine($"expr1 = expr2 ? {expr1 = expr2}");
```

The second line uses `=` (assignment) instead of `==` (equality). It compiles fine, because `expr1 = expr2` is itself an expression that evaluates to the assigned value, and it also just quietly overwrote `expr1`. This is a very common logic error and can be difficult to find in a complex program, so be careful.

### Logical and Bitwise Operators

| Operator | Meaning |
|---|---|
| `&` | Bitwise AND |
| `\|` | Bitwise OR |
| `^` | Bitwise Exclusive OR (XOR) |
| `!` | Logical Negation (NOT) |
| `~` | Bitwise Complement |
| `&&` | Logical AND |
| `\|\|` | Logical OR |

`&&` and `||` **short-circuit**: `&&` skips evaluating the right side if the left side is already `false`, `||` skips it if the left side is already `true`. There is no logical XOR operator, but it can be approximated:

```csharp
(expr1 || expr2) && !(expr1 && expr2)
  // or
(expr1 || expr2) && (!expr1 || !expr2)
  // or
(expr1 && !expr2) || (!expr1 && expr2)
  // note: in the last example, the parentheses are not necessary
  //       due to && having precedence over ||
```

**Truth tables**, for reference:

Negation (NOT)

| ! | x |
|---|---|
| F | t |
| T | f |

Conjunction (AND)

| x | && | y |
|---|---|---|
| t | T | t |
| f | F | t |
| t | F | f |
| f | F | f |

Disjunction (OR)

| x | \|\| | y |
|---|---|---|
| t | T | t |
| f | T | t |
| t | T | f |
| f | F | f |

Bitwise operators don't return boolean values, they compare each bit and return 1 or 0 bit values for the result:

```csharp
expr1 = 15; // Binary 00001111
expr2 = 10; // Binary 00001010
Console.WriteLine($"expr1 & expr2 = {Convert.ToString(expr1 & expr2, 2).PadLeft(8, '0')} = {expr1 & expr2}");
// expr1 & expr2 = Binary 00001010 = 10
Console.WriteLine($"expr1 | expr2 = {Convert.ToString(expr1 | expr2, 2).PadLeft(8, '0')} = {expr1 | expr2}");
// expr1 | expr2 = Binary 00001111 = 15
Console.WriteLine($"expr1 ^ expr2 = {Convert.ToString(expr1 ^ expr2, 2).PadLeft(8, '0')} = {expr1 ^ expr2}");
// expr1 ^ expr2 = Binary 00000101 =  5
```

The bitwise `~` operator has a gotcha:

```csharp
// The bitwise ~ operator returns a signed 32-bit integer by default, regardless of the data
// type being complemented, so be sure to cast the result where necessary to get the expected results.
Console.WriteLine($"~expr1 = {Convert.ToString((byte)~expr1, 2).PadLeft(8, '0')} = {(byte)~expr1}");
// ~expr1 = Binary 11110000 = 240
```

### The Ternary Conditional Operator

```csharp
string result = expr1 > expr2 ? "" : "not ";
Console.WriteLine($"{expr1} is {result}greater than {expr2}");
```

The syntax is `condition ? valueIfTrue : valueIfFalse`. This is equivalent to:

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

---

## If, Else, and Else If

```csharp
if (true) Console.WriteLine("This statement still executes.");
```

A single statement after an `if` doesn't strictly need braces, but it's recommended to always surround the statement governed by an `if` with curly braces.

```csharp
int x = 1;
int y = 2;

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

### Code Lab: Using If Statements

```csharp
int first = 2;
int second = 0;

// A single if statement
if (first == 2)
{
    Console.WriteLine("The if statement evaluated to true");
}

// An if statement evaluating two conditions
if (first == 2 && second == 0)
{
    Console.WriteLine("The if statement evaluated to true");
}

// Nested if statements
if (first == 2)
{
    if (second == 0)
    {
        Console.WriteLine("Both outer and inner conditions are true.");
    }
    Console.WriteLine("Outer condition is true, inner may be true.");
}
```

Three different ways of combining the same logic. The nested version makes visible what `&&`'s short-circuit behavior does under the hood: an outer check that gates whether the inner check even runs at all.

---

## Switch Statements

```csharp
string condition = "Hello";

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
```

When comparing possible values of a single variable, a `switch` is cleaner than a chain of `else if`. Don't use it if your decision branching depends on multiple variables, or to compare complex data types.

Several values can share the same result by stacking cases:

```csharp
int number = 5;

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

---

## Loops

### for

```csharp
for (int i = 1; i <= 10; i++)
{
    Console.WriteLine(i);
}
```

Use a `for` loop when you want to execute instructions a specified number of times.

Every loop needs an exit point, a condition that eventually becomes false, and something inside the loop that pushes it toward that outcome. Miss either one and you get an infinite loop:

```csharp
// Because this iterates down (-1, -2, etc.), i will *always* be less than 10, and the loop never ends
for (int i = 0; i <= 10; i--)
{
    Console.WriteLine(i);
}
```

### foreach

```csharp
int[] numbers = [5, 10, 15, 20];
foreach (int number in numbers)
{
    Console.WriteLine(number / 5);
}
```

Use `foreach` when you have a collection and want to perform an action on every item in it.

### while and do while

```csharp
int num = 0;
var r = new Random();
while (num != 10)
{
    num = r.Next(0, 11);
    Console.WriteLine(num);
}

do
{
    Console.WriteLine("Note: Even though I made the condition false, this loop ran once.");
} while (false);
```

Use `while` or `do while` when you need to loop until a condition occurs, but you're not controlling that condition outside the loop. The two are very similar, but `while` checks its condition before executing, while `do while` checks after, meaning a `do while` loop always runs at least once, even if the condition starts out false.

### Code Lab: Lottery Program

```csharp
int[] range = new int[49];
int[] picked = new int[6];
Random rnd = new();

// populate the range with values from 1 to 49
for (int i = 0; i < 49; i++)
{
    range[i] = i + 1;
}

// pick 6 random numbers
for (int select = 0; select < 6; select++)
{
    picked[select] = range[rnd.Next(49)];
}

Console.WriteLine("Your lotto numbers are:");
for (int j = 0; j < 6; j++)
{
    Console.Write(" " + picked[j] + " ");
}
```

### Code Lab: Average Grades

```csharp
int[] arrGrades = [78, 89, 90, 76, 98, 65];

int total = 0;
int gradeCount = 0;
double average;

foreach (int grade in arrGrades)
{
    total += grade;   // add each grade value to total
    gradeCount++;     // increment counter for use in average
}

if (gradeCount == 0) total = gradeCount = 1;

average = (double)total / gradeCount;
Console.WriteLine(average);
```

Casting `total` to `double` before dividing matters here, dividing two `int`s performs integer division and truncates the result, `(double)total / gradeCount` forces the division to happen in floating point instead.

### Code Lab: For Loops

```csharp
// count up by one
for (int i = 0; i < 10; i++) Console.WriteLine(i);

// count down by one
for (int i = 10; i > 0; i--) Console.WriteLine(i);

// count up by two
for (int i = 0; i < 10; i += 2) Console.WriteLine(i);

// count up by multiples of five
for (int i = 5; i < 1000; i *= 5) Console.WriteLine(i);
```

The third clause of a `for` loop doesn't have to be `i++`. Any expression works, incrementing by a fixed amount, multiplying, whatever the problem calls for.

---

## Bonus: Arithmetic Operators

```csharp
int a = 4;
int b = 2;

int c = +1;  // Unary Plus
int d = -1;  // Unary Minus

c = a + b;  // Addition
c = a - b;  // Subtraction
c = a * b;  // Multiplication
c = a / b;  // Division

c = 5;
c += 5;  // Addition/Assignment
c -= 5;  // Subtraction/Assignment
c *= 2;  // Multiplication/Assignment
c /= 2;  // Division/Assignment

d = c % b;  // Modulus, returns the remainder when dividing
```

Compound assignment operators (`+=`, `-=`, `*=`, `/=`) perform a computation and assign the result back to the same variable in one step.

## Bonus: Precedence

```csharp
Console.WriteLine($"2 + 2 * 2 = {2 + 2 * 2}");        // 6, multiplication first
Console.WriteLine($"(2 + 2) * 2 = {(2 + 2) * 2}");    // 8, parentheses override precedence
```

Multiplication and division are processed before addition and subtraction, the same order of operations you learned in grade school. Parentheses override this, processed from inner to outer and then left to right.

## Bonus: Increment and Decrement

```csharp
int a = 0;

a = a + 1;   // long form
a += 1;      // compound assignment
a++;         // postfix increment
++a;         // prefix increment

a = a - 1;   // long form
a -= 1;      // compound assignment
a--;         // postfix decrement
--a;         // prefix decrement
```

When in the **prefix** position, the operation takes place before the variable's value is used. In the **postfix** position, the operation takes place after:

```csharp
Console.WriteLine("Prefix");
Console.WriteLine($"a = {++a}");   // increments first, then reads the new value

Console.WriteLine("Postfix");
Console.WriteLine($"a = {a++}");   // reads the current value, then increments
```

One place this genuinely doesn't matter: a `for` loop's iterator step. `for (int i = 0; i < 5; i++)` and `for (int i = 0; i < 5; ++i)` produce identical output, because the iterator step runs after the loop body executes, not inside an expression that consumes its value.
