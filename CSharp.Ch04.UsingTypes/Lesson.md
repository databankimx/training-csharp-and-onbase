# Chapter 4: Using and Converting Data Types

## What This Chapter Is Actually About

Getting data from one type to another, safely, and understanding what "safely" actually means. Casting, `Parse`/`TryParse`, `System.Convert`, boxing, custom conversions, then a tour of the `string` class and the interop mechanisms (`DllImport`, COM) you'll eventually need to reach outside of managed .NET entirely.

`Program.cs` for this chapter is thick with inline commentary already. This document focuses on the parts worth a second explanation, plus a couple of things this pass actually fixed.

---

## Casting: Widening, Narrowing, and the `checked` Block

A **widening** conversion (small type into a bigger compatible one, `byte` into `int`) always succeeds; there's nowhere for data to get lost. A **narrowing** conversion (`int` into `byte`) only succeeds cleanly if the value actually fits.

```csharp
byte b = 127;
int i = (int)b;   // widening - the explicit cast is optional here

i = 64;
b = (byte)i;      // narrowing, and 64 fits, so this is fine

i = 264;
b = (byte)i;
// b is now 8, not 264. No exception. The extra bits just vanish.
```

`byte` tops out at 255, and 264 doesn't fit. C# doesn't stop you, it just silently keeps the low 8 bits and throws away the rest: `264 - 256 = 8`. No error, no warning at runtime, just a value that's confidently wrong.

Wrapping the same code in a `checked` block flips that behavior. The same overflow now throws an `OverflowException` instead of silently truncating:

```csharp
checked
{
    i = 264;
    b = (byte)i; // throws here instead of truncating
}
```

`checked` is scoped locally to the block it wraps. It does **not** propagate into methods called from inside it. Worth remembering, since it's easy to assume `checked` protects an entire call chain when it only protects the literal lines inside its braces.

Floating-point narrowing has the same "no complaint" problem, but a different symptom. Overflow there produces infinity rather than a wrapped-around value, so the check is different:

```csharp
double big = -1E40;
float small = (float)big;

Console.WriteLine(float.IsInfinity(small)
    ? "Whoops! Must have overflowed the type..."
    : small.ToString(CultureInfo.InvariantCulture));
```

`float.IsInfinity` (and `float.IsNaN`) are the guard rails here, because `checked` doesn't apply to floating-point math at all. That's a genuine gap people trip over: `checked` covers integral conversions and integral arithmetic, and nothing else.

---

## `is` and `as`: Checking and Casting Reference Types

```csharp
var employee = new Employee("Joe", "Programmer", "Development", "Software Engineer");
Person person = employee; // widening: Employee IS-A Person, always safe

Console.WriteLine(person is Employee ? "yes" : "no");   // type check, returns bool
var backToEmployee = person as Employee;                // cast that returns null instead of throwing
```

The underlying object never actually changes type here. `person` still IS an `Employee` object, in memory, the entire time. What changes is which *members* the compiler will let you reach through that variable. `person.Department` won't compile, even though the data is right there, because the compiler only knows about what `Person` declares. `as` and casting are how you get access back.

Three ways to do the same downcast, with different failure modes:

```csharp
var a = (Employee)person;              // throws InvalidCastException on failure
var b = person as Employee;            // returns null on failure
if (person is Employee emp) { ... }    // pattern matching: checks and assigns in one step
```

The third form is the modern default. It does the check and the cast once instead of twice, and it scopes `emp` to exactly the block where it's known to be valid.

### The Array Covariance Trap

Arrays follow the same assignment rule, and it comes with a trap worth knowing. `Employee[]` can be assigned to a `Person[]` variable (an array of a more specific type is compatible with an array of its base type), but that doesn't make it safe to put a `Manager` into it through the `Person[]` reference. The array's actual element type never changed; it's still an array of `Employee` underneath.

