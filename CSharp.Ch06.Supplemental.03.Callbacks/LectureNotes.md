# Chapter 6 Supplemental 03: Callbacks

## What This Is

A callback demo built around a genuinely practical example: searching a directory for `.cs` files, with two separate callback delegates, one fired after every match (`Callback`, prints a running count), one fired once at the end (`Callback2`, prints the full list).

---

## The Bug That Was Here

`Search()` was pointed at a hardcoded absolute path:

```csharp
private const string SearchPath = @"D:\FileStore\Development\DeveloperTraining\CSharp.Ch06.DelegatesEventsAndExceptions";
```

This is a path on one specific development machine, a `D:` drive, a folder layout (`DeveloperTraining`, not `developer-training`) from before this solution was migrated. Run as originally written on any other machine, `Directory.Exists(directory)` fails immediately and the whole demo throws a `DirectoryNotFoundException` before it does anything.

Fixed using the same solution-root-discovery technique `LessonRunner` already uses elsewhere in this solution, walk up from wherever the running executable actually is until a `DataBank.DeveloperTraining.sln` is found, then search relative to that:

```csharp
string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
```

This works correctly regardless of which machine the solution is checked out to, or which drive/folder it lives in, the same portability problem `LessonRunner.FindSolutionRoot()` was already written to solve for a different reason.

---

## Callbacks in Practice

```csharp
private static async void StartSearch()
{
    string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
    await Search(".cs", searchPath, Callback, Callback2);
}

private static async Task Search(string searchTerm, string directory, Action<int> callback, Action callback2)
{
    ...
    foreach (string path in Directory.GetFiles(directory))
    {
        if (!path.Contains(searchTerm)) continue;
        matchedFiles++;
        callback(matchedFiles);
        Files.Add(Path.GetFileName(path));
    }
    callback2();
}
```

`Search()` doesn't know or care what `Callback`/`Callback2` actually do, it just calls them at the right moments (once per match, once at the end). That's the whole idea of a callback: the receiving method owns *when* to call back, the caller owns *what happens* when it does. Run this and watch `Callback` clear the console and print a running count for every `.cs` file found (with a short pause each time, so you can actually watch it happen one file at a time), then `Callback2` prints the final list once the search completes.

---

## Worth Reading: The Embedded Design Guidance

The chapter notes cite Microsoft's actual framework design guidelines on callbacks vs. events. The short version: prefer events over plain callbacks where either would work (events are more discoverable, integrate with IDE tooling), reach for `Func<...>`/`Action<...>` over a custom delegate type when defining a callback-based API, and remember that invoking a callback means executing arbitrary code someone else supplied, worth being deliberate about where that's allowed to happen.
