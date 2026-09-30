# Chapter 4 - Using and Converting Data Types (Part 1: Casting, Conversion, and Interop)

## What This Chapter Is Actually About

Getting data from one type to another, safely, and understanding what "safely" actually means in each specific case, because it turns out to mean something slightly different every time. Casting, `Parse`/`TryParse`, `System.Convert`, boxing, writing your own conversions, and then the mechanisms you'll eventually need to reach outside managed .NET entirely, calling straight into a Windows DLL, or driving an actual running copy of Excel.

This chapter is big enough that it's split across two files. This one covers everything above. [Part 2](Lesson-Part2-Strings.md) picks up with the `string` class itself, `StringBuilder`, formatting, and a genuinely important closing argument for why money should never touch a `double`.

---

## How to Write This Program

Same approach as every chapter so far: each topic below is meant to be the entire contents of `Main()` on its own, clear it out, write the next one, run it. A couple of these need a small supporting type or extension method defined alongside `Main()`, since that's what the lesson is actually about, but you still won't be organizing the *demo itself* into a pile of named helper methods.

### Mini-Program 1: Widening and Narrowing

```csharp
byte b = 127;
int i = (int)b;
Console.WriteLine($"byte to int: {i}");

i = 64;
b = (byte)i;
Console.WriteLine($"int to byte: {b}");
```

Run it. Both conversions land exactly where you'd expect, `127` and `64`.

A **widening** conversion, a smaller type into a larger compatible one, always succeeds, there's simply nowhere for data to get lost. A **narrowing** conversion, larger into smaller, only comes out clean if the value actually fits in the destination. So far, both values fit. That's about to stop being true on purpose.

### Mini-Program 2: Narrowing That Doesn't Fit

```csharp
int i = 264;
byte b = (byte)i;
Console.WriteLine($"int to byte with invalid value ({i}): {b}");
```

Run it. `b` comes out as `8`, not `264`.

`byte` tops out at `255`. `264` doesn't fit, and C# doesn't stop you, it doesn't throw, it doesn't clamp, it just silently keeps the low 8 bits and discards the rest: `264 - 256 = 8`. No error, no warning, just a value that's confidently, silently wrong. This is the single most important thing to internalize about casting in C#: the compiler will let you do this, and the runtime will hand you back a number, and that number will simply not be the one you wanted.

### Mini-Program 3: The `checked` Block

Same as Mini-Program 2, wrapped:

```csharp
checked
{
    int i = 264;
    byte b = (byte)i;
    Console.WriteLine($"int to byte with invalid value ({i}): {b}");
}
```

Run it. This time it throws an `OverflowException` instead of printing `8`.

`checked` flips the default silent-overflow behavior for integral conversions and arithmetic inside the block it wraps, loud failure instead of quiet corruption. It's scoped locally, it does **not** propagate into methods called from inside it, so wrapping a call to some other method in `checked` doesn't make that method's own internal math checked too. Reach for it deliberately anywhere a silently wrong number would be worse than a stopped program, counts, IDs, anything currency-adjacent.

### Mini-Program 4: Floating-Point Overflow

```csharp
double big = -1E40;
float small = (float)big;

Console.WriteLine(float.IsInfinity(small)
    ? "Whoops! Must have overflowed the type..."
    : small.ToString(CultureInfo.InvariantCulture));
```

Run it. You'll see the overflow message.

`checked` doesn't apply to floating-point math at all, that's a genuine gap worth knowing about rather than assuming. A narrowing cast from `double` to `float` that overflows doesn't throw and doesn't wrap around the way an integer does, it produces infinity, silently. `float.IsInfinity()` (and its sibling `float.IsNaN()`) are the actual guard rails here, not `checked`.

### Mini-Program 5: `is`, `as`, and What Actually Changes

This needs the `Person`/`Employee` types alongside `Main()`:

```csharp
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public Person() { }
    public Person(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

public class Employee : Person
{
    public string Department { get; set; }
    public string JobTitle { get; set; }

    public Employee(string firstName, string lastName, string department, string jobTitle)
        : base(firstName, lastName)
    {
        Department = department;
        JobTitle = jobTitle;
    }
}
```

```csharp
var employee = new Employee("Joe", "Programmer", "Development", "Software Engineer");
Person person = employee;

Console.WriteLine(person is Employee ? "yes" : "no");

if (person is Employee emp)
{
    Console.WriteLine($"{emp.FirstName} is a {emp.JobTitle}");
}
```

