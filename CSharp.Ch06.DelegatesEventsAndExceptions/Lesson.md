# Chapter 6 - Delegates, Events, and Exceptions

## What This Chapter Is Actually About

Methods as values. A delegate is a variable that holds a method instead of a number or a string, and once that idea lands, everything else in this chapter -- anonymous methods, lambdas, events, callbacks, threading -- is a variation on the same concept.

This chapter's main project is two WinForms applications: `Chapter6Form` (delegates, anonymous methods, lambdas, events, a background thread) and `GraphForm` (three syntaxes for the same idea), launched from a button on the first form. The supplementals are all console programs that let you examine the same ideas more deliberately.

---

## Delegate vs. Interface: Which Tool When

Before writing code, one decision worth internalizing upfront. Delegates and interfaces both let a class designer separate "what needs to happen" from "how it happens," but they're suited to different problems.

Reach for a **delegate** when:
- An eventing or callback pattern is in play.
- You want to encapsulate a static method.
- The caller doesn't need access to anything else on the implementing object.
- A class might need more than one implementation of the same method.
- Easy composition (combining several handlers) is desirable.

Reach for an **interface** when:
- There's a group of related methods, not just one.
- A class only ever needs one implementation.
- The method is intrinsically tied to the type's identity -- `IComparable` is the canonical example, because the comparison logic belongs to the class and doesn't change at runtime.

The one-line version: an interface says *what a type is*; a delegate says *what a piece of code does*.

---

## How to Write This Program

This project is WinForms -- you can't write a `Main()` and run it directly the way the last few chapters' console programs work. The walkthrough below describes building the form and its code-behind piece by piece, with notes on where to run it and what to watch for at each stage. If you want to try the concepts in isolation before wiring up a form, each supplemental project in this chapter is a console program that runs standalone.

### Step 1: Declare a Delegate Type and a Variable

In `Chapter6Form.cs`, before the constructor:

```csharp
// The delegate type -- a signature any method returning float and taking a float can satisfy
private delegate float FunctionDelegate(float x);

// The delegate variable -- holds a reference to whichever method is currently assigned
private FunctionDelegate theFunction;
```

A `delegate` declaration defines a **type**, not a variable. The second line then declares a variable of that type, exactly the way `int myInt;` declares an `int` variable. The type can be satisfied by any method with the matching signature, regardless of where that method is declared or whether it's static or instance.

### Step 2: Assign It, and Notice What "Assign" Means Here

In `Chapter6Form_Load`:

```csharp
theFunction = DelegatedFunctionForLoad;
MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));
```

Read those two lines carefully. `DelegatedFunctionForLoad` with no parentheses assigns the *method itself* to the variable. `DelegatedFunctionForLoad()` with parentheses would call it and assign the *result*, which would be a `float` and wouldn't compile against a `FunctionDelegate`. The parentheses are the entire difference between "store a reference to this method" and "call this method right now."

Once assigned, `theFunction(1)` invokes whatever method is stored there, exactly as if you'd called `DelegatedFunctionForLoad(1)` directly.

Now run the application. A message box appears with a number -- the result of calling `DelegatedFunctionForLoad(1)`.

### Step 3: Swap the Method, and Watch the Variable's Behavior Change

In `Chapter6Form_FormClosing`:

```csharp
theFunction = DelegatedFunctionForUnload;
MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));
```

The two methods (`DelegatedFunctionForLoad`, `DelegatedFunctionForUnload`) compute genuinely different results for the same input. `theFunction(1)` called on close produces a different number than `theFunction(1)` called on load, even though the call site is identical. The variable changed; the call site didn't.

That's late binding, and it's the mechanism behind dependency injection, strategy patterns, and most plugin architectures. The caller doesn't depend on a specific method -- it depends on anything that satisfies the signature.

### Step 4: Wire Up an Anonymous Method

In the constructor, before `InitializeComponent`:

```csharp
private int clicks;
public delegate void MyEventHandler();
public event MyEventHandler MyEvent;
```

Still in the constructor, after `InitializeComponent`:

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

