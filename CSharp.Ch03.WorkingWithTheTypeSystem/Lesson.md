# Chapter 3 - Working with the Type System

## What This Chapter Is Actually About

Value types versus reference types, and pretty much everything that distinction touches: structs, enums, generics, indexers, access modifiers, and a couple of genuinely strange corners of how C# stores numbers in memory. This is the chapter where "it compiles" stops being the bar and "I understand why it behaves this way" starts mattering.

It's also the first chapter where you'll define your own types, not just use the built-in ones. A `struct` you write and a `class` you write both get to have their own fields and their own methods, which is a small preview of a much bigger idea, but you don't need the bigger idea yet to follow along here.

---

## How to Write This Program

Same approach as the last chapter: every topic below stands alone, meant to be the entire contents of `Main()` on its own. Clear it out, write the next one, run it, move on. A few of these topics ask you to also define a `struct` or a `class` alongside `Main()`, since that's genuinely what those particular lessons are about, you can't demonstrate "a struct can have its own methods" without writing one. What you won't do is break the *demo* itself into a pile of named helper methods, or lean on a pause-and-continue mechanism between topics, same reasoning as before: one topic, one small program, run it and see.

(This chapter's own `Program.cs` organizes all of this into named methods, one per topic, called in sequence from a single `Main()`, exactly like last chapter's did. Ignore that shape for now. It'll make sense once you've met the chapter that actually covers breaking code into methods.)

### Mini-Program 1: Value Type Aliases

```csharp
int myInt = 0;
int myNewInt = new();

System.Int32 myInt32 = new();

Console.WriteLine($"myInt = {myInt}");
Console.WriteLine($"myNewInt = {myNewInt}");
Console.WriteLine($"myInt32 = {myInt32}");
```

Run it. All three print `0`.

`int` and `System.Int32` are the exact same type, `int` is just a keyword alias for it, shorter to type and easier to read, which is exactly why it's the preferred spelling in basically every C# codebase you'll ever touch. The only time reaching for the full `System.Int32` spelling actually matters is a reflection scenario where you need to name the type explicitly, which is rare enough that you can treat `int` as the default without a second thought.

`new()` on a value type explicitly constructs its default value, `0` for `int`. Worth knowing the syntax exists, since you'll see it used deliberately in a few places, but `int myNewInt = 0;` says the exact same thing more plainly, and that's almost always what you actually want to write.

One more thing worth trying: change `int myInt = 0;` to just `int myInt;`, no assignment, and try to build. It fails. A local variable that's only declared, never assigned, can't be read, C# enforces that as a compile error, not a runtime surprise.

### Mini-Program 2: Assigning Values

```csharp
int myInt;
int secondInt;

myInt = 2;
secondInt = myInt;

Console.WriteLine($"myInt = {myInt}");
Console.WriteLine($"secondInt = {secondInt}");
```

Run it. Both print `2`. Assigning one value type to another copies the value, `secondInt` now holds its own independent `2`, entirely disconnected from `myInt`. Change `myInt` afterward and `secondInt` doesn't move. That's the whole idea behind "value type," and it's about to get contrasted with something that behaves very differently.

### Mini-Program 3: A Tour of `sizeof`

```csharp
int myInt = 5000;
Console.WriteLine($"int: {myInt.GetType()}, {sizeof(int)} bytes");

double myDouble = 5000.0;
Console.WriteLine($"double: {myDouble.GetType()}, {sizeof(double)} bytes");

byte myByte = 254;
Console.WriteLine($"byte: {myByte.GetType()}, {sizeof(byte)} bytes");

char myChar = 'r';
Console.WriteLine($"char: {myChar.GetType()}, {sizeof(char)} bytes");

decimal myDecimal = 20987.89756M;
Console.WriteLine($"decimal: {myDecimal.GetType()}, {sizeof(decimal)} bytes");

float myFloat = 254.09F;
Console.WriteLine($"float: {myFloat.GetType()}, {sizeof(float)} bytes");

long myLong = 2544567538754;
Console.WriteLine($"long: {myLong.GetType()}, {sizeof(long)} bytes");

short myShort = 3276;
Console.WriteLine($"short: {myShort.GetType()}, {sizeof(short)} bytes");

bool myBool = true;
Console.WriteLine($"bool: {myBool.GetType()}, {sizeof(bool)} bytes");
```

