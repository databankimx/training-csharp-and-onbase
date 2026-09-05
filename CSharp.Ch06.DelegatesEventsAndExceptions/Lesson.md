# Chapter 6: Delegates, Events, and Exceptions

## What This Chapter Is Actually About

Methods as values. A delegate is a variable that holds a method instead of a number or a string, and once that idea lands, everything else in this chapter — anonymous methods, lambdas, events, callbacks, LINQ, `async`/`await` — is a variation on it.

This project is two WinForms applications: `Chapter6Form` (delegates, anonymous methods, lambdas, events, a background thread) and `GraphForm` (three ways to define the same kind of function), launched from a button on the main form. No bugs found — this is well-crafted original content, not adapted from the textbook.

Chapter 6 also has eight supplemental projects, listed at the bottom of this document and documented separately.

---

## Delegates vs. Interfaces

The chapter notes embedded in `Chapter6Form.cs` make a case worth internalizing. Delegates and interfaces both let a class designer separate a type's declaration from its implementation, but they solve differently shaped problems.

Reach for a **delegate** when:

- An eventing pattern is in play.
- You want to encapsulate a static method.
- The caller doesn't need access to anything else on the implementing object.
- You want easy composition (combining several handlers into one).
- A class might need more than one implementation of the "method."

Reach for an **interface** when:

- There's a group of related methods, not just one.
- A class only ever needs one implementation.
- Callers will want to cast between interface and class types.
- The method is intrinsically tied to the type's identity.

Comparison methods are the canonical case for that last point: `IComparable` over a delegate, because the comparison logic doesn't change at runtime and genuinely belongs to the class itself. Contrast that with a button's click behavior, which has nothing to do with what a button *is* and everything to do with what this particular screen needs — that's delegate territory.

The one-line version: an interface says *what a type is*, a delegate says *what a piece of code does*.

---

## Declaring and Using a Delegate

```csharp
private delegate float FunctionDelegate(float x);
private FunctionDelegate theFunction;
```

The `delegate` declaration defines a **type**, not a variable. It says "any method that takes a `float` and returns a `float` is compatible with this." The second line then declares a variable of that type.

```csharp
theFunction = DelegatedFunctionForLoad;
MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));
```

Note the assignment has no parentheses. `DelegatedFunctionForLoad` refers to the method itself; `DelegatedFunctionForLoad()` would *call* it and assign the result. That distinction is the single most common early mistake with delegates, and the compiler error it produces ("cannot convert `float` to `FunctionDelegate`") is at least a clear one.

Once assigned, you invoke the delegate exactly like calling the method directly: `theFunction(1)`.

`theFunction` gets reassigned to a *different* method (`DelegatedFunctionForUnload`) in `FormClosing`, which is the whole point of the demonstration. The rest of the code depends on the **variable**, not on any particular method name, so swapping the behavior requires no changes anywhere the delegate is called. That's late binding, and it's the mechanism behind dependency injection, strategy patterns, and most plugin architectures.

### The Built-In Delegate Types

Custom `delegate` declarations still work, but modern C# rarely needs them. The framework provides generic ones:

| Type | Signature |
|---|---|
| `Action` | No parameters, returns `void` |
| `Action<T>` | Takes `T`, returns `void` |
| `Action<T1, T2>` | Two parameters, returns `void` (up to 16) |
| `Func<TResult>` | No parameters, returns `TResult` |
| `Func<T, TResult>` | Takes `T`, returns `TResult` |
| `Predicate<T>` | Takes `T`, returns `bool` |

`FunctionDelegate` above is exactly `Func<float, float>` — and `GraphForm`, later in this same project, uses `Func<float, float>` directly rather than declaring its own type. Seeing both in one solution is deliberate: the custom declaration shows you what a delegate type *is*, and the `Func` version shows you what you'll actually write.

Use the built-ins unless the custom name genuinely adds clarity, or you need `ref`/`out` parameters, which `Action` and `Func` cannot express.

---

## Anonymous Methods

```csharp
BtnAnon.Click += delegate (object o, EventArgs e)
{
	clicks++;
	if (clicks > 3)
	{
		MyEvent?.Invoke();
	}
	else
	{
		MessageBox.Show($@"I'm anonymous! - Clicked [{clicks}/3] times");
	}
};
```

