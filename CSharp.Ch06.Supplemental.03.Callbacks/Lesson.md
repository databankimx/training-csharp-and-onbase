# Chapter 6 Supplemental 03: Callbacks

## What This Is

A callback demonstration built around a practical example: searching a directory for `.cs` files, with two separate callbacks - one fired after every match, one fired at the end. The project uses solution-root discovery to locate the search directory, a portable pattern worth understanding on its own.

---

## How Solution-Root Discovery Works

`Search()` locates the project directory by walking up from `AppContext.BaseDirectory` until it finds the solution file:

```csharp
private const string SolutionFileName = "DataBank.DeveloperTraining.sln";

private static string FindSolutionRoot()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory != null)
    {
        if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
            return directory.FullName;
        directory = directory.Parent;
    }
    throw new DatabankException($"Could not locate {SolutionFileName} above {AppContext.BaseDirectory}");
}
```

`AppContext.BaseDirectory` is where the running assembly lives - the correct starting point for finding anything relative to the deployment. `Path.Combine` handles directory separators correctly. The loop terminates because `DirectoryInfo.Parent` returns `null` at the drive root, and the method throws with a descriptive message rather than returning `null` for a caller to trip over. The same technique is used in `LessonRunner`.

The search path is then built from the discovered root:

```csharp
string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
```

This works correctly regardless of which machine the solution lives on or which drive it's checked out to. Never hardcode absolute paths - use discovery from `AppContext.BaseDirectory` and combine with `Path.Combine`.

---

## What a Callback Actually Is

A callback is a delegate you pass into a method so that method can call *you* back at points it chooses. Inversion of control at its smallest scale: the called method owns *when*, you own *what happens*.

---

## How to Write This Program

### Step 1: Define the Search and Its Callbacks

```csharp
private static List<string> Files = new List<string>();

private static async Task Search(string searchTerm, string directory, Action<int> callback, Action callback2)
{
    if (!Directory.Exists(directory))
        throw new DirectoryNotFoundException($"Failed to locate directory [{directory}]!");

    int matchedFiles = 0;
    foreach (string path in Directory.GetFiles(directory))
    {
        if (!path.Contains(searchTerm)) continue;
        matchedFiles++;
        callback(matchedFiles);            // fires once per match, passes running count
        Files.Add(Path.GetFileName(path));
    }
    callback2();                           // fires once at the end
}
```

Run nothing yet - this is just the structure.

`Search` doesn't know or care what `callback` and `callback2` do. It knows only their signatures (`Action<int>` and `Action`) and calls them at the two moments that make sense. That separation is what makes `Search` reusable: progress reporting, logging, cancellation checks - none of it has to be written into the search logic, because the caller supplies it.

The two callbacks have deliberately different shapes:

- `Action<int>` receives data (the running count). The search knows something the caller wants.
- `Action` receives nothing. It's a pure notification - "I'm done."

Choosing the right signature is the actual design work in a callback API. Pass what the caller genuinely needs.

### Step 2: Write the Two Concrete Callbacks

```csharp
private static void Callback(int count)
{
    Console.Clear();
    Console.WriteLine($"Found {count} files...");
    Thread.Sleep(1000);
}

private static void Callback2()
{
    if (Files == null || Files.Count == 0)
    {
        Console.WriteLine("No files found!");
        return;
    }
    Console.WriteLine($"\nFiles:");
    foreach (string name in Files)
        Console.WriteLine(name);
}
```

### Step 3: Wire Up and Run

```csharp
string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
await Search(".cs", searchPath, Callback, Callback2);
```

Run it. Watch `Callback` clear the screen and update the count for every `.cs` file found - the one-second sleep is there deliberately so you can watch it happen one file at a time - then `Callback2` prints the final list.

That `Thread.Sleep(1000)` is worth pausing on. It's inside the *callback*, but it blocks the *search*. `Search` cannot proceed to the next file until `callback(matchedFiles)` returns. A slow callback makes the whole operation slow. A callback that throws takes down the method that invoked it. Invoking a callback means executing arbitrary code you don't control. Be deliberate about where in your method that's allowed to happen - particularly if you're holding a lock or partway through mutating shared state.

---

## Try It Yourself

Run the project and watch the console update once per matching `.cs` file - the one-second sleep between each is there specifically so you can see the callback firing one file at a time rather than all at once. Then notice the final list only appears after the loop finishes, since `callback2()` is called exactly once, at the very end.

For a second pass: replace `Callback` with an inline lambda at the call site instead of a named method:

```csharp
await Search(".cs", searchPath,
    count => Console.WriteLine($"Found {count}..."),
    Callback2);
```

Same behavior, no separate method. This is what callback-as-lambda looks like in real production code - the search method doesn't care which form you use, only the signature.

---

## Design Notes Worth Keeping

**Prefer events over plain callbacks where either would work.** Events are more discoverable, integrate with tooling, and communicate optionality clearly - a callback parameter looks required; an event obviously isn't.

**Prefer `Action`/`Func` over custom delegate types.** This project follows its own advice. Callers don't need to learn a new type name.

**Understand `Expression<...>` vs. `Func<...>`.** Both describe code, but differently: `Func<...>` is compiled code that runs in-process; `Expression<...>` is a data structure describing the code, which can be inspected, serialized, or translated. That's how Entity Framework converts a lambda into SQL. It also costs more, so measure before reaching for it.

**`async void` is for event handlers only.** `StartSearch` in the real project is `async void`, which prevents callers from awaiting or catching. Use `async Task` everywhere else. Chapter 7 covers this properly.

---

## Takeaways

- A callback is a delegate passed in so the receiving method can call back at moments of its choosing.
- The receiving method owns *when*; the caller owns *what happens*.
- Design callback signatures around what the caller needs - pass data when there's data, nothing when it's a notification.
- Prefer `Action`/`Func` over custom delegate types in public APIs.
- Prefer events over callbacks when either would serve.
- Invoking a callback runs code you don't control: it can block, throw, or re-enter.
- Never hardcode absolute paths. Discover them from `AppContext.BaseDirectory` and combine with `Path.Combine`.
- `async void` is for event handlers only.
