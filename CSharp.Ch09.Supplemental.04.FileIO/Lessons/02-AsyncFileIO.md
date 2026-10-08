---
title: "Asynchronous File I/O"
chapter: 9
index: 2
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

internal static class Program
{
    // async Task Main() is valid since C# 7.1.
    // The runner wraps it with Task.Run(...).Result, same as Ch07 Supplemental 04.
    private static void Main()
    {
        Task.Run(RunAsync).Wait();
    }

    private static async Task RunAsync()
    {
        string dir = Path.Combine(Path.GetTempPath(), $"ch09-asyncio-{Guid.NewGuid():N}");
        Directory.CreateDirectory(dir);
        Console.WriteLine($"Working directory: {dir}\n");

        try
        {
            string filePath = Path.Combine(dir, "async-demo.txt");

            // .NET Framework has no File.WriteAllTextAsync() -- that arrived with .NET Core.
            // StreamWriter.WriteAsync() has been available since .NET 4.5.
            using (var writer = new StreamWriter(filePath))
                await writer.WriteAsync("Written asynchronously.");
            Console.WriteLine($"WriteAsync() wrote to {Path.GetFileName(filePath)}");

            string content;
            using (var reader = new StreamReader(filePath))
                content = await reader.ReadToEndAsync();
            Console.WriteLine($"ReadToEndAsync(): \"{content}\"");

            // useAsync: true enables OS-level overlapped I/O (genuinely non-blocking).
            // Without it, ReadAsync() calls synchronous I/O on a pool thread instead.
            // Note: plain "using", not "await using" -- FileStream in .NET Framework
            // implements IDisposable, not IAsyncDisposable (that arrived with .NET Core).
            using (var fs = new FileStream(
                filePath, FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                var buf = new byte[fs.Length];
                int n = await fs.ReadAsync(buf, 0, buf.Length);
                Console.WriteLine($"FileStream.ReadAsync(): \"{Encoding.UTF8.GetString(buf, 0, n)}\"");
            }

            Console.WriteLine();
            Console.WriteLine("In a console demo the timings look identical to synchronous I/O.");
            Console.WriteLine("The benefit is in a web server handling hundreds of concurrent requests,");
            Console.WriteLine("where blocking on disk I/O would hold a thread that could serve others.");
        }
        finally
        {
            if (Directory.Exists(dir)) Directory.Delete(dir, recursive: true);
            Console.WriteLine("Temp directory cleaned up.");
        }
    }
}
```