An anonymous method is a delegate literal -- the code exists right here at the point where it's needed, with no separately named method. Run the application and click `BtnAnon` four times. The first three clicks show a message; the fourth raises `MyEvent`.

Notice two things about `BtnAnon` versus `BtnGraphForm`:

- `BtnGraphForm.Click` is wired in `Chapter6Form.Designer.cs` -- the normal Visual Studio Designer path, which generates a named method in your code-behind.
- `BtnAnon.Click` is wired right here in the constructor, in code, using an anonymous method.

Both are valid. The anonymous method makes sense specifically because this handler is simple, used in exactly one place, and doesn't need a name the rest of the code can call.

Also notice `clicks` -- a field the anonymous method reads and increments. An anonymous method can reach variables from the scope where it was declared, and it keeps them alive for as long as the delegate exists. That's called a **closure**. `CSharp.Ch06.Supplemental.09.Closures` covers this in depth; for now, just note that the anonymous method "remembers" `clicks` across calls.

### Step 5: Declare and Raise an Event

You've already declared `MyEvent` in Step 4. Now wire up its handler in `Chapter6Form_Load`:

```csharp
MyEvent = () => MessageBox.Show(@"Too many clicks!");
```

And in the anonymous method from Step 4, it's already raised:

```csharp
MyEvent?.Invoke();
```

Two things to understand about the `event` keyword:

**What `event` adds.** `MyEvent` is declared as an `event`, not a bare delegate field. The difference is encapsulation. Outside the class, subscribers can only use `+=` and `-=`. They can't overwrite the entire handler list with `=`, and they can't invoke the event themselves. Remove `event` and you have a plain public field where any caller can wipe out every subscriber or raise the event at will. The `event` keyword prevents both.

**Why `?.Invoke()` instead of `MyEvent()`.** An event with no subscribers is `null`, not an empty list. Invoking `null` throws `NullReferenceException`. The `?.` null-conditional call handles that -- if `MyEvent` is `null`, the call is silently skipped. It's also thread-safe in a way the classic `if (MyEvent != null) MyEvent()` check isn't, since another thread could unsubscribe between the check and the call.

### Step 6: Start a Background Thread With an Anonymous Method

```csharp
private static void StartThread()
{
    var t1 = new Thread(delegate ()
    {
        MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
    });
    t1.Start();
}
```

Call `StartThread()` from the constructor. Run the application -- a second message box appears almost immediately, possibly before the form even fully loads, from a different thread than the one running the UI.

`Thread`'s constructor takes a `ThreadStart` delegate -- no parameters, returns `void`. The anonymous method satisfies that and becomes the thread's entry point. This is the textbook case for an anonymous method: code used in exactly one place, simple enough not to need a name.

Two threads running simultaneously means execution order is no longer what you'd read top-to-bottom in source. The message box appears whenever the OS schedules that thread to run. This is the opening of Chapter 7's territory; what matters here is that threading is built entirely on delegates under the hood.

Compare this to the supplemental `CSharp.Ch06.Supplemental.06.ParameterizedThreadStart`, which uses the other `Thread` constructor overload -- `ParameterizedThreadStart` -- to pass a single argument into the thread entry point instead.

### Step 7: GraphForm -- Three Syntaxes, One Variable

```csharp
private Func<float, float> theFunction;
```

In `GraphComboBox_SelectedIndexChanged`:

```csharp
case 0: // Expression lambda
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

case 2: // Statement lambda, multi-line body
    theFunction = x =>
    {
        const float a = -0.0003f;
        const float b = 0.0066f;
        const float c = -0.0580f;
        const float d = 0.2670f;
        const float e = -0.5150f;
        const float f = 0.3050f;
        const float g = 0.0000f;
        return (((((a * x + b) * x + c) * x + d) * x + e) * x + f) * x + g;
    };
    break;
```

All three cases assign to the same `Func<float, float>` variable. `DrawGraph()` calls `theFunction(x)` identically regardless of which syntax produced it. Run the form, switch between equations in the combo box, and watch the graph redraw with a genuinely different curve each time -- same call site, three different methods, swapped at runtime.