Run it. `sizeof()` returns exactly how many bytes a given type takes up in memory. One number here catches nearly everyone the first time: `sizeof(char)` is `2`, not `1`. C#'s `char` represents a single UTF-16 code unit, not a single ASCII byte, twice the size a lot of people walk in assuming.

Here's the full table these nine values came from, worth having in front of you once rather than looking each one up individually:

| Alias | Size | .NET Type | Default Value |
|---|---|---|---|
| `bool` | 1 byte | `System.Boolean` | `false` |
| `byte` | Unsigned 8-bit | `System.Byte` | `0` |
| `char` | 16-bit | `System.Char` | `'\0'` |
| `decimal` | 28-29 significant digits | `System.Decimal` | `0.0m` |
| `double` | 15-16 digits | `System.Double` | `0.0d` |
| `enum` | User-defined | — | `(E)0` |
| `float` | 7 digits | `System.Single` | `0.0f` |
| `int` | Signed 32-bit | `System.Int32` | `0` |
| `long` | Signed 64-bit | `System.Int64` | `0` |
| `sbyte` | Signed 8-bit | `System.SByte` | `0` |
| `short` | Signed 16-bit | `System.Int16` | `0` |
| `struct` | User-defined | — | `null` |
| `uint` | Unsigned 32-bit | `System.UInt32` | `0` |
| `ulong` | Unsigned 64-bit | `System.UInt64` | `0` |
| `ushort` | Unsigned 16-bit | `System.UInt16` | `0` |

Signed types lose one bit to the sign, so a signed 32-bit type doesn't range ±2^32, it ranges from -2^31 to (2^31 - 1), one more value on the negative side than the positive. That asymmetry isn't arbitrary, and by the end of this lesson you'll know exactly why it's there.

### Mini-Program 4: Your First Struct

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

That's the type definition, it needs to sit outside `Main()`, as its own thing in the file. Now `Main()`:

```csharp
var birth = new DateTime(1985, 6, 15);
int age = DateTime.Today.Year - birth.Year;
if (DateTime.Today.DayOfYear < birth.DayOfYear) age--;

var me = new Person("Alex", "Turner", (byte)age);
Console.WriteLine(me.Greet());
```

Run it. You should see a greeting with today's actual computed age in it.

A struct is a user-defined value type that meaningfully bundles related data together. That's the real difference between a struct and an array: an array can only hold one data type throughout, a struct can hold fields of several different types at once, a `string`, another `string`, and a `byte` living side by side here. A struct can also carry methods that act on its own fields, `Greet()` reaches into `FirstName`, `LastName`, and `Age` without needing them passed in, because they're already right there on the instance.

### Mini-Program 5: A Fuller Struct, With Validation

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

Run it. You'll see "Current page is now 2" followed by "Current page is now 1."

The constructor here does more than just assign fields, it clamps `CurrentPage` into a sane range, refusing to let a caller construct a `Book` sitting on page 0 or page 9,000 of a 648-page book. That's a struct genuinely behaving like a small, self-contained piece of logic, not just a bag of fields somebody happens to have grouped together.

### Mini-Program 6: Enums

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

```csharp
if (Enum.TryParse("Jul", out Months selected)) Console.WriteLine($"Jul is month {(byte)selected}");
Console.WriteLine($"The 8th month is {Enum.GetName(typeof(Months), 8)}");
```

Run it. `Jul is month 7`, then `The 8th month is Aug`.

