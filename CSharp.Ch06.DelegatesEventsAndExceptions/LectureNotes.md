# Chapter 6: Delegates, Events, and Exceptions

## What This Lesson Is

Two WinForms projects: `Chapter6Form` (delegates, anonymous methods, lambdas, events, a background thread) and `GraphForm` (three ways to define the same kind of function: named method, anonymous method, statement lambda), launched from a button on the main form. No bugs found, this is well-crafted original content, not adapted from the textbook.

---

## Delegates vs. Interfaces

The chapter notes embedded in `Chapter6Form.cs` make a case worth internalizing: delegates and interfaces both let a class designer separate a type's declaration from its implementation, but they solve different shaped problems.

Reach for a **delegate** when: an eventing pattern is in play, you want to encapsulate a static method, the caller doesn't need access to anything else on the implementing object, you want easy composition, or a class might need more than one implementation of the "method." Reach for an **interface** when: there's a group of related methods (not just one), a class only ever needs one implementation, callers will want to cast between interface/class types, or the method is intrinsically tied to the type's identity (comparison methods are the canonical example, `IComparable` over a delegate, since the comparison logic doesn't change at runtime and belongs to the class itself).

---

## Delegates and Anonymous Methods, Side by Side

```csharp
private delegate float FunctionDelegate(float x);
private FunctionDelegate theFunction;
```

```csharp
theFunction = DelegatedFunctionForLoad;
MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));
```

A delegate variable holds a reference to a method, not a value, once assigned, you invoke it exactly like calling the method directly. `theFunction` gets reassigned to a *different* method (`DelegatedFunctionForUnload`) in `FormClosing`, demonstrating that the variable, not the method name, is what the rest of the code actually depends on.

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

This is an anonymous method, a delegate literal with no separately-declared method behind it. Worth noticing what's deliberately different about `BtnAnon` versus `BtnGraphForm` in this same file: `BtnGraphForm.Click` is wired up in `Chapter6Form.Designer.cs` (the normal Designer-driven path), while `BtnAnon.Click` is wired entirely in the constructor, in code, using an anonymous method. Both are valid, and seeing them side by side in the same form is the point, one is what the Designer generates for you, the other is what you write by hand when the handler is simple enough not to need a name.

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

`MyEvent` is declared as a custom event using a custom delegate type (rather than the more idiomatic built-in `Action`, worth noticing that distinction if you compare this to newer C# style). It's assigned a lambda expression as its handler in `Form_Load`, then invoked conditionally once the click count passes a threshold. The `?.Invoke()` null-conditional call is the modern equivalent of the classic `if (MyEvent != null) MyEvent();` check, both exist to avoid throwing a `NullReferenceException` if nothing has subscribed.

---

## A Background Thread via Anonymous Method

```csharp
var t1 = new Thread(delegate ()
{
    MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
});
t1.Start();
```

This is the textbook case for an anonymous method: code that's used in exactly one place, simple enough that giving it a name would just be extra ceremony. `Thread`'s constructor takes a `ThreadStart` delegate here (no parameters), distinct from the `ParameterizedThreadStart` overload used in `CSharp.Ch06.Supplemental.06.ParameterizedThreadStart`, which lets you pass a single `object` argument into the thread's entry point, worth comparing the two once you get to that lesson.

---

## GraphForm: Three Ways to Define the Same Kind of Function

```csharp
private Func<float, float> theFunction;
```

```csharp
case 0: // Statement lambda syntax
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

All three cases assign to the exact same `Func<float, float>` variable, and `DrawGraph()` calls it identically regardless of which syntax produced it (`theFunction(x)`). That's the concrete lesson: expression lambdas, anonymous-method delegates, and statement lambdas are three different *syntaxes* for the same underlying thing, a value that can be invoked like a method. Case 1 in particular is worth reading closely since it's a full anonymous method (braces, an early `return`, multiple statements), not just a one-liner, showing that anonymous methods aren't limited to trivial bodies.

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

`GraphForm_Load` is a fully-written, correct method that's never actually wired to anything, it exists purely as a documented point of comparison, "here's what the named-method version of this exact line would look like." Unlike the unwired `Load` handlers found as real bugs elsewhere in this training set (`ShortPathNames`, `Ch05RealWorldScenario01`), this one is intentional and explained by the comment directly above it, worth telling apart from an actual bug.