The three syntaxes, in practical preference order for new code:

- **Expression lambda** (`x => expr`): no braces, no `return`, implicit return value. Shortest form, most readable for a single expression.
- **Statement lambda** (`x => { ... return v; }`): braces and explicit `return` when the body needs multiple statements.
- **Anonymous method** (`delegate (float x) { ... }`): the C# 2.0 syntax, requires writing the parameter type explicitly. Legacy; lambdas do everything it does with less ceremony.

`GraphForm` also uses `Func<float, float>` directly rather than declaring a custom delegate type. This is exactly the same concept as `Chapter6Form`'s `FunctionDelegate`, just named using the framework's built-in generic delegate instead. Using the built-in whenever possible is the standard practice -- less to declare, immediately recognizable.

---

## The Built-In Delegate Types

Custom `delegate` declarations work, but you usually don't need them. The framework provides:

| Type | Signature |
|---|---|
| `Action` | No parameters, returns `void` |
| `Action<T>` | Takes `T`, returns `void` |
| `Action<T1, T2>` | Two parameters, returns `void` (up to 16) |
| `Func<TResult>` | No parameters, returns `TResult` |
| `Func<T, TResult>` | Takes `T`, returns `TResult` |
| `Predicate<T>` | Takes `T`, returns `bool` |

`FunctionDelegate` in this project is exactly `Func<float, float>`, which is why `GraphForm` uses that and `Chapter6Form` uses the custom type -- to show both styles exist and are equivalent.

Reach for custom declarations only when you need `ref`/`out` parameters (which `Action`/`Func` can't express), or when a domain-specific name genuinely adds clarity.

---

## A Documented Loose End Worth Knowing About

```csharp
Load += delegate
{
    EquationComboBox.SelectedIndex = 0;
};
// This is equivalent to the following using a named method:
// Load += GraphForm_Load;
```

`GraphForm_Load` is a fully-written, correct named method that's never actually wired to anything. It exists purely as a comparison -- here's the named-method spelling of exactly the same code. The comment directly above it says so.

This is different from an actually-broken unwired handler, which would be a bug. Being able to tell the two apart -- intentional teaching device with a comment, versus unattended oversight with no explanation -- is a real debugging skill.

Note also the bare `delegate { ... }` with no parameter list at all. Anonymous methods allow omitting the parameter list entirely when you don't use the parameters. Lambdas can't do this; they always require a parameter list, even an empty `()`.

---

## Seeing It All Together

Chapter 6's main project is one of the few in this solution that genuinely needs to be run as-is rather than typed from scratch, because the WinForms Designer generates significant infrastructure (`InitializeComponent`, `.Designer.cs`, event wiring via the property panel) that would be tedious to reproduce manually. Open the project, run it, and work through the two forms in sequence:

1. A message box showing `theFunction(1)` appears immediately from `Chapter6Form_Load`.
2. A separate message box appears from the background thread -- possibly before the form finishes loading.
3. Click `BtnAnon` four times and watch `MyEvent` fire on the fourth.
4. Tick the checkbox and watch the event handler respond.
5. Click "Open Graph Form" to launch `GraphForm`, then switch equations and watch the curve redraw.
6. Close `Chapter6Form` and watch the closing message box from `DelegatedFunctionForUnload`.

## Also in Chapter 6

Nine supplemental projects accompany this one, documented separately. Each is a console program that covers one aspect of delegates in more depth than the WinForms demo can:

1. `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`
2. `CSharp.Ch06.Supplemental.02.LambdaExpressions`
3. `CSharp.Ch06.Supplemental.03.Callbacks`
4. `CSharp.Ch06.Supplemental.04.MulticastDelegates`
5. `CSharp.Ch06.Supplemental.05.ExceptionHandling`
6. `CSharp.Ch06.Supplemental.06.ParameterizedThreadStart`
7. `CSharp.Ch06.Supplemental.07.Events`
8. `CSharp.Ch06.Supplemental.08.Assertions`
9. `CSharp.Ch06.Supplemental.09.Closures`
