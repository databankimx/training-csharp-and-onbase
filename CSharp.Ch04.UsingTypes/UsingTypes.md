# UsingTypes

## Introduction

Getting data from one type to another, safely, and understanding what "safely" actually means. Casting, `Parse`/`TryParse`, `System.Convert`, boxing, custom conversions, then a tour of the `string` class and the interop mechanisms you'll eventually need to reach outside of managed .NET entirely.

---

## Converting Between Types

```csharp
// A widening conversion (smaller to larger of similar basic type) will always work
byte b = 127;
int i = (int)b;
Console.WriteLine($"byte to int: {i}");

// A narrowing conversion (larger to smaller) works properly only if the value fits
i = 64;
b = (byte)i;
Console.WriteLine($"int to byte: {b}");
```

A **widening** conversion always succeeds, there's nowhere for data to get lost. A **narrowing** conversion only succeeds cleanly if the value actually fits.

```csharp
i = 264;
b = (byte)i;
Console.WriteLine($"int to byte with invalid value ({i}): {b}"); // 8, not 264
```

`byte` tops out at 255, and 264 doesn't fit. C# doesn't stop you, it silently keeps the low 8 bits and discards the rest.

```csharp
checked
{
    i = 264;
    b = (byte)i; // throws OverflowException instead of truncating
}
```

Wrapping the same code in a `checked` block flips that behavior, the same overflow now throws instead of silently truncating. `checked` is scoped locally to the block it wraps, it does not propagate into methods called from inside it.

```csharp
double big = -1E40;
float small = (float)big;
Console.WriteLine(float.IsInfinity(small)
    ? "Whoops! Must have overflowed the type..."
    : small.ToString(CultureInfo.InvariantCulture));
```

The same silent-failure risk applies to narrowing floating-point casts, checking `float.IsInfinity()` catches it.

```csharp
var employee = new Employee("Joe", "Programmer", "Development", "Software Engineer");
Person person = employee;

Console.WriteLine($"\"person\" is {(person is Employee ? "" : "not")} an Employee");
Console.WriteLine($"{person.FirstName} is a {(person as Employee).JobTitle}");
```

Converting from a child class to its parent is an implicit widening conversion. The underlying object doesn't change type, only which members the compiler lets you reach through that variable changes. `is` checks the type, `as` casts and returns the original access back.

---

## Casting Arrays

```csharp
Employee[] employees = new Employee[10];
for (int id = 0; id < employees.Length; id++) employees[id] = new Employee(id);

// Implicit cast to an array of Persons (an Employee is a type of Person)
Person[] persons = employees;

Manager[] managers = persons as Manager[]; // null, not convertible

// This cast fails at run time because the array holds Employees, not Managers
try
{
    managers = (Manager[])persons;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
```

Casting doesn't generate a new array, it references the existing one. `Employee[]` can be assigned to a `Person[]` variable, since an array of a more specific type is compatible with an array of its base type, but that doesn't make the array's actual contents any different, it's still holding `Employee` objects underneath, so casting to `Manager[]` fails at runtime.

---

## Parsing

```csharp
string numString = "10";
int number = int.Parse(numString);

// But C# offers a better way to handle bad input: TryParse()
if (int.TryParse("ten", out number))
{
    Console.WriteLine($"parses to int [{number}]...");
}
else
{
    Console.WriteLine("cannot be parsed to int...");
}
```

`int.Parse` throws on bad input. `int.TryParse` never throws, it returns `false` and leaves the output at its default instead, safer by default.

```csharp
string money = "1,000.00";
Console.WriteLine(decimal.Parse(money)); // handles grouping symbols fine

money = "$1,000.00";
// decimal.Parse(money) here throws, it can't handle the currency symbol by default

Console.WriteLine(decimal.Parse(money, NumberStyles.Currency));

// NumberStyles is a set of bit-flag values, so you can stack specific options
Console.WriteLine(decimal.Parse(money,
    NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands));
```

`decimal.Parse` can handle currency symbols and grouping, but only if you tell it to via the optional `NumberStyles` argument.

---

## System.Convert

```csharp
double income = 9.50;
int rounded = Convert.ToInt32(income);
// This yields 10, as we expect from normal 5/4 rounding

income = 10.50;
rounded = Convert.ToInt32(income);
// But this also yields 10 where we would have expected 11
```

Integer conversions via `System.Convert` implement **banker's rounding**: normal rounding except exactly `.5` rounds to the nearest even integer, not always up. `Math.Round(income, MidpointRounding.AwayFromZero)` gives you the "always round .5 up" behavior instead.

```csharp
try
{
    income = 300.00;
    byte tooSmall = Convert.ToByte(income);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
```

