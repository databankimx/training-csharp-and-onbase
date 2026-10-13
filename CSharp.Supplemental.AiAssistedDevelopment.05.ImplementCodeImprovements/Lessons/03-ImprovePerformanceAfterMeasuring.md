---
title: "Improve Performance After Measuring"
chapter: 0
index: 3
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        Measure("String concatenation", BuildWithConcatenation);
        Measure("StringBuilder", BuildWithStringBuilder);
    }

    private static string BuildWithConcatenation()
    {
        string result = string.Empty;
        for (int i = 0; i < 5000; i++)
            result += i.ToString() + ",";
        return result;
    }

    private static string BuildWithStringBuilder()
    {
        var builder = new StringBuilder();
        for (int i = 0; i < 5000; i++)
            builder.Append(i).Append(',');
        return builder.ToString();
    }

    private static void Measure(string name, Func<string> action)
    {
        var sw = Stopwatch.StartNew();
        string result = action();
        sw.Stop();
        Console.WriteLine($"{name}: {result.Length} chars in {sw.Elapsed.TotalMilliseconds:F3} ms");
    }
}
```
