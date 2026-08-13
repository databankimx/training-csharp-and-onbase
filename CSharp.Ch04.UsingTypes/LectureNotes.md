# Chapter 4: Using and Converting Data Types

## What This Chapter Is Actually About

Getting data from one type to another, safely, and understanding what "safely" actually means. Casting, `Parse`/`TryParse`, `System.Convert`, boxing, custom conversions, then a tour of the `string` class and the interop mechanisms (`DllImport`, COM) you'll eventually need to reach outside of managed .NET entirely.

`Program.cs` for this chapter is thick with inline commentary already, this file focuses on the parts worth a second explanation, plus a couple of things this pass actually fixed.

---

## Casting: Widening, Narrowing, and the `checked` Block

A **widening** conversion (small type into a bigger compatible one, `byte` into `int`) always succeeds, there's nowhere for data to get lost. A **narrowing** conversion (`int` into `byte`) only succeeds cleanly if the value actually fits.

```csharp
i = 264;
b = (byte)i;
// b is now 8, not 264. No exception. The extra bits just vanish.
```

`byte` tops out at 255, and 264 doesn't fit. C# doesn't stop you, it just silently keeps the low 8 bits and throws away the rest, `264 - 256 = 8`. No error, no warning at runtime, just a value that's confidently wrong.

Wrapping the same code in a `checked` block flips that behavior, the same overflow now throws an `OverflowException` instead of silently truncating:

```csharp
checked
{
    i = 264;
    b = (byte)i; // throws here instead of truncating
}
```

`checked` is scoped locally to the block it wraps, it does not propagate into methods called from inside it. Worth remembering, since it's easy to assume `checked` protects an entire call chain when it only protects the literal lines inside its braces.

---

## `is` and `as`: Checking and Casting Reference Types

```csharp
Person person = employee; // widening: Employee IS-A Person, always safe

Console.WriteLine(person is Employee ? "yes" : "no");   // type check, returns bool
var backToEmployee = person as Employee;                 // cast that returns null instead of throwing on failure
```

The underlying object never actually changes type here, `person` still IS an `Employee` object, in memory, the entire time. What changes is which *members* the compiler will let you reach through that variable. `person.Department` won't compile, even though the data is right there, because the compiler only knows about what `Person` declares. `as` and casting are how you get access back.

Arrays follow the same rule, and it comes with a trap worth knowing: `Employee[]` can be assigned to a `Person[]` variable (an array of a more specific type is compatible with an array of its base type), but that doesn't make it safe to put a `Manager` into it through the `Person[]` reference, the array's actual element type never changed, it's still an array of `Employee` underneath. `CastingArrays()` demonstrates the failure directly: casting a `Person[]` (that's really holding `Employee`s) to `Manager[]` compiles fine and throws at runtime, since the array's real contents were never `Manager` objects to begin with.

---

## Parse, TryParse, and `System.Convert`: Three Different Safety Nets

- **`int.Parse(str)`** throws on bad input. Simple, but a try/catch you'll need to remember every time.
- **`int.TryParse(str, out result)`** never throws, returns `false` and leaves `result` at its default instead. Safer by default, which is why it's the recommended one.
- **`System.Convert.ToInt32(value)`** is a different tool for a different job, converting *between numeric types*, not parsing strings, and it comes with its own gotcha:

```csharp
double income = 10.50;
int rounded = Convert.ToInt32(income); // 10, not 11
```

`Convert.ToInt32` uses **banker's rounding**: normal rounding except exactly `.5` rounds to the nearest *even* number instead of always rounding up. `9.50` rounds to `10` (as expected), but `10.50` also rounds to `10` (not `11`, which is probably what you assumed). This isn't a bug, it's intentional (banker's rounding reduces systematic bias when rounding large numbers of values), but it will absolutely produce a value that looks wrong if you're not expecting it, and there's no exception to warn you, just a number that's off by one in specific circumstances. If you want normal "always round .5 up" behavior, be explicit: `Math.Round(income, MidpointRounding.AwayFromZero)`.

`Convert` methods also throw on out-of-range values instead of silently truncating the way a raw cast does, `Convert.ToByte(300.00)` throws, `(byte)300` would just wrap around. Different failure mode for a similar-looking operation, worth knowing which one you're actually using.

---

## Boxing and Unboxing

```csharp
int num = 10;
object boxedNum = num;        // boxing: value type wrapped as an object on the heap
int unboxedNum = (int)boxedNum; // unboxing: unwrapped back to a value type
```

Value types normally live on the stack. The moment you need to treat one as an `object` (assign it to an `object` variable, pass it somewhere expecting `object`), the runtime has to box it, copy the value into a heap-allocated wrapper. This happens invisibly all the time, `string.Format("num is {0}", num)` boxes `num` on the way in, since the method's signature expects `object`. Not something to obsess over day-to-day, but worth recognizing as the mechanism when you hear people talk about boxing overhead in performance-sensitive code.

---

## Interop: `DllImport` and COM

