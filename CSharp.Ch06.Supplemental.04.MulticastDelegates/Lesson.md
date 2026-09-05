# Chapter 6 Supplemental 04: Multicast Delegates

## What This Is

The canonical multicast-delegate example — essentially Microsoft's own documentation sample for the feature — combining two delegates with `+`, then removing one with `-`. No bugs found.

Small project, but the concept underneath it is the entire mechanism behind C# events, so it's worth more attention than its size suggests.

---

## Every Delegate Is Already a Multicast Delegate

The word "multicast" makes this sound like a special case. It isn't. Every delegate type you declare in C# derives from `System.MulticastDelegate`, which means every delegate variable holds an *invocation list* — an ordered collection of methods — rather than a single method reference.

A delegate pointing at one method is just a multicast delegate with a list of length one. Nothing changes structurally when you combine two; the list gets longer.

---

## Combining and Subtracting

```csharp
internal delegate void CustomDel(string s);
```

```csharp
CustomDel hiDel = Hello;
CustomDel byeDel = Goodbye;

CustomDel multiDel = hiDel + byeDel;
CustomDel multiMinusHiDel = multiDel - hiDel;
```

```csharp
Console.WriteLine("Invoking delegate hiDel:");
hiDel("A");                 // Hello, A!

Console.WriteLine("Invoking delegate byeDel:");
byeDel("B");                // Goodbye, B!

Console.WriteLine("Invoking delegate multiDel:");
multiDel("C");              // Hello, C!  Goodbye, C!

Console.WriteLine("Invoking delegate multiMinusHiDel:");
multiMinusHiDel("D");       // Goodbye, D!
```

`multiDel("C")` calls **both** `Hello` and `Goodbye`, in the order they were combined. That's what "multicast" means: a single delegate variable holding an ordered list of methods, all invoked in sequence on one call, each receiving the same argument.

`multiDel - hiDel` produces a new delegate whose invocation list has `Hello` removed, leaving only `Goodbye`.

Three details that matter more than the output:

**Order is guaranteed.** The invocation list runs in the order methods were added. `hiDel + byeDel` always prints "Hello" first. Don't *rely* on that in production event code — subscription order is rarely something you control across a codebase — but the mechanism itself is deterministic.

**Nothing is mutated.** `hiDel + byeDel` produces a *new* delegate. `hiDel` still points at only `Hello` afterward, which is why the subsequent `hiDel("A")` still prints just one line. Delegates are immutable; `+` and `-` always return new instances.

**Invocation is synchronous and sequential.** One call, one thread, each method running to completion before the next starts. If `Hello` blocks, `Goodbye` waits. If `Hello` throws, `Goodbye` never runs at all — see below.

---

## The Two Sharp Edges

The demo works cleanly because it uses two distinct `void`-returning named methods. Both problems below appear the moment you step outside that.

### Return values: only the last one survives

If the delegate type returns a value rather than `void`, invoking a multicast delegate returns only the result of the **last** method in the list. Every earlier return value is silently discarded.

```csharp
Func<int> f = () => 1;
f += () => 2;
int result = f();   // 2 — the first lambda still ran, its result was thrown away
```

That's why the delegate types behind events are conventionally `void`-returning. If you need results from every subscriber, you have to walk the list yourself with `GetInvocationList()` and invoke each entry individually.

### Exceptions: the list stops

If any method in the invocation list throws, the exception propagates immediately to the caller and the remaining methods never run. One badly behaved subscriber silently prevents every subscriber after it from being notified.

`GetInvocationList()` is the escape hatch for both problems:

```csharp
foreach (CustomDel d in multiDel.GetInvocationList())
{
	try { d("C"); }
	catch (Exception ex) { /* log and continue to the next subscriber */ }
}
```

Worth knowing this exists even if you rarely need it. When someone asks why an event handler "sometimes doesn't fire," an earlier subscriber throwing is a common answer.

---

## The Connection to Events

This is the payoff. `+=` on an event is exactly the `+` shown here:

```csharp
button.Click += Handler1;   // invocation list: [Handler1]
button.Click += Handler2;   // invocation list: [Handler1, Handler2]
button.Click -= Handler1;   // invocation list: [Handler2]
```

An event is a multicast delegate field with `=` and direct invocation restricted to the declaring class. That's the only difference. Everything about how multiple subscribers work — the ordering, the immutability, the "returns only the last value," the "an exception stops the list" — comes from this project's mechanics, not from anything the `event` keyword adds.

It also explains why an event with no subscribers is `null` rather than an empty list, and therefore why `?.Invoke()` is mandatory. `-=` on the last remaining handler produces `null`, not an empty delegate.

---

## Worth Noticing: The Commented-Out `Action<string>` Alternative

```csharp
// In this example, you can omit the custom delegate if you
// want to and use Action<string> instead.
//Action<string> hiDel, byeDel, multiDel, multiMinusHiDel;
```

`CustomDel` didn't need to be a custom-declared delegate type at all. `Action<string>` — the built-in generic delegate matching "takes a string, returns nothing" — would work identically here.

The custom delegate exists mainly for clarity in this teaching example, where seeing the declaration alongside the usage makes the type relationship explicit. In your own code, prefer the built-in `Func<...>`/`Action<...>` types unless you have a specific reason to declare your own: less boilerplate, and immediately recognizable to anyone else reading the code.

Remember the constraint from Supplemental 01, though: you can only combine delegates of the *same type*. `CustomDel` and `Action<string>` have identical signatures and still cannot be added together, because delegate typing is nominal. That's an argument in favor of the built-ins too — everyone using `Action<string>` can interoperate; everyone declaring their own type cannot.

---

## Worth Noticing: `// ReSharper disable once DelegateSubtraction`

Delegate subtraction (`multiDel - hiDel`) is a real, supported C# feature, but ReSharper and similar analyzers flag it by default because it has a genuine sharp edge.

If a multicast delegate's invocation list contains a method more than once, `-` removes only the **last** occurrence. And when subtracting a multi-entry delegate, it removes only a contiguous matching *sequence* — if the methods aren't adjacent in the list, nothing is removed at all, silently, with no error.

Results also get surprising with lambdas. Two lambdas with identical bodies are different delegate instances, so subtracting one won't remove the other:

```csharp
del += () => Console.WriteLine("x");
del -= () => Console.WriteLine("x");   // removes nothing
```

That's the same reason `-=` fails to unsubscribe an anonymous handler from an event — you need a stored reference to the exact delegate instance that was added. It's a real and frequent cause of event-handler memory leaks.

The subtraction works correctly and predictably in this project — two distinct named methods, no duplicates — so the ReSharper suppression is a reasonable, deliberate acknowledgment rather than something accidentally left in.

---

## Compare Against Supplemental 01

`CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`'s `CombineDelegates()` method covers the same `+`/`-` mechanics as a quick preview before moving on to other topics. This project is the fuller, dedicated treatment. Worth running both back to back if the concept didn't fully land the first time.

---

## Takeaways

- Every C# delegate is a multicast delegate holding an ordered invocation list.
- `+` and `-` return new delegates; the originals are unchanged.
- Invocation is synchronous and sequential, in subscription order.
- A multicast delegate returns only the *last* method's return value — the rest are discarded.
- An exception in any handler stops the remaining handlers from running.
- `GetInvocationList()` lets you invoke each subscriber individually to work around both limitations.
- `+=`/`-=` on events is exactly this mechanism, which is why an unsubscribed event is `null`.
- `-=` can't remove a lambda you didn't keep a reference to — a common leak source.
- Prefer `Action`/`Func` over custom delegate types; identical signatures are still incompatible types.