An enum is a named set of constant values, backed by an integer type, `byte` here, plain `int` by default if you don't say otherwise. Only `Jan` got an explicit value; everything after it just picks up the next integer automatically, which is why `Jul` lands on `7` without anyone spelling that out. `Enum.TryParse` and `Enum.GetName` are how you move between the string spelling and the underlying number, worth knowing since enums show up constantly for anything with a fixed, known set of options, statuses, log levels, days of the week.

One trap worth knowing about: an enum will happily hold a value that was never one of its named members. `(Months)99` compiles, runs, and prints `99` without complaint. `Enum.IsDefined` is the check for that, and it's worth reaching for it anywhere an enum value is arriving from outside your own code, a database column, a config file, a web request, anywhere you can't fully trust what shows up.

There's also a variant worth knowing exists, even without building it out here: mark an enum with `[Flags]` and give its members powers-of-two values (`1`, `2`, `4`, `8`...), and a single variable of that enum type can hold any *combination* of them at once, since each option owns its own bit. `HasFlag()` checks for one; `|` combines several. `[Flags]` itself doesn't change how the values behave, it only changes how `ToString()` prints them, as a comma-separated list of names instead of one raw number.

### Mini-Program 7: Using Enums

```csharp
string name = Enum.GetName(typeof(Months), 8);
Console.WriteLine("The 8th month in the enum is " + name);

foreach (byte value in Enum.GetValues(typeof(Months)))
{
    Console.WriteLine(value);
}
```

(This one needs the `Months` enum from Mini-Program 6 sitting alongside it again.)

Run it. `Enum.GetValues` hands back every member of the enum, in declaration order, which is a handy way to iterate "every possible status" without maintaining a separate list of them by hand.

### Mini-Program 8: A Class, and Its Static Field

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

Run it. `John`, `Tom`, `2`.

`StudentCount` is `static`, which means it belongs to the `Student` type itself, not to any one instance. There's exactly one `StudentCount` no matter how many `Student` objects you create, which is exactly why it's accessed as `Student.StudentCount` rather than through either instance, `firstStudent.StudentCount` wouldn't even compile.

### Mini-Program 9: A Method on That Class

Add these two methods inside `Student`:

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
Student firstStudent = new() { FirstName = "John", LastName = "Smith", Grade = "six" };
firstStudent.DisplayName();
```

Run it. `John Smith`.

`ConcatenateName()` computes and returns a value; `DisplayName()` calls it and does something with the result. Splitting "figure out a value" from "act on a value" into two separate methods keeps each one independently useful, `ConcatenateName()` could get called anywhere a full name is needed, not just from inside `DisplayName()`.

### Mini-Program 10: Value Types and Reference Types, Passed to a Method

This one needs a few small free functions sitting alongside `Main()`, not because you're organizing the demo that way by choice, but because the whole point is watching what happens when a value crosses a method boundary:

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

int result = Sum(value2: num2, value1: num1);
Console.WriteLine($"Sum is: {result}");

ChangeValues(num1, num2);
Console.WriteLine(num1); // still 2
Console.WriteLine(num2); // still 3

var firstStudent = new Student { FirstName = "John", LastName = "Smith", Grade = "six" };
ChangeName(firstStudent);
Console.WriteLine(firstStudent.FirstName); // "George"
```

Run it. `num1` and `num2` come back completely unchanged after `ChangeValues` runs, but `firstStudent.FirstName` genuinely changed after `ChangeName` ran.

That's the value-versus-reference distinction showing up in real behavior instead of a diagram. `int` is a value type, so `ChangeValues` receives copies of `num1` and `num2`, whatever it does to those copies never leaves the method. `Student` is a reference type, so `ChangeName` receives a reference to the exact same object `firstStudent` points at, and mutating a field through that reference changes what the caller sees too.

One small aside worth noticing on the way past: `Sum(value2: num2, value1: num1)` passes its arguments in reverse order using **named parameters**, and they still land in the right variables, because the names, not the position, are doing the matching. Handy any time a call site would otherwise be hard to read at a glance.

