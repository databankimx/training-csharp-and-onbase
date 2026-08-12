# WorkingWithTheTypeSystem

## Introduction

Value types versus reference types, and everything that distinction touches: structs, enums, generics, indexers, and access modifiers.

---

## Predefined Value Types

| Alias | Size | .NET Type | Default Value |
|---|---|---|---|
| `bool` | 1 byte | `System.Boolean` | `false` |
| `byte` | Unsigned 8-bit | `System.Byte` | `0` |
| `char` | 16-bit | `System.Char` | `'\0'` |
| `decimal` | 28-29 significant digits | `System.Decimal` | `0.0m` |
| `double` | 15-16 digits | `System.Double` | `0.0d` |
| `enum` | User-defined | | `(E)0` |
| `float` | 7 digits | `System.Single` | `0.0f` |
| `int` | Signed 32-bit | `System.Int32` | `0` |
| `long` | Signed 64-bit | `System.Int64` | `0` |
| `sbyte` | Signed 8-bit | `System.SByte` | `0` |
| `short` | Signed 16-bit | `System.Int16` | `0` |
| `struct` | User-defined | | `null` |
| `uint` | Unsigned 32-bit | `System.UInt32` | `0` |
| `ulong` | Unsigned 64-bit | `System.UInt64` | `0` |
| `ushort` | Unsigned 16-bit | `System.UInt16` | `0` |

Signed types lose one bit to the sign, so a signed 32-bit type doesn't range ±2^32, it ranges from -2^31 to (2^31 - 1), one more negative value than positive.

### Memory Storage

The **stack** is a region of memory reserved for the currently running application, organized so items are added and removed in a strict last-in-first-out order as methods are called and return. It's fast, but limited in size, and it's where value types live.

The **heap** is a larger, more loosely organized pool of memory used for reference types. An object on the heap sticks around until nothing references it anymore and the garbage collector reclaims it, it doesn't automatically disappear just because the method that created it has returned.

All value types are stored on the stack, whereas reference types are stored on the heap. Value types are released from memory when the stack unwinds, and assigning a value type variable to another variable results in a second, independent copy:

```csharp
int i = 1;
int j = i;
// At this point there are two System.Int32 variables stored in memory
```

Reference types work differently:

```csharp
var w1 = new StreamWriter();
var w2 = w1;
// At this point there is only one System.IO.StreamWriter stored in memory
// Both variables reference the same object
```

### Memory Efficiency

- Use the smallest data type that can accommodate your values.
- Avoid duplicating variables.
- Declare your variables within the smallest scope that is practical, so they're released from memory in a timely fashion.

---

## Two's Complement

Why do signed data types support one more value in the negative than the positive?

Binary addition works through a circuit that takes two bits plus a carry-in bit and produces a sum bit plus a carry-out bit. There's no separate binary subtraction, negative numbers get *added*.

Using a naive sign-bit scheme (leftmost bit as sign, `0` positive, `1` negative), addition breaks:

```
   0111  ( 7)
 + 1011  (-3, naive sign-bit scheme)
 -------
 1 0010  (2, wrong! Lost a carry bit and got the wrong answer)
```

**Two's complement** (flip every bit, then add 1) fixes this. Complementing zero and adding 1 lands back on zero exactly:

```
   1111  (one's complement of 0)
 + 0001
 -------
 1 0000  (zero, with a throwaway carry bit)
```

And addition works cleanly:

```
   0111  ( 7)
 + 1101  (-3, two's complement)
 -------
 1 0100  (4, correct, the lost carry bit can be ignored)
```

With 4 bits and two's complement, the values run `0000` (0) up through `0111` (7) on the positive side, and `1111` (-1) down through `1000`, which is `-8`, not `-7`. There's no matching positive `8` to pair with it, that bit pattern fills out the negative range evenly, and it's the direct reason `sbyte.MinValue` is `-128` while `sbyte.MaxValue` is only `127`, the same pattern holds at every signed integer size C# offers.

---

## Code Lab: Value Type Aliases

```csharp
int myInt = 0;
int myNewInt = new();

System.Int32 myInt32 = new();

Console.WriteLine(myInt);
Console.WriteLine(myNewInt);
Console.WriteLine(myInt32);
```

