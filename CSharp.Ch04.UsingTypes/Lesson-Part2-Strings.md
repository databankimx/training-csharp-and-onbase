# Chapter 4 - Using and Converting Data Types (Part 2: Strings, Formatting, and decimal vs. double)

Continuing from [Part 1](Lesson.md), which covered casting, `Parse`/`Convert`, boxing, and interop. This half is the `string` class itself, top to bottom, plus one closing argument that's worth internalizing permanently: never let money touch a `double`.

---

## How to Write This Program

Same rules as Part 1 and every chapter before it: each mini-program below is the whole of `Main()`, on its own, run it, then move to the next.

### Mini-Program 20: Strings Are Immutable

```csharp
string s = "hello";
s.ToUpper();
Console.WriteLine(s); // still "hello" - the result was thrown away

s = s.ToUpper();
Console.WriteLine(s); // "HELLO" - this is what you actually meant
```

Run it. `hello`, then `HELLO`.

This is the single most common beginner mistake with strings, and the compiler won't warn you about it, because "call a method and ignore its return value" is perfectly legal C#. Every method on `string` that *looks* like it modifies the string in place actually returns a brand new string and leaves the original completely untouched. `string` is a reference type, stored on the heap like any other class, but it's immutable once created, there's no method anywhere on `string` that changes the characters of an existing instance.

### Mini-Program 21: Building Strings From Characters

```csharp
char[] fNameParts = ['M', 'a', 'r', 'i', 'a'];
string fName = new string(fNameParts);

char[] lNameParts = ['W', 'a', 'r', 'd', 'e', 'n'];
string lName = new string(lNameParts, 0, 6);

string padding = new string('*', 5);

Console.WriteLine(fName);
Console.WriteLine(lName);
Console.WriteLine(padding);
```

Run it. `Maria`, `Warden`, `*****`.

Three constructor shapes: build from an entire character array, build from a range within one (start index, then count), or repeat a single character a given number of times. That last one is a genuinely handy way to build a fixed-width separator or padding string without a loop.

### Mini-Program 22: Length, Indexing, and Why You Can't Assign Through It

```csharp
string value = "12345";
Console.WriteLine(value.Length);
Console.WriteLine(value[3]);

char[] valueChars = value.ToCharArray();
valueChars[3] = '9';
string modified = new string(valueChars);
Console.WriteLine(modified);
```

Run it. `5`, `4`, then `12395`.

The indexer reads a single character by position, exactly like an array. What it won't let you do is assign through it, `value[3] = '9';` simply doesn't compile, strings are immutable, so there's no "write" half of that indexer to call. `ToCharArray()` is how you get something you can actually mutate, work on the copy, then build a new string from it with `new string(chars)`.

### Mini-Program 23: Static String Methods

```csharp
Console.WriteLine(string.Compare("A", "A")); //  0, equal
Console.WriteLine(string.Compare("A", "B")); // -1, A sorts before B
Console.WriteLine(string.Compare("A", "a", StringComparison.CurrentCultureIgnoreCase)); // 0

string[] words = ["Development ", "is ", "fun!"];
Console.WriteLine(string.Concat(words));

string original = "12345";
string copied = string.Copy(original);
Console.WriteLine(original == copied); // true, even though they're different objects

string nullString = null;
Console.WriteLine(string.IsNullOrEmpty(nullString));  // true
Console.WriteLine(string.IsNullOrWhiteSpace("   "));  // true
```

Run it. `0`, `-1`, `0`, `Development is fun!`, `True`, `True`, `True`.

`string.Compare` is what sorting code reaches for. `string.Concat` joins several strings, or an array of them, with no separator. `string.IsNullOrEmpty`/`string.IsNullOrWhiteSpace` exist as **static** methods specifically so they can be called safely on a `null` reference, you genuinely cannot call an instance method on `null`, which is exactly the case you most need to check for.

That `original == copied` line is worth sitting with. `string.Copy` really did allocate a second, distinct object, `copied` and `original` are not the same reference. But `==` on strings compares *value*, not reference identity, unlike almost every other reference type in C#, specifically because the language wants everyday string comparisons to behave the way people intuitively expect values to behave. It's one of the few places a reference type quietly acts like a value type.

### Mini-Program 24: Instance String Methods

