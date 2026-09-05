# Chapter 3: Working with the Type System

## What This Chapter Is Actually About

Value types versus reference types, and everything that distinction touches: structs, enums, generics, indexers, access modifiers, and a few genuinely strange corners of how C# stores numbers in memory. This is the chapter where "it compiles" stops being the bar and "I understand why it behaves this way" starts mattering.

Run the project and step through the menu in order. Most of the methods here print something that looks slightly wrong on purpose.

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

- **Signed types lose one bit to the sign**, so a signed 32-bit type doesn't range ±2^32, it ranges from -2^31 to (2^31 - 1), one more negative value than positive. The "Two's Complement" section below explains exactly why that asymmetry exists. It isn't arbitrary.
- **A declared-but-unassigned variable has a default value only if it's a field, not a local.** `int i = new int();` explicitly constructs the default (`0`) rather than relying on it being implicit, which is why you'll see that syntax in the type-alias lab even though `int i = 0;` says the same thing more plainly.

---

## Memory: Stack vs. Heap

**Stack**: reserved per-running-application memory, not shared with other processes. This is where value types live. It has limited space, and it unwinds automatically as scope exits, with nothing to clean up manually.

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

Every other rule in this chapter — structs being value types, classes being reference types, `ref` parameters, boxing — traces back to this one distinction.

`BonusValueVersusReference()` is the method that makes it concrete. It passes a `struct` by value, then by `ref`, then passes a `class` normally, printing the coordinates after each call so you can watch the difference happen instead of just reading about it:

```csharp
ValueCoordinates vc = new ValueCoordinates { X = 0, Y = 0 };
MoveXAxis(vc, 5);       // vc.X is still 0 - the method got a copy
MoveXAxis(ref vc, 5);   // vc.X is now 5 - the method got the original

ReferenceCoordinates rc = new ReferenceCoordinates { X = 0, Y = 0 };
MoveXAxis(rc, 5);       // rc.X is now 5, with no "ref" anywhere in sight
```

That last line is the one that surprises people. There's no `ref` keyword, and the value changed anyway, because what got copied was the reference, and both copies point at the same object. `ref` on a reference type is a different and rarer thing: it lets the method repoint your variable at an entirely different object, not just mutate the one it already has.

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

The practical rule: if it's bigger than about 16 bytes, or if it has identity that outlives a single expression, make it a class. Structs are for things that behave like numbers.

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

