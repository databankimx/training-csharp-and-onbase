---
title: "DllImport - GetShortPathName"
chapter: 4
index: 16
dependencies: []
---

```csharp
using System;
using System.Runtime.InteropServices;

internal static class Program
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern uint GetShortPathName(
        string lpszLongPath, char[] lpszShortPath, int cchBuffer);

    private static void Main()
    {
        // Assembly.GetExecutingAssembly().Location is empty when loaded from
        // an in-memory byte array (as the lesson runner does), so we use
        // AppContext.BaseDirectory which always points to a real path on disk.
        string longName = AppContext.BaseDirectory.TrimEnd('\\', '/');
        char[] buffer = new char[1024];
        uint length = GetShortPathName(longName, buffer, buffer.Length);

        if (length == 0)
        {
            Console.WriteLine($"Long path:  {longName}");
            Console.WriteLine("Short path: (unavailable - 8.3 names may be disabled on this volume)");
        }
        else
        {
            string shortName = new string(buffer, 0, (int)length);
            Console.WriteLine($"Long path:  {longName}");
            Console.WriteLine($"Short path: {shortName}");
        }
    }
}
```