```csharp
string original = "one two three four five";

Console.WriteLine(original.Contains("one"));
Console.WriteLine(original.EndsWith("FIVE", StringComparison.CurrentCultureIgnoreCase));
Console.WriteLine(original.IndexOf("two", StringComparison.CurrentCultureIgnoreCase));
Console.WriteLine(original.Insert(4, "half "));
Console.WriteLine(original.Remove(7, 6));
Console.WriteLine(original.Replace("two", "222"));
Console.WriteLine(original.Substring(4, 3));
Console.WriteLine(original.StartsWith("ONE", StringComparison.CurrentCultureIgnoreCase));

Console.WriteLine(original); // unchanged by any of the above
```

Run it. Each line reflects its operation, and the final line proves `original` never budged, same immutability from Mini-Program 20, just across a wider set of methods.

### Mini-Program 25: Padding, Trimming, and Case

```csharp
Console.WriteLine($"[{"1".PadLeft(5, ' ')}]");
Console.WriteLine($"[{"1000".PadLeft(5, ' ')}]");

string padded = "          information          ";
Console.WriteLine($"[{padded.Trim()}]");
Console.WriteLine($"[{padded.TrimStart()}]");
Console.WriteLine($"[{padded.TrimEnd()}]");

Console.WriteLine("DataBank".ToUpper());
Console.WriteLine("DataBank".ToLower());
```

Run it. Fixed-width padding, then the three trim variants, then case conversion.

`PadLeft`/`PadRight` are how you build fixed-width output without hand-counting spaces. `Trim`/`TrimStart`/`TrimEnd` differ only in which end (or both) they clean up, worth knowing there are three, not just one, since only stripping a trailing newline and not a leading space (or vice versa) is a genuinely common, genuinely annoying bug.

### Mini-Program 26: StringBuilder vs. Concatenation

```csharp
static string letters = "ABCDEFGH";

static long Factorial(long number)
{
    long result = 1;
    for (int i = 2; i <= number; i++) result *= i;
    return result;
}

static void ConcatenatePermutations(ref string permutations, string letters, string word)
{
    if (letters.Length == 0)
    {
        permutations += word + Environment.NewLine;
    }
    else
    {
        for (int i = 0; i < letters.Length; i++)
        {
            char ch = letters[i];
            string newWord = word + ch;
            string newLetters = letters.Remove(i, 1);
            ConcatenatePermutations(ref permutations, newLetters, newWord);
        }
    }
}

static void StringBuilderPermutations(StringBuilder permutations, string letters, string word)
{
    if (letters.Length == 0)
    {
        permutations.AppendLine(word);
    }
    else
    {
        for (int i = 0; i < letters.Length; i++)
        {
            char ch = letters[i];
            string newWord = word + ch;
            string newLetters = letters.Remove(i, 1);
            StringBuilderPermutations(permutations, newLetters, newWord);
        }
    }
}
```

```csharp
Console.WriteLine($"Generating all {Factorial(letters.Length):N0} permutations of \"{letters}\"...");

var sw = Stopwatch.StartNew();
string concatenated = "";
ConcatenatePermutations(ref concatenated, letters, "");
sw.Stop();
Console.WriteLine($"String concatenation: {sw.ElapsedMilliseconds} ms");

sw.Restart();
var builder = new StringBuilder();
StringBuilderPermutations(builder, letters, "");
sw.Stop();
Console.WriteLine($"StringBuilder: {sw.ElapsedMilliseconds} ms");
```

Run it. Both approaches produce all 40,320 permutations of 8 letters, correctly, the `StringBuilder` version should finish noticeably faster.

Every `+=` in `ConcatenatePermutations` creates an entirely new string, copies the old contents into it, and abandons the previous one to the garbage collector, because, again, strings are immutable. For 40,320 permutations, that's 40,320 wasted intermediate allocations. `StringBuilder` mutates one internal buffer in place instead and only materializes a real `string` when something actually asks for one. Ten string concatenations in a loop is invisible. Forty thousand is a measurable, visible difference on the screen in front of you.

`Factorial(8)` should print `40,320`. If you're ever staring at a factorial helper that looks almost right but is quietly returning `5,040` instead, the fix is almost always a loop condition that should be `i <= number` running as `i < number`, stopping one multiplication short of the actual answer, a classic off-by-one that produces a plausible-looking wrong number rather than an obvious crash.

### Mini-Program 27: `ToString()` With Format and Culture

```csharp
double d = 12345.67890;
Console.WriteLine(d.ToString());
Console.WriteLine(d.ToString(CultureInfo.InvariantCulture));

Console.WriteLine(d.ToString("c"));
Console.WriteLine(d.ToString("c", CultureInfo.CreateSpecificCulture("en-US")));
Console.WriteLine(d.ToString("c", CultureInfo.CreateSpecificCulture("en-GB")));

int i = 1234567890;
Console.WriteLine(i.ToString("0,0"));
Console.WriteLine(d.ToString("0,0.00"));
```