`int` and `System.Int32` are the exact same type, `int` is just a keyword alias for it. Calling `new()` explicitly constructs the type's default value.

---

## Assigning Values

```csharp
int myInt;
int secondInt;

myInt = 2;
secondInt = myInt;

Console.WriteLine($"myInt = {myInt}");
Console.WriteLine($"secondInt = {secondInt}");
```

Assigning one value type to another copies the value, `secondInt` gets its own independent `2`, changing `myInt` afterward wouldn't touch `secondInt`.

---

## Code Lab: Using Value Types

```csharp
int myInt = 5000;
Console.WriteLine(myInt);
Console.WriteLine(myInt.GetType());
Console.WriteLine(sizeof(int));

double myDouble = 5000.0;
Console.WriteLine(sizeof(double));

byte myByte = 254;
Console.WriteLine(sizeof(byte));

char myChar = 'r';
Console.WriteLine(sizeof(char));

decimal myDecimal = 20987.89756M;
Console.WriteLine(sizeof(decimal));

float myFloat = 254.09F;
Console.WriteLine(sizeof(float));

long myLong = 2544567538754;
Console.WriteLine(sizeof(long));

short myShort = 3276;
Console.WriteLine(sizeof(short));

bool myBool = true;
Console.WriteLine(sizeof(bool));
```

`sizeof()` returns the number of bytes a given type occupies. `sizeof(char)` returning `2`, not `1`, catches people off guard the first time, C# `char` represents a UTF-16 code unit, twice the size a lot of people assume going in.

---

## Working with Structs

```csharp
public struct Person
{
    public string FirstName;
    public string LastName;
    public byte Age;

    public Person(string firstName, string lastName, byte age)
    {
        FirstName = firstName;
        LastName = lastName;
        Age = age;
    }

    public string Greet()
    {
        return $"Hello. My name is {FirstName} {LastName}. I am {Age} years old.";
    }
}
```

A struct is a user-defined value type that meaningfully collects related data. It differs from an array or collection, which can only hold a single data type, a struct can contain fields of multiple different types. A struct can contain both variables and methods that act on those variables.

```csharp
var birth = new DateTime(1970, 1, 2);
int age = DateTime.Today.Year - birth.Year;
if (DateTime.Today.DayOfYear < birth.DayOfYear) age--;

var me = new Person("Scott", "McLean", (byte)age);
Console.WriteLine(me.Greet());
```

---

## Code Lab: Real World Scenario, Books

```csharp
public struct Book
{
    public string Title;
    public string Category;
    public string Author;
    public int NumPages;
    public int CurrentPage;
    public double ISBN;
    public string CoverStyle;

    public Book(string title, string category, string author, int numPages, int currentPage, double isbn, string coverStyle)
    {
        Title = title;
        Category = category;
        Author = author;
        NumPages = numPages;
        CurrentPage = currentPage;
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > NumPages) CurrentPage = NumPages;
        ISBN = isbn;
        CoverStyle = coverStyle;
    }

    public void NextPage()
    {
        if (CurrentPage < NumPages)
        {
            CurrentPage++;
            Console.WriteLine("Current page is now " + CurrentPage);
        }
        else
        {
            Console.WriteLine("At end of book!");
        }
    }

    public void PrevPage()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            Console.WriteLine("Current page is now " + CurrentPage);
        }
        else
        {
            Console.WriteLine("At beginning of book!");
        }
    }
}
```

```csharp
var myBook = new Book("MCSD Certification Toolkit (Exam 70-483)", "Certification", "Covaci, Tiberiu", 648, 1, 81118612095, "Softcover");

myBook.NextPage();
myBook.PrevPage();
```

A more complete example of a struct with validation logic built into its constructor, and behavior methods that mutate its own fields.

---

## Working with Enums

```csharp
public enum Months : byte
{
    Jan = 1,
    Feb,
    Mar,
    Apr,
    May,
    Jun,
    Jul,
    Aug,
    Sep,
    Oct,
    Nov,
    Dec
}
```

An enum allows you to create a list of names that refer to constant values of a single data type. By default, an enum is of type `System.Int32`, and its values start at 0 and ascend by 1, unless you specify otherwise. Only `Jan` gets an explicit value (`1`) here, every member after it picks up the next integer automatically.

