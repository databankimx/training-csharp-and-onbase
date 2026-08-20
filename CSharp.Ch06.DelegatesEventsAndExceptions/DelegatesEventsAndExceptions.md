# DelegatesEventsAndExceptions

## Introduction

Delegates, anonymous methods, lambda expressions, and events, demonstrated through two forms: `Chapter6Form` and `GraphForm` (launched from a button on the first).

---

## What Is a Delegate?

A delegate is a data type that defines the *shape* of a method, its parameters and return type, rather than a value or a class. It's a way to treat "a method" as something you can store in a variable, pass around, and reassign.

```csharp
private delegate float FunctionDelegate(float x);
private FunctionDelegate theFunction;
```

```csharp
private static float DelegatedFunctionForLoad(float x)
{
    return (float)(12 * Math.Sin(3 * x) / (1 + Math.Abs(x)));
}

theFunction = DelegatedFunctionForLoad;
MessageBox.Show(theFunction(1).ToString(CultureInfo.CurrentCulture));
```

Once `theFunction` is assigned, calling `theFunction(1)` calls whichever method it currently points to. The variable can be reassigned to a completely different method later, and every call site that uses `theFunction` picks up the new behavior automatically, without changing.

---

## Delegates vs. Interfaces

Both let you separate a declaration from its implementation, but they fit different situations.

Use a **delegate** when: you're implementing an eventing pattern, you want to encapsulate a static method, the caller doesn't need anything else from the object besides this one method, you want easy composition, or a class might need more than one implementation of "the method."

Use an **interface** when: there's a group of related methods (not just one), a class only needs a single implementation, callers will want to cast between interface and class types, or the method is intrinsically part of the type's identity, comparison logic (`IComparable`) is the classic example, since it doesn't change at runtime and genuinely belongs to the class.

---

## Anonymous Methods

An anonymous method is a delegate with no separately-named method behind it, the code is written directly where it's needed.

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

This is the most natural fit for code used in exactly one place, an event handler, or a short piece of work handed to a new thread:

```csharp
var t1 = new Thread(delegate ()
{
    MessageBox.Show(@"Hello World", @"Delegate Greeting", MessageBoxButtons.OK);
});
t1.Start();
```

Since C# 3.0, lambda expressions have mostly superseded anonymous methods for inline code, with one exception: if you genuinely don't need the parameters at all (a common case for event handlers, where you often don't use `sender` or the event args), an anonymous method lets you drop the parameter list entirely. Lambdas can't do that, they always need their parameter list, even if empty.

---

## Events

```csharp
public delegate void MyEventHandler();
public event MyEventHandler MyEvent;
```

```csharp
MyEvent = () => MessageBox.Show(@"Too many clicks!");
```

```csharp
MyEvent?.Invoke();
```

An event is declared with the `event` keyword against a delegate type, then assigned a handler (here, a lambda) and invoked with `?.Invoke()`. The `?.` matters: if nothing has subscribed, `MyEvent` is `null`, and invoking a `null` delegate throws. The null-conditional operator skips the call entirely in that case instead of crashing.

---

## GraphForm: Three Syntaxes, One Kind of Value

```csharp
private Func<float, float> theFunction;
```

```csharp
case 0: // Expression-style statement lambda
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
case 2: // Multi-line statement lambda
    theFunction = x =>
    {
        const float a = -0.0003f;
        // ...more constants...
        return (((((a * x + b) * x + c) * x + d) * x + e) * x + f) * x + g;
    };
    break;
```

Whichever case runs, `theFunction` ends up holding a `Func<float, float>`, and `DrawGraph()` calls it the exact same way (`theFunction(x)`) no matter which syntax produced it. That's the core idea to take away: a delegate, an anonymous method, and a lambda expression are different ways of *writing* the same underlying thing, something you can invoke like a method, store in a variable, and pass around.

---

## Try It Yourself

Run the project, click **Show Graph Form**, and switch between the three equations in the dropdown. Each one is defined with a different syntax internally (see above), but they all behave identically from the outside, that's the whole point.

Back on the main form, click **Click me if you dare!** four times in a row and watch what changes on the fourth click, that's the custom `MyEvent` firing once the click count crosses its threshold.
