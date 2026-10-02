# Chapter 6 Supplemental 01: Named Versus Anonymous Delegates

## What This Is

Despite the folder name, this covers considerably more than just named-vs-anonymous. Five topics run in sequence from `Main()`: assigning and reassigning a delegate variable, combining delegates with `+` and `-`, static vs. instance method binding, covariance and contravariance, and an anonymous method as a thread entry point.

---

## How to Write This Program

Each mini-program below stands alone. Write it in a fresh `Main()`, run it, then move on.

### Mini-Program 1: Assigning and Reassigning a Delegate

```csharp
private delegate void Printer(string data);

private static void DoWork(string data)
{
    Console.WriteLine(data);
}
```

```csharp
Printer p = Console.WriteLine;
p("The delegate using an anonymous method was called.");

p = DoWork;
p("The delegate using a named method was called.");
```

Run it. Both lines print normally.

Read the code before reading the output, because the two disagree in an interesting way: the first call prints a message saying "anonymous method," but `Console.WriteLine` is a named method. Both assignments here are named methods. The label in the output is just a string; it's not describing the mechanism.

The real lesson is the second assignment. `p` starts pointing at `Console.WriteLine` and gets reassigned to `DoWork` -- a private static method in a completely unrelated place. The delegate accepted both because they share the same signature: take a `string`, return `void`. The type it belongs to, whether it's static or instance, none of that matters -- only the shape.

