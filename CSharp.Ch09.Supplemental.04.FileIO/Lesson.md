# Chapter 9 Supplemental 04: File I/O

## What This Is

Database access (S01-S03) deals with structured, server-managed persistent data. This project deals with the file system directly - reading and writing data as files on disk. The two approaches are complementary: a file is often where data starts (an import) or ends up (an export), and the in-memory collections from the main lesson are almost always the intermediate stop.

What's being layered here is the abstraction stack from raw bytes up to useful data:

- **Files and Directories** - `File`/`Directory` (static utility methods) and `FileInfo`/`DirectoryInfo` (instance-based, useful when you need several pieces of information about the same path)
- **Streams** - a `Stream` is a sequence of bytes; `FileStream` reads/writes a file's raw bytes; `MemoryStream` does the same against an in-memory buffer
- **Readers and Writers** - wrap a `Stream` to work with something more useful than raw bytes: `StreamReader`/`StreamWriter` for text, `BinaryReader`/`BinaryWriter` for typed values
- **Asynchronous I/O** - every one of the above has async counterparts, because disk I/O is exactly the "waiting on something slow" scenario `async`/`await` exists for

Each layer adds meaning. A `FileStream` knows about bytes. A `StreamReader` knows about characters and lines. `File.ReadAllText()` knows about the entire file as a string. Choose the layer that matches what you actually need.

Everything here runs against a temporary working directory created on startup and deleted on exit. Run it as many times as you want - no leftover state.

---

## How to Write This Program

`Main()` is `async Task` because one of the lesson methods is async. The working directory is created at the start and deleted in `finally`:

```csharp
private static async Task Main()
{
    string workingDirectory = Path.Combine(
        Path.GetTempPath(), $"ch09-fileio-demo-{Guid.NewGuid():N}");

    try
    {
        Directory.CreateDirectory(workingDirectory);
        Console.WriteLine($"Working directory: {workingDirectory}");
        GenericFunctions.Pause();

        UsingFilesAndDirectories(workingDirectory);   GenericFunctions.Pause();
        UsingStreams(workingDirectory);                GenericFunctions.Pause();
        UsingReadersAndWriters(workingDirectory);      GenericFunctions.Pause();
        await UsingAsyncIo(workingDirectory);          GenericFunctions.Pause();
    }
    catch (Exception ex)
    {
        new DatabankException("Error Caught!", ex).Log();
        GenericFunctions.Pause();
    }
    finally
    {
        if (Directory.Exists(workingDirectory))
            Directory.Delete(workingDirectory, recursive: true);
        GenericFunctions.Pause(final: true);
    }
}
```

Build and confirm it creates and deletes the directory without crashing.

---

### Mini-Program 1: Files and Directories

Write `UsingFilesAndDirectories(string workingDirectory)`:

```csharp
private static void UsingFilesAndDirectories(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "laws.txt");

    File.WriteAllText(filePath, "Murphy's Law: Anything that can go wrong will go wrong.");
    Console.WriteLine($"File.Exists: {File.Exists(filePath)}");

    var fileInfo = new FileInfo(filePath);
    Console.WriteLine($"Length: {fileInfo.Length} bytes");
    Console.WriteLine($"Extension: {fileInfo.Extension}");

    Console.WriteLine($"\nPath.GetFileName: {Path.GetFileName(filePath)}");
    Console.WriteLine($"Path.GetDirectoryName: {Path.GetDirectoryName(filePath)}");
    Console.WriteLine($"Path.GetExtension: {Path.GetExtension(filePath)}");
    Console.WriteLine($"Path.ChangeExtension to .md: {Path.ChangeExtension(filePath, ".md")}");

    string copyPath = Path.Combine(workingDirectory, "laws-copy.txt");
    File.Copy(filePath, copyPath);
    Console.WriteLine($"\nCopied to: {Path.GetFileName(copyPath)}");

    string subDirectory = Path.Combine(workingDirectory, "archive");
    Directory.CreateDirectory(subDirectory);
    string movedPath = Path.Combine(subDirectory, "laws-copy.txt");
    File.Move(copyPath, movedPath);
    Console.WriteLine($"Moved into: {Path.GetFileName(subDirectory)}/");

    Console.WriteLine($"\nDirectory.GetFiles(workingDirectory):");
    foreach (string file in Directory.GetFiles(workingDirectory))
        Console.WriteLine($" - {Path.GetFileName(file)}");

    Console.WriteLine($"\nDirectory.GetDirectories(workingDirectory):");
    foreach (string dir in Directory.GetDirectories(workingDirectory))
        Console.WriteLine($" - {Path.GetFileName(dir)}/");
}
```