```csharp
if (Enum.TryParse("Jul", out Months selected)) Console.WriteLine($"Jul is month {(byte)selected}");
Console.WriteLine($"The 8th month is {Enum.GetName(typeof(Months), 8)}");
```

`Enum.TryParse` and `Enum.GetName` convert between the string form and the underlying value.

### Code Lab: Using Enums

```csharp
string name = Enum.GetName(typeof(Months), 8);
Console.WriteLine("The 8th month in the enum is " + name);

foreach (byte values in Enum.GetValues(typeof(Months)))
{
    Console.WriteLine(values);
}
```

---

## Reference Types

With Object-Oriented Programming, objects take the form of "classes," the general term for reference data types. A class differs from a struct in specific ways:

- A reference type variable only holds a memory address to where the data is actually stored.
- Reference types are stored on the **heap** instead of the **stack**. An instance must be released from memory, either explicitly or by the .NET garbage collector, unlike the stack, which unwinds automatically.
- When copied to another variable, a reference type only copies the memory address, not the object itself. Two variables end up pointing to the same object. A value type, by contrast, gets a full copy of the data.

```csharp
class MyClass
{
    // Fields (typically private global variables)
    // Properties (public members - can be variables or property methods pointing to fields)
    // Methods (public or private functions that model behaviors for the class)
    // Events (statements that occur when conditions change during execution)
    // Delegates (types that refer to or act on behalf of methods)
    // Nested Classes (Other classes constructed within the parent class)
}
```

---

## Struct vs. Class

Compared to a class, a struct has real limitations:

- A struct cannot have a default (parameterless) constructor or a destructor.
- Structs are value types and are copied on assignment.
- A struct cannot inherit from another struct or class, and cannot be the base of a class, all structs inherit directly from `System.ValueType`.
- A struct can implement interfaces, and can be used as a nullable type.

Structs are allocated on the stack (or inline in a containing type), classes on the heap and garbage-collected. Allocations of value types are generally cheaper than reference types, but assignments of large reference types are cheaper than assignments of large value types.

Use a struct only when you're sure it logically represents a single value (like a primitive type), is immutable, and shouldn't be boxed/unboxed frequently.

---

## Notes on Modifiers

Every class and its members should have an accessibility modifier explicitly included in the code.

### Accessibility

| Modifier | Description |
|---|---|
| `public` | The most permissive level, can be accessed from outside the object. |
| `private` | The least permissive level, can only be accessed from within the object. |
| `internal` | Can be accessed from outside the object, but only by other objects within the same assembly. |
| `protected` | Can be accessed only from within the class or derived classes. |
| `sealed` | Applied to a class, indicates the class cannot be inherited. |
| `static` | Applied to a class, indicates the class cannot be instantiated, members are called by invoking the class type name. Applied to a member, indicates only one instance of it exists across all instances of the class. |

### Behavior

| Modifier | Description |
|---|---|
| `abstract` | Used as a base model for other classes that inherit its members. An abstract class cannot be instantiated. |
| `async` | Executes asynchronously, other code statements continue to execute while it runs in the background. |
| `const` | Contains a value that cannot be modified, must be initialized when declared. |
| `event` | Declares that the associated element is an event for which an event handler method contains the code to execute when raised. |
| `extern` | Indicates a method is defined and implemented externally, commonly used with `DllImport`. |
| `new` | Used in a derived class to hide an inherited member of the base class with the same name. |
| `override` | Implements a method that should execute instead of an inherited method with the same signature. |
| `partial` | Indicates the class exists, at least in part, in another file in the assembly. |
| `readonly` | The member can only be assigned when declared or within the constructor. |
| `unsafe` | Indicates the affected code exists outside normal .NET memory management. |
| `virtual` | Explicitly permits a method to be overridden in derived classes using `override`. |
| `volatile` | Indicates the member can be modified externally (from the OS or another thread), a compiler hint for multi-threaded scenarios. |

---

## Code Lab: Accessing Member Fields of a Class

```csharp
public class Student
{
    public static int StudentCount;
    public string FirstName;
    public string LastName;
    public string Grade;
}
```