`ImportedComDll()` calls straight into `user32.dll` and `kernel32.dll` via `[DllImport]`, unmanaged Windows APIs with no .NET wrapper needed. `ExcelInterop()` goes a step further, driving an actual running instance of Excel through COM interop. Both require Windows to run at all, `DllImport` because it's calling Windows-specific DLLs by name, `ExcelInterop()` additionally requires Microsoft Excel to actually be installed, since it launches a real Excel process and manipulates it live.

The Excel reference itself got a modernization pass: the original project referenced the Excel interop assembly the old way, a raw COM reference with `EmbedInteropTypes`. The SDK-style project instead pulls in the `Microsoft.Office.Interop.Excel` NuGet package as a normal `PackageReference`, cleaner, and consistent with how every other dependency in this solution is declared.

That switch comes with one real behavior change worth knowing, since it'll bite you the moment you try to build: the old `EmbedInteropTypes` reference let the compiler treat COM indexer members (`Worksheets[1]`, `Cells[row, col]`, `Columns[n]`) as their specific interop types. The plain `PackageReference` doesn't get that treatment, those members are typed as plain `object` in the interop assembly, so `Excel.Worksheet sheet = workbook.Worksheets[1];` fails to compile with `CS0266`, and every `.Value`/`.AutoFit()` call after it fails with `CS1061`, `object` genuinely doesn't have those members. The fix is `dynamic sheet = workbook.Worksheets[1];` instead of a specific type, which defers all of that member resolution to runtime. This isn't a workaround, it's the intended fix, COM interop exactly like this is the scenario `dynamic` was added to C# 4 to solve.

---

## The `dynamic` Type

```csharp
dynamic result = JObject.Parse(json);
Console.WriteLine($"{result.Id}: {result.Data.FirstName} {result.Data.LastName}");
```

