# Chapter 6 Supplemental 03: Callbacks

## What This Is

A callback demonstration built around a genuinely practical example: searching a directory for `.cs` files, with two separate callback delegates — one fired after every match (`Callback`, prints a running count), one fired once at the end (`Callback2`, prints the full list).

This project also contains one of the real bugs in the training set, and it's a useful one, because it's the kind that passes review on the machine it was written on.

---

## The Bug That Was Here

`Search()` was pointed at a hardcoded absolute path:

```csharp
private const string SearchPath = @"D:\FileStore\Development\DeveloperTraining\CSharp.Ch06.DelegatesEventsAndExceptions";
```

Everything about that string is specific to one machine at one moment in time: a `D:` drive, a `FileStore` root, and a folder named `DeveloperTraining` — the layout from before this solution was migrated to `developer-training`. Run as originally written on any other machine, `Directory.Exists(directory)` fails immediately and the demo throws a `DirectoryNotFoundException` before it does anything at all.

The fix uses the same solution-root-discovery technique `LessonRunner` already uses elsewhere in this solution:

```csharp
private const string SolutionFileName = "DataBank.DeveloperTraining.sln";

private static string FindSolutionRoot()
{
	var directory = new DirectoryInfo(AppContext.BaseDirectory);

	while (directory != null)
	{
		if (File.Exists(Path.Combine(directory.FullName, SolutionFileName)))
		{
			return directory.FullName;
		}
		directory = directory.Parent;
	}

	throw new DatabankException($"Could not locate {SolutionFileName} above {AppContext.BaseDirectory}");
}
```

```csharp
string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
```

Walk up from wherever the running executable actually is until a `DataBank.DeveloperTraining.sln` is found, then search relative to that. This works regardless of which machine the solution is checked out to, or which drive or folder it lives in.

The transferable lessons:

- `AppContext.BaseDirectory` — where the assembly is actually running from — is the correct starting point for locating anything relative to the deployment. Don't assume the current working directory; it's whatever the launching process set it to.
- Use `Path.Combine` rather than string concatenation with `\`. It handles separators correctly and is the difference between code that works on one platform and code that works everywhere.
- The loop terminates naturally because `DirectoryInfo.Parent` returns `null` at the drive root, and it throws a descriptive exception rather than returning `null` for a caller to trip over later.

A hardcoded absolute path is a maintenance bug that behaves like a working feature right up until someone else clones the repo. Configuration, discovery, or a relative path — anything but a literal drive letter.

---

## What a Callback Actually Is

A callback is a delegate you hand to a method so it can call *you* back at points it chooses. Inversion of control at its smallest scale: the receiving method owns *when*, the caller owns *what happens*.

```csharp
private static async void StartSearch()
{
	string searchPath = Path.Combine(FindSolutionRoot(), "CSharp.Ch06.DelegatesEventsAndExceptions");
	await Search(".cs", searchPath, Callback, Callback2);
}
```

```csharp
private static async Task Search(string searchTerm, string directory, Action<int> callback, Action callback2)
{
	if (!Directory.Exists(directory))
		throw new DirectoryNotFoundException($"Failed to locate directory [{directory}]!");

	int matchedFiles = 0;

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

`Search()` doesn't know or care what `Callback` and `Callback2` actually do. It knows only their signatures — `Action<int>` and `Action` — and it calls them at the two moments that make sense: once per match, once at the end.

That separation is what makes `Search()` reusable. Progress reporting, logging, cancellation checks, UI updates — none of that has to be written into the search logic, because the caller supplies it. Swap in different callbacks and you get different behavior from the identical method.

Note the two callbacks have deliberately different shapes:

- `Action<int>` receives data (the running count). The search knows something the caller wants to know.
- `Action` receives nothing. It's a pure notification — "done."

Choosing the right signature is the actual design work in a callback API. Pass what the caller genuinely needs and nothing else.

### The two callbacks

```csharp
private static void Callback(int count)
{
	Console.Clear();
	Console.WriteLine($"Found {count} files...");
	Thread.Sleep(1000);
}
```

```csharp
private static void Callback2()
{
	if (Files == null || Files.Count == 0)
	{
		Console.WriteLine("No files found!");
		return;
	}

	Console.WriteLine($"{Environment.NewLine}Files:");

	foreach (string name in Files)
	{
		Console.WriteLine($"{Environment.NewLine}{name}");
	}
}
```

Run this and watch `Callback` clear the console and print a running count for every `.cs` file found — the one-second `Thread.Sleep` is there deliberately so you can watch it happen one file at a time — then `Callback2` prints the final list once the search completes.

That `Thread.Sleep(1000)` is worth pausing on for a different reason. It's inside the *callback*, but it blocks the *search*. `Search()` cannot proceed to the next file until `callback(matchedFiles)` returns. A slow callback makes the whole operation slow, and a callback that throws takes down the method that invoked it. This is exactly what the Microsoft guidance below means by "you are executing arbitrary code."

---

## Worth Reading: The Embedded Design Guidance

The chapter notes cite Microsoft's actual framework design guidelines on callbacks. The short version:

**Prefer events over plain callbacks where either would work.** Events are more discoverable, integrate with IDE and designer tooling, and communicate optionality clearly — a callback parameter looks required, an event obviously isn't.

**Prefer `Func<...>` / `Action<...>` over a custom delegate type** when defining a callback-based API. This project follows its own advice: `Action<int>` and `Action`, not `delegate void SearchProgressHandler(int count)`. Callers don't have to learn a new type name to use the method.

**Understand `Expression<...>` versus `Func<...>`.** They're logically similar, but a `Func<...>` is compiled code intended to run in-process, while an `Expression<...>` is a data structure describing the code — it can be inspected, serialized, translated, and evaluated elsewhere. That's how Entity Framework turns a lambda into SQL. It also costs more, so the guidance is to measure before choosing it.

**Understand that calling a delegate means executing arbitrary code.** It may be slow, it may throw, it may re-enter the calling object, it may have security implications. Be deliberate about where in your method that's allowed to happen — particularly if you're holding a lock or partway through mutating shared state.

---

## A Note on `async void`

```csharp
private static async void StartSearch()
```

`StartSearch` is `async void`, which is worth flagging rather than copying. An `async void` method returns nothing awaitable, so `Main()` cannot wait for it — the call returns at the first `await` and execution continues. Worse, an exception thrown inside an `async void` method cannot be caught by the caller's `try`/`catch`; it's raised on the synchronization context and typically terminates the process.

The demo works because `GenericFunctions.Pause()` immediately follows the call and holds the console open. Note also that `Search()` is declared `async Task` but contains no `await` — the work is entirely synchronous, so nothing actually yields.

The rule: `async void` is acceptable only for event handlers, where the signature is imposed on you. Everywhere else, return `Task`. Chapter 7 covers this properly.

---

## Takeaways

- A callback is a delegate passed into a method so it can call back at moments of its choosing.
- The receiving method owns *when*; the caller owns *what happens*.
- Design callback signatures around what the caller needs — pass data when there's data, nothing when it's just a notification.
- Prefer `Action`/`Func` over custom delegate types in public APIs.
- Prefer events over callbacks when either would serve.
- Invoking a callback runs code you don't control: it can block, throw, or re-enter. Choose the invocation point deliberately.
- `Expression<...>` is code-as-data for remote evaluation; `Func<...>` is compiled code for local execution.
- Never hardcode absolute paths. Discover them from `AppContext.BaseDirectory` and build them with `Path.Combine`.
- `async void` prevents callers from awaiting or catching. Use it only for event handlers.