Call it from `Main()` and run it.

`File` and `Directory` are static helper classes - everything you need for common operations without managing any objects yourself. `FileInfo` and `DirectoryInfo` are instance-based equivalents that cache metadata; useful when you need `Length`, `LastWriteTime`, and `Extension` from the same file without three separate filesystem calls.

`Path` works purely on the string representation of a path - no filesystem access, no exceptions for nonexistent paths. `Path.ChangeExtension` returns a new string; it doesn't rename anything.

While the program is paused, open the working directory in File Explorer. The files are real and visible.

### Mini-Program 2: Streams

Write `UsingStreams(string workingDirectory)`:

```csharp
private static void UsingStreams(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "stream-demo.bin");
    byte[] data = Encoding.UTF8.GetBytes("Streamed as raw bytes.");

    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
    {
        fileStream.Write(data, 0, data.Length);
    }
    Console.WriteLine($"Wrote {data.Length} raw bytes to {Path.GetFileName(filePath)}");

    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    {
        var buffer = new byte[fileStream.Length];
        int byteCount = fileStream.Read(buffer, 0, buffer.Length);
        Console.WriteLine($"Read {byteCount} bytes back: \"{Encoding.UTF8.GetString(buffer)}\"");
    }

    using var memoryStream = new MemoryStream();
    memoryStream.Write(data, 0, data.Length);
    Console.WriteLine($"\nMemoryStream holds {memoryStream.Length} bytes in memory - no file.");
}
```

Run it. `FileStream` opens files at the byte level - raw bytes in, raw bytes out. The `FileMode` and `FileAccess` enums express intent explicitly in the constructor call.

`MemoryStream` is the same `Stream` abstraction against an in-memory byte buffer. It's useful whenever a library or API expects a `Stream` but you don't actually need a file - for example, processing uploaded file content in a web application without writing it to disk.

Both use `using` - streams hold OS resources and must be closed when done.

### Mini-Program 3: Readers and Writers

Write `UsingReadersAndWriters(string workingDirectory)`:

```csharp
private static void UsingReadersAndWriters(string workingDirectory)
{
    string textPath = Path.Combine(workingDirectory, "reader-writer-demo.txt");

    using (var writer = new StreamWriter(textPath))
    {
        writer.WriteLine("Line one.");
        writer.WriteLine("Line two.");
    }

    using (var reader = new StreamReader(textPath))
    {
        Console.WriteLine("StreamReader, line by line:");
        string line;
        while ((line = reader.ReadLine()) != null)
            Console.WriteLine($" - {line}");
    }

    string binaryPath = Path.Combine(workingDirectory, "reader-writer-demo.bin");

    using (var writer = new BinaryWriter(File.Open(binaryPath, FileMode.Create)))
    {
        writer.Write(42);
        writer.Write(3.14);
        writer.Write("Murphy's Law");
    }

    using (var reader = new BinaryReader(File.Open(binaryPath, FileMode.Open)))
    {
        int intValue    = reader.ReadInt32();
        double dblValue = reader.ReadDouble();
        string strValue = reader.ReadString();
        Console.WriteLine($"\nBinaryReader read back: {intValue}, {dblValue}, \"{strValue}\"");
    }
}
```

Run it. `StreamReader` and `StreamWriter` handle character encoding transparently - UTF-8 by default. `File.ReadAllText()` / `File.WriteAllText()` from Mini-Program 1 are convenience wrappers around these.

