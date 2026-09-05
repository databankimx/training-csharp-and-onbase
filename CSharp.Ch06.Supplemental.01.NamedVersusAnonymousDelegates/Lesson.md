# Chapter 6 Supplemental 01: Named Versus Anonymous Delegates

## What This Is

Despite the folder name, this project covers considerably more ground than just named-vs-anonymous. It's five console demonstrations run in sequence from `Main()`:

1. `CallDelegates()` — assigning and reassigning a delegate variable
2. `CombineDelegates()` — `+` and `-` on delegates
3. `StaticAndInstanceDelegates()` — what a delegate carries along with the method
4. `CovarianceAndContravariance()` — why assignment compatibility isn't strict equality
5. `ThreadDelegate()` — an anonymous method as a thread entry point

No bugs found.

---

## `CallDelegates()`: The Core Distinction

```csharp
private delegate void Printer(string data);
```

```csharp
Printer p = Console.WriteLine;      // named method
p("The delegate using an anonymous method was called.");

p = DoWork;                          // reassigned to a different named method
p("The delegate using a named method was called.");
```

Read the code before you read the console output, because the two disagree. Both assignments are named methods — `Console.WriteLine` and `DoWork` are both real, named, separately-declared methods. The message printed by the first call says "anonymous," but nothing anonymous is happening on that line.

That mismatch is worth sitting with rather than glossing over. The real lesson in this method is the one the output doesn't state: `p` is a **variable** that can be pointed at any method matching the `Printer` signature, and reassigned freely. `Console.WriteLine(string)` and `DoWork(string)` come from entirely unrelated types — one is in the BCL, one is a private static method in this file — and the delegate accepts both because the only thing it checks is the shape of the signature: takes a `string`, returns `void`.

Note also that `Console.WriteLine` is heavily overloaded. The compiler picks the `WriteLine(string)` overload specifically because that's the one matching `Printer`. Method group conversion resolves overloads against the target delegate type, which is why the same identifier can mean different methods in different assignments.

For a genuine anonymous method, compare `BtnAnon.Click` in the main `CSharp.Ch06.DelegatesEventsAndExceptions` project — a delegate literal with no separately named method behind it.

---

## `CombineDelegates()`: Delegates Are a Data Type

```csharp
private delegate void Step(string data);
```

```csharp
Step one = StepOne;
Step two = StepTwo;

Step combined = one + two;
combined("Test");           // runs StepOne, then StepTwo

Step truncated = combined - one;
truncated("Test");          // runs StepTwo only
```

`StepOne` uses `Console.Write` (no newline), `StepTwo` uses `Console.WriteLine`, so the combined call produces `Test Test` on one line and the truncated call produces just `Test` — visible proof that the invocation list actually changed.

Three things to take from this:

**Delegates are values you can do arithmetic on.** `+` combines two delegates into a multicast delegate that invokes every method in the list, in the order added. `-` removes a method from the invocation list.

**Both operands must be the same delegate type.** `Step` and `Printer` have identical signatures — `void` returning, one `string` parameter — and still cannot be combined. Delegate types are nominal, not structural. Two delegate declarations that look the same are different types.

**Nothing is mutated.** `one + two` produces a *new* delegate; `one` still points at only `StepOne` afterward. Delegates are immutable, which is exactly why `+=` on an event works the way it does — it's `x = x + y` under the hood, not an in-place append.

The code comment flags this as a preview, and it is: `CSharp.Ch06.Supplemental.04.MulticastDelegates` covers the return-value and exception behavior that this snippet deliberately avoids.

---

## `StaticAndInstanceDelegates()`: Same Signature, Different Binding

```csharp
alice.InstanceMethod = alice.GetName;   // bound to Alice's own instance
alice.StaticMethod = Person.StaticName;

bob.InstanceMethod = alice.GetName;     // Bob's delegate points at Alice's instance method
bob.StaticMethod = Person.StaticName;
```

Output:

```
Alice's InstanceMethod returns: Alice
Bob's InstanceMethod returns: Alice
Alice's StaticMethod returns: Static
Bob's StaticMethod returns: Static
```

`GetStringDelegate` doesn't care whether the method it points to is static or instance — both fit the same signature (`string GetStringDelegate()`).

What matters is what the delegate carries. An **instance** method delegate stores two things: the method *and* the object to call it on (`Delegate.Target`). `bob.InstanceMethod` genuinely calls Alice's `GetName()` and returns `"Alice"`, even though the delegate is stored in a field on Bob. The field it lives in has nothing to do with the object it's bound to — that was fixed at the moment of assignment.