Run it. You'll see the same number rendered several genuinely different ways, including two different currency symbols for the exact same value.

`ToString()` accepts a format specifier to control exactly how a value renders, and for anything culture-sensitive (currency, dates, thousands separators), it's worth passing a `CultureInfo` explicitly rather than trusting whatever locale happens to be configured on the machine that ends up running your code. The same code producing `$12,345.68` on one machine and `£12,345.68` (or `12.345,68 €`) on another is exactly correct behavior for something a person is going to read, and exactly the wrong behavior for something a machine is going to parse back later.

### Mini-Program 28: `string.Format` and Interpolation

```csharp
int i = 163;
Console.WriteLine(string.Format("{0} = {1,4} or 0x{2:X}", (char)i, i, i));
Console.WriteLine($"{(char)i} = {i,4} or 0x{i:X}");

string text = string.Format("{1} {4} {2} {1} {3}", "who", "I", "therefore", "am", "think");
Console.WriteLine(text);
```

Run it. Both format lines produce identical output; the last line assembles a sentence from arguments used out of order, with one of them, `{1}`, reused twice.

`string.Format`'s placeholders are `{index[,alignment][:format]}`, numbered positions into the argument list, which is exactly what makes reordering and reuse possible. String interpolation's placeholders embed the expression directly instead of an index, `{name_or_expression[,alignment][:format]}`, more readable in the common case since the value and its position in the string are the same thing, and it's the preferred style for new code. You'll still run into `string.Format`-heavy code constantly, though, especially anywhere a format string itself needs to live somewhere separate from the code, a resource file for localization being the main legitimate reason to still reach for it.

`DateTime` also carries its own dedicated formatting methods worth knowing exist, `ToShortDateString()`, `ToLongDateString()`, `ToShortTimeString()`, `ToLongTimeString()`, alongside the general-purpose format specifiers below.

### Format Specifier Reference

Worth having as a lookup rather than rediscovering by trial and error each time.

**Numeric**

| Specifier | Meaning |
|---|---|
| `C` / `c` | Currency |
| `D` / `d` | Decimal |
| `E` / `e` | Scientific notation |
| `F` / `f` | Fixed point |
| `G` / `g` | General, whichever of fixed-point or scientific notation is shorter |
| `N` / `n` | Number, with thousands separators |
| `P` / `p` | Percent |
| `X` / `x` | Hexadecimal |

**Date/Time**

| Specifier | Meaning | Example |
|---|---|---|
| `d` | Short date | `M/d/yyyy` |
| `D` | Long date | `dddd, MMMM d, yyyy` |
| `f` | Full, short time | `dddd, MMMM d, yyyy h:mm tt` |
| `F` | Full, long time | `dddd, MMMM d, yyyy h:mm:ss tt` |
| `g` | General, short time | `M/d/yyyy h:mm tt` |
| `G` | General, long time | `M/d/yyyy h:mm:ss tt` |
| `m` / `M` | Month and day | `MMMM d` |
| `s` | Sortable (ISO 8601) | `2026-09-29T14:30:00` |
| `t` | Short time | `h:mm tt` |
| `T` | Long time | `h:mm:ss tt` |
| `y` / `Y` | Month and year | `MMMM, yyyy` |

Case matters here in a way that's easy to trip over: lowercase `d` means something different depending on what you're formatting, a numeric decimal specifier in one context, the date-formatting short-date specifier in another. C# tells the two apart from the type of the value being formatted, not from anything in the format string itself, so the same letter genuinely means two different things depending on what's sitting to the left of it. For anything a machine is going to read back later rather than a person, prefer `s` for dates and pass `CultureInfo.InvariantCulture` explicitly for numbers, for the exact same reason culture-sensitive rendering was worth flagging back in Mini-Program 27.

---

## Bonus: Why `decimal`, Not `double`, for Money

This one isn't from the textbook. It's earned a permanent place in this chapter because it looks like a rounding curiosity in a training exercise and looks like a production incident absolutely everywhere else.

```csharp
double d = 0.1 + 0.2;
Console.WriteLine(d); // 0.30000000000000004, not 0.3
```

Run it. That's not a typo in the output, it genuinely prints seventeen digits.

