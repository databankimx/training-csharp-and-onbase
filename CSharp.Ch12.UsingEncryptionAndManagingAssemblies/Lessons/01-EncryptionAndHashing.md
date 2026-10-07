---
title: "Symmetric and Asymmetric Encryption"
chapter: 12
index: 1
dependencies: []
---

```csharp
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        // --- Symmetric encryption (AES): same key encrypts and decrypts ---
        // Aes.Create() generates a cryptographically random Key and IV automatically.
        // Never reuse the same Key+IV pair for more than one message -- doing so
        // can leak plaintext information even without recovering the key.
        const string plaintext = "The shipment arrives Tuesday at 3 PM.";
        using var aes = Aes.Create();

        byte[] encrypted;
        using (var encryptor = aes.CreateEncryptor())
        using (var ms = new MemoryStream())
        {
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cs))
                writer.Write(plaintext);
            encrypted = ms.ToArray();
        }

        string decrypted;
        using (var decryptor = aes.CreateDecryptor())
        using (var ms = new MemoryStream(encrypted))
        using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
        using (var reader = new StreamReader(cs))
            decrypted = reader.ReadToEnd();

        Console.WriteLine("=== Symmetric (AES) ===");
        Console.WriteLine($"Original:  {plaintext}");
        Console.WriteLine($"Encrypted: {Convert.ToBase64String(encrypted)}");
        Console.WriteLine($"Decrypted: {decrypted}");
        Console.WriteLine("Both parties need the SAME key -- the \"key distribution problem.\"");

        // --- Asymmetric encryption (RSA): public key encrypts, private key decrypts ---
        // Solves key distribution: the public key really can be public.
        // But RSA is slow and has a hard size limit (~key size minus padding overhead).
        // Real systems use RSA to encrypt a random AES key, then AES for the actual data
        // -- that's "hybrid encryption."
        //
        // OaepSHA1 (not OaepSHA256): on .NET Framework RSA.Create() returns an
        // RSACryptoServiceProvider (CAPI-based) which only supports OaepSHA1.
        // Using SHA-1 for OAEP's internal padding is still acceptable -- SHA-1's
        // weakness is in collision attacks (relevant to signatures), not OAEP padding.
        const string message = "Meet at the usual place.";
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using var rsa = RSA.Create();
        RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: false);

        using var rsaPublicOnly = RSA.Create();
        rsaPublicOnly.ImportParameters(publicKey);

        byte[] rsaEncrypted = rsaPublicOnly.Encrypt(messageBytes, RSAEncryptionPadding.OaepSHA1);
        byte[] rsaDecrypted = rsa.Decrypt(rsaEncrypted, RSAEncryptionPadding.OaepSHA1);

        Console.WriteLine("\n=== Asymmetric (RSA) ===");
        Console.WriteLine($"Original:  {message}");
        Console.WriteLine($"Encrypted: {Convert.ToBase64String(rsaEncrypted)}");
        Console.WriteLine($"Decrypted: {Encoding.UTF8.GetString(rsaDecrypted)}");
        Console.WriteLine("Public key encrypted, private key decrypted -- key distribution problem solved.");

        // --- Stream encryption: CryptoStream chained directly onto FileStream ---
        // Data flows through the cipher a chunk at a time -- no need to load the
        // whole file into memory. Matters for multi-gigabyte files.
        string plainPath = Path.Combine(Path.GetTempPath(), $"ch12-plain-{Guid.NewGuid():N}.txt");
        string encPath   = Path.Combine(Path.GetTempPath(), $"ch12-enc-{Guid.NewGuid():N}.bin");
        string decPath   = Path.Combine(Path.GetTempPath(), $"ch12-dec-{Guid.NewGuid():N}.txt");
        try
        {
            File.WriteAllText(plainPath, "This file never loads into memory as one plaintext blob.");
            using var aes2 = Aes.Create();
            using (var src = File.OpenRead(plainPath))
            using (var dst = File.Create(encPath))
            using (var cs  = new CryptoStream(dst, aes2.CreateEncryptor(), CryptoStreamMode.Write))
                src.CopyTo(cs);
            using (var src = File.OpenRead(encPath))
            using (var cs  = new CryptoStream(src, aes2.CreateDecryptor(), CryptoStreamMode.Read))
            using (var dst = File.Create(decPath))
                cs.CopyTo(dst);
            Console.WriteLine($"\n=== Stream Encryption ===");
            Console.WriteLine($"Encrypted file: {new FileInfo(encPath).Length} bytes");
            Console.WriteLine($"Decrypted: {File.ReadAllText(decPath)}");
        }
        finally
        {
            foreach (var p in new[] { plainPath, encPath, decPath })
                if (File.Exists(p)) File.Delete(p);
        }

        // --- Hashing: one-way fingerprint, no key, no reverse operation ---
        // The avalanche effect: one character different -> completely different hash.
        // SHA256.HashData() is .NET 5+ only; ComputeHash() is net48-compatible.
        // Convert.ToHexString() is also .NET 5+; BitConverter.ToString() is the fallback.
        // Note: hashing a plain password directly (as here) is NOT correct password storage.
        // See Supplemental.02.PasswordHashingDoneRight.
        const string original = "password123";
        const string tampered = "password124";
        using var sha256 = SHA256.Create();
        string originalHex = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(original))).Replace("-", "");
        string tamperedHex = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(tampered))).Replace("-", "");
        Console.WriteLine("\n=== Hashing (SHA-256) ===");
        Console.WriteLine($"\"{original}\" -> {originalHex}");
        Console.WriteLine($"\"{tampered}\" -> {tamperedHex}");
        Console.WriteLine("One character changed, completely different hash -- the avalanche effect.");
        Console.WriteLine("Hashing is one-way: no key, no \"unhash\" operation.");
    }
}
```
