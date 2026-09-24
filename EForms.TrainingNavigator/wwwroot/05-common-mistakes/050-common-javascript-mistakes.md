# Common JavaScript Mistakes

Nine classic JavaScript pitfalls, each demonstrated with a runnable example. None of these are exotic edge cases - they're mistakes that come up constantly in real form scripting, often without any error message to point you toward them. Code that looks obviously correct can still misbehave in a way that's genuinely hard to spot without already knowing the specific pattern to watch for.

## How Each Lesson Works

Every lesson in this chapter shows the mistake actually running, side by side with a working fix for it - two live demos you can click and compare directly, not just a description of what goes wrong. Open the browser console (F12) before clicking either "Execute" button; each demo logs its explanation there as well as showing it on the page.

## The Nine Mistakes

1. **Losing Track of "this"** - handing off a method as a bare callback detaches it from the object it belonged to.
2. **Expecting Block-Level Scope** - `var` is function-scoped, not block-scoped, so a loop counter outlives the loop itself.
3. **Functions Created Inside a Loop** - a closure built with `var` in a loop captures the variable, not the value it held at that moment.
4. **Memory Leaks** - a closure keeps everything in its enclosing scope alive for as long as it's reachable, even variables it never actually uses.
5. **Circular Reference & Over-Subscribing** - an event handler that adds another handler to the same element every time it fires, compounding without bound.
6. **Inefficient DOM Updates** - touching the live DOM repeatedly in a loop is measurably slower than building the structure off-DOM first.
7. **Misunderstanding Prototypal Inheritance** - a property assigned directly on a constructor's `prototype` is one shared value, not a per-instance copy.
8. **Extracting a Method Loses Its "this"** - closely related to the first mistake, but worth its own lesson: pulling a method off an object into a standalone variable detaches it entirely.
9. **Calling a Function Instead of Referencing It** - `setInterval`/`setTimeout` need a function reference, not the result of calling one.
