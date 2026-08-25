# File I/O

## Introduction

This lesson covers working with files and directories in C#: creating and inspecting files, reading and writing raw bytes, reading and writing text and typed binary data, and doing all of it asynchronously. Everything runs against a temporary folder created just for this lesson and cleaned up automatically when it's done.

---

## Files and Directories

```csharp
File.WriteAllText(filePath, "Hello!");
bool exists = File.Exists(filePath);

var fileInfo = new FileInfo(filePath);
long size = fileInfo.Length;
```

`File` and `Directory` give you static methods for one-off operations. `FileInfo`/`DirectoryInfo` are useful when you need several pieces of information about the same file, create one, then check its length, extension, or last-write time without separate calls each touching the filesystem again.

```csharp
Path.GetFileName(filePath);
Path.GetExtension(filePath);
Path.ChangeExtension(filePath, ".md");
```

`Path` works purely on the text of a file path, it never touches the actual filesystem. That's different from everything else in this lesson, which all does.

---

## Streams: Just Bytes

```csharp
using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
fileStream.Write(data, 0, data.Length);
```

A `Stream` is the most basic building block: a sequence of bytes, nothing more. `FileStream` connects that to an actual file; `MemoryStream` connects it to memory instead, for when you need something that behaves like a file without actually being one.

---

## Readers and Writers: Streams That Understand Text or Types

```csharp
// Text
using var writer = new StreamWriter(path);
writer.WriteLine("Hello!");

// Typed binary values
using var writer = new BinaryWriter(File.Open(path, FileMode.Create));
writer.Write(42);
writer.Write("some text");
```

`StreamReader`/`StreamWriter` handle text, converting characters to and from bytes for you. `BinaryReader`/`BinaryWriter` handle specific typed values directly in binary form. Binary I/O is compact and fast, but has no built-in structure describing what's stored where, you have to read values back in exactly the order and type they were written, unlike a format like JSON that describes its own shape.

---

## Doing All of This Asynchronously

```csharp
using var writer = new StreamWriter(filePath);
await writer.WriteAsync("content");

using var reader = new StreamReader(filePath);
string content = await reader.ReadToEndAsync();
```

Disk access is slow enough that it's worth not blocking your program while it happens, exactly the situation `async`/`await` is built for. One thing worth knowing if you've used newer .NET before: `File.WriteAllTextAsync()`/`File.ReadAllTextAsync()` don't exist in classic .NET Framework (this project's target), those were added later, in .NET Core. `StreamWriter.WriteAsync()`/`StreamReader.ReadToEndAsync()` do the same job and have been around since .NET 4.5.

---

## Try It Yourself

Run the project and watch the console print the actual temporary directory path it's using. While the program is paused (waiting for you to press Enter), open that folder in File Explorer and look at the files it's created, `laws.txt`, `stream-demo.bin`, `reader-writer-demo.txt`, and the rest, they're real files on your actual disk for as long as the program is running.
