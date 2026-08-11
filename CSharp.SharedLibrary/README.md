# CSharp.SharedLibrary

## What This Is

Not a lesson. This is the toolbox every later chapter reaches into so nobody has to re-write the same exception wrapper or boolean parser for the fifteenth time. If a chapter references `CSharp.SharedLibrary`, that's your cue this project needs to build first.

---

## What's In It

### `Models/DatabankException.cs`

A custom `Exception` subclass that remembers what kind of exception it originally wrapped (`ExceptionType`), and knows how to print itself, and its entire chain of inner exceptions, to the console with a stack trace attached. This is the same `GetType().Name` / `Message` / `StackTrace` pattern you'll see repeated by hand in every chapter's `Main()` method, `DatabankException.Log()` is where that pattern actually lives, the console apps are just doing it inline instead of calling this.

> **This is a teaching version, not the production one.** DataBank's actual custom exception type ships as the `Databank.Exceptions` NuGet package (`Databank.Models.DatabankException`), and it's a different, more capable animal: an `ErrorCodes` enum for classifying failures, an `IsFatal` flag, a `Description` pulled from `[Description]` attributes on those codes, and no `Log()` method of its own, logging is handled separately by `Databank.Logging`'s `HandleException()` extension method instead. That split between the exception model and how it gets logged is a deliberate design choice in the real package that this teaching version doesn't bother with yet. Different namespace (`Databank.Models` vs. `CSharp.SharedLibrary.Models`), so there's no compiler collision if a later chapter references both, but don't be surprised when the real one doesn't have the method you learned here. We'll introduce `Databank.Exceptions` by name once we reach a chapter that calls for it.

### `Models/Item.cs`

A deliberately boring class, two string properties and nothing else. Its entire job is to exist as a generic "some object" for chapters that need a sample type without wanting to explain what the type does.

### `HelperClasses/GenericExtensions.cs`

A grab bag of extension methods: string-to-number conversions that don't throw on bad input (`ToInt`, `ToDouble`, and friends all return a default value instead of blowing up), a more forgiving boolean parser than `bool.Parse` ever bothered to be, list/dictionary type checks, bit-flag checking, and a generic `Swap<T>`. This file doubles as a teaching example for what extension methods look like in practice, the block comments throughout explain the syntax as they go.

### `HelperClasses/GenericFunctions.cs`

`Pause()` and `FinishChapter()`, the two functions almost every textbook-code console app in this solution calls to wait for a keypress and print a "you're done with this chapter" summary. If you're wondering why so many chapters look identical at the very start and very end of `Main()`, it's because they're both calling in here.

### `HelperClasses/Ch07SharedFunctions.cs`

Simulated I/O-bound and CPU-bound work (`SimulateReadDataFromIo`, `DoIntensiveCalculations`) used specifically by the multithreading and async chapters to have something that takes measurable time without needing a real database or a real spreadsheet to churn through.

---

## Testing

`CSharp.SharedLibrary.Tests` covers the parts of this library with actual inputs and outputs worth verifying, the extension methods and `DatabankException`. `Item` isn't tested, there's nothing to test, it's two auto-properties. The simulated-work methods in `Ch07SharedFunctions` aren't unit tested either, `Thread.Sleep(2000)` in a test suite is a good way to make your coworkers hate you.

### Framework and Conventions

Tests are written in NUnit 4.x using the `Assert.That` constraint syntax (`Assert.That(actual, Is.EqualTo(expected))`) rather than the older classic assertions (`Assert.AreEqual(expected, actual)`). Constraint syntax reads closer to plain English and keeps the "what you expect" argument from getting mixed up with the "what you got" argument, a mistake that's easy to make with classic assertions since the order is backwards from how you'd say it out loud.

Each test class is a `[TestFixture]`, each test method is `[Test]` or, when the same logic needs checking against several inputs, `[TestCase]` with the inputs and expected result supplied right in the attribute. `GenericExtensionsTests.cs` leans heavily on `[TestCase]` since most of what it's testing is "given this string, does the conversion return the right value," which is exactly the kind of thing that turns into a wall of near-identical test methods without it.

Test names follow `MethodName_Scenario_ExpectedResult`, so a failing test tells you what broke before you've even opened the file.

### Running the Tests

Run them with `dotnet test` from the solution or project folder, or through Visual Studio's Test Explorer once the solution is open, they'll show up automatically thanks to the `NUnit3TestAdapter` package reference.