`BinaryWriter` writes typed values in their native binary representations - a `double` is exactly 8 bytes, an `int` is 4, a `string` is a length-prefixed UTF-8 sequence. `BinaryReader` must read them back **in exactly the same order and type they were written**. There's no self-describing structure the way JSON or XML provides. Change the write order, get nonsense on read. That's the trade - binary files are compact and fast, but fragile against format changes. Compare this against `Supplemental.05.Serialization`'s JSON/XML approaches, which trade some size and speed for exactly the self-describing structure binary I/O lacks.

### Mini-Program 4: Asynchronous I/O

Write `UsingAsyncIo(string workingDirectory)` as an async method:

```csharp
private static async Task UsingAsyncIo(string workingDirectory)
{
    string filePath = Path.Combine(workingDirectory, "async-demo.txt");

    // .NET Framework has no File.WriteAllTextAsync() - that arrived with .NET Core.
    // StreamWriter.WriteAsync() has been available since .NET 4.5.
    using (var writer = new StreamWriter(filePath))
    {
        await writer.WriteAsync("Written asynchronously.");
    }
    Console.WriteLine($"WriteAsync() wrote to {Path.GetFileName(filePath)}");

    string content;
    using (var reader = new StreamReader(filePath))
    {
        content = await reader.ReadToEndAsync();
    }
    Console.WriteLine($"ReadToEndAsync() read back: \"{content}\"");

    // FileStream.ReadAsync() for lower-level async access.
    // Note: plain "using", not "await using" - FileStream in .NET Framework implements
    // IDisposable, not IAsyncDisposable (which is a .NET Core addition).
    using (var fileStream = new FileStream(
        filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
        bufferSize: 4096, useAsync: true))
    {
        var buffer = new byte[fileStream.Length];
        int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
        string rawContent = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        Console.WriteLine($"\nFileStream.ReadAsync(): \"{rawContent}\"");
    }
}
```

Run it. The behavior is identical to the synchronous versions, but the thread is returned to the pool while the disk I/O completes rather than blocking. For a console demo with one operation this makes no visible difference - the benefit is in a web server handling hundreds of simultaneous requests, where blocking on disk I/O would tie up a thread that could be serving other requests.

Note `useAsync: true` in the `FileStream` constructor - this enables overlapped I/O at the OS level, which is what makes `ReadAsync()` genuinely non-blocking rather than synchronous I/O on a pool thread.

---

## Try It Yourself

While the program is paused between mini-programs, open the working directory in File Explorer and look at the files it created - `laws.txt`, `stream-demo.bin`, `reader-writer-demo.txt`, and the rest are real files on disk. Try opening `stream-demo.bin` in a text editor and compare it to `reader-writer-demo.txt`. Then let the program finish and confirm the directory is cleaned up.

---

## Summary: The Abstraction Stack

| Layer | Class | Works with | When to use |
|---|---|---|---|
| Convenience | `File.ReadAllText` / `WriteAllText` | Strings | Reading/writing a complete text file in one call |
| Text | `StreamReader` / `StreamWriter` | Lines, strings | Line-by-line text processing |
| Binary | `BinaryReader` / `BinaryWriter` | Typed values | Compact binary data with known structure |
| Bytes | `FileStream` | Raw bytes | Maximum control over what's read/written |
| In-memory | `MemoryStream` | Raw bytes | When an API needs a `Stream` but no file is wanted |

---

## Takeaways

- `File` and `Directory` are static helpers for common operations. `FileInfo` and `DirectoryInfo` are instance-based for repeated queries about the same path.
- `Path` methods work purely on strings - no filesystem access, no exceptions for nonexistent paths.
- `FileStream` operates at the byte level. `StreamReader`/`StreamWriter` add text encoding. `BinaryReader`/`BinaryWriter` add typed values.
- `MemoryStream` is the same `Stream` abstraction without a file - useful when an API needs a `Stream` but you don't want to write a file.
- `BinaryReader` must read values back in exactly the same order and type they were written. There's no self-describing structure.
- `useAsync: true` on `FileStream` enables genuine OS-level async I/O, not just blocking I/O on a pool thread.
- .NET Framework lacks `File.WriteAllTextAsync()` / `ReadAllTextAsync()` - use `StreamWriter.WriteAsync()` / `StreamReader.ReadToEndAsync()` instead.
