# Chapter 9 Supplemental 04: File I/O

## What This Is

The "Performing I/O Operations" section of Chapter 9: files and directories, streams, readers and writers, and asynchronous I/O. Fully self-contained, no external database or setup required, everything runs against a temporary working directory this project creates on startup and deletes on exit.

---

## Files and Directories: Two Ways to Work With the Same Thing

```csharp
File.WriteAllText(filePath, "...");
Console.WriteLine(File.Exists(filePath));

var fileInfo = new FileInfo(filePath);
Console.WriteLine(fileInfo.Length);
```

`File` and `Directory` expose static methods, each call independently touches the filesystem. `FileInfo`/`DirectoryInfo` are instance-based instead, worth reaching for specifically when you need several pieces of information about the *same* file (its length, its last-write time, its extension), since creating one `FileInfo` and querying it repeatedly avoids re-touching the filesystem for each piece of information the way calling several different `File.*` static methods would.

```csharp
Console.WriteLine(Path.GetFileName(filePath));
Console.WriteLine(Path.GetExtension(filePath));
Console.WriteLine(Path.ChangeExtension(filePath, ".md"));
```

Worth noticing: every `Path` method here works purely on the *string*, none of them touch the filesystem at all, `Path.GetExtension(@"C:\made\up\path.txt")` returns `.txt` even if that path doesn't exist anywhere. `Path` is for manipulating file path *text*; `File`/`Directory`/`FileInfo`/`DirectoryInfo` are for actually touching the filesystem.

---

## Streams: Raw Bytes, Nothing More

```csharp
using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
fileStream.Write(data, 0, data.Length);
```

A `Stream` is the lowest-level abstraction here, just a sequence of bytes, with no concept of "text" or "typed values" built in. `FileStream` reads/writes those bytes to/from an actual file; `MemoryStream` does the identical thing against an in-memory buffer instead, useful whenever an API expects a `Stream` but you don't actually want a file involved at all.

---

## Readers and Writers: Streams With Meaning Attached

```csharp
// Text
using var writer = new StreamWriter(textPath);
writer.WriteLine("Line one.");

// Typed binary values
using var writer = new BinaryWriter(File.Open(binaryPath, FileMode.Create));
writer.Write(42);
writer.Write(3.14);
writer.Write("Murphy's Law");
```

`StreamReader`/`StreamWriter` wrap a stream and add character encoding, letting you work with text (lines, strings) instead of raw bytes directly. `BinaryReader`/`BinaryWriter` wrap a stream differently, writing typed values (`int`, `double`, `string`) in their native binary layout. Worth internalizing the tradeoff: binary readers/writers are compact and fast, but have **no self-describing structure**, you must read values back in the exact same order and type they were written in, there's nothing in the file itself telling you what comes next. Compare this against Supplemental 05's JSON/XML serialization, which trades some size and speed for exactly the self-describing structure binary I/O lacks.

---

## Asynchronous I/O: The Same Operations, Non-Blocking, With .NET Framework's Actual APIs

```csharp
using var writer = new StreamWriter(filePath);
await writer.WriteAsync("...");
```
```csharp
using var reader = new StreamReader(filePath);
string content = await reader.ReadToEndAsync();
```
```csharp
using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
var buffer = new byte[fileStream.Length];
int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
```

Disk I/O is exactly the kind of "waiting on something slow" scenario `async`/`await` exists for (see Chapter 7's coverage for the underlying mechanics). Worth knowing specifically because it's easy to reach for the wrong API by habit: `File.WriteAllTextAsync()`/`File.ReadAllTextAsync()` don't exist in classic .NET Framework at all, those were only added starting with .NET Core, so on net48 the equivalent is `StreamWriter.WriteAsync()`/`StreamReader.ReadToEndAsync()` instead, both of which have actually been available since .NET 4.5. The same applies to `await using`: `FileStream` in .NET Framework only implements `IDisposable`, not `IAsyncDisposable` (also a .NET Core addition), so disposal here uses a plain `using`, there's no async form of it to await. Worth noticing `Main()` itself is still declared `static async Task Main()`, the modern way to have an async entry point directly, that part works fine on net48, it's specifically the newer *library* APIs (not the language feature) that aren't available.

---

## Worth Noticing: Cleanup, Guaranteed

```csharp
finally
{
    if (Directory.Exists(workingDirectory)) Directory.Delete(workingDirectory, recursive: true);
    ...
}
```

The temporary working directory this project creates is deleted in the `finally` block, guaranteed to run whether the demo completes normally or an exception interrupts it partway through. Worth recognizing as the general pattern for any resource that needs cleanup regardless of how a method exits, matching the same reasoning behind `using` statements for individual streams/readers/writers throughout this file.