An enum is a named set of constant values, backed by an integer type (`byte` here, `int` by default if you don't specify one). `Feb` through `Dec` don't need explicit values; each one is automatically one more than the previous.

`Enum.TryParse` and `Enum.GetName` are how you convert between the string form and the underlying value. Worth knowing, since enums show up constantly for anything with a fixed, known set of options (status codes, days of the week, log levels).

One trap: an enum will happily hold a value that isn't one of its named members. `(Months)99` compiles, runs, and prints `99`. `Enum.IsDefined` is the check for that, and it matters any time an enum value arrives from outside your code (a database column, a config file, a web request).

### Bit Flags

```csharp
[Flags]
public enum Permissions
{
    None    = 0,
    Read    = 1,
    Write   = 2,
    Execute = 4,
    All     = Read | Write | Execute
}
```

When the values are powers of two, a single enum variable can hold any combination of them at once, because each option owns its own bit. `Read | Write` produces `3`, and `HasFlag(Permissions.Write)` answers correctly. The `[Flags]` attribute doesn't change the behavior, it changes `ToString()` so the value prints as `Read, Write` instead of `3`. The powers-of-two spacing is doing the actual work.

---

## Generics, via a Stack and a Queue

`BaseStackOrQueue<T>` holds the shared plumbing, `GenericQueue<T>` removes from the front (first in, first out), and `GenericStack<T>` removes from the end (last in, first out). Same underlying `List<T>`, opposite removal point, and that's the entire difference between a queue and a stack, which is worth noticing since it's easy to assume they're more different than they are.

`CallingGenericTypes()` builds one of each and runs the same three names through both, which is the fastest way to actually feel the FIFO-vs-LIFO difference: the queue serves Alex, Andy, Alan in that order; the stack serves them Alan, Andy, Alex.

Both throw `IndexOutOfRangeException` when asked for an item from an empty collection rather than silently returning a default value. Read the comment in `GenericQueue.Next()` about why that's the better default: a silent `default(T)` can hide a real bug for a long time before anyone notices something's wrong. A `null` that shows up three method calls later is a much worse debugging session than an exception at the exact line that caused it.

The point of the generic parameter itself is that `GenericQueue<string>` and `GenericQueue<Person>` are two genuinely distinct types, checked at compile time, generated from one piece of source. The pre-generics alternative was a collection of `object` plus a cast on every read, which moved every type error from compile time to runtime and boxed every value type on the way in.

---

## Indexers

```csharp
public int this[int index]
{
    get { ... }
    set { ... }
}
```

An indexer lets your own type support `[]` syntax the same way arrays and `List<T>` do. `IpAddress` uses one to expose 32 individual bits (one per array position) as if the whole thing were itself an array, so `myIp[i] = 0;` reads naturally even though there's a full get/set property with validation logic running behind it.

That validation is the actual point. An indexer isn't just syntax sugar for array access, it's a property, which means it can enforce rules a raw array never could, in this case rejecting anything that isn't a 0 or a 1.

---

## Bit Shifts and the Overflow Demos

`BitShifts()` and `BonusWrapAroundAndOverflow()` both exist to make an abstract idea (integers are stored as a fixed number of bits) impossible to ignore. `GetIntBinaryString()` is the helper that prints the actual bit pattern, which is what makes these demos readable at all.

Two things worth watching for specifically:

- **Shifting past the type's width doesn't do what you'd guess.** `ig << 33` on an `int` doesn't shift 33 bits. The shift amount itself gets reduced modulo the type's bit width first (32 for `int`, 64 for `long`), so shifting an `int` by 33 behaves identically to shifting it by 1.
- **Doubling a number by shifting left eventually flips its sign.** Keep shifting a positive `int` left and eventually you push a 1 into the bit reserved for the sign, and the number silently becomes negative. No exception, no warning, just a wrong-looking answer. `BonusWrapAroundAndOverflow()` runs the same doubling with `*= 2` afterward to prove it's not a shift-specific quirk; regular multiplication overflows exactly the same way.

The reason it's silent is that C# defaults to unchecked arithmetic. Wrap the same math in a `checked` block and you get an `OverflowException` instead of a wrong number:

```csharp
checked
{
    int max = int.MaxValue;
    max += 1; // OverflowException, instead of quietly becoming int.MinValue
}
```

`checked` costs a little performance and is scoped to the block it wraps, so it's not something to blanket the codebase with. It's worth reaching for deliberately anywhere a silently wrong number would be worse than a loud failure, which for us usually means anything touching counts, IDs, or money.

If you want the deeper "why does a signed 8-bit type bottom out at -128 instead of -127" explanation, that's exactly what the next section works through.

---

## Two's Complement: Why Signed Types Have One Extra Negative Value

Purely a curiosity section. Understanding this won't change how you write code, but it's worth actually explaining rather than leaving as an unexplained asterisk on the value-type table above.

Binary addition works through a circuit that takes two bits plus a carry-in bit, and produces a sum bit plus a carry-out bit. There's no separate binary subtraction; negative numbers get *added*. The question is how to represent negative numbers in the first place so that plain addition still works.

**The naive approach** (steal the leftmost bit as a sign flag, `0` = positive, `1` = negative) breaks immediately. Using 4 bits as an example, `0111` is `7` and `1011` would represent `-3` under this scheme. Add them the way normal binary addition works:

```text
   0111  ( 7)
 + 1011  (-3, naive sign-bit scheme)
 -------
 1 0010  (2, wrong! Lost a carry bit and got the wrong answer)
```

That scheme also gives you two zeros (`0000` and `1000`, positive and negative zero), which is its own headache.

**One's complement** (flip every bit to represent the negative) fixes the addition problem mostly, but not all the way:

```text
   0111  ( 7)
 + 1100  (-3, one's complement)
 -------
 1 0011  (3, still wrong, but the lost carry bit fixes it if you wrap it back around and re-add)
   0001
 -------
   0100  (4, correct, but only after manually re-adding the carry)
```

Still has a positive and negative zero, and the "wrap the carry bit back around" step needs hardware nobody wants to build just for that.

**Two's complement** is one's complement plus 1, and it resolves both problems in one move. Complementing zero and adding 1 lands back on zero exactly, so there's no separate negative zero:

```text
   1111  (one's complement of 0)
 + 0001
 -------
 1 0000  (zero, with a throwaway carry bit, exactly what we want)
```

And addition works with no manual carry-wrapping required:

```text
   0111  ( 7)
 + 1101  (-3, two's complement)
 -------
 1 0100  (4, correct, and the lost carry bit can just be ignored)
```

This is also exactly why a signed type has one extra value on the negative side. With 4 bits and two's complement, the values run `0000` (0) up through `0111` (7) on the positive side, and `1111` (-1) down through `1000`, which is `-8`, not `-7`. There's no matching positive `8` to pair with it; that bit pattern was needed to fill out the negative range evenly. It's the direct reason `sbyte.MinValue` is `-128` while `sbyte.MaxValue` is only `127`, and the same pattern holds at every signed integer size C# offers.

One practical consequence: `Math.Abs(int.MinValue)` throws, because the positive result simply doesn't exist in the type. That's the asymmetry showing up in real code rather than in a diagram.

---

## Access Modifiers and Behavior Modifiers

Every class member should have an explicit accessibility modifier. Don't rely on the default. C# has a lot of modifiers, and they split into two categories that answer different questions: **accessibility** (who can see this?) and **behavior** (what is this allowed to do?).

### Accessibility

| Modifier | Meaning |
|---|---|
| `public` | Accessible from anywhere. |
| `private` | Accessible only from within the declaring type. |
| `internal` | Accessible anywhere in the same assembly, not from outside it (an `internal` member in a referenced DLL is invisible to the program consuming that DLL). |
| `protected` | Accessible from the declaring type and anything that inherits from it, nothing else, not even other classes in the same assembly. |
| `sealed` | On a class, prevents anything from inheriting from it. |
| `static` | On a class, prevents instantiation entirely; members are called through the type name. On a member, means there's exactly one instance of it shared across every instance of the class. |

### Behavior

| Modifier | Meaning |
|---|---|
| `abstract` | Marks a class as a base model that can't be instantiated directly, only inherited from. |
| `async` | Marks a method or lambda as running asynchronously; the caller keeps going while it executes in the background. |
| `const` | The value can never change, and must be assigned at the point of declaration. |
| `event` | Declares that a member is an event, with a handler method containing the code that runs when it's raised. |
| `extern` | Indicates a method (usually just a signature, no body) is implemented externally, most commonly paired with `[DllImport]` for calling into unmanaged DLLs. |
| `new` | Used in a derived class to deliberately hide an inherited member sharing the same name. Distinct from the `new` keyword used to invoke a constructor. |
| `override` | Implements a method that replaces an inherited one sharing the same signature. |
| `partial` | Indicates the class is also defined (at least in part) in another file in the same assembly. |
| `readonly` | The member can only be assigned at declaration or in the constructor. Similar to `const`, except it works for reference types and allows the value to be set programmatically when the instance is built. |
| `virtual` | Marks a member as available to be overridden by a derived class. Without it, a derived class can only hide the member with `new`, which is not the same thing and behaves differently when accessed through a base-type reference. |

The `const` versus `readonly` distinction is the one that bites in practice. `const` is baked into the calling assembly at compile time, so changing a `public const` in a shared library and redeploying only that library leaves every consumer still using the old value until they're rebuilt too. `readonly` is read at runtime and doesn't have that problem. For anything public and shared, prefer `readonly`.

---

## Chapter Takeaways

- Value types copy data, reference types copy addresses. Everything else in this chapter follows from that.
- `struct` for small value-like bundles, `class` for everything else.
- Enums are integers with names, and they'll hold values you never declared, so validate anything that arrives from outside.
- Generics give you compile-time type safety and no boxing, from one piece of source.
- Indexers are properties, which means they can validate; arrays can't.
- Integer overflow is silent by default. `checked` makes it loud where that matters.
- Signed types have one extra negative value because of two's complement, not because of an off-by-one somewhere.
- Always write the accessibility modifier explicitly, and prefer `readonly` over `const` for anything crossing an assembly boundary.