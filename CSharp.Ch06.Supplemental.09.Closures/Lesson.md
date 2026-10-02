# Chapter 6 Supplemental 09: Closures

## What is a closure?

### Definition:

A closure is a "*first-class function** with **free variables** that are **bound in the lexical environment*."

So that's that. We're done here.

...no? Fine. Let's break that down, one word at a time, like the definition owes us an explanation. It does.

> Side note: I'm going to talk a bit about other languages in this document. Don't panic. The concepts are the same, the syntax is different, and the bugs are sometimes worse. But the ideas transfer, so it's worth looking at a few other languages to see how they handle closures.

---

### First-Class Functions

First-class functions are functions that can be assigned to a variable. "First-class" here just means "treated like any other piece of data." No royal treatment, no velvet rope, just a value like an `int` or a `string`, except this one happens to do something when you call it.

C# gives you three ways to write one. They all produce the same result, so pick whichever reads best in context.

```csharp
// A local function. Named, boring, easy to read.
static string GreetLocal(string name) => $"Hello, {name}!";

// An anonymous delegate. The original syntax, and the one we'll lean on
// for the rest of this document since it makes the "this is just a
// value" point the most obvious.
Func<string, string> greetDelegate = delegate (string name)
{
    return $"Hello, {name}!";
};

// A lambda. Shorthand for the delegate above, nothing more.
Func<string, string> greetLambda = name => $"Hello, {name}!";

Console.WriteLine(GreetLocal("Ada"));
Console.WriteLine(greetDelegate("Ada"));
Console.WriteLine(greetLambda("Ada"));
```

Run it. Three identical greetings.

Three different spellings, one idea: a function sitting in a variable, ready to be passed around like any other value. Hold onto the delegate version - we're going to keep building on it.

---

### Free Variables Defined Within the Lexical Environment

A free variable is a variable that is **not defined within the function** but is **used by the function**. It's borrowed from an outer scope, and the function has no problem reaching outside itself to grab it.

The lexical environment is just the neighborhood a function was born in: all the variables in scope at the moment the function was defined.

```csharp
string salutation = "Hello";

Func<string, string> greet = delegate (string name)
{
    return $"{salutation}, {name}!";
};

Console.WriteLine(greet("Ada"));    // Hello, Ada!

salutation = "Howdy";

Console.WriteLine(greet("Alan"));   // Howdy, Alan!
```

Run it. `Hello, Ada!` then `Howdy, Alan!`.

Notice what just happened. We changed `salutation` after `greet` was already created, and `greet` noticed. That's the tell: a closure doesn't take a snapshot of the free variable's value at creation time - it holds onto the variable itself. Keep that in mind, it's going to matter later, in a slightly annoying way.

---

### What "Closes" the Free Variables?

When a function is defined, it "closes over" its free variables - it keeps a reference to them, not just a snapshot. Even after the outer function has finished running and its stack frame is long gone, the inner function still has access to those variables and can still change them.

```csharp
static Func<string, string> MakeGreeter(string salutation)
{
    var greetingCount = 0;

    Func<string, string> greet = delegate (string name)
    {
        greetingCount++;
        return $"{salutation}, {name}! (greeting #{greetingCount})";
    };

    return greet;
}
```

```csharp
var greet = MakeGreeter("Hello");

Console.WriteLine(greet("Ada"));   // Hello, Ada! (greeting #1)
Console.WriteLine(greet("Alan"));  // Hello, Alan! (greeting #2)
```

Run it. The counter increments across calls even though `MakeGreeter` returned long ago.

`MakeGreeter` runs, returns `greet`, and then technically exits. `salutation` and `greetingCount` should be gone, evicted along with the rest of the method's local variables. Except they're not. The returned function closed over both of them, so they live on for as long as `greet` does, quietly keeping count in the background.

Call `MakeGreeter` again and you get a brand new `greetingCount`, entirely separate from the first. Each call gets its own private copy of the lexical environment, not a shared one.

That's a closure: a first-class function, holding onto free variables from the lexical environment it was born into, refusing to let go even after that environment has technically ceased to exist.

---

### The Modified Closure Gotcha

`MakeGreeter` avoids trouble because every call gets its own fresh local variables. But if you skip the factory and write two closures directly in the same scope, sharing the same outer variable, you don't get two independent memories. You get two closures pointing at the exact same variable, stepping on each other.

```csharp
int exp = 2;
Func<int, int> square = x => (int)Math.Pow(x, exp);

Console.WriteLine(square(2));   // 4, as expected

exp = 3;
Func<int, int> cube = x => (int)Math.Pow(x, exp);

Console.WriteLine(cube(2));     // 8, also as expected

Console.WriteLine(square(2));   // 8, WRONG - and completely deserved
```

Run it. `4`, then `8`, then `8` from `square` - even though `square` was supposed to be squaring.

`square` was never told to remember `2`. It was told to remember `exp`, and `exp` is a variable, not a value, so it changed its mind the moment we reassigned it. Both closures are reading from the same shared mailbox, so whichever one wrote to it last wins, retroactively, for everybody.

The fix is exactly what `MakeGreeter` was already doing: give each closure its own private variable instead of letting them share one from a common outer scope.

