---
title: "Password Hashing Done Right - PBKDF2, Salting, Constant-Time Comparison"
chapter: 12
index: 1
dependencies: []
---

```csharp
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        // --- Why fast hashing is dangerous for passwords ---
        // SHA-256 is fast by design -- that's the wrong design goal for password storage.
        // An attacker with a stolen database of SHA-256 hashes can try millions of
        // guesses per second against every password in it simultaneously.
        const int iterations = 200_000;
        using var sha256 = SHA256.Create();
        byte[] input = Encoding.UTF8.GetBytes("password123");
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++) sha256.ComputeHash(input);
        sw.Stop();
        double hashesPerSec = iterations / sw.Elapsed.TotalSeconds;
        Console.WriteLine($"=== Why Fast Hashing Is Dangerous ===");
        Console.WriteLine($"{iterations:N0} SHA-256 hashes in {sw.ElapsedMilliseconds:N0} ms = ~{hashesPerSec:N0} hashes/sec.");
        Console.WriteLine("An attacker can try that many password guesses per second against a leaked database.");

        // --- Why salting matters ---
        // Without a salt, identical passwords produce identical hashes everywhere.
        // Consequences: an attacker instantly identifies users sharing a password,
        // and precomputed rainbow tables (hash->plaintext for common passwords) work
        // against every user simultaneously -- one table cracks them all.
        // A random per-user salt makes identical passwords hash differently and
        // renders any precomputed table useless.
        byte[] hash1 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));
        byte[] hash2 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));
        Console.WriteLine($"\n=== Why Salting Matters ===");
        Console.WriteLine($"User A (\"Summer2026!\"): {BitConverter.ToString(hash1).Replace("-", "")}");
        Console.WriteLine($"User B (\"Summer2026!\"): {BitConverter.ToString(hash2).Replace("-", "")}");
        Console.WriteLine($"Identical: {BitConverter.ToString(hash1) == BitConverter.ToString(hash2)}");
        Console.WriteLine("Same password -> same hash. Rainbow tables attack this directly.");

        // --- PBKDF2: the right approach ---
        // Three things stored alongside the hash: salt, iteration count, algorithm.
        // At login: re-derive with the same salt and iteration count, then compare.
        // The original password never touches persistent storage.
        // The iteration count is stored so it can be increased as hardware gets faster --
        // existing hashes remain valid; re-hash on next successful login.
        const int pbkdf2Iterations = 100_000;
        byte[] salt       = GenerateSalt();
        byte[] storedHash = DeriveHash("CorrectHorseBatteryStaple", salt, pbkdf2Iterations);

        Console.WriteLine($"\n=== PBKDF2 (Rfc2898DeriveBytes) ===");
        Console.WriteLine($"Salt:       {Convert.ToBase64String(salt)}");
        Console.WriteLine($"Hash:       {Convert.ToBase64String(storedHash)}");
        Console.WriteLine($"Iterations: {pbkdf2Iterations:N0}  <-- the \"slow\" dial");

        byte[] correctHash = DeriveHash("CorrectHorseBatteryStaple", salt, pbkdf2Iterations);
        byte[] wrongHash   = DeriveHash("TrustNo1",                  salt, pbkdf2Iterations);
        Console.WriteLine($"Correct password: {ConstantTimeEquals(storedHash, correctHash)}");
        Console.WriteLine($"Wrong password:   {ConstantTimeEquals(storedHash, wrongHash)}");
        Console.WriteLine("Legitimate logins do one derivation and barely notice.");
        Console.WriteLine("An attacker trying millions of guesses pays that cost every time.");

        // --- Constant-time comparison ---
        // Naive comparison (SequenceEqual, or a loop that returns false on first mismatch)
        // returns faster the earlier a mismatch occurs. An attacker measuring that timing
        // difference -- hard but possible over a fast network with enough attempts -- could
        // guess a hash one byte at a time rather than the whole thing at once.
        // Fix: always compare every byte, regardless of where a mismatch occurs.
        // Modern .NET: CryptographicOperations.FixedTimeEquals() (added in .NET Core 2.1).
        // net48: hand-roll the XOR-accumulate pattern (as done here).
        Console.WriteLine($"\n=== Constant-Time Comparison ===");
        byte[] a = Encoding.UTF8.GetBytes("expected-hash-value");
        byte[] b = Encoding.UTF8.GetBytes("expected-hash-value");
        Console.WriteLine($"ConstantTimeEquals: {ConstantTimeEquals(a, b)}");
        Console.WriteLine("XOR-accumulate: difference |= a[i] ^ b[i]. Any mismatch sets a bit that never clears.");
        Console.WriteLine("No early return -- comparison time is independent of where arrays differ.");
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[16];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private static byte[] DeriveHash(string password, byte[] salt, int iterations)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }

    private static bool ConstantTimeEquals(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        int difference = 0;
        for (int i = 0; i < a.Length; i++)
            difference |= a[i] ^ b[i]; // XOR is 0 on match, non-zero on mismatch; OR never clears a set bit
        return difference == 0;
    }
}
```