### Mini-Program 11: Generics, a Queue and a Stack

This needs a small amount of supporting code, since a queue and a stack are genuinely different types from anything built-in:

```csharp
public abstract class BaseStackOrQueue<T>
{
    protected readonly List<T> Values = [];

    public void Add(T obj) => Values.Add(obj);
    public abstract T Next();
    public bool Waiting() => Values.Count > 0;
}

public class GenericQueue<T> : BaseStackOrQueue<T>
{
    public override T Next()
    {
        if (Values.Count <= 0) throw new IndexOutOfRangeException("GenericQueue is empty!");
        var ret = Values[0];
        Values.RemoveAt(0);
        return ret;
    }
}

public class GenericStack<T> : BaseStackOrQueue<T>
{
    public override T Next()
    {
        if (Values.Count <= 0) throw new IndexOutOfRangeException("GenericStack is empty!");
        var ret = Values[Values.Count - 1];
        Values.RemoveAt(Values.Count - 1);
        return ret;
    }
}
```

```csharp
var queue = new GenericQueue<string>();
queue.Add("Alex");
queue.Add("Andy");
queue.Add("Alan");

Console.WriteLine("Queue:");
while (queue.Waiting())
{
    Console.WriteLine($"Now serving {queue.Next()}");
}

var stack = new GenericStack<string>();
stack.Add("Alex");
stack.Add("Andy");
stack.Add("Alan");

Console.WriteLine("Stack:");
while (stack.Waiting())
{
    Console.WriteLine($"Now serving {stack.Next()}");
}
```

Run it. The queue serves Alex, Andy, Alan, that order, first in, first out. The stack serves Alan, Andy, Alex, the reverse, last in, first out.

Same underlying storage in both, a `List<T>`, opposite removal point, and that's the entire difference between a queue and a stack, worth noticing since it's easy to assume the two are more different than they actually are. The `<T>` is what makes this a generic type: `GenericQueue<string>` and a hypothetical `GenericQueue<int>` are two genuinely distinct types, checked at compile time, generated from one piece of source you only had to write once. The alternative, a collection of plain `object` with a cast on every read, would move every type mistake from compile time to runtime, exactly the kind of bug you'd rather the compiler catch for you before it ships.

Both `Next()` implementations throw rather than silently handing back a default value when the collection is empty. A silent `default(T)`, a `null` or a `0` depending on `T`, can hide a real bug for a long time before anyone notices something's actually wrong three calls downstream. An exception at the exact line that caused the problem is a much shorter debugging session.

### Mini-Program 12: Bit Shifts

```csharp
int ig = 1;
Console.WriteLine("0x{0:x}", ig << 1); // 2

long lg = 1;
Console.WriteLine("0x{0:x}", lg << 33); // 0x200000000
```

Run it. `<<` and `>>` shift a value's bits left or right. Shifting left by one doubles the value (until it overflows); shifting right by one halves it. Watch the second line specifically: shifting an `int` left by `33` doesn't actually shift by 33, the shift amount itself gets reduced modulo the type's bit width first, 32 for `int`, so shifting by 33 behaves identically to shifting by 1. `lg` is a `long`, 64 bits wide, so `33` is a perfectly ordinary shift there, and the result reflects a genuine 33-bit shift.

### Mini-Program 13: Bit Flags

```csharp
byte b = 73; // 01001001

for (int i = 0; i < 8; i++)
{
    bool isSet = (b & (1 << i)) != 0;
    Console.WriteLine($"Bit {i} is {(isSet ? "" : "not ")}set");
}
```

Run it. Bits 0, 3, and 6 report as set, matching `01001001` read right to left.

`1 << i` builds a mask with exactly one bit turned on, at position `i`. `&` against `b` isolates whatever's at that same position in `b`, zero if it's off, non-zero if it's on. This is the standard technique for packing several true/false flags into one small value instead of a handful of separate `bool` fields, and it's the same underlying idea `[Flags]` enums (mentioned back in Mini-Program 6) are built on.