---

### Access to Modified Closure

This same bug has a more famous costume: a `for` loop. If your IDE has ever underlined a variable and muttered "access to modified closure" at you, this is what it was talking about.

```csharp
var actions = new List<Action>();

for (int i = 0; i < 3; i++)
{
    actions.Add(() => Console.WriteLine(i));
}

foreach (var action in actions)
{
    action();
}
```

Run it. `3`, `3`, `3`. Not `0`, `1`, `2`.

Same disease as `square` and `cube`, just wearing a loop. Every lambda closed over the same `i`, not a copy of whatever `i` happened to be at the time. By the time any of these actions actually run, the loop is done and `i` has settled on `3`.

The fix:

```csharp
var fixedActions = new List<Action>();

for (int i = 0; i < 3; i++)
{
    int local = i;
    fixedActions.Add(() => Console.WriteLine(local));
}

foreach (var action in fixedActions)
{
    action();
}
```

Run it. `0`, `1`, `2`.

`local` gets declared fresh on every pass through the loop, so each closure gets its own private variable instead of fighting over the loop's. One asterisk: `foreach` has been immune to this since C# 5 - the loop variable there is already scoped per iteration. It's specifically `for` loops, `while` loops, and anything else reusing a single variable across iterations that will get you.

---

## Closures in Other Languages

The concept transfers everywhere. The syntax, and in a couple of cases the entire bug, does not.

### Python

Python closures read free variables by reference without any fuss. Reassigning one from inside the nested function requires the `nonlocal` keyword - otherwise Python quietly creates a brand new local variable instead of touching the enclosing one.

```python
def make_greeter(salutation):
    greeting_count = 0

    def greet(name):
        nonlocal greeting_count
        greeting_count += 1
        return f"{salutation}, {name}! (greeting #{greeting_count})"

    return greet

polite_greeter = make_greeter("Hello")
print(polite_greeter("Ada"))
print(polite_greeter("Alan"))
```

Python has the loop gotcha too:

```python
actions = []
for i in range(3):
    actions.append(lambda: print(i))

for action in actions:
    action()  # prints 2, 2, 2. Same bug, different accent.

# Fix: a default argument is evaluated at definition time
fixed_actions = []
for i in range(3):
    fixed_actions.append(lambda i=i: print(i))

for action in fixed_actions:
    action()  # prints 0, 1, 2
```

### JavaScript

JavaScript is basically where this bug grew up, and also where the language eventually apologized for it. `var` gives one shared variable; `let` scopes per iteration by design.

```javascript
function makeGreeter(salutation) {
  let greetingCount = 0;
  return function (name) {
    greetingCount++;
    return `${salutation}, ${name}! (greeting #${greetingCount})`;
  };
}

// The classic var gotcha:
var actions = [];
for (var i = 0; i < 3; i++) {
  actions.push(() => console.log(i));
}
actions.forEach((action) => action()); // 3, 3, 3

// The let fix:
var fixedActions = [];
for (let i = 0; i < 3; i++) {
  fixedActions.push(() => console.log(i));
}
fixedActions.forEach((action) => action()); // 0, 1, 2
```

### Rust

Rust's borrow checker mostly refuses to let you write this bug in the first place. Closures capture by reference, mutable reference, or move, and the compiler decides which is legal.

```rust
fn make_greeter(salutation: String) -> impl FnMut(&str) -> String {
    let mut greeting_count = 0;
    move |name: &str| {
        greeting_count += 1;
        format!("{}, {}! (greeting #{})", salutation, name, greeting_count)
    }
}
```

`move` forces the closure to take ownership of its free variables instead of borrowing them. Because `i` is an integer and integers implement `Copy`, `move` captures a fresh value each iteration rather than a shared reference.

### C++

C++ lambdas make you say out loud whether you're capturing by value `[=]` or by reference `[&]`. Capture by reference is where the trouble lives - and it's a meaner version, because a reference to a variable that went out of scope is undefined behavior, not just a stale value.

```cpp
// Capture by value - each closure gets its own copy
auto polite_greeter = [salutation, greeting_count = 0](std::string name) mutable {
    greeting_count++;
    return salutation + ", " + name + "! (greeting #" + std::to_string(greeting_count) + ")";
};
```

### Java

Java sidesteps this by refusing to let you write it. A lambda can only capture "effectively final" local variables - ones the compiler can prove you never reassign. The loop gotcha won't compile.

```java
for (int i = 0; i < 3; i++) {
    actions.add(() -> System.out.println(i)); // Compile error: i is not effectively final
}
```

### The Scorecard

| Language | Captures by default | Loop gotcha possible? |
|---|---|---|
| C# | By reference | Yes, in `for`/`while` (fixed in `foreach` since C# 5) |
| Python | By reference (`nonlocal` required to reassign) | Yes |
| JavaScript | By reference (`var`), per-iteration (`let`) | Yes with `var`, no with `let` |
| Rust | Programmer's choice, enforced by the compiler | Effectively no |
| C++ | Programmer's choice (`[=]` or `[&]`) | Yes with `[&]`, undefined behavior possible |
| Java | By value, effectively-final only | No, won't compile |

Same concept, six different opinions about how much rope to hand you.
