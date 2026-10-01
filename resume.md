# Resume: DataBank.DeveloperTraining Lesson.md Rewrite Sweep

## How to Use This File

This file is a handoff document for continuing the Lesson.md rewrite sweep in a new conversation. Start there with: "Read resume.md and pick up where we left off."

---

## The Project

**Solution:** `C:\Development\training\developer-training\DataBank.DeveloperTraining.sln`
**Owner:** Scott McLean, DataBank IMX, GHE admin (smclean@databankimx.com)

This is a full-solution rewrite of every project's `Lesson.md`, turning reference-doc stubs and lecture notes into walkthrough-style tutorials. The goal is a reader-driven "how to write this program" experience: conversational voice, concrete code at each step, explicit "run it" checkpoints before adding more, no references to porting or bug-fixing history.

---

## Solution Structure

- **Language:** C#, .NET Framework 4.8 (`net48` set solution-wide in `Directory.Build.props`)
- **Project file format:** SDK-style `.csproj` throughout. Visual Studio's `.NET Framework` wizard generates old-style `.csproj` by default -- new projects require manual conversion (replace the entire file contents with the three-property SDK-style format, delete `Properties\AssemblyInfo.cs` and `packages.config`). VS Code / `dotnet new console -f net48` produces the correct format on the first try.
- **Shared library:** `CSharp.SharedLibrary` -- `GenericFunctions` (Pause, FinishChapter), `GenericExtensions` (ToBoolean, TryParse, ToInt, etc.), `DatabankException`.
- **LessonRunner:** Registers chapters by number; the lesson runner itself is a separate project.

---

## Rewrite Conventions

### Voice and tone
- Conversational, dry-humor -- not textbook, not terse. Think "senior developer talking to a junior." Reference Scott's own written articles as the voice model.
- No em dashes; use hyphens. No emojis.
- No porting/bug-fix references in the reader-facing text. Bugs that were fixed should be explained as "this is how to do it correctly" without mentioning a prior broken version.

### Structure
- **Mini-program approach (Ch2+):** Each topic is presented as standalone code in `Main()`. Clear and rebuild between topics. The reader types it fresh, runs it, sees output, then continues.
- No named helper methods and no `Pause()` in the walkthrough code -- these concepts haven't been introduced yet for early chapters.
- **"Seeing it all together" section at the end:** Explains that the real `Program.cs` uses named methods (a preview of a later chapter), and points the reader at the finished reference copy.
- **Build-and-run checkpoints** explicitly after each meaningful addition -- "Run it" followed by what to expect.
- **Split long chapters** into `Lesson.md` + `Lesson-Part2-*.md` with links between. Ch04 and Ch05 are already split.

### Skip rule
**Skip all projects with "TextbookCode" in the name.** These are verbatim textbook samples, not ours.

---

## What Has Been Done (Chapters 1-6)

### Chapter 1 -- `CSharp.Ch01.HelloWorld`
`Lesson.md` complete. Includes:
- "Creating the Project" section: Visual Studio (Console App .NET Framework, 4.8/4.8.2, SDK-style `.csproj` conversion required), VS Code (`dotnet new console -f net48`, no conversion needed).
- Framework vs. Core distinction explained.
- Steps 1-8 with explicit build-and-run after the bare Hello World (Step 3).
- "Congratulations, you're now a programmer" beat at the first successful run.

### Chapter 2 -- `CSharp.Ch02.BasicProgramStructure`
`Lesson.md` complete. 17 mini-programs covering: simple/complex statements, empty statement gotcha, conditional operators (3 parts including = vs == gotcha), ternary, bool, if/else/else-if, nested if, switch, for+lottery, foreach+grades, while/do-while, for-loop catalog, arithmetic, precedence, increment/decrement. No Pause(), no functions.

### Chapter 3 -- `CSharp.Ch03.WorkingWithTheTypeSystem`
`Lesson.md` complete. 17 mini-programs covering: value type aliases, sizeof tour, structs (Person, Book with validation), enums, using enums, class with static field, class methods, value vs reference passing, generics (queue/stack), bit shifts, bit flags, indexer, alias vs system type, wrap-around overflow, value vs reference side-by-side. Bonus: two's complement / signed type asymmetry explained.

### Chapter 4 -- `CSharp.Ch04.UsingTypes`
Split into two files:
- `Lesson.md` (Part 1): 19 mini-programs -- casting/widening/narrowing, checked, is/as, array covariance, Parse/TryParse, decimal.Parse with NumberStyles, Convert (banker's rounding, throws vs silent wrap), Convert.ChangeType, BitConverter, boxing/unboxing, custom conversion (ToBoolean/TryParse -- these live in `CSharp.SharedLibrary/HelperClasses/GenericExtensions.cs`, NOT in this project's StringExtensions.cs), DllImport (MessageBox, GetShortPathName), COM/Excel interop with dynamic, array cloning.
- `Lesson-Part2-Strings.md` (Part 2): string immutability, building from chars, length/index, static/instance methods, padding/trimming/case, StringBuilder vs concatenation (permutations benchmark), ToString with format/culture, string.Format vs interpolation, format specifier reference tables, decimal vs double bonus.

