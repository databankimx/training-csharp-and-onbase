#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch12.Supplemental._02.PasswordHashingDoneRight
{
    internal static class Program
    {
        #region Chapter Notes
        /*
         * The main lesson's HashingData() deliberately hashed a plain password directly
         *   with SHA-256, purely to illustrate the avalanche effect, and flagged that this
         *   is NOT how real systems should store passwords. This Supplemental explains
         *   exactly why, and shows the actual right way: a per-user random SALT, and a
         *   deliberately SLOW, purpose-built algorithm (PBKDF2 here, via .NET's built-in
         *   Rfc2898DeriveBytes), not a fast general-purpose hash like SHA-256 at all.
         */
        #endregion

        #region Main Method
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                WhyFastHashingIsDangerous();
                GenericFunctions.Pause();

                WhySaltingMatters();
                GenericFunctions.Pause();

                UsingPbkdf2ForPasswordHashing();
                GenericFunctions.Pause();

                ConstantTimeComparison();
                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Lesson Methods
        // A concrete demonstration of WHY a fast hash is dangerous for passwords
        private static void WhyFastHashingIsDangerous()
        {
            const int iterations = 200_000;

            using var sha256 = SHA256.Create();
            byte[] input = Encoding.UTF8.GetBytes("password123");

            var stopwatch = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                sha256.ComputeHash(input);
            }
            stopwatch.Stop();

            double hashesPerSecond = iterations / stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"Computed {iterations:N0} SHA-256 hashes in {stopwatch.ElapsedMilliseconds:N0} ms.");
            Console.WriteLine($"That's roughly {hashesPerSecond:N0} hashes per second, on a single ordinary CPU core,");
            Console.WriteLine("no special hardware at all. An attacker with a stolen database of SHA-256 password");
            Console.WriteLine("hashes could try that many guesses per second against EVERY password in it");
            Console.WriteLine("simultaneously (a dictionary or brute-force attack), SHA-256 being FAST, exactly");
            Console.WriteLine("the property that makes it good for integrity checking (see the main lesson), is");
            Console.WriteLine("exactly the property that makes it dangerous for password storage specifically.");
        }

        // Without a salt, identical passwords produce identical hashes
        private static void WhySaltingMatters()
        {
            using var sha256 = SHA256.Create();

            byte[] hash1 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));
            byte[] hash2 = sha256.ComputeHash(Encoding.UTF8.GetBytes("Summer2026!"));

            Console.WriteLine("Two DIFFERENT users, both happen to choose the password \"Summer2026!\":");
            Console.WriteLine($"User A's hash: {BitConverter.ToString(hash1).Replace("-", "")}");
            Console.WriteLine($"User B's hash: {BitConverter.ToString(hash2).Replace("-", "")}");
            Console.WriteLine($"Identical: {BitConverter.ToString(hash1) == BitConverter.ToString(hash2)}");
            Console.WriteLine($"{Environment.NewLine}Without a salt, identical passwords ALWAYS produce identical hashes, worth two");
            Console.WriteLine("real, separate consequences: an attacker who leaks the database instantly knows");
            Console.WriteLine("every pair of users who share a password, AND a \"rainbow table\" (a giant, precomputed");
            Console.WriteLine("lookup table of hash -> plaintext for common passwords) can crack EVERY matching");
            Console.WriteLine("hash in the database at once, since the same table works against every user.");
            Console.WriteLine($"{Environment.NewLine}A SALT, a random value unique per user, mixed into the input before hashing, fixes");
            Console.WriteLine("both: even identical passwords now hash to completely different results, and a");
            Console.WriteLine("precomputed rainbow table becomes useless (it would need a separate table per salt).");
        }

        // The actual right way: a purpose-built, deliberately SLOW algorithm (PBKDF2 here),
        //   with a random, per-user salt, both stored alongside the resulting hash
        private static void UsingPbkdf2ForPasswordHashing()
        {
            // "Registering" a new user: generate a random salt, derive a hash from the
            //   password using that salt, store BOTH the salt and the derived hash (never
            //   the original password itself).
            const int iterations = 100_000;
            byte[] salt = GenerateSalt();
            byte[] storedHash = DeriveHash("CorrectHorseBatteryStaple", salt, iterations);

            Console.WriteLine("Registered a new user:");
            Console.WriteLine($"  Salt (stored): {Convert.ToBase64String(salt)}");
            Console.WriteLine($"  Hash (stored): {Convert.ToBase64String(storedHash)}");
            Console.WriteLine($"  Iterations (stored): {iterations:N0}");

            // "Logging in": re-derive a hash from the ATTEMPTED password using the SAME
            //   stored salt and iteration count, then compare against the stored hash.
            //   The original password is never stored anywhere, only ever compared
            //   transiently, in memory, at login time.
            byte[] correctAttemptHash = DeriveHash("CorrectHorseBatteryStaple", salt, iterations);
            byte[] wrongAttemptHash = DeriveHash("TrustNo1", salt, iterations);

            Console.WriteLine($"{Environment.NewLine}Login attempt with the CORRECT password: {ConstantTimeEquals(storedHash, correctAttemptHash)}");
            Console.WriteLine($"Login attempt with an INCORRECT password: {ConstantTimeEquals(storedHash, wrongAttemptHash)}");

            Console.WriteLine($"{Environment.NewLine}PBKDF2 (Rfc2898DeriveBytes here) is deliberately configurable to be SLOW, that");
            Console.WriteLine($"iteration count ({iterations:N0}) is the knob, more iterations means more work per");
            Console.WriteLine("hash attempt, for both a legitimate login AND an attacker's brute-force guess alike.");
            Console.WriteLine("Contrast this against WhyFastHashingIsDangerous() above, that same 200,000-hash");
            Console.WriteLine("benchmark against PBKDF2 at this iteration count would take vastly longer, exactly");
            Console.WriteLine("the point: legitimate logins do ONE derivation and barely notice the delay, while");
            Console.WriteLine("an attacker trying millions of guesses feels that same per-guess cost multiplied out.");
        }

        // Generates a cryptographically random salt
        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        // PBKDF2 via Rfc2898DeriveBytes: password + salt + iteration count -> a derived hash
        private static byte[] DeriveHash(string password, byte[] salt, int iterations)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32);
        }

        // Constant-time comparison: WHY naive comparison is a real (if narrow) risk
        private static void ConstantTimeComparison()
        {
            Console.WriteLine("A naive byte-array comparison (like Enumerable.SequenceEqual(), or a hand-written");
            Console.WriteLine("loop that returns false the MOMENT it finds a mismatched byte) returns slightly");
            Console.WriteLine("FASTER the earlier a mismatch occurs, and slightly SLOWER the more leading bytes");
            Console.WriteLine("happen to match. In theory, an attacker precise enough to measure that timing");
            Console.WriteLine("difference (a genuinely hard but not impossible attack, over a fast local network");
            Console.WriteLine("or with enough repeated attempts) could use it to guess a hash one byte at a time,");
            Console.WriteLine("rather than needing to guess the whole thing at once.");
            Console.WriteLine();

            byte[] a = Encoding.UTF8.GetBytes("expected-hash-value");
            byte[] b = Encoding.UTF8.GetBytes("expected-hash-value");

            bool isEqual = ConstantTimeEquals(a, b);
            Console.WriteLine($"ConstantTimeEquals() result: {isEqual}");
            Console.WriteLine($"{Environment.NewLine}The fix, shown in ConstantTimeEquals() below: always compare EVERY byte, regardless");
            Console.WriteLine("of whether an earlier byte already mismatched, so the comparison takes the same");
            Console.WriteLine("amount of time no matter where (or whether) a mismatch occurs. Worth knowing modern");
            Console.WriteLine(".NET has a built-in for this, CryptographicOperations.FixedTimeEquals(), added in");
            Console.WriteLine(".NET Core 2.1, not available on net48 (this project's target), which is why this");
            Console.WriteLine("Supplemental hand-writes the equivalent instead.");
        }

        // A constant-time byte array comparison: always examines every byte, never
        //   short-circuits on the first mismatch, so the time taken doesn't leak
        //   information about WHERE (or whether) the arrays differ
        private static bool ConstantTimeEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;

            int difference = 0;
            for (int i = 0; i < a.Length; i++)
            {
                #pragma warning disable S125
                // XOR-and-OR-accumulate: this touches every byte unconditionally, no
                //   branch that could exit early depending on the data, unlike an
                //   "if (a[i] != b[i]) return false;" loop would.
                difference |= a[i] ^ b[i];
                #pragma warning restore S125
            }

            return difference == 0;
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
