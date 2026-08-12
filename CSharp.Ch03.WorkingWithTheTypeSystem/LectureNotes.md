# Chapter 3: Working with the Type System

## What This Chapter Is Actually About

Value types versus reference types, and everything that distinction touches: structs, enums, generics, indexers, access modifiers, and a couple of genuinely weird corners of how C# stores numbers in memory. This is the chapter where "it compiles" stops being the bar and "I understand why it behaves this way" starts mattering.

---

## Predefined Value Types

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

Two things worth flagging while this table is in front of you:

- **Signed types lose one bit to the sign**, so a signed 32-bit type doesn't range ±2^32, it ranges from -2^31 to (2^31 - 1), one more negative value than positive. The "Two's Complement" section below explains exactly why that asymmetry exists, it's not arbitrary.
- **A declared-but-unassigned variable has a default value only if it's a field, not a local.** `int i = new int();` explicitly constructs the default (`0`) rather than relying on it being implicit, which is why you'll see that syntax in the type-alias lab even though `int i = 0;` says the same thing more plainly.

---

## Memory: Stack vs. Heap

**Stack**: reserved per-running-application memory, not shared with other processes. This is where value types live. It has limited space, and it unwinds automatically as scope exits, nothing to clean up manually.

**Heap**: a larger memory pool for reference types (classes, interfaces, delegates). An object here holds both its member variables and its methods, which makes it more memory-intensive than a value type, and it needs to be released, either explicitly or by the garbage collector, rather than unwinding automatically the way the stack does.

This is the mechanical reason behind the copy-semantics difference covered below: value types copy their actual data because that's what stack storage does, reference types copy only the memory address because that's what makes the heap navigable at all.

### Class Structural Syntax

```csharp
class MyClass
{
    // Fields (typically private, backing variables)
    // Properties (public members - can be variables or property methods pointing to fields)
    // Methods (public or private functions that model behaviors for the class)
    // Events (statements that occur when conditions change during execution)
    // Delegates (types that refer to or act on behalf of methods)
    // Nested Classes (other classes constructed within the parent class)
}
```

Not every class needs all six, but that's the rough order they tend to appear in when a class has more than one.

---

## Value Types vs. Reference Types, the Short Version

A value type variable holds its data directly, on the stack. Copy it, and you get a second, fully independent copy of the data.

A reference type variable holds a memory address pointing at data on the heap. Copy it, and you get a second variable pointing at the *same* data, not a copy of it.

```csharp
int i = 1;
int j = i;
// Two separate ints. Changing j does nothing to i.

var w1 = new StreamWriter();
var w2 = w1;
// One StreamWriter. w1 and w2 both point at it.
```

Every other rule in this chapter, structs being value types, classes being reference types, `ref` parameters, boxing, all of it traces back to this one distinction. `BonusValueVersusReference()` is the method that makes it concrete: it passes a `struct` by value, then by `ref`, then passes a `class` normally, and prints the coordinates after each call so you can watch the difference happen instead of just reading about it.

---

## Struct vs. Class: When to Reach for Which

Both let you group related data together. The difference is what they cost and how they behave when copied.

| | `struct` | `class` |
|---|---|---|
| Storage | Stack (or inline in a containing type) | Heap |
| Copy semantics | Full copy of the data | Copy of the reference, same underlying object |
| Inheritance | Cannot inherit or be inherited from | Full inheritance support |
| Default constructor | Cannot define a custom parameterless one | Can |
| When to use | Small, immutable, "feels like a single value" (a coordinate pair, a color) | Everything else |

`Person` and `Book` in this chapter are `struct`s specifically because they're small bundles of read-mostly data. `ValueCoordinates` and `ReferenceCoordinates` exist side by side as a `struct` and a `class` version of the exact same shape, purely so `BonusValueVersusReference()` can demonstrate the copy-semantics difference without anything else changing between the two examples.

---

## Enums

```csharp
public enum Months : byte
{
    Jan = 1,
    Feb,
    Mar,
    // ...
}
```