`Convert` methods throw exceptions when a value is out of range, instead of rolling over into unexpected values the way a raw cast does.

```csharp
rounded = (int)Convert.ChangeType(income, typeof(int));
```

`Convert.ChangeType` lets you use C# type aliases instead of the underlying .NET types. Since it's a generic method returning `object`, you need to cast the result back to your target type.

---

## System.BitConverter

```csharp
int packedValue = PackTwoIntegers(15, 25);
byte[] packedBytes = BitConverter.GetBytes(packedValue);

short left = BitConverter.ToInt16(packedBytes, 0);
short right = BitConverter.ToInt16(packedBytes, 2);
```

`System.BitConverter` converts values to and from byte arrays, useful for staging data into a stream or serializing a binary file.

```csharp
string text = "At every crossway...";
byte[] textBytes = Encoding.UTF8.GetBytes(text);
text = Encoding.UTF8.GetString(textBytes);
```

For strings, use `System.Text.Encoding` instead, since text encoding affects the resulting byte values.

---

## Boxing and Unboxing

```csharp
int num = 10;

object boxedNum = num;             // boxing: value type wrapped as an object
int unboxedNum = (int)boxedNum;    // unboxing: unwrapped back to a value type
```

Boxing converts a value type into an object. Value types normally live on the stack, boxing copies the value into a heap-allocated wrapper. This happens invisibly all the time, `string.Format("num is {0}", num)` boxes `num` on the way in, since the method's signature expects `object`.

---

## Custom Conversions

```csharp
// bool.Parse can't handle most of the values you'd expect it to
Console.WriteLine(bool.Parse("true"));   // works
Console.WriteLine(bool.Parse("yes"));    // throws FormatException

// bool.TryParse protects against exceptions but still can't handle "yes"/"no"/"1"/"0"
bool.TryParse("yes", out bool parsed);   // returns false

// A custom extension method can fill the gap
"yes".ToBoolean();  // true
"no".ToBoolean();   // false
"1".ToBoolean();    // true
"0".ToBoolean();    // false
```

The built-in `bool.Parse`/`TryParse` only understand `"true"`/`"false"`. When the built-in conversion methods don't cover your real-world input formats, a custom extension method can. `ToBoolean()`, `Parse()`, and `TryParse()` here are defined once as extension methods and reused everywhere a more forgiving boolean conversion is needed.

---

## DllImport and P/Invoke

```csharp
[DllImport("user32.dll", CharSet = CharSet.Unicode)]
private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
```

`[DllImport]` lets you call into unmanaged, native Windows DLLs directly. This declares the signature of a function that actually lives in `user32.dll`, not in your own code.

```csharp
MessageBox(new IntPtr(0), "Hello World!", "Hello Dialog", 0);
```

Calling it pops up a real native Windows message box.

```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
private static extern uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);
```

```csharp
string longName = Assembly.GetExecutingAssembly().Location;
char[] buffer = new char[1024];
long length = GetShortPathName(longName, buffer, buffer.Length);
string shortName = new string(buffer).Substring(0, (int)length);
```

`GetShortPathName` converts a full path into the old-style 8.3 short path format.

---

## Excel Interop (COM)

```csharp
Excel._Application excelApp = new Excel.Application();
Excel.Workbook workbook = excelApp.Workbooks.Add();
dynamic sheet = workbook.Worksheets[1];

excelApp.Visible = true;

sheet.Cells[1, 1].Value = "Value";
sheet.Cells[1, 2].Value = "Value Squared";

for (int i = 1; i <= 10; i++)
{
    sheet.Cells[i + 1, 1].Value = i;
    sheet.Cells[i + 1, 2].Value = (i * i).ToString();
}

sheet.Columns[1].AutoFit();
sheet.Columns[2].AutoFit();
```

COM interop lets managed .NET code drive an actual COM application, in this case Excel itself. `dynamic` is used for `sheet` since several of its members are typed generically in the interop assembly, `dynamic` defers exactly which members exist to runtime instead of requiring the compiler to know ahead of time.

---

## The dynamic Type

```csharp
const string json = "{\"Id\":\"1234-5678\",\"Data\":{\"FirstName\":\"Maria\",\"LastName\":\"Warden\"}}";
dynamic result = JObject.Parse(json);
Console.WriteLine($"{result.Id}: {result.Data.FirstName} {result.Data.LastName}");
```

A `dynamic` variable lets you access named members, but bypasses type-checking at compile time. Useful when you're working with a shape of data (like arbitrary JSON) that you don't have, and don't want to write, a class for.

---

## Cloning Arrays

```csharp
int[] array1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

// This works, because we're casting the result back to int[]
int[] array3 = (int[])array1.Clone();

// This also works
dynamic array4 = array1.Clone();
Console.WriteLine(array4[9]);
```