Run it. `yes`, then the job title.

`Employee : Person` means an `Employee` IS-A `Person`, always safely, which is why `Person person = employee;` needs no cast at all, that's a widening conversion, the same idea from Mini-Program 1, just for reference types instead of numbers. The object itself never changes what it actually is, `person` still IS an `Employee`, in memory, for the entire lifetime of that object. What changes is which *members the compiler will let you reach* through the `person` variable. `person.Department` won't compile, even though the data is genuinely sitting right there, because the compiler only knows about what `Person` itself declares.

`if (person is Employee emp)` is pattern matching, doing the type check and the cast in one step, and scoping `emp` to exactly the block where it's known to be valid. That's the modern, preferred spelling. The older two-step alternatives still show up constantly in code you'll read: `(Employee)person` throws `InvalidCastException` if the check would have failed, `person as Employee` returns `null` instead of throwing.

### Mini-Program 6: The Array Covariance Trap

Same `Person`/`Employee` types, add a `Manager`:

```csharp
public class Manager : Employee
{
    public Manager(string firstName, string lastName, string department, string jobTitle)
        : base(firstName, lastName, department, jobTitle) { }
}
```

```csharp
Employee[] employees = { new("Joe", "Programmer", "Development", "Software Engineer") };
Person[] persons = employees; // implicit, an array of a more specific type fits a base-type array variable

Manager[] managers = persons as Manager[]; // null - not actually convertible
Console.WriteLine(managers == null ? "as returned null" : "converted");

try
{
    managers = (Manager[])persons; // compiles, throws at runtime
}
catch (Exception ex)
{
    Console.WriteLine(ex.GetType().Name);
}
```

Run it. `as returned null`, then `ArrayTypeMismatchException`.

Casting an array doesn't create a new one, it just gives you a different-typed reference to the *same* underlying array. `Employee[]` fits into a `Person[]` variable for the same reason a single `Employee` fits into a `Person` variable, but that assignment doesn't change what the array actually, physically holds underneath, it's still an array of `Employee` objects. Casting that same array to `Manager[]` compiles cleanly and then fails the instant it actually runs, because the array's real element type was never `Manager` to begin with.

This is exactly why generic collections deliberately don't allow the equivalent move: `List<Employee>` cannot be assigned to a `List<Person>` variable, full stop, a compile error instead of a landmine waiting for the wrong line to run. Arrays kept the older, riskier behavior for historical reasons; generics were designed after this exact problem was already well understood.

### Mini-Program 7: Parse vs. TryParse

```csharp
string numString = "10";
int number = int.Parse(numString);
Console.WriteLine($"Parsed: {number}");

if (int.TryParse("ten", out number))
{
    Console.WriteLine($"parses to int [{number}]...");
}
else
{
    Console.WriteLine("cannot be parsed to int...");
}
```

Run it. `Parsed: 10`, then `cannot be parsed to int...`.

`int.Parse` throws `FormatException` on bad input, straightforward, but a try/catch you have to remember every single time you use it. `int.TryParse` never throws, it hands back `false` and leaves the `out` parameter at its default instead. Safer by default, and the reason it's the one you should reach for first.

### Mini-Program 8: `decimal.Parse` and `NumberStyles`

```csharp
string money = "1,000.00";
Console.WriteLine(decimal.Parse(money)); // handles the comma fine

money = "$1,000.00";
try
{
    Console.WriteLine(decimal.Parse(money)); // throws - can't handle the currency symbol by default
}
catch (FormatException ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine(decimal.Parse(money, NumberStyles.Currency));
```

Run it. `1000.00`, then a `FormatException` message, then `1000.00` again, this time successfully.

`decimal.Parse` handles thousands separators without being asked, but a currency symbol needs explicit permission via the optional `NumberStyles` argument, `NumberStyles.Currency` bundles together everything a typical currency string needs (the symbol, thousands separators, a decimal point). `NumberStyles` is a set of bit-flag values, so you can also stack exactly the options you want by hand instead of using one of the bundled presets: `NumberStyles.AllowCurrencySymbol | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands` says precisely what it means.

### Mini-Program 9: `System.Convert` and Banker's Rounding

