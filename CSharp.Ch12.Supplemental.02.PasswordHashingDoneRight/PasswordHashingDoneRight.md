# Password Hashing Done Right

## Introduction

The main lesson hashed a password directly with SHA-256, purely to show how hashing works, and warned that this isn't actually safe for real password storage. This lesson explains why, with a measured demonstration, and shows what to do instead.

---

## The Problem, Measured

```csharp
// Time how many SHA-256 hashes a single CPU core can compute per second
```

SHA-256 is fast, that's the whole point of it for most uses. But "fast" means an attacker with a stolen database of password hashes can try an enormous number of guesses per second against every password in it. The exact same speed that makes SHA-256 great for checking file integrity makes it a poor choice for protecting passwords.

---

## Why You Need a Salt

```csharp
// Two different users, same password, no salt:
hash("Summer2026!") == hash("Summer2026!")   // identical, every time
```

Without something unique mixed in per user, two people with the same password get the exact same hash. That's bad for two reasons: an attacker can instantly see who shares a password, and precomputed "rainbow tables" of common password hashes crack every match in the database at once.

A **salt** is a random value, unique to each user, mixed in before hashing. With it, identical passwords produce completely different hashes, and rainbow tables stop working.

---

## The Right Algorithm: PBKDF2

```csharp
var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
byte[] hash = pbkdf2.GetBytes(32);
```

PBKDF2 is built specifically to be *slow*, and you control exactly how slow with the `iterations` number. A real login only does this once, so a well-chosen delay is unnoticeable to a real user. But an attacker trying millions of guesses feels that same delay multiplied millions of times over, turning a fast attack into an impractically slow one.

**What actually gets stored**: the salt, the resulting hash, and the iteration count. Never the password itself.

**Checking a login**: re-run the same derivation with the attempted password and the stored salt/iterations, then compare the result to the stored hash.

---

## A Smaller, Related Detail: Comparing Hashes Safely

```csharp
// Bad: exits early on the first mismatch, timing can leak information
if (a[i] != b[i]) return false;

// Good: always checks every byte, takes the same time regardless
difference |= a[i] ^ b[i];
```

A normal comparison stops as soon as it finds a difference, which means it runs very slightly faster or slower depending on *where* the mismatch is. In theory, that tiny timing difference could be measured and exploited. The fix is a comparison that always checks every single byte no matter what, so it always takes the same amount of time.

---

## Try It Yourself

Run `WhyFastHashingIsDangerous()` and note the hashes-per-second number, then compare it mentally against how many total password guesses an attacker would realistically want to try. That gap is exactly why PBKDF2's deliberate slowness matters.
