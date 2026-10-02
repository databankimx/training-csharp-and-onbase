# CSharp.SharedLibrary.Tests

## What This Project Is

An NUnit 4.x test suite covering the testable parts of `CSharp.SharedLibrary`. Two test fixtures, two classes under test:

- `DatabankExceptionTests` -- constructors, the `ExceptionType` property, and `Log()` output
- `GenericExtensionsTests` -- every extension method in `GenericExtensions.cs`

Deliberately not covered: `Item` (two auto-properties with no logic) and `Ch07SharedFunctions` (`Thread.Sleep(2000)` in a test suite is a reliable way to make the whole team hate the test run).

---

## Running the Tests

From the command line:

```pwsh
dotnet test .\CSharp.SharedLibrary.Tests\CSharp.SharedLibrary.Tests.csproj
```

Or through Visual Studio's Test Explorer, where NUnit3TestAdapter makes the tests appear automatically.

---

## Conventions Used Here

### NUnit constraint syntax

All assertions use `Assert.That(actual, Is.EqualTo(expected))` rather than the classic `Assert.AreEqual(expected, actual)`. The old form puts expected first and actual second -- backwards from how you'd say it aloud -- and when a test fails, the message reports which value was "expected" and which was "actual" in the wrong order. The constraint syntax matches plain English and gets the message right.

### `[TestCase]` for data-driven tests

Methods with multiple input/output combinations use `[TestCase]` instead of a separate `[Test]` for each. The fixture for `ToInt`, for example:

```csharp
[TestCase("42", 42)]
[TestCase("not a number", 0)]
[TestCase("", 0)]
public void ToInt_VariousInputs_ReturnsExpectedValue(string input, int expected)
{
    Assert.That(input.ToInt(), Is.EqualTo(expected));
}
```

Three cases, one method, all visible in Test Explorer as distinct named entries. Adding a new case is one line, not a new method.

### `Assert.Multiple`

Where a single test reasonably checks several things about one object, `Assert.Multiple` runs all assertions even when one fails, so a single red test shows everything that's wrong rather than stopping at the first failure:

```csharp
Assert.Multiple(() =>
{
    Assert.That(ex.Message, Is.EqualTo("Something went wrong!"));
    Assert.That(ex.ExceptionType, Is.EqualTo("DatabankException"));
    Assert.That(ex.InnerException, Is.Null);
});
```

### Test method naming

`MethodName_Scenario_ExpectedResult`. A failing test in the runner names the problem before you open anything:

```
DatabankExceptionTests.Constructor_MessageOnly_SetsMessageAndDefaultExceptionType -- FAILED
```

---

## DatabankExceptionTests

### What's covered

Three constructor shapes: message only, message plus inner exception, and the wrapping constructor that takes any exception and captures its runtime type name. The `ExceptionType` property setter. And `Log()` output, which requires redirecting `Console.Out` to a `StringWriter` to assert on what would otherwise go directly to the screen.

### The Console redirection pattern

`DatabankException.Log()` writes to `Console.Out` directly. Asserting on what it actually writes requires capturing that output:

```csharp
var originalOut = Console.Out;
try
{
    using var writer = new StringWriter();
    Console.SetOut(writer);

    ex.Log();

    string output = writer.ToString();
    Assert.That(output, Does.Contain("DatabankException"));
}
finally
{
    Console.SetOut(originalOut);
}
```

The `finally` block is not ceremony -- it's necessary. If an assertion fails and throws, the `using` block disposes the `StringWriter`, but `Console.Out` is still pointing at it. Every test that runs afterward in the same session inherits a writer nobody is reading from. Always restore in `finally`, not at the end of the `try`.

### What's intentionally not tested

The wrapping constructor's handling of `ex.InnerException` -- specifically the design decision discussed in `CSharp.SharedLibrary/Lesson.md` that passes `ex.InnerException` rather than `ex` itself, discarding the original exception's stack trace. The test confirms the message is preserved; it deliberately does not assert on `InnerException` identity, since that behavior is a documented tradeoff rather than a bug to guard against.

---

## GenericExtensionsTests

### What's covered

Every method in `GenericExtensions.cs` with inputs and expected outputs: `Square`, the case-insensitive `Replace`, the numeric conversion family (`ToInt`, `ToDouble`), `ToBoolean`, `Parse`/`TryParse`, both `ToArray` overloads, `IsNumeric`, `IsPositive`, `IsList`, `IsDictionary`, `IsBitSet`, and `Swap`.

### What's intentionally not tested

`IsBitSet` with `pos >= 32` on a `long` -- the latent overflow bug documented in `CSharp.SharedLibrary/Lesson.md`. The existing tests confirm correct behavior within the valid range; a test for the bug would assert on behavior the implementation gets wrong, which is more appropriately an exercise (exercise 2 in the SharedLibrary lesson) than a passing green test.

The ambiguous `ToArray()` no-argument call is also not tested -- it doesn't compile, so there's nothing to assert on. The `ToArray_CharDelimiter` and `ToArray_StringDelimiter` tests pass the delimiter explicitly, which is the only valid usage.

---

## The Exercises Connection

The SharedLibrary Lesson.md lists six exercises, several of which call for new or modified tests:

- **Exercise 2** (fix the `IsBitSet` bit shift): write a failing test for `pos = 40` on a `long`, fix the implementation, watch it go green.
- **Exercise 4** (add `ToIntOrNull`): write the test fixture alongside the new method.
- **Exercise 6** (cover all `TryParse` behaviors): the existing test covers two cases; a complete fixture would cover null, empty, positive numeric, zero/negative numeric, `t`/`y` prefix, `f`/`n` prefix, and unrecognised input -- seven `[TestCase]` entries.
- **Exercise 7** (break the base call): the test to write asserts that `Message` is empty after removing `: base(message, innerException)`. It goes red immediately, which is the point.

Running the full test suite after each exercise confirms nothing else broke.
