# Closures

## What is a closure?

### Definition:

A closure is a "***first-class function** with **free variables** that are **bound in the lexical environment**.*"

So that's that. We're done here.

...no? Fine. Let's break that down, one word at a time, like the definition owes us an explanation. It does.

---

### First-Class Functions

First-class functions are functions that can be assigned to a variable. "First-class" here just means "treated like any other piece of data." No royal treatment, no velvet rope, just a value like an `int` or a `string`, except this one happens to do something when you call it.

C# gives you three ways to write one. They all produce the same result, so pick whichever reads best in context.

#### Example:

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
```

Three different spellings, one idea: a function sitting in a variable, ready to be passed around like any other value. Hold onto the delegate version, we're going to keep building on it.

---

### Free Variables Defined Within the Lexical Environment

A free variable is a variable that is **not defined within the function** but is **used by the function**. It's borrowed from an outer scope, and the function has no problem reaching outside itself to grab it.

#### What is the Lexical Environment?

The lexical environment is just the neighborhood a function was born in: all the variables in scope at the moment the function was defined. When the function runs, it can use its own local variables plus anything it inherited from that neighborhood.

#### Example:

```csharp
// Same idea as before, but now "salutation" is a free variable,
// borrowed from outside the function rather than declared inside it.
string salutation = "Hello";

Func<string, string> greet = delegate (string name)
{
    return $"{salutation}, {name}!";
};

Console.WriteLine(greet("Ada"));    // Hello, Ada!

salutation = "Howdy";

Console.WriteLine(greet("Alan"));   // Howdy, Alan!
```

Notice what just happened. We changed `salutation` after `greet` was already created, and `greet` noticed. That's the tell: a closure doesn't take a snapshot of the free variable's value at creation time, it holds onto the variable itself. Keep that in mind, it's going to matter later, in a slightly annoying way.

---

### What "Closes" the Free Variables?

Here's the part that makes closures interesting instead of just a vocabulary exercise. When a function is defined, it "closes over" its free variables, meaning it keeps a reference to them, not just a snapshot. Even after the outer function has finished running and its stack frame is long gone, the inner function still has access to those variables, and can still change them.

This is where our humble greeter earns its keep. Let's extend it into something that remembers things between calls, exactly the kind of behavior a normal variable can't pull off on its own.

#### Example:

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

static void Main(string[] args)
{
    var greet = MakeGreeter("Hello");

    Console.WriteLine(greet("Ada"));   // Hello, Ada! (greeting #1)
    Console.WriteLine(greet("Alan"));  // Hello, Alan! (greeting #2)
}
```

`MakeGreeter` runs, returns `greet`, and then technically exits. `salutation` and `greetingCount` should be gone, evicted along with the rest of the method's local variables. Except they're not. The returned function closed over both of them, so they live on for as long as `greet` does, quietly keeping count in the background like a nosy neighbor who somehow remembers your entire mail history.

Call `MakeGreeter` again and you get a brand new `greetingCount`, entirely separate from the first one. Each call gets its own private copy of the lexical environment, not a shared one.

That's a closure: a first-class function, holding onto free variables, from the lexical environment it was born into, refusing to let go even after that environment has technically ceased to exist.

---

### The Modified Closure Gotcha

Remember a few paragraphs ago, when `greet` noticed `salutation` changing out from under it, and we said to keep that in mind? Here's where it stops being a fun fact and starts being a bug report.

`MakeGreeter` avoids trouble because every call gets its own fresh local variables, a private scope nobody else can reach. But if you skip the factory and just write two closures directly in the same scope, sharing the same outer variable, you don't get two independent memories. You get two closures pointing at the exact same variable, stepping on each other.

#### Example:

```csharp
int exp = 2;
Func<int, int> square = x => (int)Math.Pow(x, exp);

Console.WriteLine(square(2));   // 4, as expected

exp = 3;
Func<int, int> cube = x => (int)Math.Pow(x, exp);

Console.WriteLine(cube(2));     // 8, also as expected

Console.WriteLine(square(2));   // 8, WRONG, and yet completely deserved
```

`square` was never told to remember 2. It was told to remember `exp`, and `exp` is a variable, not a value, so it changed its mind the moment we reassigned it. Both closures are reading from the same shared mailbox, so whichever one wrote to it last wins, retroactively, for everybody.

The fix is exactly what `MakeGreeter` was already doing: give each closure its own private variable instead of letting them share one from a common outer scope. A closure factory isn't just a nice pattern, it's how you keep your closures from gossiping about each other's state.

