# OnBase Preprocessor

## What This Is

Adapted and modernized from `Samples.Preprocessor` in the old training solution - a sample OnBase COLD/DIP preprocessor: a console executable OnBase itself invokes during document import to clean up a text file before OnBase processes it. Not related to `CSharp.Ch11.Supplemental.02.PreprocessorDirectivesDeepDive`, which covers C#'s own `#if`/`#define` compile-time directives - same word, unrelated topics.

Placed under the new `Preprocessors` solution folder, nested under `OnBase` alongside `REST API`, `Unity API`, and `E-Forms` - this is an OnBase integration pattern, not a general C# language topic, so it belongs there rather than under `CSharp`/`Supplementary`.

## What Changed From the Original

- **`log4net` replaced with Serilog**, per the team's stated standard. The old project had a separate `<log4net>` XML config section with a hardcoded log file path (`C:\Temp\WSTesting\Logs\OnBaseWebService.log`) baked into `App.config`. `LogWriter` (`HelperClasses/Extensions/LogWriter.cs`) keeps the exact same public surface every caller already used (`LogWriter.Log(...)`, the `Exception.Log()` extension, `DebugMode`/`Interactive`) - only its internals changed, plus one new `Initialize(logFilePath)` call added to `Program.Initialize()` to set up the Serilog file sink before anything else runs.
- **`App.config` kept**, per direction - this genuinely is how a real OnBase-invoked preprocessor gets configured in practice, and the realism is part of the lesson. The custom `<preprocessorSettings>` section is unchanged except for one addition: a new `logFilePath` attribute, so the log destination lives in the one config section this tool already had, rather than reviving a second one just for logging.
- **Target framework kept at `net48`**, per direction - no override needed, that's already this solution's default.
- **The project's own local `DatabankException.cs` removed**, replaced with a project reference to `CSharp.SharedLibrary` - same pattern `CSharp.Ch09.Supplemental.02.SqlInjection` already established. The constructor signature (`DatabankException(message, innerException)`) is identical, so nothing calling it needed to change.
- **`internal class Program` → `internal static class Program`** - the class has no instance members, matching the console-app convention used throughout this solution.
- One small defensive fix along the way: the original's `finally` block referenced `settings.Interactive` unconditionally, which would throw a `NullReferenceException` on top of whatever exception `Initialize()` already threw if the config section itself was missing or malformed before `settings` got assigned. Now guarded with a null check.

## Two Bugs Found in the Original (Fixed Here)

Both were carried forward verbatim during the initial port, then caught by actually running the tool against real input:

- **`Initialize()`'s output-directory check** treated a bare output filename with no directory component (e.g. `sample-output.txt`, meaning "write it to the current directory") as an error, because `Path.GetDirectoryName()` returns an empty string in that case and the check was `string.IsNullOrEmpty(outputDirectory) || !Directory.Exists(outputDirectory)`. An empty directory isn't an error, it just means "current directory," which always exists. Fixed to only fail when a *non-empty* directory was actually specified and doesn't exist.
- **`CharacterCollection`'s key-based indexer** (`this[char original]`) called `BaseGet(original)` without an explicit cast. `char` converts implicitly to `int`, which C# prefers over boxing to `object` during overload resolution - so this was silently calling `ConfigurationElementCollection.BaseGet(int index)` instead of the intended `BaseGet(object key)`, treating each character's numeric code point as a collection index. With only 2 replacement entries configured, almost any character being processed (anything with a code point of 2 or higher) threw "index out of range." Fixed with an explicit `(object)` cast to force the correct overload.

## Not Registered With `LessonRunner`

Deliberately left out of `LessonRunner`'s catalog. Every other console lesson there is meant to be picked from a menu and just run - this one requires two file path arguments (`%I`/`%O` in OnBase's own terms) to do anything meaningful, and `LessonRunner` has no mechanism to prompt for or pass per-lesson arguments. Running it with no arguments just demonstrates the "missing argument" exit path, which isn't nothing, but isn't really the point either. `Lesson.md` documents the actual command to run it by hand instead, which is a closer match to how a real OnBase-invoked preprocessor is actually exercised anyway - non-interactively, from a command line, with explicit file paths.