`CastingArrays()` demonstrates the failure directly: casting a `Person[]` (that's really holding `Employee`s) to `Manager[]` compiles fine and throws at runtime, since the array's real contents were never `Manager` objects to begin with. This is why generic collections are invariant by default — `List<Employee>` is deliberately *not* assignable to `List<Person>`, precisely to make this class of bug a compile error instead of a runtime one.

---

## Parse, TryParse, and `System.Convert`: Three Different Safety Nets

- **`int.Parse(str)`** throws on bad input. Simple, but it's a try/catch you'll need to remember every time.
- **`int.TryParse(str, out result)`** never throws; returns `false` and leaves `result` at its default instead. Safer by default, which is why it's the recommended one.
- **`System.Convert.ToInt32(value)`** is a different tool for a different job — converting *between numeric types*, not parsing strings — and it comes with its own gotcha:

```csharp
double income = 10.50;
int rounded = Convert.ToInt32(income); // 10, not 11
```

`Convert.ToInt32` uses **banker's rounding**: normal rounding, except exactly `.5` rounds to the nearest *even* number instead of always rounding up. `9.50` rounds to `10` (as expected), but `10.50` also rounds to `10` (not `11`, which is probably what you assumed).

This isn't a bug, it's intentional — banker's rounding reduces systematic bias when rounding large numbers of values — but it will absolutely produce a value that looks wrong if you're not expecting it, and there's no exception to warn you, just a number that's off by one in specific circumstances. If you want normal "always round .5 up" behavior, be explicit:

```csharp
Math.Round(income, MidpointRounding.AwayFromZero);
```

`Convert` methods also throw on out-of-range values instead of silently truncating the way a raw cast does. `Convert.ToByte(300.00)` throws; `(byte)300` would just wrap around. Different failure mode for a similar-looking operation, so it's worth knowing which one you're actually using.

### `System.BitConverter`

`SystemBitConverter()` covers the lower-level sibling: `BitConverter` doesn't convert *values*, it reinterprets *bytes*. `BitConverter.GetBytes(int)` hands you the raw four bytes; `BitConverter.ToInt32(bytes, 0)` puts them back together. Useful for binary file formats, network protocols, and hashing, and almost never what you want for ordinary data conversion.

Note `BitConverter.IsLittleEndian`. Byte order is platform-dependent, and code that reads a binary format written on another machine has to account for that explicitly.

---

## Boxing and Unboxing

```csharp
int num = 10;
object boxedNum = num;          // boxing: value type wrapped as an object on the heap
int unboxedNum = (int)boxedNum; // unboxing: unwrapped back to a value type
```

Value types normally live on the stack. The moment you need to treat one as an `object` (assign it to an `object` variable, pass it somewhere expecting `object`), the runtime has to box it — copy the value into a heap-allocated wrapper.

This happens invisibly all the time. `string.Format("num is {0}", num)` boxes `num` on the way in, since the method's signature expects `object`. Not something to obsess over day-to-day, but worth recognizing as the mechanism when you hear people talk about boxing overhead in performance-sensitive code. It's also one of the concrete reasons generics exist: `List<int>` stores actual `int`s, while the old `ArrayList` boxed every single one.

Unboxing is stricter than you might expect. A boxed `int` can only be unboxed to `int`, not to `long`, even though `int` widens to `long` freely in normal code:

```csharp
object boxed = 42;
long wrong = (long)boxed;        // InvalidCastException
long right = (long)(int)boxed;   // unbox first, then widen
```

---

## Custom Conversions

`CustomConversions()` builds out the "what if the framework doesn't already know how to convert this" case, using boolean parsing as the example. The built-ins handle the easy path:

```csharp
bool.Parse("true");                  // works
bool.TryParse("true", out bool ok);  // works, doesn't throw
bool.Parse("yes");                   // FormatException - "yes" is not "True" or "False"
```

`bool.Parse` accepts essentially `"True"` and `"False"` (case-insensitive, whitespace trimmed) and nothing else. Real-world input is rarely that disciplined — you'll get `Y`, `1`, `on`, `yes`, `T`. The chapter's helper extension methods (`ToBoolean()`, `Parse()`, `TryParse()` in `HelperClasses.Extensions`) exist to widen that vocabulary, following the same three-flavor pattern the framework uses: one that throws, one that returns a default, one that returns a `bool` success flag with an `out` result.

The pattern is the lesson here more than the boolean specifics. When you write your own conversion, mirror the framework's shape — a `Parse` that throws and a `TryParse` that doesn't — so callers already know how to use it.

C# also lets you define conversions directly on a type with `implicit` and `explicit` operators. Rule of thumb: make it `implicit` only when the conversion can never fail and never loses information. Everything else should be `explicit`, so the cast is visible at the call site.

---

## Interop: `DllImport` and COM

`ImportedComDll()` calls straight into `user32.dll` and `kernel32.dll` via `[DllImport]` — unmanaged Windows APIs with no .NET wrapper needed:

```csharp
[DllImport("user32.dll", CharSet = CharSet.Auto)]
private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
private static extern int GetShortPathName(string longPath, StringBuilder shortPath, int bufferSize);
```

Two details worth noticing. The `extern` modifier means "there's no body here, the implementation lives outside managed code" — this is the one place that modifier from the Chapter 3 table actually shows up. And `GetShortPathName` takes a `StringBuilder` rather than a `string` specifically because the unmanaged function writes into a caller-supplied buffer, and `string` is immutable. The marshaller can't hand a native function something it's allowed to overwrite if that thing is a `string`.

`ExcelInterop()` goes a step further, driving an actual running instance of Excel through COM interop. Both methods require Windows to run at all — `DllImport` because it's calling Windows-specific DLLs by name, and `ExcelInterop()` additionally requires Microsoft Excel to be installed, since it launches a real Excel process and manipulates it live.

### The Excel Reference Modernization

The Excel reference got a modernization pass in this solution. The original project referenced the Excel interop assembly the old way — a raw COM reference with `EmbedInteropTypes`. The SDK-style project instead pulls in the `Microsoft.Office.Interop.Excel` NuGet package as a normal `PackageReference`. Cleaner, and consistent with how every other dependency in this solution is declared.

That switch comes with one real behavior change worth knowing, because it will bite you the moment you try to build. The old `EmbedInteropTypes` reference let the compiler treat COM indexer members (`Worksheets[1]`, `Cells[row, col]`, `Columns[n]`) as their specific interop types. The plain `PackageReference` doesn't get that treatment — those members are typed as plain `object` in the interop assembly. So:

```csharp
Excel.Worksheet sheet = workbook.Worksheets[1];  // CS0266: cannot convert object to Worksheet
sheet.Cells[1, 1].Value = "Header";              // CS1061: object has no member 'Value'
```

The fix is `dynamic` instead of a specific type:

```csharp
dynamic sheet = workbook.Worksheets[1];
sheet.Cells[1, 1].Value = "Header";
sheet.Columns[1].AutoFit();
```

`dynamic` defers member resolution to runtime, which is exactly what COM was always doing anyway underneath. You lose IntelliSense and compile-time checking on those calls, which is the honest trade: a typo in a member name becomes a runtime exception instead of a build error. That's tolerable here because COM interop is inherently late-bound, but it's not a pattern to reach for in ordinary managed code.

Interop objects also need releasing. A COM reference left dangling is why "I closed Excel but `EXCEL.EXE` is still in Task Manager" happens.

---

## Bonus: `dynamic` Beyond Interop

`BonusLessonDynamics()` shows the other place `dynamic` genuinely earns its keep — data whose shape isn't known until runtime, like parsed JSON:

```csharp
dynamic parsed = JObject.Parse(json);
Console.WriteLine(parsed.name);
Console.WriteLine(parsed.address.city);
```

No class definition, no deserialization target, just navigate the structure directly. Convenient for a quick script or a one-off, and a genuine liability in production code, where a strongly-typed model catches a renamed field at build time instead of at 2 AM. The lesson isn't "avoid `dynamic`," it's "know that you're trading compile-time safety for flexibility, and be sure you're actually getting flexibility you need."

---

## Cloning Arrays

`CloningArrays()` demonstrates `Clone()`, and the important word is **shallow**:

```csharp
int[] original = { 1, 2, 3 };
int[] copy = (int[])original.Clone();
copy[0] = 99;   // original[0] is still 1
```

That works cleanly because `int` is a value type — the copied array holds copies of the actual values. Do the same with an array of a reference type and you get a new array holding the *same object references*. Change a property on `copy[0]` and `original[0]` changes too, because they're the same object. A deep copy needs to be written explicitly, element by element.

`Clone()` also returns `object`, so it always needs a cast. `Array.Copy` and `CopyTo` are the alternatives when you want to control the destination rather than allocate a new array.

---

## The `string` Class

### Immutability

```csharp
string s = "hello";
s.ToUpper();              // does nothing useful - result thrown away
s = s.ToUpper();          // this is what you meant
```

`ManipulatingStrings()` makes this concrete. Every method on `string` that looks like it modifies the string actually returns a *new* string and leaves the original untouched. `string` is immutable, full stop. `ToCharArray()` is how you get something you can actually mutate in place, and `new string(chars)` gets you back.

This is the single most common beginner bug with strings, and the compiler won't warn you about it, because "call a method and ignore its return value" is legal.

### Static vs. Instance Methods

`StaticStringMethods()` and `InstanceStringMethods()` split the API by how it's called.

Static (called on the type, and they tolerate `null`):

| Method | Purpose |
|---|---|
| `string.IsNullOrEmpty(s)` | `null` or `""` |
| `string.IsNullOrWhiteSpace(s)` | `null`, `""`, or only whitespace |
| `string.Join(sep, values)` | Combine a collection into one delimited string |
| `string.Concat(a, b, ...)` | Combine without a separator |
| `string.Compare(a, b)` | Ordering comparison, with culture and case options |
| `string.Format(fmt, args)` | Build a string from a template |

Instance (called on a string, and they throw on `null`):

| Method | Purpose |
|---|---|
| `Substring`, `Split` | Break a string apart |
| `IndexOf`, `LastIndexOf`, `Contains` | Find things |
| `StartsWith`, `EndsWith` | Check the edges |
| `Replace`, `Insert`, `Remove` | Produce a modified copy |
| `Trim`, `TrimStart`, `TrimEnd` | Strip whitespace (or specified characters) |
| `PadLeft`, `PadRight` | Fixed-width alignment |
| `ToUpper`, `ToLower` | Case conversion |

The static/instance split is exactly why `string.IsNullOrEmpty(s)` exists as a static method: you can't call an instance method on a `null` reference, which is the case you most need to check for.

---

## `StringBuilder`, and Why Concatenation Gets Expensive

Because strings are immutable, `s += "x"` in a loop doesn't append anything. It allocates an entirely new string every single iteration, copies the old contents in, and abandons the previous one to the garbage collector. Ten iterations is invisible. Ten thousand is a measurable problem.

```csharp
var sb = new StringBuilder();
for (int i = 0; i < 10000; i++)
{
    sb.Append(i);
}
string result = sb.ToString();
```

`StringBuilder` maintains a mutable internal buffer and only materializes a `string` when you call `ToString()`. `UsingStringBuilder()` runs both approaches and times them so the difference is a number on screen rather than an assertion.

The practical threshold: a handful of concatenations in one expression is fine (the compiler optimizes those into a single `string.Concat` call anyway). Concatenation inside a loop is where `StringBuilder` belongs.

`StringWriter` and `StringReader` show up alongside it — they're the `TextWriter`/`TextReader` implementations backed by a string, which lets you point code that expects a stream at an in-memory string instead of a file.

### A Bug This Pass Found

The `Factorial` method used to compute the wrong answer:

```csharp
// Before
for (long i = 1; i < number; i++)

// After
for (long i = 1; i <= number; i++)
```

Classic off-by-one. `Factorial(5)` was multiplying 1×2×3×4 and returning 24 instead of 120, because the loop stopped one short of including `number` itself. It produced plausible-looking output, which is exactly why it survived as long as it did — a wrong number that's still a number doesn't announce itself the way an exception does.

---

## `ToString()` and Formatting

`UsingToString()` covers the override. Every type inherits `ToString()` from `object`, and the default implementation returns the fully qualified type name, which is almost never useful. Overriding it on your own types is cheap and pays off constantly — in the debugger, in log output, in string interpolation.

`StringFormat()` covers the two ways to build a formatted string:

```csharp
string.Format("Name: {0}, Total: {1:C}", name, total);
$"Name: {name}, Total: {total:C}";
```

Interpolation (the `$` form) compiles down to essentially the same thing but keeps the values next to their placeholders, which is more readable and removes an entire category of "argument in the wrong position" bugs. Use it unless you specifically need a format string stored separately, such as one loaded from a resource file for localization.

### Standard Numeric Format Specifiers

| Specifier | Meaning | Example |
|---|---|---|
| `C` | Currency | `$1,234.56` |
| `D` | Decimal (integers, with padding) | `D5` → `00042` |
| `E` | Scientific | `1.234560E+003` |
| `F` | Fixed-point | `F2` → `1234.56` |
| `G` | General (shortest reasonable) | `1234.56` |
| `N` | Number, with group separators | `1,234.56` |
| `P` | Percent | `12.35%` |
| `X` | Hexadecimal | `X` → `4D2` |

### Common Date/Time Specifiers

| Specifier | Meaning |
|---|---|
| `d` / `D` | Short / long date |
| `t` / `T` | Short / long time |
| `f` / `F` | Full date+time, short / long |
| `g` / `G` | General date+time, short / long |
| `s` | Sortable (ISO 8601) |
| `u` / `U` | Universal sortable / universal full |

`C`, `N`, `P`, and every date specifier are **culture-sensitive**. The same code produces `$1,234.56` on a US machine and `1.234,56 €` elsewhere. That's correct behavior for anything a human reads, and a serious bug for anything written to a file, a database, or a network request. For machine-readable output, pass `CultureInfo.InvariantCulture` explicitly, and use the `s` specifier for dates.

Alignment is also available: `{0,10}` right-aligns in a 10-character field, `{0,-10}` left-aligns. Combine with a format: `{0,12:C}`.

---

## Bonus: Why `decimal`, Not `double`, for Money

`BonusDecimalVsDouble()` is the section to actually pay attention to, because this one causes real production bugs.

```csharp
double a = 0.1 + 0.2;
Console.WriteLine(a == 0.3);   // False
Console.WriteLine(a);          // 0.30000000000000004
```

`double` is binary floating-point. It stores values as a sum of powers of two, and `0.1` simply cannot be represented exactly in that scheme — same reason `1/3` can't be written exactly in decimal notation. The value stored is very slightly off, and those tiny errors accumulate through arithmetic.

For scientific and graphics work, that's fine and the speed is worth it. For money, it is not fine. A rounding error of one ten-quadrillionth becomes a penny after enough operations, and pennies that don't reconcile are the kind of bug that gets escalated.

```csharp
decimal a = 0.1m + 0.2m;
Console.WriteLine(a == 0.3m);  // True
```

`decimal` stores values in base 10, exactly the way people write them. It's slower and has a smaller range than `double`, and neither of those matters for currency. The rule is simple: **if it's money, it's `decimal`.**

### The OnBase Angle

This matters specifically in our work because OnBase currency keywords are backed by a decimal type. Passing a `double`-derived value into a currency keyword can produce a value with more precision than the keyword accepts, and the API responds with an `InvalidKeywordValueException` — which is a confusing error to debug if you don't already know that the root cause was choosing the wrong numeric type several layers earlier.

Use `decimal` end to end for any currency value that will eventually reach a keyword, and don't convert through `double` on the way.

---

## Chapter Takeaways

- Narrowing casts fail silently by default. `checked` makes integral overflow throw; floating-point needs `IsInfinity`/`IsNaN` instead.
- `is`/`as`/pattern matching change what the compiler lets you *reach*, not what the object *is*.
- Array covariance compiles and then fails at runtime. Generic collections deliberately don't allow it.
- Prefer `TryParse` over `Parse`. Know that `Convert.ToInt32` uses banker's rounding.
- Boxing is the bridge between stack-allocated values and `object`, and it costs an allocation every time.
- `DllImport` needs `extern`, and buffer-writing native APIs need `StringBuilder`, not `string`.
- The Excel interop `PackageReference` requires `dynamic` for COM indexer members. That's expected, not a workaround gone wrong.
- Strings are immutable. Assign the result, and use `StringBuilder` inside loops.
- Format specifiers are culture-sensitive — use `InvariantCulture` for anything a machine will read.
- Money is `decimal`. Always. Especially anything headed for an OnBase currency keyword.