`Array.Clone()` returns a plain `object`, so you need to cast it back to your actual array type, or use `dynamic` to sidestep the cast, at the cost of losing compile-time type checking.

```csharp
try
{
    array4[0] = "one"; // no compiler-time error, but a runtime type mismatch
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
```

The tradeoff with `dynamic`, an assignment that's actually wrong for the array's real element type compiles fine and only fails when that line runs.

---

## Manipulating Strings

A `string` is a series of 16-bit Unicode characters. Unlike `char`/`char[]`, which are value types stored on the stack, `string` is a reference type stored on the heap, but it's immutable once created.

```csharp
char[] fNameParts = ['M', 'a', 'r', 'i', 'a'];
string fName = new string(fNameParts);

char[] lNameParts = ['W', 'a', 'r', 'd', 'e', 'n'];
string lName = new string(lNameParts, 0, 6);

string padding = new string('*', 5);
```

Three constructor forms: from a full character array, from a range within one, and from a character repeated a given number of times.

```csharp
string value = "12345";
Console.WriteLine(value.Length);          // Length property
Console.WriteLine(value[3]);              // indexer, reads a single character

char[] valueChars = value.ToCharArray();  // converts to an actual character array
```

The indexer lets you read a character by position, but you can't assign through it, `value[3] = '5';` won't compile, since strings are immutable.

---

## Static String Methods

```csharp
Console.WriteLine(string.Compare("A", "A")); //  0 (equal)
Console.WriteLine(string.Compare("A", "B")); // -1 (A before B)
Console.WriteLine(string.Compare("A", "a", StringComparison.CurrentCultureIgnoreCase)); // 0

string[] words = ["Development ", "is ", "fun!"];
Console.WriteLine(string.Concat(words));

string original = "12345";
string copied = string.Copy(original);
Console.WriteLine(original == copied); // true

string[] pieces = original.Split();
```

`string.Compare` is useful for sorting. `string.Concat` joins multiple strings or an array of strings. `string.Copy` creates a genuinely separate string object, and interestingly, `==` on strings still returns `true` for it, since `==` compares value, not reference identity, unlike most other reference types.

```csharp
string nullString = null;
string emptyString = string.Empty;
Console.WriteLine(string.IsNullOrEmpty(nullString));       // true
Console.WriteLine(string.IsNullOrWhiteSpace("   "));       // true
```

`string.IsNullOrEmpty` and `string.IsNullOrWhiteSpace` cover the common cases where a plain null check isn't enough.

---

## Instance String Methods

```csharp
string original = "one two three four five";

original.Contains("one");                                          // true
original.EndsWith("FIVE", StringComparison.CurrentCultureIgnoreCase); // true
original.IndexOf("two", StringComparison.CurrentCultureIgnoreCase);   // position
original.Insert(4, "half ");                                        // new string, original unchanged
original.Remove(7, 6);                                              // new string, original unchanged
original.Replace("two", "222");                                     // new string, original unchanged
original.Substring(4, 3);                                           // extracts a portion
original.StartsWith("ONE", StringComparison.CurrentCultureIgnoreCase); // true
```

Every one of these returns a *new* string rather than modifying `original`, since strings are immutable, the original variable's value never changes underneath you.

```csharp
"1".PadLeft(5, ' ');   // "    1"
"1000".PadLeft(5, ' '); // " 1000"

"          information          ".Trim();     // removes leading and trailing whitespace
"          information          ".TrimStart(); // removes only leading whitespace
"          information          ".TrimEnd();   // removes only trailing whitespace

"DataBank".ToUpper();  // "DATABANK"
"DataBank".ToLower();  // "databank"
```

---

## StringBuilder vs. String Concatenation

```csharp
string permutations = "";
ConcatenatePermutations(ref permutations, letters, "");
```

```csharp
private static void ConcatenatePermutations(ref string permutations, string letters, string word)
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
```

```csharp
StringBuilder permutationsBuilder = new StringBuilder();
StringBuilderPermutations(permutationsBuilder, letters, "");
```

```csharp
private static void StringBuilderPermutations(StringBuilder permutations, string letters, string word)
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

Both generate every permutation of a set of letters, one using repeated string concatenation, the other using `StringBuilder.AppendLine`. Because strings are immutable, every `+=` in the concatenation version creates a brand new string, for thousands of permutations, that's thousands of throwaway allocations. `StringBuilder` mutates an internal buffer in place instead. For large amounts of string building, `StringBuilder` is significantly faster.

---

## Using ToString()

```csharp
double d = 12345.67890;
Console.WriteLine(d.ToString());
Console.WriteLine(d.ToString(CultureInfo.InvariantCulture));

