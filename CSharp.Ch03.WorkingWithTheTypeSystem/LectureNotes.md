# Chapter 3: Working with the Type System

## What This Chapter Is Actually About

Value types versus reference types, and everything that distinction touches: structs, enums, generics, indexers, access modifiers, and a couple of genuinely weird corners of how C# stores numbers in memory. This is the chapter where "it compiles" stops being the bar and "I understand why it behaves this way" starts mattering.

The `Program.cs` for this chapter carries extensive inline notes (the two's-complement aside, the full modifier table, the struct-vs-class breakdown), deliberately left intact rather than duplicated here. These notes cover the parts worth calling out separately, plus one real bug this pass turned up.

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

If you want the deeper "why does a signed 8-bit type max out at -128 instead of -127" explanation, that's what the two's-complement aside in `Program.cs` is for. It's genuinely optional, understanding it won't change how you write code, but it answers a question a lot of people privately wonder about and never look up.

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

## Access Modifiers, Condensed

The full table lives in `Program.cs`, worth having memorized regardless:

- **`public`** — accessible from anywhere.
- **`private`** — accessible only within the declaring type. The default for class members if you don't specify one, though the house style here is to always write it explicitly.
- **`internal`** — accessible anywhere in the same assembly, not outside it.
- **`protected`** — accessible from the declaring type and anything that inherits from it.

Everything else in that table (`abstract`, `static`, `virtual`, `override`, `sealed`, `readonly`, and the rest) describes *behavior* rather than *accessibility*, worth keeping those two categories mentally separate since the table groups them together but they answer different questions: who can see this, versus what is this allowed to do.
