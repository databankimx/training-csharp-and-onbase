# Chapter 12 Supplemental 02: Password Hashing Done Right

## What This Is

The main lesson's `HashingData()` deliberately hashed a plain password directly with SHA-256, purely to illustrate the avalanche effect, and flagged that this is not how real systems should store passwords. This project explains exactly why, then shows the actual right approach: a per-user random salt, and a deliberately slow, purpose-built algorithm (PBKDF2 via `Rfc2898DeriveBytes`), not a fast general-purpose hash at all.

---

## How to Write This Program

### Mini-Program 1: Why Fast Hashing Is Dangerous for Passwords

Clear `Main()` and write:

```csharp
const int iterations = 200_000;
using var sha256 = SHA256.Create();
byte[] input = Encoding.UTF8.GetBytes("password123");

var sw = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++)
    sha256.ComputeHash(input);
sw.Stop();

double hashesPerSecond = iterations / sw.Elapsed.TotalSeconds;
Console.WriteLine($"Computed {iterations:N0} SHA-256 hashes in {sw.ElapsedMilliseconds:N0} ms.");
Console.WriteLine($"Roughly {hashesPerSecond:N0} hashes/second on a single ordinary CPU core.");
Console.WriteLine("\nAn attacker with a stolen database of SHA-256 password hashes could try");
Console.WriteLine("that many guesses per second against every password in it simultaneously.");
Console.WriteLine("\nSHA-256 being FAST -- the property that makes it good for integrity checking");
Console.WriteLine("-- is exactly the property that makes it dangerous for password storage.");

GenericFunctions.Pause();
```

Run it. The number will be in the millions per second on modern hardware. That's not a vulnerability in SHA-256 -- it's working exactly as designed. The design goal of a general-purpose hash is speed. That goal is precisely the wrong goal for password storage.

### Mini-Program 2: Why Salting Matters

Clear `Main()` and write:

```csharp
using var sha256 = SHA256.Create();

byte[] hash1 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));
byte[] hash2 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));

string hex1 = BitConverter.ToString(hash1).Replace("-", "");
string hex2 = BitConverter.ToString(hash2).Replace("-", "");

Console.WriteLine("Two different users, both chose \"Summer2026!\":");
Console.WriteLine($"User A: {hex1}");
Console.WriteLine($"User B: {hex2}");
Console.WriteLine($"Identical: {hex1 == hex2}");
Console.WriteLine("\nWithout a salt, identical passwords always produce identical hashes.");
Console.WriteLine("Two real consequences:");
Console.WriteLine("  1. An attacker who leaks the database instantly knows which users share a password.");
Console.WriteLine("  2. A precomputed \"rainbow table\" (hash -> plaintext for common passwords)");
Console.WriteLine("     cracks every matching hash at once, since the same table works for every user.");
Console.WriteLine("\nA random per-user salt mixed in before hashing fixes both: identical passwords");
Console.WriteLine("hash to different results, and any precomputed table becomes useless.");

GenericFunctions.Pause();
```

Run it. Identical inputs, identical hashes -- every time, on any machine. That predictability is the problem.

### Mini-Program 3: PBKDF2 -- The Actual Right Way

Add helpers above `Main()`:

```csharp
private static byte[] GenerateSalt()
{
    byte[] salt = new byte[16];
    using (var rng = RandomNumberGenerator.Create())
        rng.GetBytes(salt);
    return salt;
}

private static byte[] DeriveHash(string password, byte[] salt, int iterations)
{
    using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
    return pbkdf2.GetBytes(32);
}
```

Clear `Main()` and write:

```csharp
const int iterations = 100_000;

// Registration: generate a random salt, derive a hash, store BOTH.
// Never store the original password.
byte[] salt = GenerateSalt();
byte[] storedHash = DeriveHash("CorrectHorseBatteryStaple", salt, iterations);

Console.WriteLine("Registered a new user:");
Console.WriteLine($"  Salt (stored):       {Convert.ToBase64String(salt)}");
Console.WriteLine($"  Hash (stored):       {Convert.ToBase64String(storedHash)}");
Console.WriteLine($"  Iterations (stored): {iterations:N0}");

// Login: re-derive from the attempted password using the SAME stored salt and
// iteration count, then compare. The original password is never stored anywhere.
byte[] correctHash = DeriveHash("CorrectHorseBatteryStaple", salt, iterations);
byte[] wrongHash   = DeriveHash("TrustNo1", salt, iterations);

Console.WriteLine($"\nCorrect password attempt: {ConstantTimeEquals(storedHash, correctHash)}");
Console.WriteLine($"Wrong password attempt:   {ConstantTimeEquals(storedHash, wrongHash)}");
Console.WriteLine($"\nPBKDF2 is deliberately configurable to be SLOW. That iteration count ({iterations:N0})");
Console.WriteLine("is the knob. More iterations = more work per hash attempt, for both a legitimate");
Console.WriteLine("login and an attacker's brute-force guess. Legitimate logins do one derivation");
Console.WriteLine("and barely notice. An attacker trying millions of guesses feels the cost multiplied out.");

GenericFunctions.Pause();
```