Console.WriteLine(d.ToString("c"));                                          // local currency format
Console.WriteLine(d.ToString("c", CultureInfo.CreateSpecificCulture("en-US"))); // US currency
Console.WriteLine(d.ToString("c", CultureInfo.CreateSpecificCulture("en-GB"))); // British currency

int i = 1234567890;
Console.WriteLine(i.ToString("0,0"));       // thousands separators
Console.WriteLine(d.ToString("0,0.00"));    // fixed decimal places
```

`ToString()` accepts format specifiers to control exactly how a value renders. For floating-point numbers, it's recommended to specify a culture explicitly.

---

## string.Format and String Interpolation

```csharp
int i = 163;
Console.WriteLine(string.Format("{0} = {1,4} or 0x{2:X}", (char)i, i, i));
Console.WriteLine($"{(char)i} = {i,4} or 0x{i:X}");
```

Same output, two syntaxes. `string.Format`'s placeholder syntax is `{index[,length][:format]}`, interpolation's is `{name_or_literal[,length][:format]}`. Interpolation embeds the expression directly instead of an index, more readable in the common case, and the preferred style.

```csharp
string text = string.Format("{1} {4} {2} {1} {3}", "who", "I", "therefore", "am", "think");
```

Argument indices in `string.Format` can be used in any order, and can repeat, `{1}` appears twice here.

```csharp
DateTime now = DateTime.Now;
Console.WriteLine(now.ToString("d"));
Console.WriteLine($"{now:d}");
Console.WriteLine(now.ToShortDateString());
```

`DateTime` also has its own dedicated `ToShortDateString()`, `ToLongDateString()`, `ToShortTimeString()`, and `ToLongTimeString()` methods alongside the general-purpose format specifiers.

---

## Standard Format Specifiers

### Numeric

| Specifier | Meaning |
|---|---|
| `C` / `c` | Currency |
| `D` / `d` | Decimal |
| `E` / `e` | Scientific notation |
| `F` / `f` | Fixed point |
| `G` / `g` | General, whichever of fixed-point or scientific notation is shorter |
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

---

## Bonus: Why decimal, Not double, for Money

```csharp
double d = 0.1 + 0.2;
Console.WriteLine(d); // 0.30000000000000004, not 0.3
```

`double` doesn't store the number you typed; it stores the closest binary approximation it can manage. `double` is built out of powers of two, and most everyday decimal fractions, `0.1`, `0.2`, don't have an exact binary equivalent, the same way `1/3` can't be written exactly in decimal. `decimal` is base-10 under the hood instead, so it represents `0.1m` and `0.2m` exactly. It speaks the same language money does.

```csharp
double total = 0;
for (int i = 0; i < 10; i++) total += 0.1;
Console.WriteLine(total); // 0.9999999999999999, not 1.0

Console.WriteLine(total == 1.0 ? "Equal" : "Not Equal"); // Not Equal
```

Small as it looks, this kind of error compounds. Currency math rarely happens in isolation, it's thousands of additions, tax calculations, and interest calculations chained together, and an equality check like `total == 1.0` can quietly fail even when the math looks like it should have landed exactly there.

```csharp
decimal dm = 0.1m + 0.2m;
Console.WriteLine(dm); // 0.3, exactly

decimal totalM = 0;
for (int i = 0; i < 10; i++) totalM += 0.1m;
Console.WriteLine(totalM == 1.0m ? "Equal" : "Not Equal"); // Equal, every time
```

Same math, `decimal` version, exact every time, on every machine, because it's defined in terms of base-10 digits rather than binary approximation.

```csharp
decimal roundPrice = 19.995m;
decimal rounded = Math.Round(roundPrice, 2, MidpointRounding.ToEven);
Console.WriteLine(rounded); // 20.00
```

`decimal` also rounds predictably with `Math.Round` and an explicit `MidpointRounding` strategy, which matters when a system needs to round to the cent in a specific, defensible way.

### Quick Reference

| Aspect | `double` | `decimal` |
|---|---|---|
| Base | Binary (base 2) | Base 10, scaled integer |
| Size | 8 bytes | 16 bytes |
| Precision | ~15-17 significant digits | 28-29 significant digits |
| Range | Very large (±5.0 × 10^308) | Smaller (±7.9 × 10^28) |
| Exact decimal fractions | No (e.g. `0.1` is approximate) | Yes (e.g. `0.1m` is exact) |
| Best for | Scientific / engineering / graphics math | Currency, pricing, financial calculations |

Use `decimal` for money, prices, tax calculations, and financial reporting, basically anything where a person would be upset if the math didn't match what's printed on paper. Use `double`/`float` for scientific computation, graphics, physics, and statistics, places that need a huge dynamic range and can tolerate a tiny relative error, where the values are measurements rather than currency.
