# Callbacks

## Introduction

A callback is a method passed as an argument to another method, so that method can "call back" into your code at the right moment. In C#, that means passing a delegate.

---

## What This Project Does

It searches a folder for `.cs` files, and uses two callbacks to report progress:

```csharp
await Search(".cs", searchPath, Callback, Callback2);
```

- `Callback` fires once per matching file found, printing a running count.
- `Callback2` fires once, after the search finishes, printing the full list of files found.

---

## Passing the Callbacks

```csharp
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

`Search()` takes both callbacks as parameters (`Action<int>` and `Action`), so it doesn't need to know anything about what happens when a file is found or when the search finishes, it just calls the delegate at the right moment and lets the caller decide what that means.

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
    Console.WriteLine($"{Environment.NewLine}Files:");
    foreach (string name in Files) Console.WriteLine($"{Environment.NewLine}{name}");
}
```

---

## Callbacks vs. Events

Both let one piece of code trigger another. Microsoft's own framework design guidelines recommend leaning toward **events** over plain callbacks when either would work, events are more familiar to a broader range of developers and show up in IDE tooling (IntelliSense, the Properties window in a Designer). Reach for a callback specifically when you want lightweight, one-off customization without the ceremony of declaring an event.

When you do define a callback-based API, prefer the built-in `Func<...>`/`Action<...>` delegate types over declaring a custom delegate, unless you have a specific reason to name your own (readability, a very particular signature convention).

---

## Try It Yourself

Run the project and watch the console clear and update once per matching file, that pause between each one (`Thread.Sleep(1000)`) is there specifically so you can watch the callback fire multiple times rather than all at once. Then notice the final list only appears after the loop finishes, since `callback2()` is only called once, at the very end.
