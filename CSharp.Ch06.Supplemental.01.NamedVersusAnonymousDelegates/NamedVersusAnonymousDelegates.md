# Named Versus Anonymous Delegates

## Introduction

A delegate is a variable that holds a reference to a method rather than a value. This lesson walks through five related ideas: named vs. anonymous delegates, combining delegates, static vs. instance method delegates, covariance/contravariance, and using a delegate as a thread's starting point.

---

## Named Delegates

```csharp
private delegate void Printer(string data);
```

```csharp
Printer p = Console.WriteLine;
p("The delegate using an anonymous method was called.");

p = DoWork;
p("The delegate using a named method was called.");
```

```csharp
private static void DoWork(string data)
{
    Console.WriteLine(data);
}
```

`p` is a variable of type `Printer`. It can be assigned any method matching that signature (a single `string` parameter, no return value), `Console.WriteLine` and `DoWork` both qualify. Once assigned, calling `p(...)` calls whichever method it currently points to.

---

## Combining Delegates

```csharp
private delegate void Step(string data);
```

```csharp
Step one = StepOne;
Step two = StepTwo;
Step combined = one + two;
combined("Test");   // calls StepOne("Test"), then StepTwo("Test")

Step truncated = combined - one;
truncated("Test");  // calls only StepTwo("Test")
```

Delegates support `+` and `-`. Adding two delegates together creates a *multicast* delegate, one that calls every method in its list, in order, when invoked. Subtracting removes a method from that list. This only works between delegates of the same type.

---

## Static vs. Instance Method Delegates

```csharp
public delegate string GetStringDelegate();
public GetStringDelegate StaticMethod;
public GetStringDelegate InstanceMethod;

public static string StaticName() => "Static";
public string GetName() => Name;
```

```csharp
var alice = new Person { Name = "Alice" };
var bob = new Person { Name = "Bob" };

alice.InstanceMethod = alice.GetName;
bob.InstanceMethod = alice.GetName;   // bob's delegate points at Alice's own method

alice.StaticMethod = Person.StaticName;
bob.StaticMethod = Person.StaticName;
```

Calling `bob.InstanceMethod()` returns **"Alice"**, not "Bob". An instance-method delegate carries its target object along with it, `bob.InstanceMethod` really does call `alice.GetName()`, it just happens to be stored in a field on `bob`. A static method has no instance to carry, so `alice.StaticMethod()` and `bob.StaticMethod()` behave identically no matter which object they're called through.

---

## Covariance and Contravariance

**Covariance**: a method that returns a *more derived* type can be assigned to a delegate declared to return a *less derived* (base) type.

```csharp
private static Func<Person> returnPersonMethod;
private static Employee ReturnEmployee() => new Employee();

returnPersonMethod = ReturnEmployee;   // valid: Employee is a Person
```

**Contravariance**: a method with a *less derived* (base) parameter type can be assigned to a delegate declared with a *more derived* parameter type.

```csharp
private static Action<Employee> employeeParameterMethod;
private static void PersonParameter(Person person) { person.Name = "John Smith"; }

employeeParameterMethod = PersonParameter;   // valid: anything the delegate hands it will be an Employee, and Employee is a Person
```

Both rules exist because an `Employee` genuinely *is a* `Person`, covariance applies it to what a method gives back, contravariance applies it to what a method is willing to accept.

```csharp
var person = returnPersonMethod();
Console.WriteLine(person.GetType().Name);   // prints "Employee"
```

`person`'s declared type is `Person` (that's what the delegate says), but its actual runtime type is still `Employee`, covariance loosened the delegate's declared type, it didn't change what object actually got created.

---

## A Delegate as a Thread's Entry Point

```csharp
var thread = new Thread(delegate ()
{
    Thread.Sleep(1000);
    Console.WriteLine("Step 1...");
});
thread.Start();
Console.WriteLine("Step 2...");
```

`Thread`'s constructor takes a delegate (here, an anonymous method) to run on the new thread. Run this and you'll see "Step 2..." print *before* "Step 1...", the new thread sleeps for a second while the main thread keeps going without waiting for it. This is a first, small taste of the asynchronous behavior Chapter 7 covers in depth.

---

## Try It Yourself

Run the project and step through each section in order. For the covariance/contravariance section specifically, try uncommenting the line the code deliberately leaves commented out:

```csharp
// employeeParameterMethod(person);
```

and see what compile error it produces, then think through why `person`'s *compile-time* type (`Person`) is what the compiler checks against, even though its runtime type is `Employee`.