### Mini-Program 14: An Indexer

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

Run it. Thirty-two zeros.

An indexer lets your own type support `[ ]` syntax the same way an array or `List<T>` does, `myIp[i]` reads exactly like array access. Unlike a raw array, though, an indexer is backed by a real property, `get`/`set` with actual code behind them, so it can validate what gets assigned. Try `myIp[0] = 5;` and watch it throw, a plain array would have just silently accepted a nonsense value.

### Mini-Program 15: Alias vs. System Type, One More Time

```csharp
System.Int32 mySystemInt = new();
Console.WriteLine($"My System int is [{mySystemInt}]"); // 0
```

Run it. This is the same idea as Mini-Program 1, worth restating on its own since it's a real team convention, not just trivia: prefer `int`, `string`, `bool`, and the other keyword aliases over their full `System.` spellings in ordinary code. Shorter, easier to read, and the only time you'd reach for the full name deliberately is a reflection scenario where the actual type needs to be named explicitly.

### Mini-Program 16: Wrap-Around and Overflow

```csharp
short num = 0;
do
{
    num++;
    if (num > 32766 || num < 0) Console.WriteLine($"num = {num}");
    if (num < 0) break;
} while (num <= 32767);
```

Run it. You'll see `num = 32767`, then `num = -32768`.

`short.MaxValue` is `32767`. Increment past it and the value doesn't throw, doesn't clamp, it silently wraps around to `short.MinValue`, `-32768`, and keeps going as if nothing happened. That `if (num < 0) break;` exists purely to escape what would otherwise be an infinite loop, since without it, `num <= 32767` would eventually become true again on the way back up.

There's a second, subtler version of the same wrap-around, worth building separately:

```csharp
int x = 1;
for (int i = 1; i < 32; i++)
{
    x <<= 1;
}
Console.WriteLine(x);
```

Run it and watch what prints. In C#, the leftmost bit of a signed value is reserved for the sign, so a 32-bit `int` really only has 31 usable bits for magnitude. Keep doubling a positive `int` by shifting it left, and eventually a `1` gets pushed into that sign bit, and the number silently becomes negative, no exception, no warning, just an answer that looks wrong the moment you know what to look for.

Try the same doubling with plain multiplication instead of a shift, `x *= 2` in place of `x <<= 1`, run it, and you'll get the identical overflow at the identical point. It's not a shift-specific quirk, it's what happens to any signed integer type once you push past what it can represent, and by default C# lets it happen silently rather than telling you. If silent-but-wrong is worse than loud-but-stopped for whatever you're computing, wrap the math in a `checked` block instead:

```csharp
checked
{
    int max = int.MaxValue;
    max += 1; // throws OverflowException instead of quietly becoming int.MinValue
}
```

`checked` costs a little performance and only applies to the block it wraps, so it's not something to blanket an entire codebase with, reach for it deliberately anywhere a silently wrong number would genuinely be worse than a loud failure: counts, IDs, money.

### Mini-Program 17: Value vs. Reference, Side by Side

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

Run it. Watch the three lines of output carefully, they're the entire chapter distilled into one small program.

Passing a `struct` normally passes a copy, so the first `MoveXAxis` call changes nothing the caller can see. Passing it explicitly with `ref` passes the actual variable, its real memory location, not a copy, so *that* call's change genuinely sticks. A `class`, on the other hand, always behaves like the `ref` case, with no `ref` keyword anywhere in sight, because what gets passed to `MoveXAxis(ReferenceCoordinates coords, ...)` was already a reference to the object, not the object's data. Copying a reference still leaves you pointing at the same thing.

`ValueCoordinates` and `ReferenceCoordinates` are otherwise identical, same two fields, same constructor shape, on purpose. The only thing this mini-program is actually testing is `struct` versus `class`, everything else about the two types was deliberately kept the same so nothing else could explain the difference in behavior.