This is an anonymous method — a delegate literal with no separately declared method behind it. The code exists only at the point where it's needed, and nothing else can call it because nothing else can name it.

Worth noticing what's deliberately different about `BtnAnon` versus `BtnGraphForm` in this same file. `BtnGraphForm.Click` is wired up in `Chapter6Form.Designer.cs` — the normal Designer-driven path, which generates a named handler method. `BtnAnon.Click` is wired entirely in the constructor, in code, using an anonymous method. Both are valid, and seeing them side by side in the same form is the point: one is what the Designer generates for you, the other is what you write by hand when the handler is simple enough not to need a name.

Also worth noticing: `clicks` is a field the anonymous method reads and increments. An anonymous method can reach variables from the scope where it was written, and it keeps them alive for as long as the delegate exists. That's a **closure**, and it's covered properly in the lambda expressions supplemental — but this is where it first appears.

---

## Events

```csharp
public delegate void MyEventHandler();
public event MyEventHandler MyEvent;
```

```csharp
// In Chapter6Form_Load:
MyEvent = () => MessageBox.Show(@"Too many clicks!");
```

```csharp
// In the BtnAnon click handler, once clicks > 3:
MyEvent?.Invoke();
```

`MyEvent` is declared as a custom event using a custom delegate type. (More idiomatic modern code would use `Action`, or the `EventHandler<T>` pattern — worth noticing that distinction if you compare this to newer C# style.) It's assigned a lambda expression as its handler in `Form_Load`, then invoked conditionally once the click count passes a threshold.

**What `event` actually adds.** An `event` is a delegate field with restrictions applied. Outside the declaring class, subscribers can only use `+=` and `-=`. They cannot assign with `=` (which would wipe out every other subscriber), and they cannot invoke it (only the declaring type decides when the event fires). Remove the `event` keyword and you have a plain public delegate field that any caller can overwrite or raise — which is exactly the encapsulation failure `event` exists to prevent.

Note that inside `Chapter6Form`, the code *does* use `MyEvent = ...`, which is legal precisely because that restriction only applies to external code. In a class with multiple potential subscribers, `+=` would be the safer habit even internally.

**The `?.Invoke()` pattern.** The null-conditional call is the modern equivalent of the classic `if (MyEvent != null) MyEvent();` check. Both exist because an event with no subscribers is `null`, not empty, and invoking `null` throws. The `?.` form is also thread-safe in a way the `if` check isn't — between the `null` test and the call, another thread could unsubscribe the last handler, and `?.` reads the reference once to avoid that race.

---

## A Background Thread via Anonymous Method

```csharp
var t1 = new Thread(delegate ()
{
	MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
});
t1.Start();
```

This is the textbook case for an anonymous method: code used in exactly one place, simple enough that giving it a name would just be extra ceremony.

It's also a demonstration that `Thread` doesn't take "some code" — it takes a **delegate**. `ThreadStart` is a delegate type (no parameters, returns `void`), and the anonymous method is being converted to it. Every threading API in .NET works this way, which is why delegates come before threading in the curriculum rather than after.

The constructor used here takes a `ThreadStart`. There's also a `ParameterizedThreadStart` overload that lets you pass a single `object` argument into the thread's entry point, which is exactly what `CSharp.Ch06.Supplemental.06.ParameterizedThreadStart` covers. Worth comparing the two once you get to that lesson.

---

## GraphForm: Three Ways to Define the Same Kind of Function

```csharp
private Func<float, float> theFunction;
```

```csharp
case 0: // Expression lambda syntax
	theFunction = x => (float)(12 * Math.Sin(3 * x) / (1 + Math.Abs(x)));
	break;

case 1: // Anonymous method delegate syntax
	theFunction = delegate (float x)
	{
		x = Math.Abs(x);
		if (x < 0.001) return 20;
		return (float)Math.Abs(20 * Math.Cos(x) / (x + 1));
	};
	break;

case 2: // Statement lambda syntax, multi-line body
	theFunction = x =>
	{
		const float a = -0.0003f;
		// ...six more constants...
		return (((((a * x + b) * x + c) * x + d) * x + e) * x + f) * x + g;
	};
	break;
```

All three cases assign to the exact same `Func<float, float>` variable, and `DrawGraph()` calls it identically regardless of which syntax produced it (`theFunction(x)`).

That's the concrete lesson: expression lambdas, anonymous-method delegates, and statement lambdas are three different *syntaxes* for the same underlying thing — a value that can be invoked like a method. The consuming code neither knows nor cares which one was used.

Case 1 is worth reading closely, since it's a full anonymous method (braces, an early `return`, multiple statements), not just a one-liner. Anonymous methods aren't limited to trivial bodies.

The practical distinction between the three, in order of preference for new code:

- **Expression lambda** (`x => expr`) — shortest, no `return` keyword, no braces. Use when the body is one expression.
- **Statement lambda** (`x => { ...; return v; }`) — braces and explicit `return`, but still infers the parameter type from context. Use when you need multiple statements.
- **Anonymous method** (`delegate (float x) { ... }`) — the C# 2.0 syntax, requires the parameter type to be written out. Effectively legacy; lambdas do everything it does with less typing.

`GraphForm` also demonstrates that `theFunction` can be swapped at runtime by changing a combo box selection, which is the same late-binding idea from `Chapter6Form` applied to something you can watch redraw on screen.

---

## Worth Knowing: A Documented, Intentional Loose End

```csharp
Load += delegate
{
	EquationComboBox.SelectedIndex = 0;
};
// This is equivalent to the following using a named method:
// Load += GraphForm_Load;
```

`GraphForm_Load` is a fully written, correct method that's never actually wired to anything. It exists purely as a documented point of comparison — "here's what the named-method version of this exact line would look like."

Unlike the unwired `Load` handlers found as real bugs elsewhere in this training set (`ShortPathNames`, `Ch05RealWorldScenario01`), this one is intentional and explained by the comment directly above it. Worth being able to tell the two apart: an unwired handler with a comment explaining why is a teaching device; an unwired handler with no explanation is usually a bug someone introduced while refactoring.

Note also the bare `delegate { ... }` with no parameter list at all. That's an anonymous-method-only shortcut: when you don't need the parameters, you can omit them entirely and the compiler will still match the delegate signature. Lambdas can't do this — they require either the parameters or a discard.

---

## Where Exceptions Fit

The chapter title includes exceptions, but the main project stays focused on delegates and events. Exception handling gets its own dedicated treatment in `CSharp.Ch06.Supplemental.05.ExceptionHandling`, and assertion-based defensive checking in `CSharp.Ch06.Supplemental.08.Assertions`.

The connection between the two halves of the chapter is real, though, and worth flagging now: an exception thrown inside a delegate invoked by someone else's code — an event handler, a callback, a thread entry point — does not propagate back to whoever set it up. It surfaces wherever the invocation actually happened, which may be a framework method you don't control. That's why event handlers and thread bodies typically need their own `try`/`catch`, and it's a genuine source of "the application just disappeared" bugs.

---

## Chapter Takeaways

- A delegate is a type whose values are methods. Assign without parentheses; invoke with them.
- Prefer `Action`/`Func`/`Predicate` over custom `delegate` declarations unless you need `ref`/`out`.
- Delegate for "what this code does," interface for "what this type is."
- `event` is a delegate field with `=` and invocation locked down to the declaring class.
- Always raise events with `?.Invoke()` — no subscribers means `null`, not empty.
- Anonymous methods and lambdas are syntax variations on the same concept; prefer lambdas in new code.
- Threading APIs take delegates, which is why delegates are taught first.
- Exceptions thrown inside a delegate surface at the invocation site, not the subscription site.

---

## Also in Chapter 6

Eight supplemental projects accompany this one, documented separately:

1. `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`
2. `CSharp.Ch06.Supplemental.02.LambdaExpressions`
3. `CSharp.Ch06.Supplemental.03.Callbacks`
4. `CSharp.Ch06.Supplemental.04.MulticastDelegates`
5. `CSharp.Ch06.Supplemental.05.ExceptionHandling`
6. `CSharp.Ch06.Supplemental.06.ParameterizedThreadStart`
7. `CSharp.Ch06.Supplemental.07.Events`
8. `CSharp.Ch06.Supplemental.08.Assertions`