A **static** method delegate has no target object to carry (`Target` is `null`), so calling it through `alice.StaticMethod` or `bob.StaticMethod` makes no difference. Both return `"Static"`.

The practical consequence is one worth remembering: a delegate holding an instance method keeps that object alive. If a long-lived object subscribes to an event with `someShortLivedObject.Handler`, the short-lived object can't be collected until it unsubscribes. That's the mechanism behind the most common managed memory leak in .NET applications, and it exists precisely because of the target reference demonstrated here.

---

## `CovarianceAndContravariance()`: The Two Directions

```csharp
private static Func<Person> returnPersonMethod;
// Equivalent to:
//   private delegate Person ReturnPersonDelegate();
//   private static ReturnPersonDelegate returnPersonMethod;

private static Action<Employee> employeeParameterMethod;
```

```csharp
// COVARIANCE: a method returning a derived class can be assigned to a
// delegate declared to return the base class.
returnPersonMethod = ReturnEmployee;          // Func<Person> = a method returning Employee

// CONTRAVARIANCE: a method with a base-class parameter can be assigned to a
// delegate declared with a derived-class parameter.
employeeParameterMethod = PersonParameter;    // Action<Employee> = a method taking Person
```

Both directions rest on the same fact — an `Employee` *is a* `Person` — applied to opposite positions.

**Covariance (output position).** The caller asked for a `Person`. A method that hands back an `Employee` satisfies that, because every `Employee` is a `Person`. Safe.

**Contravariance (input position).** The caller will supply an `Employee`. A method that only needs a `Person` can handle that, because it will never ask for anything an `Employee` doesn't have. Safe.

The reverse of either would break. A method returning `Person` can't satisfy a delegate promising `Employee` — the caller might get a plain `Person` and try to access `Salary`. A method requiring `Employee` can't satisfy a delegate accepting `Person` — it might be handed a plain `Person` and ask for a field that isn't there. Neither compiles, which is the correct outcome.

The commented-out `employeeParameterMethod(person)` line in the source is exactly that mistake, left in deliberately as a labeled example of what doesn't work. The delegate is declared `Action<Employee>`; passing a `Person` to it is rejected regardless of which method is currently assigned.

The useful mnemonic: **covariance is about what comes out, contravariance is about what goes in.** Broader going in, narrower coming out — both make the contract easier to satisfy, never harder.

### Compile-Time vs. Runtime Type

```csharp
var person = returnPersonMethod();
Console.WriteLine($"'person' is a(n) [{person.GetType().Name}] named [{person.Name}]");
```

`person`'s *compile-time* type is `Person` — that's what the delegate declares, so that's all the compiler will let you touch. But `GetType().Name` reports `"Employee"`. The object never stopped being an `Employee`; covariance just let the delegate's declared type be looser than the concrete object underneath it.

This is the same base/derived distinction from Chapter 5, showing up in a new place. The declared type controls what you can *write*; the runtime type controls what actually *executes*.

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

Run this and "Step 2..." prints before "Step 1...", even though "Step 1" appears first in the source. The anonymous method runs on its own thread, which sleeps for a full second; meanwhile the main thread moves straight on to print "Step 2..." without waiting.

Two things are being demonstrated at once. The delegate lesson: `Thread`'s constructor takes a `ThreadStart` delegate, and the anonymous method converts to it — threading APIs are delegate consumers. The concurrency lesson: `Start()` returns immediately, it does not block. Code written top-to-bottom no longer *executes* top-to-bottom once a second thread is involved.

Note that `thread.Join()` is not called here, so nothing waits for the thread to finish. It works in this program only because `GenericFunctions.Pause()` follows the call and holds the process open longer than the one-second sleep. That's incidental, not a pattern to copy — Chapter 7 covers doing it properly.

---

## Takeaways

- A delegate variable can point at any method with a matching signature, from any type, and can be reassigned freely.
- Method group conversion resolves overloads (like `Console.WriteLine`) against the target delegate type.
- Two delegate types with identical signatures are still different types — delegate typing is nominal.
- `+` and `-` produce new delegates; delegates are immutable.
- An instance-method delegate carries its target object, which keeps that object alive — the root cause of most event-handler memory leaks.
- A static-method delegate has a `null` target, so the field holding it is irrelevant.
- Covariance = a more derived return type is acceptable. Contravariance = a less derived parameter type is acceptable.
- The compile-time type limits what you can write; the runtime type is unchanged by either.
- `Thread.Start()` does not block, so source order stops predicting execution order.
