# Samples.NUnitTests

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

NUnit unit tests for `Samples.NuGetLibrary`'s `ZipCodeValidator` and `LocationFormatter`, pure, dependency-free logic and exactly the kind of code unit tests are most valuable for. This project also gives `Samples.NuGetLibrary` its first real consumer in this training set.

---

## When to Write Unit Tests Like These

For logic with clear inputs and outputs, no database, no network call, no UI, that's genuinely worth testing in isolation. `ZipCodeValidator.IsValid()` is a perfect fit: a pure function, easy to reason about, and a real place where a subtle regex mistake (off-by-one length check, wrong character class) could silently ship. Code that's mostly orchestration (a controller action, a `BackgroundService`'s `ExecuteAsync`) is usually better covered by integration tests or left to manual verification, unit tests shine specifically on logic like this.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `ZipCodeValidatorTests.cs` | Parameterized tests (`[TestCase]`) covering valid/invalid ZIP code inputs |
| `LocationFormatterTests.cs` | Tests for formatting single locations, multiple locations, and empty results |

---

## How to Run

```
dotnet test
```

Or run through Visual Studio's Test Explorer (Test > Test Explorer), which discovers and runs all `[Test]`/`[TestCase]` methods in this project automatically.

---

## Related Samples

- **`Samples.NuGetLibrary`** — the library under test.
