# Chapter 6 Supplemental 01: Named Versus Anonymous Delegates

## What This Is

Despite the folder name, this covers more ground than just named-vs-anonymous: named vs. anonymous delegates, combining delegates with `+`/`-`, static vs. instance method delegates, covariance and contravariance, and passing a delegate to a `Thread`. No bugs found.

---

## `CallDelegates()`: The Core Distinction

```csharp
private delegate void Printer(string data);
...
Printer p = Console.WriteLine;      // named method
p("The delegate using an anonymous method was called.");

p = DoWork;                          // reassigned to a different named method
p("The delegate using a named method was called.");
```

Both assignments here are actually named methods (`Console.WriteLine`, `DoWork`), the point isn't that one line is "anonymous" and the other "named", it's that `p` is a variable that can be pointed at any method matching the `Printer` signature, and reassigned freely. Compare this to `CSharp.Ch06.DelegatesEventsAndExceptions`, where `BtnAnon.Click` is wired with a genuine anonymous method (a delegate literal with no separately-named method behind it) rather than a named-method reference like these.

---

## `CombineDelegates()`: Delegates Are a Data Type

```csharp
Step one = StepOne;
Step two = StepTwo;
Step combined = one + two;
combined("Test");           // runs StepOne, then StepTwo

Step truncated = combined - one;
truncated("Test");          // runs StepTwo only
```

Delegates support `+` (combine into a multicast delegate that invokes every combined method in order) and `-` (remove a method from the invocation list). This is a preview, the code comment says as much, of the fuller multicast delegate lesson elsewhere in this chapter.

---

## `StaticAndInstanceDelegates()`: Same Signature, Different Binding

```csharp
alice.InstanceMethod = alice.GetName;   // bound to Alice's own instance
bob.InstanceMethod = alice.GetName;     // bob's delegate points at Alice's instance method too

alice.StaticMethod = Person.StaticName;
bob.StaticMethod = Person.StaticName;
```

`GetStringDelegate` doesn't care whether the method it points to is static or instance, both fit the same signature (`string GetStringDelegate()`). What matters is that an *instance* method delegate carries its target object along with it, `bob.InstanceMethod` genuinely calls Alice's `GetName()`, returning "Alice", not "Bob", even though it's stored on Bob's own field. A static method delegate has no target object to carry, calling it through `alice.StaticMethod` or `bob.StaticMethod` makes no difference, both return "Static".

---

## `CovarianceAndContravariance()`: The Two Directions

```csharp
// COVARIANCE: a method returning a derived class can be assigned to a
// delegate declared to return the base class.
returnPersonMethod = ReturnEmployee;   // Func<Person> = a method returning Employee

// CONTRAVARIANCE: a method with a base-class parameter can be assigned to a
// delegate declared with a derived-class parameter.
employeeParameterMethod = PersonParameter;   // Action<Employee> = a method taking Person
```

Both directions rest on the same idea (an `Employee` *is a* `Person`), just applied to opposite positions. A method that hands back an `Employee` satisfies "give me a `Person`" (covariance, output position). A method that only needs a `Person` can still handle being handed an `Employee` (contravariance, input position). Trying to go the other way in either case (assigning a method that returns `Person` to a delegate that must return `Employee`, or one that requires an `Employee` parameter to a delegate declared with a `Person` parameter) wouldn't compile, the code's commented-out `employeeParameterMethod(person)` line is exactly that mistake, left in as a labeled example of what doesn't work.

Worth noticing the runtime-vs-compile-time split this method calls out directly:

```csharp
var person = returnPersonMethod();
Console.WriteLine($"'person' is a(n) [{person.GetType().Name}] named [{person.Name}]");
```

`person`'s *compile-time* type is `Person` (that's what the delegate declares), but `GetType().Name` reports "Employee", the actual runtime type never stopped being `Employee`, covariance just let the delegate declaration be looser than the concrete object underneath it.

---

## `ThreadDelegate()`: An Anonymous Method as a Thread's Entry Point

```csharp
var thread = new Thread(delegate ()
{
    Thread.Sleep(1000);
    Console.WriteLine("Step 1...");
});
thread.Start();
Console.WriteLine("Step 2...");
```

Run this and "Step 2..." prints before "Step 1...", even though "Step 1" appears first in the code. The anonymous method runs on its own thread, which sleeps for a full second, meanwhile the main thread moves straight on to print "Step 2..." without waiting. A small, concrete first look at asynchronous execution ahead of Chapter 7's deeper dive into multithreading.