---

### Access to Modified Closure

This same bug has a more famous costume: a `for` loop. If your compiler or IDE has ever underlined a variable and muttered "access to modified closure" at you, this is what it was talking about, and it's the version of this problem you'll actually run into in the wild.

#### Example:

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

// Prints 3, 3, 3. Not 0, 1, 2.
```

Same disease as `square` and `cube`, just wearing a loop for a costume. Every lambda closed over the same `i`, not a copy of whatever `i` happened to be at the time. By the time any of these actions actually run, the loop is done and `i` has settled on 3, so that's what all three of them report.

The fix is, once again, the same fix: give each iteration its own private variable instead of letting them all share the loop's.

```csharp
var actions = new List<Action>();

for (int i = 0; i < 3; i++)
{
    int local = i;
    actions.Add(() => Console.WriteLine(local));
}

foreach (var action in actions)
{
    action();
}

// Prints 0, 1, 2, like a reasonable person would expect.
```

`local` gets declared fresh on every pass through the loop, so each closure gets its own private variable to hang onto instead of fighting over the loop's.

One asterisk: `foreach` has been immune to this since C# 5, the loop variable there is already scoped per iteration. It's specifically `for` loops, `while` loops, and anything else reusing a single variable across iterations that will get you.

---

## Closures in Other Languages

The concept transfers everywhere. The syntax, and in a couple of cases the entire bug, does not.

### Python

Python closures read free variables by reference without any fuss. Reassigning one from inside the nested function is where it gets particular: you need the `nonlocal` keyword, otherwise Python quietly creates a brand new local variable instead of touching the enclosing one. Yet another way to shoot yourself with closures, filed under "at least it's a different way."

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

And yes, Python has the loop gotcha too:

```python
actions = []
for i in range(3):
    actions.append(lambda: print(i))

for action in actions:
    action()  # prints 2, 2, 2. Same bug, different accent.

# Fix: a default argument is evaluated at definition time, capturing
# the current value of i instead of a live reference to it.
fixed_actions = []
for i in range(3):
    fixed_actions.append(lambda i=i: print(i))

for action in fixed_actions:
    action()  # prints 0, 1, 2
```

### JavaScript

JavaScript is basically where this bug grew up, and also where the language eventually apologized for it. `var` behaves exactly like C#'s `for` loop problem, one shared variable, every closure reading whatever it ends up on. `let` fixes it, because `let` is scoped per iteration by design.

```javascript
function makeGreeter(salutation) {
  let greetingCount = 0;
  return function (name) {
    greetingCount++;
    return `${salutation}, ${name}! (greeting #${greetingCount})`;
  };
}

const politeGreeter = makeGreeter("Hello");
console.log(politeGreeter("Ada"));
console.log(politeGreeter("Alan"));
```

```javascript
// The classic, with var:
var actions = [];
for (var i = 0; i < 3; i++) {
  actions.push(() => console.log(i));
}
actions.forEach((action) => action()); // 3, 3, 3

// The fix, with let:
var fixedActions = [];
for (let i = 0; i < 3; i++) {
  fixedActions.push(() => console.log(i));
}
fixedActions.forEach((action) => action()); // 0, 1, 2
```

No `local` variable required, `let` just does the right thing on its own. It's the language quietly admitting `var` was a mistake, one keyword at a time.

### Rust

Rust's borrow checker mostly refuses to let you write this bug in the first place, which is either delightful or infuriating depending on your deadline. Closures capture by reference, by mutable reference, or by move, and the compiler decides which is legal based on how the closure is used and how long it needs to live.

```rust
fn make_greeter(salutation: String) -> impl FnMut(&str) -> String {
    let mut greeting_count = 0;
    move |name: &str| {
        greeting_count += 1;
        format!("{}, {}! (greeting #{})", salutation, name, greeting_count)
    }
}

fn main() {
    let mut polite_greeter = make_greeter("Hello".to_string());
    println!("{}", polite_greeter("Ada"));
    println!("{}", polite_greeter("Alan"));
}
```

`move` forces the closure to take ownership of its free variables instead of borrowing them, which is exactly the discipline that keeps closures from sharing state they have no business sharing. Try to reproduce the loop gotcha in Rust and the compiler hands you the fix for free:

```rust
let mut actions: Vec<Box<dyn Fn()>> = Vec::new();
for i in 0..3 {
    actions.push(Box::new(move || println!("{}", i)));
}
for action in &actions {
    action(); // 0, 1, 2. "move" grabbed a fresh copy of i on every iteration.
}
```

Because `i` is an integer, and integers implement `Copy`, `move` captures a fresh value each time through the loop rather than a shared reference. The gotcha needs a variable that multiple closures can secretly share, and Rust's ownership rules make that arrangement hard to write by accident, even if you were trying.

### C++

C++ lambdas make you say out loud whether you're capturing by value `[=]` or by reference `[&]`. Capture by reference is where the trouble lives, and it's a meaner version of the bug than C#'s: if the referenced variable goes out of scope before the lambda is called, you're not looking at a stale value anymore, you're looking at undefined behavior.

```cpp
#include <functional>
#include <iostream>