`dynamic` defers type checking from compile time to runtime. `result.Id` and `result.Data.FirstName` aren't real compile-time members of anything, the compiler just trusts you and generates code that looks them up when the line actually executes. Useful for exactly this scenario, JSON with a shape you don't have (and don't want to write) a class for, but it trades away the safety net: `CloningArrays()` shows a `dynamic` array accepting an assignment of the wrong element type with zero compile-time complaint, and only failing when that line actually runs.

---

## Strings Are Immutable, and the Intern Pool Is Why `==` Sometimes Lies

Every string-returning method on `string` (`Trim()`, `Replace()`, `Substring()`, all of them) returns a *new* string rather than modifying the original, because strings can't be modified once created. `original.Replace("two", "222")` doesn't touch `original` at all, it hands back a different string and leaves the original exactly as it was.

That immutability is also why this looks surprising the first time you see it:

```csharp
string original = "12345";
string copied = string.Copy(original);
Console.WriteLine(original == copied); // true
```

`copied` and `original` are, technically, different objects, `string.Copy` really did allocate a second string. But `==` on strings compares *value*, not reference identity (unlike most other reference types), specifically because the language wants strings to behave like values in everyday comparisons. Combined with the intern pool (identical string literals across your whole program often share the exact same underlying memory), this is one of the few places in C# where a reference type quietly behaves like a value type for comparison purposes.

---

## `StringBuilder` vs. String Concatenation, and a Bug This Pass Fixed

`UsingStringBuilder()` generates every permutation of 8 letters two ways: repeated string concatenation (`permutations += word + newline`) and `StringBuilder.AppendLine`, timing both. String concatenation creates a new string on every single append (see immutability, above), for 40,320 permutations, that's 40,320 wasted intermediate strings. `StringBuilder` mutates an internal buffer in place instead, no throwaway allocations. The timing difference between the two approaches is the whole point of running them back to back.

Speaking of 40,320: the `Factorial()` helper used to compute the wrong number entirely.

```csharp
private static long Factorial(long number)
{
    long result = 1;
    for (int i = 2; i <= number; i++) result *= i; // was: i < number
    return result;
}
```

The loop condition was `i < number`, which stops one short and multiplies up through `number - 1` instead of `number` itself. `Factorial(8)` was returning `5040` (that's `7!`) instead of the correct `40320`. The actual permutation-generating code was never affected, it correctly produces all 40,320 permutations either way, only the printed summary line claiming to report that count was wrong. Fixed to `i <= number`.

---

## `string.Format` and String Interpolation

```csharp
Console.WriteLine(string.Format("{0} = {1,4} or 0x{2:X}", (char)i, i, i));
Console.WriteLine($"{(char)i} = {i,4} or 0x{i:X}");
```

Same output, two syntaxes. `string.Format` numbers its placeholders and matches them to positional arguments, which means arguments can be reused or reordered (`"{1} {4} {2} {1} {3}"` is completely legal, `{1}` just appears twice). Interpolation embeds the expression directly in the string instead of an index, more readable in the common case, which is why it's the team's preferred style, but you'll still run into `string.Format`-style code regularly enough that it's worth being able to read both. Both support the same `{index,alignment:format}` syntax after the value itself, alignment for padding, format specifiers (`X` for hex, `d`/`D` for dates, `c` for currency, and so on) for how the value gets rendered.

---

## Standard Format Specifiers

Both `.ToString("X")` and `$"{value:X}"` accept the same specifier letters, worth having as a reference rather than re-discovering by trial and error each time.

### Numeric

| Specifier | Meaning |
|---|---|
| `C` / `c` | Currency |
| `D` / `d` | Decimal |
| `E` / `e` | Scientific notation |
| `F` / `f` | Fixed point |
| `G` / `g` | General, whichever of fixed-point or scientific notation is shorter, like a calculator display |
| `N` / `n` | Number, includes thousands separators |
| `P` / `p` | Percent |
| `X` / `x` | Hexadecimal |

### Date/Time

| Specifier | Meaning | Example |
|---|---|---|
| `d` | Short date | `M/d/yyyy` |
| `D` | Long date | `dddd, MMMM d, yyyy` |
| `f` | "Full" with short time | `dddd, MMMM d, yyyy h:mm tt` |
| `F` | "Full" with long time | `dddd, MMMM d, yyyy h:mm:ss tt` |
| `g` | "General" with short time | `M/d/yyyy h:mm tt` |
| `G` | "General" with long time | `M/d/yyyy h:mm:ss tt` |
| `m` / `M` | Month and day | `MMMM d` |
| `t` | Short time | `h:mm tt` |
| `T` | Long time | `h:mm:ss tt` |
| `y` / `Y` | Month and year | `MMMM, yyyy` |

Note that case matters and means something different depending on the category, lowercase `d` is a numeric-formatting decimal, but also the date-formatting short-date specifier, C# tells them apart by the type of the value being formatted, not by anything in the format string itself.

---

## Bonus: Why decimal, Not double, for Money

This one's not from the textbook, it's adapted from an internal DataBank "Safe-Coding" guidance doc, and it earned a permanent spot in this chapter because it's exactly the kind of thing that looks like a rounding curiosity in a training exercise and looks like a production incident everywhere else.

```csharp
double d = 0.1 + 0.2;
Console.WriteLine(d); // 0.30000000000000004, not 0.3
```

`double` doesn't store the number you typed, it stores the closest binary approximation it can manage. `double` is built out of powers of two, and most everyday decimal fractions, `0.1`, `0.2`, don't have an exact binary equivalent, the same way `1/3` can't be written exactly in decimal. `decimal` is base-10 under the hood instead (128-bit, a scaled integer plus a power-of-10 exponent), so it represents `0.1m` and `0.2m` exactly. It speaks the same language money does.

```csharp
double total = 0;
for (int i = 0; i < 10; i++) total += 0.1;
Console.WriteLine(total); // 0.9999999999999999, not 1.0
```

The drift compounds. A fraction-of-a-cent error looks trivial in isolation, but currency math rarely happens in isolation, thousands or millions of additions, tax calculations, interest calculations, all chained together. Eventually somebody's total doesn't match the invoice, and there's no exception anywhere in the chain to point at, just a number that's quietly wrong.

```csharp
Console.WriteLine(total == 1.0 ? "Equal" : "Not Equal"); // Not Equal
```

Equality checks are where this gets people. `if (total == 1.0)` fails here, not because the logic is wrong, but because `total` was never actually `1.0` to begin with, it's `0.9999999999999999`, and `double` will happily let that discrepancy hide from you until exactly the wrong moment. `decimal`'s version of the same loop lands on exactly `1.0m`, every time, on every machine, because it's defined in terms of base-10 digits instead of relying on the finer points of IEEE 754 binary representation.

```csharp
decimal roundPrice = 19.995m;
decimal rounded = Math.Round(roundPrice, 2, MidpointRounding.ToEven);
Console.WriteLine(rounded); // 20.00
```

`decimal` also plays nicely with `Math.Round` and explicit `MidpointRounding` strategies, which matters for audit trails, financial systems are usually expected to round to the cent in specific, predictable ways, and `decimal` gets you there without the rounding already being thrown off before you reach the rounding step.

Worth knowing the trade-offs go both directions: `decimal` gives up range for precision (28-29 significant digits, but nowhere near `double`'s exponent range, it was never meant for scientific magnitudes), it's slower since it runs in software rather than on the CPU's floating-point hardware, and it's twice the size at 16 bytes versus 8. For business logic, none of that cost is visible. It is not a reason to reach for `double` on a pricing field.

One DataBank-specific consequence worth knowing by name: OnBase Currency Keywords don't support `double` precision. Using `double` there can, and eventually will, produce `Hyland.Common.Core.Keywords.Exceptions.InvalidKeywordValueException: Value has more decimal places than the keyword allows for Keyword Type 'Name' (ID)`. If you're touching a currency value anywhere near OnBase, `decimal` isn't optional.

The rule: use `decimal` for money, prices, tax calculations, financial reporting, anything where a person would be upset if the math didn't match what's printed on paper. Use `double`/`float` for scientific computation, graphics, physics, statistics, places that need a huge dynamic range and can tolerate a tiny relative error, where the values are measurements rather than currency. This isn't really a debate, it's picking the right tool for the domain, and money's domain is base 10.