`double` is binary floating-point, it stores a value as a sum of powers of two, and most everyday decimal fractions, `0.1` very much included, simply have no exact representation in that scheme, the same underlying reason `1/3` can't be written exactly in decimal notation no matter how many digits you allow. `decimal` is base-10 under the hood instead, a 128-bit scaled integer plus a power-of-10 exponent, so it represents `0.1m` and `0.2m` exactly, because it's speaking the same base-10 language money is already written in.

```csharp
double total = 0;
for (int i = 0; i < 10; i++) total += 0.1;
Console.WriteLine(total); // 0.9999999999999999, not 1.0
Console.WriteLine(total == 1.0 ? "Equal" : "Not Equal"); // Not Equal

decimal totalM = 0;
for (int i = 0; i < 10; i++) totalM += 0.1m;
Console.WriteLine(totalM == 1.0m ? "Equal" : "Not Equal"); // Equal, every time
```

Run it. The `double` total prints `0.9999999999999999` and fails the equality check outright. The `decimal` version lands on exactly `1.0m`, every time, on every machine, because it's defined in terms of base-10 digits rather than the finer points of binary floating-point representation.

A single ten-quadrillionth of an error looks trivial in isolation. Currency math never actually happens in isolation, though, it's thousands or millions of additions, tax calculations, and interest calculations all chained together, and eventually somebody's running total quietly stops matching the invoice, with no exception anywhere in the chain to point a finger at, just a number that's wrong.

```csharp
decimal roundPrice = 19.995m;
decimal rounded = Math.Round(roundPrice, 2, MidpointRounding.ToEven);
Console.WriteLine(rounded); // 20.00
```

`decimal` also rounds predictably with `Math.Round` and an explicit `MidpointRounding` strategy, which matters anywhere a system needs to round to the cent in a specific, defensible, auditable way.

| Aspect | `double` | `decimal` |
|---|---|---|
| Base | Binary (base 2) | Base 10, scaled integer |
| Size | 8 bytes | 16 bytes |
| Precision | ~15-17 significant digits | 28-29 significant digits |
| Range | Very large (±5.0 × 10^308) | Smaller (±7.9 × 10^28) |
| Exact decimal fractions | No | Yes |
| Best for | Scientific, engineering, graphics | Currency, pricing, financial calculations |

Use `decimal` for money, prices, tax calculations, and financial reporting, basically anything where a real person would be upset if the math didn't match what's printed on the receipt. Use `double`/`float` for scientific computation, graphics, physics, statistics, anywhere that needs a huge dynamic range and can tolerate a tiny relative error, because the values in play are measurements, not currency. Worth knowing by name specifically because of where this work sometimes leads: OnBase currency keywords are backed by a decimal type, and pushing a `double`-derived value into one can produce more decimal places than the keyword accepts, surfacing as an `InvalidKeywordValueException` that's genuinely confusing to debug if you don't already know the real cause was a numeric type chosen several layers upstream. If a value is ever headed for a currency keyword, it should be `decimal` from the moment it's created, never converted through `double` along the way.

---

## Seeing It All Together

Twenty-eight small demos across two files, same deal as every chapter so far, this project's own `Program.cs` organizes all of it into named methods and runs them in sequence from one `Main()`. Open it once you're done with both parts and you'll recognize essentially everything, just packaged the way a full chapter of demos actually ships.

## Run It Yourself

- **Trigger the array covariance failure a different way.** Build a `Person[]` that's genuinely holding `Person` objects (not `Employee`s), and try casting *that* to `Employee[]`. Confirm it fails too, and explain why, unlike Mini-Program 6, this one was never going to work no matter what was in the array.
- **Break `ToBoolean()` on purpose.** Feed it something like `"maybe"` and confirm you get the `FormatException` the implementation promises, then extend the extension method's vocabulary to also accept `"sure"` and `"nope"`.
- **Time a bigger permutation set.** Change `letters` in Mini-Program 26 to 9 or 10 letters and rerun both approaches. Watch how much faster the `StringBuilder` gap grows relative to the plain-concatenation version as the permutation count climbs.
- **Find the `Convert.ToByte` boundary.** Binary-search by hand for the smallest `double` value that makes `Convert.ToByte` throw, and compare it to where a plain `(byte)` cast would have silently wrapped instead.
- **Prove the `decimal` claim wrong on purpose, if you can.** Try to construct a `decimal` calculation that still drifts the way the `double` one did. (You won't succeed with ordinary arithmetic, division that doesn't terminate cleanly in base 10 is the one real edge case, worth finding on your own.)