std::function<std::string(std::string)> make_greeter(std::string salutation) {
    int greeting_count = 0;
    return [salutation, greeting_count](std::string name) mutable {
        greeting_count++;
        return salutation + ", " + name + "! (greeting #" + std::to_string(greeting_count) + ")";
    };
}

int main() {
    auto polite_greeter = make_greeter("Hello");
    std::cout << polite_greeter("Ada") << std::endl;
    std::cout << polite_greeter("Alan") << std::endl;
}
```

Capturing by value gives each closure its own private `greeting_count`. `mutable` is required because a value-captured variable is read-only inside the lambda by default. The loop gotcha shows up the moment you switch to capturing by reference instead:

```cpp
#include <functional>
#include <iostream>
#include <vector>

int main() {
    std::vector<std::function<void()>> actions;
    for (int i = 0; i < 3; i++) {
        actions.push_back([&i]() { std::cout << i << std::endl; });
    }
    for (auto& action : actions) {
        action(); // Undefined behavior. i went out of scope when the loop ended.
    }

    std::vector<std::function<void()>> fixed_actions;
    for (int i = 0; i < 3; i++) {
        fixed_actions.push_back([i]() { std::cout << i << std::endl; }); // capture by value
    }
    for (auto& action : fixed_actions) {
        action(); // 0, 1, 2
    }
}
```

Capture by value fixes the loop gotcha the same way `int local = i;` did back in C#, it just moves the copy into the capture clause instead of a separate line. Worth remembering that the reference version above isn't just "wrong output" the way it was in C#, it's a dangling reference to a variable that no longer exists, which is a considerably worse Tuesday.

### Java

Java sidesteps this entire category of bug by refusing to let you write it. A lambda can only capture local variables that are "effectively final," meaning the compiler can prove you never reassign them after initialization. Try to write the loop gotcha in Java and it simply will not compile.

```java
import java.util.function.Function;

public class ClosureExample {
    static Function<String, String> makeGreeter(String salutation) {
        int[] greetingCount = { 0 }; // an array, because a plain int can't be reassigned from inside the lambda
        return name -> {
            greetingCount[0]++;
            return salutation + ", " + name + "! (greeting #" + greetingCount[0] + ")";
        };
    }

    public static void main(String[] args) {
        Function<String, String> politeGreeter = makeGreeter("Hello");
        System.out.println(politeGreeter.apply("Ada"));
        System.out.println(politeGreeter.apply("Alan"));
    }
}
```

Note the one-element array trick for `greetingCount`. Java lambdas can read effectively final variables but never reassign them, and a plain `int` incremented by `greetingCount++` would break that rule immediately. Wrapping it in an array sidesteps the restriction, since the array reference itself never changes, only its contents do. This workaround exists purely because Java's compiler is stricter here than anyone asked it to be.

As for the loop gotcha:

```java
for (int i = 0; i < 3; i++) {
    actions.add(() -> System.out.println(i)); // Compile error: i is not effectively final
}
```

This does not compile. Java would rather stop you at build time than let you discover at runtime that all three closures printed 3. Petty, in a good way.

### The Scorecard

| Language   | Captures by default                          | Loop gotcha possible?                                   |
|------------|-----------------------------------------------|-----------------------------------------------------------|
| C#         | By reference                                   | Yes, in `for`/`while` (fine in `foreach` since C# 5)      |
| Python     | By reference (`nonlocal` required to reassign) | Yes                                                        |
| JavaScript | By reference (`var`), per-iteration (`let`)    | Yes with `var`, no with `let`                              |
| Rust       | Programmer's choice, enforced by the compiler  | Effectively no, ownership rules block it                  |
| C++        | Programmer's choice (`[=]` or `[&]`)           | Yes with `[&]` (and worse, it can be undefined behavior), no with `[=]` |
| Java       | By value, effectively-final only               | No, won't compile                                          |

Same concept, six different opinions about how much rope to hand you.