That's also the practical rule for picking one over the other: reach for a `struct` when something is small and genuinely feels like a single value, a coordinate pair, a color, and reach for a `class` for basically everything else, particularly anything with identity that ought to outlive a single expression.

| | `struct` | `class` |
|---|---|---|
| Storage | Stack (or inline in a containing type) | Heap |
| Copy semantics | Full copy of the data | Copy of the reference, same underlying object |
| Inheritance | Cannot inherit or be inherited from | Full inheritance support |
| Default constructor | Cannot define a custom parameterless one | Can |

---

## Why Signed Types Have One Extra Negative Value

A curiosity section, worth reading once, genuinely doesn't change how you write code day to day. But you've now run into the `-32768` versus `32767` asymmetry twice in this chapter, and an unexplained asterisk isn't satisfying.

Binary addition happens through a circuit that takes two bits plus a carry-in bit and produces a sum bit plus a carry-out bit. There's no separate binary subtraction, negative numbers get *added*. The whole question is how to represent a negative number in the first place so that plain addition still works correctly.

The naive approach, steal the leftmost bit as a sign flag, `0` for positive, `1` for negative, breaks immediately. Using 4 bits, `0111` is `7`, and `1011` would be `-3` under this naive scheme. Add them the ordinary way:

```
   0111  ( 7)
 + 1011  (-3, naive sign-bit scheme)
 -------
 1 0010  (2, wrong! Lost a carry bit and got the wrong answer)
```

That scheme also gives you two zeros, `0000` and `1000`, positive and negative zero, a separate headache entirely.

One's complement, flip every bit to represent the negative, mostly fixes addition, but not all the way:

```
   0111  ( 7)
 + 1100  (-3, one's complement)
 -------
 1 0011  (3, still wrong, though the lost carry bit fixes it if you wrap it around and re-add)
   0001
 -------
   0100  (4, correct, but only after manually re-adding the carry)
```

Still has a positive and negative zero, and "wrap the lost carry bit back around" needs extra hardware nobody particularly wants to build just for that.

Two's complement is one's complement plus 1, and it resolves both problems in a single move. Complementing zero and adding 1 lands exactly back on zero:

```
   1111  (one's complement of 0)
 + 0001
 -------
 1 0000  (zero, with a throwaway carry bit, exactly what we want)
```

And addition works cleanly, no manual carry-wrapping needed:

```
   0111  ( 7)
 + 1101  (-3, two's complement)
 -------
 1 0100  (4, correct, and the lost carry bit can simply be ignored)
```

This is also exactly why a signed type gets one extra value on the negative side. With 4 bits under two's complement, the values run `0000` (0) up through `0111` (7) on the positive side, and `1111` (-1) down through `1000`, which is `-8`, not `-7`. There's no matching positive `8` to pair with it, that bit pattern was needed to fill out the negative range evenly. That's the direct reason `sbyte.MinValue` is `-128` while `sbyte.MaxValue` is only `127`, and the exact same pattern holds at every signed integer size C# offers.

One practical consequence worth knowing: `Math.Abs(int.MinValue)` throws. The positive result it would need to return, `2147483648`, simply doesn't exist as a valid `int`, it's one past `int.MaxValue`. That's the asymmetry showing up in real code instead of a diagram.

---

## Access Modifiers

Every class member should carry an explicit accessibility modifier, don't rely on the default. C# has a lot of modifiers, and they answer two different questions: **accessibility** (who can see this?) and **behavior** (what is this allowed to do?).

### Accessibility

| Modifier | Meaning |
|---|---|
| `public` | Accessible from anywhere. |
| `private` | Accessible only from within the declaring type. |
| `internal` | Accessible anywhere in the same assembly, not from outside it. |
| `protected` | Accessible from the declaring type and anything that inherits from it, nothing else. |
| `sealed` | On a class, prevents anything from inheriting from it. |
| `static` | On a class, prevents instantiation entirely. On a member, means there's exactly one instance shared across every instance of the class, exactly what you watched `Student.StudentCount` do earlier. |