```csharp
double income = 9.50;
Console.WriteLine(Convert.ToInt32(income)); // 10, ordinary rounding

income = 10.50;
Console.WriteLine(Convert.ToInt32(income)); // 10, NOT 11
```

Run it. Both print `10`.

`Convert.ToInt32` implements **banker's rounding**: ordinary rounding, except a value landing exactly on `.5` rounds to the nearest *even* number rather than always rounding up. `9.50` behaves the way you'd expect, but `10.50` also rounds down to `10`, not up to `11`. This isn't a bug, it's a deliberate choice that reduces systematic bias when you're rounding large volumes of numbers, but it will absolutely produce an answer that looks wrong the first time you hit it, with nothing raised to warn you, just a number that's off by one in specific, predictable circumstances. If you actually want "always round `.5` up" behavior, say so explicitly: `Math.Round(income, MidpointRounding.AwayFromZero)`.

### Mini-Program 10: `Convert` Throws Where a Cast Wraps

```csharp
double income = 300.00;
try
{
    byte tooSmall = Convert.ToByte(income);
}
catch (OverflowException ex)
{
    Console.WriteLine(ex.Message);
}
```

Run it. An `OverflowException`.

Compare this to Mini-Program 2, `(byte)264` silently wrapped to `8`. `Convert.ToByte(300.00)` does the opposite, it throws instead of truncating. Same-looking job, converting a bigger number into a `byte`, genuinely different failure mode depending on which mechanism you reach for. Worth knowing which one you're actually using before you're debugging why one path silently corrupted data and another didn't.

### Mini-Program 11: `Convert.ChangeType`

```csharp
double income = 10.50;
int rounded = (int)Convert.ChangeType(income, typeof(int));
Console.WriteLine(rounded);
```

Run it. `10`.

`Convert.ChangeType` is a generic conversion method that works from a runtime `Type` value rather than a specific named method like `ToInt32`. Since it returns a plain `object`, you always need to cast the result back to whatever you actually expect. Not something you'll reach for often, but it shows up in genuinely generic code that doesn't know its target type until runtime.

### Mini-Program 12: `BitConverter`

```csharp
int packedValue = 42;
byte[] packedBytes = BitConverter.GetBytes(packedValue);

Console.WriteLine(string.Join(" ", packedBytes));
Console.WriteLine(BitConverter.IsLittleEndian);

int unpacked = BitConverter.ToInt32(packedBytes, 0);
Console.WriteLine(unpacked);
```