```csharp
Student firstStudent = new();
Student.StudentCount++;
Student secondStudent = new();
Student.StudentCount++;

firstStudent.FirstName = "John";
firstStudent.LastName = "Smith";
firstStudent.Grade = "six";

secondStudent.FirstName = "Tom";
secondStudent.LastName = "Thumb";
secondStudent.Grade = "two";

Console.WriteLine(firstStudent.FirstName);
Console.WriteLine(secondStudent.FirstName);
Console.WriteLine(Student.StudentCount);
```

`StudentCount` is `static`, so it belongs to the `Student` type itself, not to any individual instance. There's only ever one `StudentCount`, no matter how many `Student` objects exist, which is why it's accessed as `Student.StudentCount` rather than through an instance.

## Code Lab: Accessing Member Methods of a Class

```csharp
public string ConcatenateName()
{
    string fullName = FirstName + " " + LastName;
    return fullName;
}

public void DisplayName()
{
    string name = ConcatenateName();
    Console.WriteLine(name);
}
```

```csharp
firstStudent.DisplayName();
```

`ConcatenateName()` returns a value that could be used elsewhere, `DisplayName()` calls it and does something with the result. Splitting "compute a value" from "act on a value" into separate methods keeps each one independently useful.

## Code Lab: Passing Value Types to a Member Method

```csharp
private static int Sum(int value1, int value2)
{
    return value1 + value2;
}

private static void ChangeValues(int value1, int value2)
{
    value1--;
    value2 += 5;
    Console.WriteLine("value1 is now " + value1);
    Console.WriteLine("value2 is now " + value2);
}

private static void ChangeName(Student refValue)
{
    refValue.FirstName = "George";
}
```

```csharp
int num1 = 2;
int num2 = 3;

// Named parameters let you pass arguments in any order
int result = Sum(value2: num2, value1: num1);

ChangeValues(num1, num2);
Console.WriteLine(num1);  // still 2, unaffected
Console.WriteLine(num2);  // still 3, unaffected

var firstStudent = new Student { FirstName = "John", LastName = "Smith", Grade = "six" };
ChangeName(firstStudent);
Console.WriteLine(firstStudent.FirstName);  // "George", changed
```

`int` is a value type, so `ChangeValues` receives copies, whatever it does to its parameters stays inside that method. `Student` is a reference type, so `ChangeName` receives a reference to the same object the caller has, mutating a field through that reference changes what the caller sees too.

---

## Calling Generic Types

```csharp
var queue = new GenericQueue<string>();
queue.Add("Scott");
queue.Add("Andy");
queue.Add("Alan");

while (queue.Waiting())
{
    Console.WriteLine($"Now serving {queue.Next()}");
}

var stack = new GenericStack<string>();
stack.Add("Scott");
stack.Add("Andy");
stack.Add("Alan");

while (stack.Waiting())
{
    Console.WriteLine($"Now serving {stack.Next()}");
}
```

A queue serves items first-in, first-out, Scott, Andy, Alan in that order. A stack serves last-in, first-out, Alan, Andy, Scott. Both share the same underlying storage, opposite removal points.

---

## Using Bit-Shifts

```csharp
int ig = 1;
Console.WriteLine("0x{0:x}", ig << 1);
// Shift i one bit to the left. The result is 2.

long lg = 1;
Console.WriteLine("0x{0:x}", lg << 33);
// Because the type of lg is long, the shift is the value of the six low-order bits.
// In this example, the shift is 33, and the value of lg is shifted 33 bits to the left.
```

`<<` and `>>` shift a value's bits left or right. Shifting left by one doubles the value (until it overflows), shifting right by one halves it.

---

## Using Bit-Flags

```csharp
public static bool IsBitSet<T>(this T t, int pos) where T : struct, IConvertible
{
    var value = t.ToInt64(CultureInfo.CurrentCulture);
    return (value & (1 << pos)) != 0;
}
```

`IsBitSet` is an extension method (defined in `CSharp.SharedLibrary`) that checks whether a specific bit, by position, is set to `1` in a value. It works by shifting `1` left to the position being checked, then using `&` to see if that single bit overlaps with a `1` in the value.

```csharp
byte b = 73; // 01001001

for (int i = 0; i < 8; i++)
{
    Console.WriteLine($"Bit {i} is {(b.IsBitSet(i) ? "" : "not ")}set");
}
```

Individual bits within an integer can be checked directly, useful for compactly storing several true/false flags in a single value.

