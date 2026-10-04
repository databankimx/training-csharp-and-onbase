---
title: "DllImport - MessageBox"
chapter: 4
index: 15
dependencies: []
---

```csharp
using System;
using System.Runtime.InteropServices;

internal static class Program
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    private static void Main()
    {
        // LESSON_RUNNER_HWND is set by the runner so the dialog appears
        // in front of the runner window rather than behind it.
        IntPtr owner = long.TryParse(
            Environment.GetEnvironmentVariable("LESSON_RUNNER_HWND"),
            out long hwnd) ? new IntPtr(hwnd) : IntPtr.Zero;

        MessageBox(owner, "Hello World!", "Hello Dialog", 0);
        Console.WriteLine("MessageBox closed.");
    }
}
```
