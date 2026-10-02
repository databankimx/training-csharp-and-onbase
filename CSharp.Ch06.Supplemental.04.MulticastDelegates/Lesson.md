# Chapter 6 Supplemental 04: Multicast Delegates

## What This Is

The canonical multicast-delegate example: combining two delegates with `+`, removing one with `-`. Small project, but the concept underneath is the entire mechanism behind C# events, so it's worth more attention than its size suggests.

---

## Every Delegate Is Already a Multicast Delegate

The word "multicast" makes this sound like a special case. It isn't. Every delegate type in C# derives from `System.MulticastDelegate`, which means every delegate variable holds an *invocation list* - an ordered collection of methods - rather than a single method reference. A delegate pointing at one method is just a multicast delegate with a list of length one.

---

## How to Write This Program

### Mini-Program: Combine, Invoke, Subtract

```csharp
internal delegate void CustomDel(string s);

private static void Hello(string s)   { Console.WriteLine($"Hello, {s}!"); }
private static void Goodbye(string s) { Console.WriteLine($"Goodbye, {s}!"); }
```

```csharp
CustomDel hiDel  = Hello;
CustomDel byeDel = Goodbye;

CustomDel multiDel        = hiDel + byeDel;
CustomDel multiMinusHiDel = multiDel - hiDel;

Console.WriteLine("Invoking hiDel:");
hiDel("A");

Console.WriteLine("\nInvoking byeDel:");
byeDel("B");

Console.WriteLine("\nInvoking multiDel:");
multiDel("C");

Console.WriteLine("\nInvoking multiMinusHiDel:");
multiMinusHiDel("D");
```

Run it. `hiDel("A")` prints one line. `byeDel("B")` prints one line. `multiDel("C")` prints *two* lines - `Hello, C!` then `Goodbye, C!`. `multiMinusHiDel("D")` prints one line again.

Three details that matter more than the output:

**Order is guaranteed.** The invocation list runs in the order methods were added. `hiDel + byeDel` always runs `Hello` before `Goodbye`.

**Nothing is mutated.** `hiDel + byeDel` produces a *new* delegate. `hiDel` still points at only `Hello` afterward. Delegates are immutable. `+=` on an event is `x = x + y` under the hood, not an in-place append.

**Invocation is synchronous and sequential.** One call, one thread, each method runs to completion before the next starts. If `Hello` blocks, `Goodbye` waits. If `Hello` throws, `Goodbye` never runs.

---

## You Don't Always Need a Custom Delegate Type

The source file includes a commented-out alternative worth reading:

```csharp
// Action<string> hiDel, byeDel, multiDel, multiMinusHiDel;
```

`CustomDel` didn't need to be a custom-declared delegate type at all. `Action<string>` is a built-in generic delegate matching "takes a string, returns nothing" and would work identically here. The custom type exists for clarity in a teaching context. In production code, prefer the built-in `Func<...>`/`Action<...>` types unless you have a specific reason to declare your own - less to declare, and immediately recognizable to anyone reading the code.

---

## The Two Sharp Edges

The demo works cleanly because both methods return `void` and are distinct named methods. Both problems appear the moment you step outside that.

### Return values: only the last one survives

If the delegate type returns a value, invoking a multicast delegate returns only the *last* method's result. Every earlier return value is silently discarded.

```csharp
Func<int> f = () => 1;
f += () => 2;
int result = f();   // 2 - the first lambda ran, its result was thrown away
```

That's why events use `void`-returning delegates by convention. If you need results from every subscriber, walk the list yourself with `GetInvocationList()` and invoke each entry individually.

### Exceptions: the list stops

If any method throws, the exception propagates immediately and the remaining methods never run. One badly-behaved subscriber silently prevents every subscriber after it from being notified.

`GetInvocationList()` is the escape hatch for both problems:

```csharp
foreach (CustomDel d in multiDel.GetInvocationList())
{
    try { d("C"); }
    catch (Exception ex) { /* log and continue */ }
}
```

### Delegate subtraction has edge cases

`multiDel - hiDel` works correctly here - two distinct named methods, no duplicates. Two situations where it gets surprising: if a method appears more than once in the list, `-` removes only the *last* occurrence; and subtracting a lambda fails silently unless you stored a reference to the exact same delegate instance that was added. That's the leading cause of event-handler memory leaks.

The `// ReSharper disable once DelegateSubtraction` comment in the source is a deliberate acknowledgment that the subtraction works correctly in this specific case.

---

## The Connection to Events

`+=` on an event is exactly the `+` shown here:

```csharp
button.Click += Handler1;   // invocation list: [Handler1]
button.Click += Handler2;   // invocation list: [Handler1, Handler2]
button.Click -= Handler1;   // invocation list: [Handler2]
```

An `event` is a multicast delegate field with `=` and direct invocation restricted to the declaring class. That's the only difference. Everything about how multiple subscribers work - ordering, immutability, last-return-wins, exception-stops-the-list - comes from this project's mechanics, not from anything the `event` keyword adds.

It also explains why an event with no subscribers is `null` rather than an empty list: `-=` on the last remaining handler produces `null`, not an empty delegate. Which is exactly why `?.Invoke()` is mandatory.

---

## Try It Yourself

Add a third method (say, `Welcome(string s)`) and combine all three into one multicast delegate. Then subtract the *middle* one and confirm the remaining two still fire in their original relative order.

Compare this project against `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`'s `CombineDelegates()` method, which previews `+`/`-` briefly. This project is the fuller, dedicated treatment.

---

## Takeaways

- Every C# delegate is a multicast delegate - the invocation list is always there, even when it has one entry.
- `+` and `-` return new delegates; originals are unchanged.
- Invocation is synchronous and sequential.
- A multicast delegate returns only the *last* return value - the rest are discarded.
- An exception in any handler stops the remaining handlers.
- `GetInvocationList()` lets you invoke each subscriber individually to work around both.
- `+=`/`-=` on events is exactly this mechanism.
- `-=` can't remove a lambda you didn't keep a reference to.
- Prefer `Action`/`Func` over custom delegate types - identical signatures are still incompatible types.