Note also: `p = Console.WriteLine` with no parentheses stores the method itself. `p = Console.WriteLine(...)` with parentheses would call it immediately and try to assign the result (a `void`, which wouldn't compile). This is the single most common mistake with delegates.

### Mini-Program 2: Combining Delegates With `+` and `-`

```csharp
private delegate void Step(string data);

private static void StepOne(string s) { Console.Write(s + " "); }
private static void StepTwo(string s) { Console.WriteLine(s); }
```

```csharp
Step one = StepOne;
Step two = StepTwo;

Step combined = one + two;
combined("Test");      // StepOne runs, then StepTwo runs

Step truncated = combined - one;
truncated("Test");     // only StepTwo runs
```

Run it. `Test Test` on one line (StepOne writes without a newline, StepTwo follows with one), then `Test` alone.

Three things to take from this. First, `+` combines two delegates into one that runs both in order -- that's multicast behavior, and it's the mechanism behind events. Second, `-` removes a method from the invocation list. Third, nothing is mutated: `one + two` produces a *new* delegate. `one` still points at only `StepOne` after the combination.

Try combining `Step` with a `Printer` from Mini-Program 1. They have identical signatures and still won't compile. Delegate types are nominal -- two declarations that look the same are different types.

### Mini-Program 3: Static vs. Instance Method Binding

```csharp
public delegate string GetStringDelegate();

public class Person
{
    public string Name { get; set; }
    public GetStringDelegate InstanceMethod;
    public GetStringDelegate StaticMethod;

    public string GetName() => Name;
    public static string StaticName() => "Static";
}
```

```csharp
var alice = new Person { Name = "Alice" };
var bob   = new Person { Name = "Bob" };

alice.InstanceMethod = alice.GetName;    // bound to Alice's instance
alice.StaticMethod   = Person.StaticName;

bob.InstanceMethod   = alice.GetName;   // Bob's field points at Alice's method
bob.StaticMethod     = Person.StaticName;

Console.WriteLine("Alice's InstanceMethod: " + alice.InstanceMethod()); // Alice
Console.WriteLine("Bob's InstanceMethod:   " + bob.InstanceMethod());   // Alice
Console.WriteLine("Alice's StaticMethod:   " + alice.StaticMethod());   // Static
Console.WriteLine("Bob's StaticMethod:     " + bob.StaticMethod());     // Static
```

Run it. Both instances print `Alice` for their `InstanceMethod`, and both print `Static` for their `StaticMethod`.

An instance method delegate carries two things: the method *and* the object to call it on. `bob.InstanceMethod = alice.GetName` stores Alice's *object*, not Bob's. Calling it returns `"Alice"` regardless of where the delegate field lives.

A static method delegate has no target object (`Delegate.Target` is `null`). Calling it through `alice.StaticMethod` or `bob.StaticMethod` makes no difference.

The practical consequence: a delegate holding an instance method keeps that object alive. A long-lived subscriber holding a reference to a short-lived publisher is the most common managed memory leak in .NET.

### Mini-Program 4: Covariance and Contravariance

```csharp
public class Person { public string Name { get; set; } }
public class Employee : Person { }

private static Func<Person> returnPersonMethod;
private static Action<Employee> employeeParameterMethod;
```

```csharp
// Covariance: a method returning Employee satisfies a delegate returning Person
returnPersonMethod = () => new Employee { Name = "Jane" };

// Contravariance: a method taking Person satisfies a delegate taking Employee
employeeParameterMethod = p => { p.Name = "John Smith"; };

var person = returnPersonMethod();
Console.WriteLine($"Type: {person.GetType().Name}, Name: {person.Name}");

var employee = new Employee();
employeeParameterMethod(employee);
Console.WriteLine($"Type: {employee.GetType().Name}, Name: {employee.Name}");
```

Run it. The first line reports `Employee` even though the delegate is typed `Func<Person>`. The second assigns a name to an `Employee` through a delegate typed `Action<Employee>`.

Both directions rest on the same fact: `Employee` IS-A `Person`.

**Covariance** (output position): the caller asked for a `Person` and got an `Employee` back. That's always safe -- every `Employee` is a `Person`.

**Contravariance** (input position): the caller will pass an `Employee`, and the method only needs a `Person`. That's also safe -- the method will never ask for something an `Employee` doesn't have.

The reverse of either breaks. A method returning `Person` can't satisfy a `Func<Employee>` -- the caller might get a plain `Person` and try to use `Employee`-specific members. A method taking `Employee` can't satisfy an `Action<Person>` -- it might be handed a plain `Person` and try to access `Salary`. Neither compiles, which is the correct outcome.

The runtime type of `person` is still `Employee` -- covariance let the declared type be looser, but the underlying object didn't change.

### Mini-Program 5: An Anonymous Method as a Thread Entry Point

```csharp
var thread = new Thread(delegate ()
{
    Thread.Sleep(1000);
    Console.WriteLine("Step 1...");
});
thread.Start();
Console.WriteLine("Step 2...");
```

Run it. "Step 2..." appears immediately, then "Step 1..." appears a second later, even though Step 1 comes first in the source.

`Thread`'s constructor takes a `ThreadStart` delegate (no parameters, returns `void`), and the anonymous method is being converted to it. `Start()` returns immediately -- the thread is scheduled but not waited for. The main thread moves straight to "Step 2..." while the thread sleeps.

Source order no longer predicts execution order once a second thread is involved. This is Chapter 7's territory in full; what matters here is that threading runs on delegates, so delegates come first.

---

## Try It Yourself

For the covariance/contravariance mini-program, try uncommenting this line in the code:

```csharp
// employeeParameterMethod(person);
```

and see what compile error it produces. `person`'s compile-time type is `Person`, but `employeeParameterMethod` expects an `Employee` -- the compiler uses the declared type, not the runtime type. Covariance loosened the delegate's return-type declaration; it didn't change what the compiler knows about `person` at the call site.

---

## Takeaways

- A delegate variable can point at any method with a matching signature, from any type, and can be reassigned freely.
- Assign without parentheses to store the method; add parentheses to call it.
- `+` and `-` on delegates return new instances -- delegates are immutable.
- Two delegate types with identical signatures are still different types. Delegate typing is nominal.
- An instance-method delegate carries its target object, keeping it alive.
- A static-method delegate has a `null` target.
- Covariance: narrower return type is fine. Contravariance: broader parameter type is fine.
- `Thread.Start()` does not block. Source order stops predicting execution order.
