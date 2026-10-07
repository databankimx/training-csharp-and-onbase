---
title: "XOR Encryption"
chapter: 0
index: 8
dependencies: []
---

```csharp
using System;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        int key      = 30;
        string text  = "Hello World!";

        Console.WriteLine($"Plain Text:     {text}");

        string cipher = EncryptDecrypt(text, key);
        Console.WriteLine($"Encrypted Text: {cipher}");

        string plain = EncryptDecrypt(cipher, key);
        Console.WriteLine($"Decrypted Text: {plain}");

        Console.WriteLine();
        Console.WriteLine("XOR encryption is symmetric - running the same text through again");
        Console.WriteLine("with the same key restores the original.");
        Console.WriteLine("This is a toy example; do not use XOR alone for real security.");
    }

    // XOR each character against the key. Running the output through again with the
    // same key reverses the operation because (a ^ k) ^ k == a.
    private static string EncryptDecrypt(string text, int key)
    {
        var sb = new StringBuilder(text.Length);
        foreach (char c in text)
            sb.Append((char)(c ^ key));
        return sb.ToString();
    }
}
```
