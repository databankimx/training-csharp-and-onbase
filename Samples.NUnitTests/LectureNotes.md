# Samples.NUnitTests

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port). See `README.md` for the fuller when-to-test discussion.

---

## `[TestCase]`: One Method, Many Inputs

```csharp
[TestCase("75067", ExpectedResult = true, TestName = "IsValid_ValidFiveDigitZipCode_ReturnsTrue")]
[TestCase("7506", ExpectedResult = false, TestName = "IsValid_TooShort_ReturnsFalse")]
[TestCase(null, ExpectedResult = false, TestName = "IsValid_Null_ReturnsFalse")]
public bool IsValid_VariousInputs_ReturnsExpectedResult(string? zipCode)
{
    return ZipCodeValidator.IsValid(zipCode);
}
```

Each `[TestCase]` runs the same method body against a different input, and `ExpectedResult` lets NUnit assert the return value directly, no `Assert.That(...)` line needed inside the method at all for this simple case. Each case still shows up as its own named result in a test runner, `TestName` here makes those names read clearly rather than defaulting to the case's raw argument values. This is the idiomatic NUnit way to cover a range of inputs against one piece of logic without writing (and maintaining) a separate test method per case.

---

## `Assert.That(...)`: The Constraint Model

```csharp
Assert.That(result, Is.EqualTo("75067"));
Assert.That(result, Is.Null);
```

This is NUnit's current recommended assertion style, the "constraint model," `Is.EqualTo`, `Is.Null`, `Is.True`, and others compose into readable, self-describing assertions. The older "classic model" (`Assert.AreEqual(...)`, `Assert.IsNull(...)`) still works and still appears in plenty of existing code, but `Assert.That(...)` is what NUnit's own documentation steers new tests toward, worth using by default in anything written today.

---

## `[SetUp]`: Fresh State Per Test, Not Shared

```csharp
[SetUp]
public void SetUp()
{
    sampleLocation = new Location("75067", "Lewisville", "Denton", "TX");
}
```

`[SetUp]` runs before **every** `[Test]` method in the fixture, giving each test its own fresh `sampleLocation` rather than one shared instance reused across tests. NUnit doesn't guarantee test methods within a fixture run in a particular order (and, depending on runner configuration, they can run in parallel), so a test that accidentally depended on a previous test's mutation of shared state would be a real, hard-to-diagnose source of flaky failures. `Location` being an immutable `record` means this particular fixture couldn't actually hit that bug even with shared state, but the pattern (fresh setup per test) is worth establishing as a habit regardless, it's what keeps a test suite reliable as more tests (and more mutable fixtures) get added to it over time.

---

## Try It Yourself

Run `dotnet test` and watch each `[TestCase]` report as its own named result. Then try breaking `ZipCodeValidator`'s regex on purpose (loosen it to allow 4 digits, for instance) and re-run, watch exactly which parameterized case catches it.
