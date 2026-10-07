---
title: "StringBuilder vs. Concatenation"
chapter: 4
index: 26
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Text;

internal static class Program
{
    private static readonly string Letters = "ABCDEFGH";

    private static long Factorial(long n)
    {
        long result = 1;
        for (int i = 2; i <= n; i++) result *= i;
        return result;
    }

    private static void ConcatenatePermutations(ref string permutations, string letters, string word)
    {
        if (letters.Length == 0)
        {
            permutations += word + Environment.NewLine;
        }
        else
        {
            for (int i = 0; i < letters.Length; i++)
            {
                char ch = letters[i];
                string newWord = word + ch;
                string newLetters = letters.Remove(i, 1);
                ConcatenatePermutations(ref permutations, newLetters, newWord);
            }
        }
    }

    private static void StringBuilderPermutations(StringBuilder permutations, string letters, string word)
    {
        if (letters.Length == 0)
        {
            permutations.AppendLine(word);
        }
        else
        {
            for (int i = 0; i < letters.Length; i++)
            {
                char ch = letters[i];
                string newWord = word + ch;
                string newLetters = letters.Remove(i, 1);
                StringBuilderPermutations(permutations, newLetters, newWord);
            }
        }
    }

    private static void Main()
    {
        Console.WriteLine($"Generating all {Factorial(Letters.Length):N0} permutations of \"{Letters}\"...");

        var sw = Stopwatch.StartNew();
        string concatenated = "";
        ConcatenatePermutations(ref concatenated, Letters, "");
        sw.Stop();
        Console.WriteLine($"String concatenation: {sw.ElapsedMilliseconds} ms");

        sw.Restart();
        var builder = new StringBuilder();
        StringBuilderPermutations(builder, Letters, "");
        sw.Stop();
        Console.WriteLine($"StringBuilder: {sw.ElapsedMilliseconds} ms");
    }
}
```