Run it. Four bytes, a `True` (on essentially every machine you'll touch), then `42` again.

`BitConverter` doesn't convert *values* the way `Convert` does, it reinterprets *bytes*. `GetBytes` hands you the raw underlying bytes of a value; `ToInt32` puts a set of bytes back together into an `int`. Useful for binary file formats, network protocols, hashing, and essentially never what you want for ordinary data conversion. `IsLittleEndian` matters the moment you're reading a binary format written on a different machine, byte order is platform-dependent, and code that ignores that will misread perfectly valid data.

### Mini-Program 13: Boxing and Unboxing

```csharp
int num = 10;
object boxedNum = num;           // boxing
int unboxedNum = (int)boxedNum;  // unboxing
Console.WriteLine(unboxedNum);

object boxed = 42;
try
{
    long wrong = (long)boxed; // InvalidCastException - can't unbox directly to a different type
}
catch (InvalidCastException ex)
{
    Console.WriteLine(ex.Message);
}

long right = (long)(int)boxed; // unbox to the original type first, then widen
Console.WriteLine(right);
```

Run it. `10`, then an `InvalidCastException` message, then `42`.

Value types normally live on the stack. The moment one needs to be treated as an `object`, assigned to an `object` variable, passed somewhere expecting `object`, the runtime has to box it: copy the value into a heap-allocated wrapper. This happens invisibly all the time, `string.Format("num is {0}", num)` boxes `num` on the way in, since the method signature expects `object`.

Unboxing is stricter than most people expect walking in. A boxed `int` can only be unboxed back to `int`, not to `long`, even though `int` widens to `long` freely in ordinary, non-boxed code. Unbox to the original boxed type first, then widen afterward, as a completely separate step.

### Mini-Program 14: A Custom Conversion, Because the Built-In One Isn't Flexible Enough

```csharp
Console.WriteLine(bool.Parse("true"));  // works

try
{
    bool.Parse("yes"); // FormatException - "yes" isn't "True" or "False"
}
catch (FormatException ex)
{
    Console.WriteLine(ex.Message);
}

Console.WriteLine("yes".ToBoolean()); // true
Console.WriteLine("no".ToBoolean());  // false
Console.WriteLine("1".ToBoolean());   // true
Console.WriteLine("0".ToBoolean());   // false
```

(`ToBoolean()` here is a custom extension method in `CSharp.SharedLibrary/HelperClasses/GenericExtensions.cs`, not something built into the framework.)

Run it. `True`, then the `FormatException` message, then `True`/`False`/`True`/`False`.

`bool.Parse` only ever accepts `"True"`/`"False"` (case-insensitive, whitespace trimmed), and nothing else, which is a lot stricter than most real-world input actually looks like, you'll get `yes`, `1`, `on`, `y`, from users and from other systems constantly. When the framework's own conversion doesn't cover the shapes of input you actually need to handle, an extension method fills the gap cleanly. The one in `GenericExtensions` handles numeric strings (`"1"` is `true`, `"0"` is `false`) and checks the first character of anything else against known true/false prefixes (`"t"`/`"y"` for true, `"f"`/`"n"` for false), so `"true"`, `"yes"`, `"false"`, `"no"` all work as expected:

```csharp
public static bool ToBoolean(this string value)
{
    if (string.IsNullOrEmpty(value)) return false;
    if (int.TryParse(value, out int num)) return num > 0;

    string[] trueValues = ["t", "y"]; // matches "true", "yes"
    return Array.IndexOf(trueValues, value.Substring(0, 1).ToLower()) > -1;
}
```

The companion `TryParse` extension (also in `GenericExtensions`) follows the same pattern but never throws, returning `false` for unrecognized input instead:

```csharp
public static bool TryParse(this string value, out bool result)
{
    result = false;
    if (string.IsNullOrEmpty(value)) { result = false; return true; }
    if (int.TryParse(value, out int num)) { result = num > 0; return true; }

    string[] trueValues  = ["t", "y"];
    string[] falseValues = ["f", "n"];

    if (Array.IndexOf(trueValues,  value.Substring(0, 1).ToLower()) > -1) { result = true;  return true; }
    if (Array.IndexOf(falseValues, value.Substring(0, 1).ToLower()) > -1) { result = false; return true; }

    return false;
}
```

Notice the shape mirrors the framework's own convention deliberately: a version that throws (`ToBoolean`, playing the role `bool.Parse` plays) and a version that doesn't (`TryParse`). When you write your own conversion helpers, matching that same throws/doesn't-throw pairing means anyone who already knows how `Parse`/`TryParse` behave immediately knows how to use yours too.

Also worth a look while you're in that file: `GenericExtensions` is full of similar helpers, `ToInt`, `ToLong`, `ToFloat`, `ToDouble`, `ToDecimal`, `IsNumeric`, `IsPositive`, a case-insensitive `Replace` overload, `IsBitSet`, `Swap`, and more. Extension methods in a shared library are the right place for any conversion or utility behavior that isn't specific to one project but isn't general enough to belong in the framework itself. Open `CSharp.SharedLibrary/HelperClasses/GenericExtensions.cs` now and read through it before moving on, the code comments in there form a mini-lesson of their own on exactly when and why extension methods are the right tool.

### Mini-Program 15: Calling a Native Windows DLL Directly

```csharp
[DllImport("user32.dll", CharSet = CharSet.Unicode)]
private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);
```

(This declaration needs to sit at class scope, not inside `Main()`, same as the `Person`/`Employee` types earlier.)

```csharp
MessageBox(new IntPtr(0), "Hello World!", "Hello Dialog", 0);
```

Run it. A real, native Windows message box pops up.

`[DllImport]` declares the *signature* of a function that actually lives somewhere else entirely, in this case `user32.dll`, a core piece of the Windows operating system itself, not anything written in .NET. `extern` is the tell, no method body, because there isn't one here, the implementation lives outside managed code entirely. This only runs on Windows, since it's calling a Windows-specific DLL by name.

### Mini-Program 16: A Second DLL Call, With a Buffer

```csharp
[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
private static extern uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);
```

```csharp
string longName = Assembly.GetExecutingAssembly().Location;
char[] buffer = new char[1024];
long length = GetShortPathName(longName, buffer, buffer.Length);
string shortName = new string(buffer).Substring(0, (int)length);

Console.WriteLine(longName);
Console.WriteLine(shortName);
```

Run it. Two paths, the long form and the old-style 8.3 short form.

`char[]` here, not `string`, and that's not an arbitrary choice: the unmanaged function writes its result directly into whatever buffer you hand it, and `string` is immutable in .NET, there's no way to marshal a native function something it's allowed to overwrite if that something is a `string`. A `char[]` (or `StringBuilder`, which you'll see used the same way elsewhere) genuinely can be written into.

