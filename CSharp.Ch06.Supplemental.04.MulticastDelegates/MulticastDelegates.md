# Multicast Delegates

## Introduction

A delegate variable doesn't have to point at just one method. It can hold an ordered list of methods, all matching the same signature, and calling the delegate invokes every method in that list, in order. That's a multicast delegate.

---

## Building One

```csharp
internal delegate void CustomDel(string s);

private static void Hello(string s) => Console.WriteLine("  Hello, {0}!", s);
private static void Goodbye(string s) => Console.WriteLine("  Goodbye, {0}!", s);
```

```csharp
CustomDel hiDel = Hello;
CustomDel byeDel = Goodbye;

CustomDel multiDel = hiDel + byeDel;
```

`multiDel` now holds both `Hello` and `Goodbye`. Calling `multiDel("C")` runs both:

```text
  Hello, C!
  Goodbye, C!
```

---

## Removing a Method

```csharp
CustomDel multiMinusHiDel = multiDel - hiDel;
```

Subtracting a delegate removes that method from the invocation list. `multiMinusHiDel("D")` now only calls `Goodbye`:

```text
  Goodbye, D!
```

Note that `-` doesn't modify `multiDel`, delegates are immutable, `hiDel + byeDel` and `multiDel - hiDel` both produce brand new delegate instances rather than changing an existing one in place.

---

## You Don't Always Need a Custom Delegate Type

```csharp
// This works just as well as declaring CustomDel:
Action<string> hiDel = Hello;
```

`Action<string>` is a built-in generic delegate for "a method that takes a `string` and returns nothing." Custom delegate types like `CustomDel` are mostly useful for readability or when you need a very specific, named signature convention. Otherwise, `Func<...>`/`Action<...>` cover the vast majority of cases without any declaration needed.

---

## A Sharp Edge Worth Knowing

Delegate subtraction only removes the *last* matching occurrence from the invocation list, and can behave surprisingly if the same method (or equivalent lambdas) appear more than once. It's a real, supported feature, and works predictably for the common case shown here (two distinct named methods, no duplicates), but it's worth being a little cautious with in more complex multicast scenarios.

---

## Try It Yourself

Add a third method (say, `Welcome(string s)`) and combine all three into one multicast delegate. Then try subtracting the *middle* one and confirm the remaining two still fire in their original relative order.
