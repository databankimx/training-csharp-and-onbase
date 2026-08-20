# Ch06 Textbook Code: Graph Function

## What This Is

The foundational version of the graphing demo this whole chapter builds on. Three functions, three plain named methods (`Function1`, `Function2`, `Function3`), assigned to a hand-declared `FunctionDelegate` in a `switch`:

```csharp
private delegate float FunctionDelegate(float x);
private FunctionDelegate TheFunction;
```

```csharp
case 0: TheFunction = Function1; break;
case 1: TheFunction = Function2; break;
case 2: TheFunction = Function3; break;
```

No bugs found. `Load` correctly wired.

---

## Read This One First

Both `CSharp.Ch06.TextbookCode.AnonymousGraph` and `CSharp.Ch06.DelegatesEventsAndExceptions`'s `GraphForm` take these exact same three mathematical functions and rewrite them using anonymous-method and lambda syntax instead. This project is the "before" picture, worth reading *first*, in this order, if you haven't already: this one shows the functions as ordinary named methods (nothing unfamiliar here, just a delegate variable pointing at whichever method is currently selected), then `AnonymousGraph` shows the same three functions rewritten as an expression lambda, an anonymous method, and a multi-line statement lambda, and `GraphForm` repeats that rewrite again under house naming conventions.

Reading all three in sequence makes the syntax evolution concrete: same math, same delegate mechanics underneath, progressively terser syntax for writing the function itself.
