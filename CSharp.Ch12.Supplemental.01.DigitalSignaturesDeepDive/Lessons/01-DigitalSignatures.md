---
title: "Digital Signatures - Sign, Verify, Tamper Detection, HMAC"
chapter: 12
index: 1
dependencies: []
---

```csharp
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

internal static class Program
{
    private static void Main()
    {
        // --- The key reversal: signing vs encryption ---
        // Encryption: PUBLIC key encrypts, PRIVATE key decrypts. Purpose: secrecy.
        // Signing:    PRIVATE key signs,   PUBLIC key verifies.  Purpose: authenticity + integrity.
        // Same key pair, opposite roles. A signed message is still perfectly readable --
        // signing proves authorship and integrity, NOT confidentiality.
        Console.WriteLine("=== Key Role Reversal ===");
        Console.WriteLine("ENCRYPTING: recipient's PUBLIC key encrypts, recipient's PRIVATE key decrypts.");
        Console.WriteLine("  -> Secrecy. Only the recipient can read it.");
        Console.WriteLine("SIGNING:    sender's PRIVATE key signs, sender's PUBLIC key verifies.");
        Console.WriteLine("  -> Authenticity + integrity. The message is still readable by anyone.");
        Console.WriteLine("  SignData hashes the data internally -- RSA signs the hash, not the raw data.");

        // --- Sign and verify ---
        // Anyone with the sender's public key can verify -- no private key needed.
        byte[] data = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");
        using var sender = RSA.Create();
        byte[] signature = sender.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using var verifier = RSA.Create();
        verifier.ImportParameters(sender.ExportParameters(includePrivateParameters: false));
        bool valid = verifier.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        Console.WriteLine($"\n=== Sign and Verify ===");
        Console.WriteLine($"Signature: {Convert.ToBase64String(signature)}");
        Console.WriteLine($"Verified (original data + original signature): {valid}");

        // --- Tamper detection: data changed ---
        // Any change to the data changes its hash, which breaks the signature.
        // An attacker who changes the data cannot produce a valid signature without the private key.
        byte[] tamperedData = Encoding.UTF8.GetBytes("Transfer $500 to account 99999.");
        bool tamperedValid = verifier.VerifyData(tamperedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Console.WriteLine($"\n=== Tamper Detection ===");
        Console.WriteLine($"Verified (tampered data + original signature): {tamperedValid}");

        // --- Tamper detection: signature changed ---
        // Either side being altered breaks verification -- both are bound together.
        byte[] tamperedSig = (byte[])signature.Clone();
        tamperedSig[0] ^= 0xFF; // flip bits in the first byte
        bool sigTamperedValid = verifier.VerifyData(data, tamperedSig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        Console.WriteLine($"Verified (original data + tampered signature): {sigTamperedValid}");
        Console.WriteLine("Both the data AND the signature must arrive exactly as signed.");

        // --- HMAC: symmetric alternative to RSA signing ---
        // Same authenticity + integrity guarantee, much faster, no key pair needed.
        // Tradeoff: both parties need the SAME shared secret key.
        // Use HMAC for internal services that already share a key.
        // Use RSA signatures when the verifier and signer are strangers.
        // RandomNumberGenerator.GetBytes(int) is .NET 6+ -- use Create() + GetBytes() for net48.
        byte[] sharedKey = new byte[32];
        using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(sharedKey);

        using var hmac1 = new HMACSHA256(sharedKey);
        using var hmac2 = new HMACSHA256(sharedKey);
        byte[] mac            = hmac1.ComputeHash(data);
        byte[] recomputedMac  = hmac2.ComputeHash(data);
        bool   hmacValid      = mac.SequenceEqual(recomputedMac);

        Console.WriteLine($"\n=== HMAC (Symmetric Alternative) ===");
        Console.WriteLine($"MAC: {Convert.ToBase64String(mac)}");
        Console.WriteLine($"Verified: {hmacValid}");
        Console.WriteLine("HMAC: faster than RSA, no key pair. Needs shared secret (key distribution problem returns).");
    }
}
```