### Behavior

| Modifier | Meaning |
|---|---|
| `abstract` | Marks a class as a base model that can't be instantiated directly, only inherited from. |
| `async` | Marks a method or lambda as running asynchronously, the caller keeps going while it executes in the background. |
| `const` | The value can never change, and must be assigned at the point of declaration. |
| `event` | Declares a member as an event, with a handler containing the code that runs when it's raised. |
| `extern` | Indicates a method is implemented externally, most commonly paired with `[DllImport]` for calling into unmanaged DLLs. |
| `new` | In a derived class, deliberately hides an inherited member sharing the same name. |
| `override` | Implements a method that replaces an inherited one sharing the same signature. |
| `partial` | Indicates the class is also defined, at least in part, in another file in the same assembly. |
| `readonly` | The member can only be assigned at declaration or inside the constructor. |
| `unsafe` | Marks code that steps outside normal .NET memory management. Avoid it unless there's a specific reason not to. |
| `virtual` | Explicitly permits a method to be replaced in a derived class via `override`. |
| `volatile` | Hints that a field can change from outside the current code path, another thread, the OS, relevant in multi-threaded scenarios. |

`const` versus `readonly` is the one that actually bites in practice. A `const` value gets baked directly into whatever calling code references it, at compile time, so changing a `public const` in a shared library and redeploying only that library leaves every *consumer* still using the old value until they're individually rebuilt too, a genuinely surprising failure mode the first time you hit it. `readonly` is read at runtime instead, and doesn't have that problem. For anything public that crosses an assembly boundary, prefer `readonly`.

---

## Code Standards: Variable Naming

Every variable name should be meaningful enough that the code reads like a sentence. `double accountBalance` over `double amount`, and never something like `double myDouble` or `double num`, a name that just restates the type tells a reader nothing the type declaration didn't already say.

- **Public members and properties**: `PascalCase`.
- **Classes, constants, and method names** (public or private): `PascalCase`.
- **Private and locally-scoped variables**: `camelCase`.
- **Never**: `snake_case`, `kebab-case`, or `ALL_CAPS`.
- **Never**: Hungarian notation (`strName`, `arr10Numbers`), prefixing a name with an abbreviation of its type. Modern IDEs show you the type on hover, the prefix just adds noise and goes stale the moment the type changes.

---

## Seeing It All Together

Seventeen small demos, each one gone the moment you moved to the next, same as last chapter. This chapter's own `Program.cs` is the shipped version, everything above organized into named methods and called in turn from one `Main()`, pausing between each. Open it once you're done and you'll recognize essentially every line, now just arranged the way a chapter's worth of demos actually gets packaged for reuse, a technique that gets its proper introduction once you reach the chapter on methods.

## Run It Yourself

- **Break the empty-collection guard.** Remove the `throw` from `GenericQueue.Next()` and return `default(T)` instead. Call `Next()` on an empty queue and watch it silently hand back `null` rather than telling you anything went wrong. Decide for yourself which failure mode you'd rather debug at 2am.
- **Give the indexer a bad value.** Try `myIp[0] = 5;` against Mini-Program 14's `IpAddress` and read the exception it throws. Then try the same thing against a plain `int[]` and notice nothing stops you.
- **Push `Math.Abs` off the edge.** Run `Math.Abs(int.MinValue)` and read the exception. Then explain, using what you now know about two's complement, exactly why there's no valid answer for it to return.
- **Make `[Flags]` real.** Write your own enum with powers-of-two values and the `[Flags]` attribute, combine a couple with `|`, and print the result with and without `[Flags]` applied to see what actually changes.
- **Overflow something that matters.** Wrap a counter in a `checked` block, push it past its type's max value on purpose, and compare the `OverflowException` you get against the silent wrap-around from Mini-Program 16.
