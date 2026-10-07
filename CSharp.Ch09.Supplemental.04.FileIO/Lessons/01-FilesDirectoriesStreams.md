---
title: "Files, Directories, and Streams"
chapter: 9
index: 1
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        string dir  = Path.Combine(Path.GetTempPath(), $"ch09-fileio-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        Console.WriteLine($"Working directory: {dir}\n");

        try
        {
            // --- File and Directory (static helpers) ---
            string filePath = Path.Combine(dir, "laws.txt");
            File.WriteAllText(filePath, "Murphy's Law: Anything that can go wrong will go wrong.");
            Console.WriteLine($"File.Exists: {File.Exists(filePath)}");

            // FileInfo: instance-based; useful when you need several attributes
            // of the same file without three separate filesystem calls.
            var fi = new FileInfo(filePath);
            Console.WriteLine($"FileInfo.Length: {fi.Length} bytes");
            Console.WriteLine($"FileInfo.Extension: {fi.Extension}");

            // Path methods work purely on strings -- no filesystem access, no exceptions for
            // nonexistent paths.
            Console.WriteLine($"\nPath.GetFileName:    {Path.GetFileName(filePath)}");
            Console.WriteLine($"Path.GetDirectoryName: {Path.GetDirectoryName(filePath)}");
            Console.WriteLine($"Path.ChangeExtension:  {Path.ChangeExtension(filePath, ".md")}");

            string copyPath = Path.Combine(dir, "laws-copy.txt");
            File.Copy(filePath, copyPath);
            string subDir  = Path.Combine(dir, "archive");
            Directory.CreateDirectory(subDir);
            File.Move(copyPath, Path.Combine(subDir, "laws-copy.txt"));
            Console.WriteLine($"\nFiles in {Path.GetFileName(dir)}/:");
            foreach (var f in Directory.GetFiles(dir))
                Console.WriteLine($"  {Path.GetFileName(f)}");
            Console.WriteLine($"Subdirectories:");
            foreach (var d in Directory.GetDirectories(dir))
                Console.WriteLine($"  {Path.GetFileName(d)}/");

            // --- Streams (raw bytes) ---
            string binPath = Path.Combine(dir, "raw.bin");
            byte[] data = Encoding.UTF8.GetBytes("Streamed as raw bytes.");

            using (var fs = new FileStream(binPath, FileMode.Create, FileAccess.Write))
                fs.Write(data, 0, data.Length);

            using (var fs = new FileStream(binPath, FileMode.Open, FileAccess.Read))
            {
                var buf = new byte[fs.Length];
                int n = fs.Read(buf, 0, buf.Length);
                Console.WriteLine($"\nFileStream read {n} bytes: \"{Encoding.UTF8.GetString(buf)}\"");
            }

            // MemoryStream: same Stream API, no file. Useful when a library needs a Stream
            // but you don't want to write to disk.
            using var ms = new MemoryStream();
            ms.Write(data, 0, data.Length);
            Console.WriteLine($"MemoryStream holds {ms.Length} bytes in memory, no file.");

            // --- Readers and Writers (text and typed binary) ---
            string txtPath = Path.Combine(dir, "lines.txt");
            using (var w = new StreamWriter(txtPath))
            {
                w.WriteLine("Line one."); w.WriteLine("Line two.");
            }
            using (var r = new StreamReader(txtPath))
            {
                Console.WriteLine("\nStreamReader line by line:");
                string line;
                while ((line = r.ReadLine()) != null)
                    Console.WriteLine($"  {line}");
            }

            // BinaryWriter/BinaryReader: typed values in native binary representation.
            // Must read back in the EXACT same order and type they were written.
            // No self-describing structure -- compare to JSON/XML in Supplemental 05.
            string binTxt = Path.Combine(dir, "typed.bin");
            using (var bw = new BinaryWriter(File.Open(binTxt, FileMode.Create)))
            {
                bw.Write(42); bw.Write(3.14); bw.Write("Murphy's Law");
            }
            using (var br = new BinaryReader(File.Open(binTxt, FileMode.Open)))
            {
                Console.WriteLine($"\nBinaryReader: {br.ReadInt32()}, {br.ReadDouble()}, \"{br.ReadString()}\"");
            }
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
            Console.WriteLine("\nTemp directory cleaned up.");
        }
    }
}
```