An enum is a named set of constant values, backed by an integer type (`byte` here, `int` by default if you don't specify one). `Feb` through `Dec` don't need explicit values, each one is automatically one more than the previous. `Enum.TryParse` and `Enum.GetName` are how you convert between the string form and the underlying value, worth knowing since enums show up constantly for anything with a fixed, known set of options (status codes, days of the week, log levels).

---

## Generics, via a Stack and a Queue

`BaseStackOrQueue<T>` holds the shared plumbing, `GenericQueue<T>` removes from the front (first in, first out), `GenericStack<T>` removes from the end (last in, first out). Same underlying `List<T>`, opposite removal point, and that's the entire difference between a queue and a stack, worth noticing since it's easy to assume they're more different than they are.

`CallingGenericTypes()` builds one of each and runs the same three names through both, which is the fastest way to actually feel the FIFO-vs-LIFO difference: the queue serves Scott, Andy, Alan in that order, the stack serves them Alan, Andy, Scott.

Both throw `IndexOutOfRangeException` when asked for an item from an empty collection rather than silently returning a default value, worth reading the comment in `GenericQueue.Next()` about why that's the better default: a silent `default(T)` can hide a real bug for a long time before anyone notices something's wrong.

---

## Indexers

```csharp
public int this[int index]
{
    get => ip[index];
    set { ... }
}
```

An indexer lets your own type support `[]` syntax the same way arrays and `List<T>` do. `IpAddress` uses one to expose 32 individual bits (one per array position) as if the whole thing were itself an array, `myIp[i] = 0;` reads naturally even though there's a full get/set property with validation logic running behind it. That validation is the actual point, an indexer isn't just syntax sugar for array access, it's a property, which means it can enforce rules a raw array never could, in this case rejecting anything that isn't a 0 or a 1.

---

## Bit Shifts and the Overflow Demos

`BitShifts()` and `BonusWrapAroundAndOverflow()` both exist to make an abstract idea (integers are stored as a fixed number of bits) impossible to ignore. Two things worth watching for specifically:

- **Shifting past the type's width doesn't do what you'd guess.** `ig << 33` on an `int` doesn't shift 33 bits, the shift amount itself gets reduced modulo the type's bit width first (32 for `int`, 64 for `long`), so shifting an `int` by 33 behaves identically to shifting it by 1.
- **Doubling a number by shifting left eventually flips its sign.** Keep shifting a positive `int` left and eventually you push a 1 into the bit reserved for the sign, and the number silently becomes negative, no exception, no warning, just a wrong-looking answer. `BonusWrapAroundAndOverflow()` runs the same doubling with `*= 2` afterward to prove it's not a shift-specific quirk, regular multiplication overflows exactly the same way.

If you want the deeper "why does a signed 8-bit type max out at -128 instead of -127" explanation, that's exactly what the next section works through.

---

## Two's Complement: Why Signed Types Have One Extra Negative Value

Purely a curiosity section, understanding this won't change how you write code, but it's worth actually explaining rather than leaving as an unexplained asterisk on the value-type table above.

Binary addition works through a circuit that takes two bits plus a carry-in bit, and produces a sum bit plus a carry-out bit. There's no separate binary subtraction, negative numbers get *added*. The question is how to represent negative numbers in the first place so that plain addition still works.

**The naive approach** (steal the leftmost bit as a sign flag, `0` = positive, `1` = negative) breaks immediately. Using 4 bits as an example, `0111` is `7` and `1011` would represent `-3` under this scheme. Add them the way normal binary addition works:

```
   0111  ( 7)
 + 1011  (-3, naive sign-bit scheme)
 -------
 1 0010  (2, wrong! Lost a carry bit and got the wrong answer)
```

That scheme also gives you two zeros (`0000` and `1000`, positive and negative zero), which is its own headache.

**One's complement** (flip every bit to represent the negative) fixes the addition problem mostly, but not all the way:

```
   0111  ( 7)
 + 1100  (-3, one's complement)
 -------
 1 0011  (3, still wrong, but the lost carry bit fixes it if you wrap it back around and re-add)
   0001
 -------
   0100  (4, correct, but only after manually re-adding the carry)
```

Still has a positive and negative zero, and the "wrap the carry bit back around" step needs hardware nobody wants to build just for that.

**Two's complement** is one's complement plus 1, and it resolves both problems in one move. Complementing zero and adding 1 lands back on zero exactly, no separate negative zero:

```
   1111  (one's complement of 0)
 + 0001
 -------
 1 0000  (zero, with a throwaway carry bit, exactly what we want)
```

And addition works with no manual carry-wrapping required:

```
   0111  ( 7)
 + 1101  (-3, two's complement)
 -------
 1 0100  (4, correct, and the lost carry bit can just be ignored)
```

This is also exactly why a signed type has one extra value on the negative side. With 4 bits and two's complement, the values run `0000` (0) up through `0111` (7) on the positive side, and `1111` (-1) down through `1000`, which is `-8`, not `-7`. There's no matching positive `8` to pair with it, that bit pattern was needed to fill out the negative range evenly, and it's the direct reason `sbyte.MinValue` is `-128` while `sbyte.MaxValue` is only `127`, and the same pattern holds at every signed integer size C# offers.

---

## A Bug This Pass Found

```csharp
/// <summary>
/// Defines an X/Y coordinate location as a value type
/// </summary>
public class ReferenceCoordinates
```

The doc comment on `ReferenceCoordinates` said "value type." It's a `class`, a reference type, the entire reason it exists in this file is to be the reference-type counterpart to `ValueCoordinates`. Almost certainly a copy-paste artifact from duplicating the `ValueCoordinates` doc comment as a starting point and not updating the one word that mattered. Fixed to say "reference type."

---

## Access Modifiers and Behavior Modifiers

Every class member should have an explicit accessibility modifier, don't rely on the default. C# has a lot of modifiers, and they split into two categories that answer different questions: **accessibility** (who can see this?) and **behavior** (what is this allowed to do?).

### Accessibility

| Modifier | Meaning |
|---|---|
| `public` | Accessible from anywhere. |
| `private` | Accessible only from within the declaring type. |
| `internal` | Accessible anywhere in the same assembly, not from outside it (an `internal` member in a referenced DLL is invisible to the program consuming that DLL). |
| `protected` | Accessible from the declaring type and anything that inherits from it, nothing else, not even other classes in the same assembly. |
| `sealed` | On a class, prevents anything from inheriting from it. |
| `static` | On a class, prevents instantiation entirely, members are called through the type name. On a member, means there's exactly one instance of it shared across every instance of the class. |

### Behavior

| Modifier | Meaning |
|---|---|
| `abstract` | Marks a class as a base model that can't be instantiated directly, only inherited from. |
| `async` | Marks a method or lambda as running asynchronously, the caller keeps going while it executes in the background. |
| `const` | The value can never change, and must be assigned at the point of declaration. |
| `event` | Declares that a member is an event, with a handler method containing the code that runs when it's raised. |
| `extern` | Indicates a method (usually just a signature, no body) is implemented externally, most commonly paired with `[DllImport]` for calling into unmanaged DLLs. |
| `new` | Used in a derived class to deliberately hide an inherited member sharing the same name, distinct from the `new` keyword used to invoke a constructor. |
| `override` | Implements a method that replaces an inherited one sharing the same signature. |
| `partial` | Indicates the class is also defined (at least in part) in another file in the same assembly. |
| `readonly` | The member can only be assigned at declaration or inside the constructor. Similar to `const`, but allows the value to be computed at construction time rather than requiring a compile-time literal. |
| `unsafe` | Marks code that steps outside normal .NET memory management (pointer access, for example). Powerful, easy to misuse, avoid unless there's a specific reason not to. |
| `virtual` | On a method, property, or event, explicitly permits it to be replaced in a derived class via `override`. |
| `volatile` | On a field, hints to the compiler that the value can change from outside the current code path (another thread, the OS), relevant for multi-threaded scenarios. Not needed alongside a `lock` statement, which already serializes access. |

---

## Code Standards: Variable Naming

Every variable name should be meaningful enough that the code reads like a sentence. `double accountBalance` over `double amount`, and never something like `double myDouble` or `double num`, a name that just restates the type tells a reader nothing the type declaration didn't already say.

- **Public members and properties**: `PascalCase`.
- **Classes, constants, and method names** (public or private): `PascalCase`.
- **Private and locally-scoped variables**: `camelCase`.
- **Never**: `snake_case`, `kebab-case`, or `ALL_CAPS`.
- **Never**: Hungarian notation (`strName`, `arr10Numbers`), prefixing a name with an abbreviation of its type. Modern IDEs show you the type on hover, the prefix just adds noise and goes stale the moment the type changes.
