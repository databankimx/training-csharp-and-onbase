# Samples.NUnitTests

## What This Is

NUnit unit tests for `Samples.NuGetLibrary`'s `ZipCodeValidator` and `LocationFormatter` -- pure, dependency-free logic, exactly the kind of code unit tests are most valuable for. This is also `Samples.NuGetLibrary`'s first real consumer in this training set.

---

## When to Write Unit Tests Like These

For logic with clear inputs and outputs and no external dependencies -- no database, no network, no UI. `ZipCodeValidator.IsValid()` is a perfect fit: a pure function where a subtle mistake (off-by-one length check, wrong character class in the regex) could silently ship. Code that's mostly orchestration (a controller action, a `BackgroundService`) is usually better covered by integration tests.

---

## How NUnit Works

`[TestFixture]` marks a test class. `[Test]` marks a single test method. `[TestCase]` parameterizes a test with multiple input/output combinations:

```csharp
[TestFixture]
public class ZipCodeValidatorTests
{
    [TestCase("75067", true)]
    [TestCase("00000", true)]
    [TestCase("1234", false)]   // too short
    [TestCase("123456", false)] // too long
    [TestCase("ABCDE", false)]  // non-numeric
    [TestCase("", false)]
    [TestCase(null, false)]
    public void IsValid_VariousInputs_ReturnsExpected(string input, bool expected)
    {
        Assert.That(ZipCodeValidator.IsValid(input), Is.EqualTo(expected));
    }
}
```

`Assert.That(actual, Is.EqualTo(expected))` is the constraint-model syntax -- preferred over the classic `Assert.AreEqual(expected, actual)`, which puts the arguments in the counterintuitive order and generates confusing failure messages.

`Assert.Multiple` runs all assertions in a block even when one fails, so a single red test shows everything that's wrong:

```csharp
Assert.Multiple(() =>
{
    Assert.That(result.City, Is.EqualTo("Lewisville"));
    Assert.That(result.State, Is.EqualTo("TX"));
    Assert.That(result.ZipCode, Is.EqualTo("75067"));
});
```

Test method names follow `MethodName_Scenario_ExpectedResult` -- a failing test names the problem before you open anything.

---

## Creating an NUnit Test Project

### Visual Studio

**File > New > Project**, search "NUnit Test Project", choose .NET version, click Create. The scaffolding creates a test class with `[SetUp]` and a starter `[Test]`. Add a project reference to the library under test: right-click the test project > **Add > Project Reference**.

Or add NUnit to an existing class library:

```powershell
Install-Package NUnit
Install-Package NUnit3TestAdapter
Install-Package Microsoft.NET.Test.Sdk
```

### VS Code

```powershell
dotnet new nunit -n MyLibrary.Tests -f net10.0
dotnet add reference ../MyLibrary/MyLibrary.csproj
dotnet test
```

The `nunit` template adds `NUnit`, `NUnit3TestAdapter`, and `Microsoft.NET.Test.Sdk` automatically.

---

## Running Tests

```powershell
dotnet test
```

Or through Visual Studio's Test Explorer (Test > Test Explorer), which discovers and displays every `[Test]`/`[TestCase]` method. Failed tests show the specific input values and expected vs. actual values inline.

---

## Related Projects

- `Samples.NuGetLibrary` -- the library under test.
- `CSharp.SharedLibrary.Tests` -- a larger NUnit test suite in this solution showing the same patterns applied to a more complex library.