### Mini-Program 17: Driving Excel Through COM

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

This one needs Microsoft Excel actually installed to run at all, it launches a real, visible Excel process and drives it live, so treat it as something to read rather than necessarily run unless Excel is available on your machine.

Notice `dynamic sheet`, not a specific interop type. This project references the `Microsoft.Office.Interop.Excel` package the modern way, an ordinary `PackageReference`, rather than the older embedded-interop-types style some existing COM code still uses. That switch has one real consequence worth knowing before it surprises you: several COM indexer members (`Worksheets[1]`, `Cells[row, col]`, `Columns[n]`) come back typed as plain `object` in this style of reference, not their specific interop type, so `Excel.Worksheet sheet = workbook.Worksheets[1];` fails to compile, and every `.Value`/`.AutoFit()` call after it fails too, `object` genuinely doesn't have those members. `dynamic` is the fix, not a workaround, it defers all of that member resolution to runtime, which is exactly what COM interop is doing underneath regardless of which reference style you chose. The trade is real: you lose IntelliSense and compile-time checking on anything routed through `sheet`, a typo in a member name becomes a runtime exception instead of a build error. That's an acceptable trade specifically because COM is inherently late-bound to begin with, it's not a pattern to reach for casually in ordinary managed code.

### Mini-Program 18: `dynamic` Beyond COM

```csharp
const string json = "{\"Id\":\"1234-5678\",\"Data\":{\"FirstName\":\"Maria\",\"LastName\":\"Warden\"}}";
dynamic result = JObject.Parse(json);
Console.WriteLine($"{result.Id}: {result.Data.FirstName} {result.Data.LastName}");
```

Run it. `1234-5678: Maria Warden`.

Same mechanism as the Excel example, different reason to reach for it. `JObject.Parse` hands back a JSON structure with a shape you don't know at compile time, and rather than writing (and maintaining) a class just to read three fields once, `dynamic` lets you navigate straight into the structure by name. Convenient for exactly this, quick scripts, one-off tooling, poking at data you don't control the shape of. In production code touching data you *do* control, a strongly-typed model is almost always the better call, since it catches a renamed field at build time instead of at runtime, possibly in front of a user. The lesson isn't "avoid `dynamic`," it's that you're trading compile-time safety for flexibility every time you reach for it, and it's worth being sure you're actually getting flexibility you need in exchange.

### Mini-Program 19: Cloning Arrays

```csharp
int[] array1 = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

int[] array2 = (int[])array1.Clone();
array2[0] = 99;
Console.WriteLine($"array1[0] = {array1[0]}, array2[0] = {array2[0]}"); // 1, 99, independent

dynamic array3 = array1.Clone();
Console.WriteLine(array3[9]);

try
{
    array3[0] = "one"; // no compile-time error - array3 is dynamic
}
catch (InvalidCastException ex)
{
    Console.WriteLine(ex.Message); // fails at runtime instead
}
```

Run it. Independent values on the first pair, `10` from the clone, then an `InvalidCastException` from the last block.

`Array.Clone()` returns a plain `object`, so it needs casting back to your actual array type, `(int[])`, or you can sidestep the cast with `dynamic`, at the cost of losing compile-time type checking entirely, `array3[0] = "one";` compiles without complaint and only fails once that exact line actually executes.

Worth knowing what `Clone()` doesn't do, too: it's a **shallow** copy. That's invisible here because `int` is a value type, `array2` genuinely holds its own independent copies of each number. Clone an array of a *reference* type instead, and the clone holds the same object references the original does, changing a property on one of those shared objects through either array changes what the other array sees too. A true deep copy needs to be written by hand, element by element.

---

Continue to [Part 2](Lesson-Part2-Strings.md) for the `string` class, `StringBuilder`, formatting, and why money should never be a `double`.