Add the comparison helper (also used in Mini-Program 4):

```csharp
private static bool ConstantTimeEquals(byte[] a, byte[] b)
{
    if (a.Length != b.Length) return false;
    int difference = 0;
    for (int i = 0; i < a.Length; i++)
        difference |= a[i] ^ b[i];
    return difference == 0;
}
```

Run it. Three things are stored alongside the hash: the salt, the iteration count, and the algorithm identifier (implicitly SHA-256 here). At login time, the same salt and iteration count are used to re-derive from the attempted password. The output is compared against the stored hash. The original password never touches persistent storage.

The iteration count is stored alongside the hash specifically so it can be increased as hardware gets faster, without invalidating existing stored hashes -- when a user next logs in successfully, re-hash with the new count and update the stored value.

### Mini-Program 4: Constant-Time Comparison

Clear `Main()` and write:

```csharp
Console.WriteLine("A naive byte comparison (like SequenceEqual(), or a loop that returns false");
Console.WriteLine("the moment it finds a mismatch) returns slightly FASTER the earlier a mismatch");
Console.WriteLine("occurs. An attacker precise enough to measure that timing difference -- hard but");
Console.WriteLine("not impossible over a fast local network with enough repeated attempts -- could");
Console.WriteLine("use it to guess a hash one byte at a time rather than needing to guess the whole");
Console.WriteLine("thing at once.");
Console.WriteLine();

byte[] a = Encoding.UTF8.GetBytes("expected-hash-value");
byte[] b = Encoding.UTF8.GetBytes("expected-hash-value");

bool isEqual = ConstantTimeEquals(a, b);
Console.WriteLine($"ConstantTimeEquals() result: {isEqual}");
Console.WriteLine("\nThe fix: always compare EVERY byte, regardless of whether an earlier byte");
Console.WriteLine("already mismatched, so the time taken is the same no matter where the arrays differ.");
Console.WriteLine("\nModern .NET has CryptographicOperations.FixedTimeEquals() for this (added in");
Console.WriteLine(".NET Core 2.1). Not available on net48, so this Supplemental hand-writes it.");
Console.WriteLine("\nThe implementation uses XOR-accumulate: difference |= a[i] ^ b[i].");
Console.WriteLine("XOR is 0 if the bytes match, non-zero if they differ. OR-accumulating means");
Console.WriteLine("any mismatch anywhere sets a bit that never clears. No early return.");

GenericFunctions.Pause();
```

Run it. The `ConstantTimeEquals` implementation at the bottom of this file is the same one used in Mini-Program 3 -- both use it so the lesson is concrete.

Timing attacks on password hash comparison are a real, documented class of attack. Constant-time comparison is the standard defense. The XOR-accumulate pattern (`difference |= a[i] ^ b[i]`) is the idiomatic implementation: any mismatched byte sets a bit in `difference` via XOR, the OR ensures that bit is never cleared by a later matching byte, and the final `return difference == 0` only returns `true` if no mismatch was ever encountered.

---

## Takeaways

- SHA-256 is fast by design. Fast is the wrong design goal for password storage.
- Without a salt, identical passwords produce identical hashes on every machine. Rainbow tables attack this directly.
- Per-user random salts ensure identical passwords hash to different results and make precomputed tables useless.
- PBKDF2 (via `Rfc2898DeriveBytes`) is deliberately slow and configurable. The iteration count is the dial.
- Store the salt, hash, iteration count, and algorithm identifier. Never store the original password.
- Store the iteration count alongside the hash so it can be increased as hardware gets faster.
- Use constant-time comparison for hash comparison. Naive comparison leaks timing information about where the arrays differ.
- `CryptographicOperations.FixedTimeEquals()` is the built-in for this on modern .NET. Hand-roll it on net48.
