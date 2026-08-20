# Ch06 Textbook Code: Covariance and Contravariance

## What This Is

The direct source `CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`'s `CovarianceAndContravariance()` method was adapted from. Pure reference code, a blank window, `Form1_Load()` sets up the covariant and contravariant delegate assignments and does nothing else, best inspected with a debugger breakpoint on the closing brace rather than run straight through.

No bugs found.

---

## Worth Noticing: Commented-Out Alternatives, Left in Place

```csharp
private delegate Person ReturnPersonDelegate();
//private ReturnPersonDelegate ReturnPersonMethod;
private Func<Person> ReturnPersonMethod;
```

```csharp
private delegate void EmployeeParameterDelegate(Employee employee);
private EmployeeParameterDelegate EmployeeParameterMethod;
//private Action<Employee> EmployeeParameterMethod;
```

Both fields have a commented-out sibling declaration right next to the active one, a custom-delegate version next to a `Func<Person>`/`Action<Employee>` version. Neither pairing changes the demonstration's behavior, `Func<Person>` and a custom `ReturnPersonDelegate` returning `Person` behave identically for this purpose. This is a small, deliberate side note baked into the code itself: covariance and contravariance apply the same way whether you're using the built-in generic delegates or a hand-declared one, the behavior comes from the delegate's *signature*, not from which specific delegate type is doing the declaring.

---

## Compare Against the Supplemental Version

`CSharp.Ch06.Supplemental.01.NamedVersusAnonymousDelegates`'s `CovarianceAndContravariance()` takes this exact same setup and actually *runs* it, calling `returnPersonMethod()`, printing `person.GetType().Name`, invoking `employeeParameterMethod(employee)`, all the visible, concrete parts this reference-only version deliberately leaves out. Worth reading both, this project shows the minimal setup in isolation, that one shows what actually happens when you use it.