**Code fix in this chapter:** `StringExtensions.cs` was reverted to `CompareTo`-only after a duplicate `ToBoolean`/`TryParse` was erroneously added. The actual implementations are in `GenericExtensions.cs`.

### Chapter 5 -- `CSharp.Ch05.ImplementingClassHierarchies`
Split into two files:
- `Lesson.md` (Part 1): Person/Employee/Faculty/TeachingAssistant hierarchy with constructor chaining (this/base), interfaces vs abstract classes table, IComparable, IComparer, IEquatable, ICloneable (three implementations side by side).
- `Lesson-Part2-Enumerable-and-Disposable.md` (Part 2): IEnumerable/IEnumerator (TreeNode org chart, foreach desugaring shown explicitly), IDisposable (three disposal paths: explicit, GC finalizer, using block), operator overloading bonus (Equals/GetHashCode/operators built on CompareTo).

Supplementals:
- `CSharp.Ch05.Supplemental.Cloning/Lesson.md`: Three mini-programs (reference assignment, shallow clone, deep clone) with ReferenceEquals making difference observable.
- `CSharp.Ch05.Supplemental.ImplementingClassHierarchies/Lesson.md`: Address/BusinessAddress/Telephone (self-validating)/Person/Contact built in dependency order, is-a vs has-a distinction.
- `CSharp.Ch05.Supplemental.ConfigurationClasses/Lesson.md`: Six steps (XML first, then KeywordTypeElement, KeywordTypeCollection, DocumentTypeElement/Collection, ServiceLocation with PostDeserialize, OnBaseSettings root). DPAPI credential encryption explained.

**Code fix in this chapter:** `CSharp.Ch05.Supplemental.07.Events` -- `OverdrawnEventArgs` was missing `: EventArgs` inheritance (compile-breaking bug, fixed).

### Chapter 6 -- `CSharp.Ch06.DelegatesEventsAndExceptions`
`Lesson.md` complete. WinForms project (different from console mini-program approach). Covers: delegate declaration/assignment, named vs anonymous, event declaration with `event` keyword, `?.Invoke()` pattern, background thread via anonymous method, GraphForm three syntaxes (expression lambda, anonymous method, statement lambda). No mini-programs (WinForms); instead, step-by-step build with explicit "run the application" checkpoints.

All 9 supplementals complete:
- `Supplemental.01.NamedVersusAnonymousDelegates/Lesson.md`: 5 mini-programs -- assignment/reassignment, +/- combining, static vs instance binding, covariance/contravariance, thread delegate.
- `Supplemental.02.LambdaExpressions/Lesson.md`: 6 mini-programs -- zero/one/multiple params, value-returning lambda, statement lambda, LINQ method vs query syntax, `Where` with index, four historical delegate spellings.
- `Supplemental.03.Callbacks/Lesson.md`: Bug fix documented (hardcoded `D:\FileStore` path replaced with solution-root-discovery). Callback design guidance. `async void` warning.
- `Supplemental.04.MulticastDelegates/Lesson.md`: Single mini-program demonstrating `+`/`-`. Last-return-wins and exception-stops-list sharp edges. `GetInvocationList()` escape hatch.
- `Supplemental.05.ExceptionHandling/Lesson.md`: try/catch/finally as real application entry point, exception wrapping, catch ordering, using vs try/finally, nondeterministic throw, four arithmetic cases (checked vs unchecked integer, float overflow, float 0/0).
- `Supplemental.06.ParameterizedThreadStart/Lesson.md`: ThreadStart vs ParameterizedThreadStart, static vs instance delegate, deliberately non-static Program class.
- `Supplemental.07.Events/Lesson.md`: Bug fix documented (`OverdrawnEventArgs : EventArgs` added -- was compile-breaking). 5 progressive bank account implementations.
- `Supplemental.08.Assertions/Lesson.md`: Was empty stub, filled in. Debug.Assert vs Trace.Assert, assertions vs exceptions rule, BinarySearch precondition example.
- `Supplemental.09.Closures/Lesson.md`: Already excellent; light touch only. First-class functions, free variables, closure factory pattern, modified closure gotcha, for-loop variant, cross-language comparison (Python, JavaScript, Rust, C++, Java).

**Code fix in this chapter:** `CSharp.Ch06.Supplemental.03.Callbacks` -- hardcoded `D:\FileStore` path replaced with solution-root-discovery using `AppContext.BaseDirectory`.

---

## What Remains (Chapters 7-12, then Supplementals)

Work through these in order. Apply the same mini-program walkthrough pattern throughout.

### Chapter 7 -- Multithreading and Async
**Complete.** All ten projects done (main + Supplementals 01-09). Key conventions established during this chapter:
- Each example within a Lesson.md is a standalone mini-program -- clear `Main()` and rebuild between each one. No menu-driven multi-lesson programs.
- Wry, self-deprecating humor tone. "That's not a coincidence." / "The array of task handles is a monument to good intentions that went nowhere."
- Skip all `CSharp.Ch07.TextbookCode.*`

