# Chapter 12 Supplemental 02: Password Hashing Done Right

## What This Is

The main lesson's `HashingData()` deliberately hashed a plain password directly with SHA-256, purely to illustrate the avalanche effect, and explicitly flagged that this is not how real systems should store passwords. This Supplemental explains exactly why, with a concrete, measured demonstration, and shows the actual right way: a per-user random salt, and a deliberately slow, purpose-built algorithm, not a fast general-purpose hash at all.

---

## Why Fast Is Dangerous, Measured Directly

```csharp
var stopwatch = Stopwatch.StartNew();
for (int i = 0; i < iterations; i++) { sha256.ComputeHash(input); }
stopwatch.Stop();
double hashesPerSecond = iterations / stopwatch.Elapsed.TotalSeconds;
```

This doesn't just claim SHA-256 is fast, it measures it, on a single ordinary CPU core, no special hardware. That number is exactly what an attacker with a stolen database of SHA-256 password hashes could try per second against *every* password in that database simultaneously (a dictionary or brute-force attack). The property that makes SHA-256 excellent for integrity checking (see the main lesson), being fast and cheap to compute, is precisely the property that makes it dangerous for password storage specifically.

---

## Why Salting Matters

```csharp
byte[] hash1 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));
byte[] hash2 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));
// hash1 == hash2, always, without a salt
```

Two different users who happen to choose the same password get *identical* hashes without a salt. This has two separate, real consequences: an attacker who leaks the database instantly knows every pair of users sharing a password, and a "rainbow table" (a giant, precomputed hash-to-plaintext lookup table for common passwords) cracks *every* matching hash in the database at once, since the same table works against every user who chose that password.

A **salt**, a random value unique per user, mixed into the input before hashing, fixes both problems: identical passwords now hash to completely different results, and a precomputed rainbow table becomes useless against this database specifically (an attacker would need a separate table per salt, defeating the entire point of precomputing one).

---

## The Actual Right Way: PBKDF2

```csharp
byte[] salt = GenerateSalt();   // random, unique per user, stored alongside the hash
using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
byte[] hash = pbkdf2.GetBytes(32);
```

`Rfc2898DeriveBytes` is .NET's built-in implementation of PBKDF2 (Password-Based Key Derivation Function 2), a purpose-built algorithm specifically designed to be slow, deliberately, and *configurably* so, via the `iterations` parameter. More iterations means more computational work per hash attempt, for both a legitimate login and an attacker's brute-force guess alike. This is the opposite design goal from SHA-256's, which is optimized to be as fast as possible.

**Registration** stores three things: the salt, the derived hash, and the iteration count used, never the original password. **Login** re-derives a hash from the attempted password using that same stored salt and iteration count, then compares the result against the stored hash. The original password only ever exists transiently, in memory, at the moment of derivation, it's never written anywhere.

Worth internalizing the actual tradeoff this buys: a legitimate login does exactly *one* derivation and barely notices a well-tuned iteration count's delay (tens to low hundreds of milliseconds), while an attacker trying millions of guesses feels that same per-guess cost multiplied out to an impractical total. The main lesson's password-hashing example, timed in `WhyFastHashingIsDangerous()` above, makes this contrast concrete: the same iteration count that's imperceptible for one login becomes a real, meaningful barrier at brute-force scale.

---

## A Related, Narrower Concern: Constant-Time Comparison

```csharp
private static bool ConstantTimeEquals(byte[] a, byte[] b)
{
    if (a.Length != b.Length) return false;
    int difference = 0;
    for (int i = 0; i < a.Length; i++)
    {
        difference |= a[i] ^ b[i];
    }
    return difference == 0;
}
```

A naive comparison (`Enumerable.SequenceEqual()`, or a hand-written loop that returns `false` the moment it finds a mismatched byte) runs measurably faster when a mismatch occurs early and measurably slower when more leading bytes happen to match. In theory, an attacker precise enough to measure that timing difference, a genuinely hard but not impossible attack, over a fast local network, or with enough repeated attempts averaged together, could exploit it to guess a hash one byte at a time rather than needing to guess the whole thing simultaneously.

The fix: always examine every byte, unconditionally, regardless of whether an earlier byte already mismatched, so the comparison takes the same amount of time no matter where (or whether) a difference occurs. The `|=` (OR-accumulate) pattern above does exactly that, no branch exists that could exit early based on the data being compared. Worth knowing modern .NET has a built-in for this exact purpose, `CryptographicOperations.FixedTimeEquals()`, added in .NET Core 2.1, not available on net48 (this project's target, and the rest of this training set's), which is why this Supplemental hand-writes the equivalent instead, a good exercise in understanding what that built-in actually does under the hood.