---

## Indexers

```csharp
public class IpAddress
{
    private readonly int[] ip = new int[32];

    public int this[int index]
    {
        get => ip[index];
        set
        {
            if (value == 0 || value == 1) ip[index] = value;
            else throw new ArgumentException("Invalid value, must be 0 or 1", nameof(value));
        }
    }
}
```

```csharp
var myIp = new IpAddress();
for (int i = 0; i < 32; i++)
{
    myIp[i] = 0;
    Console.Write($"{myIp[i]} ");
}
```

An indexer lets your own type support `[]` syntax the same way arrays and `List<T>` do. Unlike a raw array, an indexer is backed by a real property, so it can validate what gets assigned, here rejecting anything that isn't a 0 or a 1.

---

## Code Standards: Variable Naming

Every variable name should be meaningful, resulting in self-commenting code. `double accountBalance` is better than `double amount`, never use something like `double myDouble` or `double num`.

- Public members and properties should use `PascalCase`.
- Classes, constants, and method names (public or private) should use `PascalCase`.
- Private and locally scoped variables should use `camelCase`. Optionally, you can prefix private fields with an underscore, like `_myPrivateField`, but it's not required.
- Do not use `snake_case` or `kebab-case`.
- Never use `ALL_CAPS`.
- Do not use Hungarian notation, like `strName` or `arr10Numbers`.

---

## Bonus: Alias Versus System Type

```csharp
System.Int32 mySystemInt = new();
Console.WriteLine($"My System int is [{mySystemInt}]"); // Prints zero
```

`int` and `System.Int32` are the same type under the hood, only the spelling differs.

> For our team, it is always preferable to use the alias (`int`, `string`, `bool`, etc.) rather than the system type (`System.Int32`, `System.String`, `System.Boolean`, etc.) in code, because it is shorter and easier to read.
> 
> The only time we would use the system type is when we need to explicitly reference the type in a reflection scenario.

## Bonus: Wrap-Around and Overflow

```csharp
short num = 0;
do
{
    num++;
    if (num > 32766 || num < 0) Console.WriteLine($"num = {num}");
    if (num < 0) break;
} while (num <= 32767);
```

If a value is equal to `System.Int16.MaxValue` (32767), incrementing it wraps around to `System.Int16.MinValue` (-32768), silently, without an exception.

```csharp
int x = 1;
for (int i = 1; i < 32; i++)
{
    x <<= 1;
}
// We have overflowed into the sign bit!
```

In C#, the leftmost bit of a signed value is used for the sign, so a 32-bit `int` only has 31 usable bits for the actual number. Keep shifting a positive `int` left and eventually you push a 1 into that sign bit, and the number silently becomes negative. Regular multiplication (`x *= 2`) hits the exact same wall, it's not a shift-specific quirk.

## Bonus: Value Versus Reference

```csharp
public struct ValueCoordinates
{
    public int X;
    public int Y;

    public ValueCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class ReferenceCoordinates
{
    public int X { get; set; }
    public int Y { get; set; }

    public ReferenceCoordinates(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

```csharp
private static void MoveXAxis(ValueCoordinates coords, int distance = 1)
{
    coords.X += distance;
}

private static void MoveXAxis(ref ValueCoordinates coords, int distance = 1)
{
    coords.X += distance;
}

private static void MoveXAxis(ReferenceCoordinates coords, int distance = 1)
{
    coords.X += distance;
}
```

```csharp
var valueCoords = new ValueCoordinates(0, 0);
MoveXAxis(valueCoords);
Console.WriteLine($"{valueCoords.X},{valueCoords.Y}"); // 0,0, unchanged

MoveXAxis(ref valueCoords);
Console.WriteLine($"{valueCoords.X},{valueCoords.Y}"); // 1,0, changed

var refCoords = new ReferenceCoordinates(0, 0);
MoveXAxis(refCoords);
Console.WriteLine($"{refCoords.X},{refCoords.Y}"); // 1,0, changed
```

Passing a value type normally passes a copy, so changes inside the method don't affect the caller's variable. Passing it explicitly `ref` passes the actual memory location instead, so changes do stick. A reference type always behaves like the `ref` case, since what's being passed is already a reference to the object, not the object's data.