### Chapter 8 -- Reflection
**Complete.** All five projects done (main + Supplementals 01-04).
- Skip `CSharp.Ch08.TextbookCode.*`

### Chapter 9 -- Working With Data
**Complete.** All six projects done (main + Supplementals 01-05; Supplemental 02 was already done).
- Skip `CSharp.Ch09.TextbookCode.*`

### Chapter 10 -- LINQ
**Complete.** All five projects done (main + Supplementals 01-04).
- Skip `CSharp.Ch10.TextbookCode.*`

### Chapter 11 -- Debugging and Instrumentation
**Complete.** All five projects done (main + Supplementals 01-04).
- Skip `CSharp.Ch11.TextbookCode.*`

### Chapter 12 -- Encryption and Assemblies
**Complete.** All five projects done (main + Supplementals 01-04).
- Skip `CSharp.Ch12.TextbookCode.*`

### Solution-Wide Supplementals (after all chapters)
These live at the solution root level, not inside a chapter folder:
- `CSharp.Supplemental.BitwiseOperations`
- `CSharp.Supplemental.DataStructureFundamentals`
- `CSharp.Supplemental.TrieExamples`
- `CSharp.Supplemental.LoggingWithLog4Net`
- `CSharp.Supplemental.LoggingWithSerilog`
- `CSharp.Supplemental.LoggingWithDatabankLogging`
- `CSharp.Supplemental.FactoryPattern.01.NoFactory`
- `CSharp.Supplemental.FactoryPattern.02.BasicFactory`
- `CSharp.Supplemental.FactoryPattern.03.ImprovingPattern`
- `CSharp.Supplemental.Algorithms.*` (BigOConcepts, Recursion, ReducingComplexity, Search, Sort, Visualizations)
- `CSharp.Supplemental.StringPerformance`

### Already Done (Non-Chapter Projects)
- `OnBase.Preprocessor/Lesson.md` -- complete
- `CSharp.Ch09.Supplemental.02.SqlInjection/Lesson.md` -- complete

---

## Cleanup Tasks (After Sweep Is Complete)

These are deferred until every project has been touched. Do NOT do them during the sweep.

1. **Delete all `LectureNotes.md` files** across the entire solution. They've been folded into the new `Lesson.md` files. A simple PowerShell sweep:
   ```powershell
   Get-ChildItem -Path "C:\Development\training\developer-training" -Filter "LectureNotes.md" -Recurse | Remove-Item
   ```

2. **Delete all extra `*.md` files** that are now redundant subsets of the new `Lesson.md` files -- specifically the `BasicProgramStructure.md`, `WorkingWithTheTypeSystem.md`, `UsingTypes.md`, `DelegatesEventsAndExceptions.md`, `HelloWorld.md`, and `Cloning.md` files that existed alongside `Lesson.md` in their respective project folders and contained nothing not covered by the rewrite.

3. **Update the solution `README.md`** to recommend reading each project's `Lesson.md` alongside running the pre-built code. The pre-built code runs immediately without any setup -- readers should use it as a reference while working through the walkthrough.

4. **Delete this `resume.md` file** once it's no longer needed.

---

## Key Files Modified (Not Just Lesson.md)

These source code files were changed during the sweep and should not be reverted:

| File | Change |
|---|---|
| `CSharp.Ch04.UsingTypes/HelperClasses/Extensions/StringExtensions.cs` | Reverted to `CompareTo`-only after duplicate ToBoolean/TryParse were added in error. The real implementations are in `CSharp.SharedLibrary/HelperClasses/GenericExtensions.cs`. |
| `CSharp.Ch05.Supplemental.07.Events` -- `OverdrawnEventArgs.cs` | Added `: EventArgs` inheritance. Was compile-breaking. |
| `CSharp.Ch06.Supplemental.03.Callbacks` -- `Program.cs` | Replaced hardcoded `D:\FileStore\...` path with solution-root-discovery via `AppContext.BaseDirectory`. |
| `CSharp.Ch06.Supplemental.08.Assertions` -- `Program.cs` | Was empty stub. Filled in with `BasicAssertions`, `AssertionsVersusExceptions`, `DebugAssertVersusTraceAssert`, `AssertingAnInternalInvariant` (BinarySearch). |

---

## Notes on Specific Chapters Ahead

- **Chapter 7 (Threading/Async):** Likely needs splitting into multiple files like Ch4/Ch5. The async content alone (Task, async/await, cancellation tokens) is substantial. The supplementals cover Thread pool, unblocking UI, TPL, race conditions, barriers, locking, lock-free alternatives, concurrent collections.
- **Chapter 9 (Data):** Has ADO.NET, Entity Framework, file I/O, serialization. The SQL Injection supplemental is already done and can serve as the template for the others.
- **Chapter 10 (LINQ):** Heavy on query expression syntax vs method syntax, deferred execution, `IQueryable` vs `IEnumerable`. The lambda expressions groundwork from Ch6 Supplemental 02 makes this cleaner to reference.
- **Chapter 12 (Encryption):** Covers symmetric/asymmetric encryption, digital signatures, strong naming, GAC. The supplementals go deep into each.
