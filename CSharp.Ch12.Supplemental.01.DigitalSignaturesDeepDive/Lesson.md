# Chapter 12 Supplemental 01: Digital Signatures Deep Dive

## What This Is

The main lesson used RSA for **encryption** - hiding a message's contents. This project covers RSA's other major job: **signing** - proving a message really came from the holder of a specific private key, and that it wasn't altered after signing. The two use the key pair in genuinely opposite roles, which is the most important thing to get straight before writing either.

What's being extended here is the RSA API surface. The main lesson demonstrated encryption with the public key and decryption with the private key. Signing reverses that: the private key signs and the public key verifies. The same mathematical operation, opposite direction, completely different security guarantee. This reversal trips people up consistently and is worth internalizing directly before moving to code.

---

## How to Write This Program

### Mini-Program 1: Sign and Verify

Clear `Main()` and write:

```csharp
const string message = "Transfer $500 to account 12345.";
byte[] data = Encoding.UTF8.GetBytes(message);

// The SENDER signs with their OWN private key.
using var sender = RSA.Create();
byte[] signature = sender.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Anyone with the SENDER's PUBLIC key can verify -- they don't need the private key.
RSAParameters senderPublicKey = sender.ExportParameters(includePrivateParameters: false);
using var verifier = RSA.Create();
verifier.ImportParameters(senderPublicKey);

bool isValid = verifier.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

Console.WriteLine($"Message:   \"{message}\"");
Console.WriteLine($"Signature: {Convert.ToBase64String(signature)}");
Console.WriteLine($"Verified:  {isValid}");

GenericFunctions.Pause();
```

Run it. The verifier only ever had the public key - it can confirm the signature is valid without knowing the private key that produced it.

`SignData` hashes the data internally, signs the hash with the private key, and returns the signature bytes. `VerifyData` hashes the data the same way, then uses the public key to check that the signature matches. The hash step is what makes large-data signing practical - RSA operates on the hash, not the data itself.

### Mini-Program 2: The Keys Are Opposite From Encryption

Clear `Main()` and write:

```csharp
Console.WriteLine("ENCRYPTING (hiding a message's contents):");
Console.WriteLine("  1. Encrypt with the RECIPIENT's PUBLIC key");
Console.WriteLine("  2. Only the RECIPIENT's PRIVATE key can decrypt it");
Console.WriteLine("  -> Purpose: secrecy. The message is unreadable to everyone except the recipient.");
Console.WriteLine();
Console.WriteLine("SIGNING (proving who sent a message and that it wasn't altered):");
Console.WriteLine("  1. Sign with the SENDER's PRIVATE key");
Console.WriteLine("  2. Anyone with the SENDER's PUBLIC key can verify it");
Console.WriteLine("  -> Purpose: authenticity and integrity. NOT secrecy.");
Console.WriteLine("     A signed message is still perfectly readable by anyone.");
Console.WriteLine("     Signing doesn't hide anything.");
Console.WriteLine();
Console.WriteLine("Same key pair, opposite roles depending on which operation you're doing.");
Console.WriteLine("\"Public key encrypts, private key decrypts\" is only true for ENCRYPTION.");
Console.WriteLine("For SIGNING it's flipped: \"private key signs, public key verifies.\"");

GenericFunctions.Pause();
```

Run it. This is the conceptual anchor for everything else in this project. The role reversal is counterintuitive and consistently confused.

### Mini-Program 3: Detecting Tampered Data

Clear `Main()` and write:

```csharp
byte[] originalData = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");
byte[] tamperedData = Encoding.UTF8.GetBytes("Transfer $500 to account 99999.");

using var rsa = RSA.Create();
byte[] signature = rsa.SignData(originalData, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

bool originalValid = rsa.VerifyData(originalData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
bool tamperedValid = rsa.VerifyData(tamperedData, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

Console.WriteLine($"Original data  + original signature: {originalValid}");
Console.WriteLine($"Tampered data  + original signature: {tamperedValid}");
Console.WriteLine("\nChanging the account number by one digit produces a completely different hash.");
Console.WriteLine("The signature was computed over the original hash, so it no longer matches.");

GenericFunctions.Pause();
```

Run it. The tampered data fails verification. The signature was computed over the hash of the original data - any change to the data changes its hash, and the signature no longer matches. An attacker who changes the data cannot produce a valid signature for it without the private key.

### Mini-Program 4: Detecting a Tampered Signature

Clear `Main()` and write:

```csharp
byte[] data = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");

using var rsa = RSA.Create();
byte[] signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

// Flip one bit in the signature, simulating corruption or a forgery attempt.
byte[] tamperedSig = (byte[])signature.Clone();
tamperedSig[0] ^= 0xFF;

bool isValid = rsa.VerifyData(data, tamperedSig, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

Console.WriteLine($"Original data + tampered signature: {isValid}");
Console.WriteLine("\nEither side being altered -- the DATA or the SIGNATURE itself -- breaks verification.");
Console.WriteLine("Both need to arrive exactly as they were when signed.");

GenericFunctions.Pause();
```

Run it. Tampering with the signature alone is enough - the data doesn't have to change. The complete protection is bidirectional: the signature is bound to specific data via a specific private key. Modify either side and verification fails.

### Mini-Program 5: HMAC as a Symmetric Alternative

Clear `Main()` and write:

```csharp
// RandomNumberGenerator.GetBytes(int) is .NET 6+ only.
// RandomNumberGenerator.Create() + GetBytes() is the net48-compatible way.
byte[] sharedSecretKey = new byte[32];
using (var rng = RandomNumberGenerator.Create())
    rng.GetBytes(sharedSecretKey);

byte[] data = Encoding.UTF8.GetBytes("Transfer $500 to account 12345.");

using var hmac = new HMACSHA256(sharedSecretKey);
byte[] mac = hmac.ComputeHash(data);

using var verifyingHmac = new HMACSHA256(sharedSecretKey);
byte[] recomputedMac = verifyingHmac.ComputeHash(data);

bool isValid = mac.SequenceEqual(recomputedMac);

Console.WriteLine($"HMAC: {Convert.ToBase64String(mac)}");
Console.WriteLine($"Verified: {isValid}");
Console.WriteLine("\nHMAC is much faster than RSA signing and needs no key pair or certificate infrastructure.");
Console.WriteLine("The tradeoff: both parties need the SAME shared secret key -- the same");
Console.WriteLine("key distribution problem symmetric encryption has.");
Console.WriteLine("\nReach for HMAC when both sides already share a key (internal service-to-service).");
Console.WriteLine("Reach for RSA signatures when the verifier and signer are strangers.");

GenericFunctions.Pause();
```

Run it. One key both computes and verifies - unlike RSA where the private key signs and the public key verifies.

HMAC provides the same authenticity and integrity guarantees as an RSA signature, with dramatically less computational overhead and no key pair infrastructure. The catch is that HMAC requires a shared secret key, which brings back the key distribution problem. RSA signatures sidestep it: the verifier only needs the sender's public key, which can be distributed freely.

---

## Try It Yourself

Run `DetectingTamperedData()` and `DetectingTamperedSignature()` back to back. Both fail verification, for different reasons - either half of a signed message being altered is enough to break the whole thing. That's the complete integrity guarantee signatures provide.

---

## Summary: Encryption vs. Signing

| | Encryption | Signing |
|---|---|---|
| Private key role | Decrypts | Signs |
| Public key role | Encrypts | Verifies |
| Who uses the private key | Recipient | Sender |
| What it provides | Secrecy | Authenticity + integrity |
| Is the message readable? | No (without the key) | Yes - always |
| HMAC alternative? | No - needs asymmetric | Yes, if parties share a key |

---

## Takeaways

- Signing and encrypting use RSA's key pair in opposite roles.
- Encrypting: public key encrypts, private key decrypts. Signing: private key signs, public key verifies.
- A signed message is not secret - it's still perfectly readable. Signing proves authorship and integrity, not confidentiality.
- Any change to the data or the signature, on either side, breaks verification.
- `SignData` hashes internally - RSA signs the hash, not the raw data.
- HMAC provides equivalent authenticity/integrity with less overhead. Tradeoff: requires a shared secret key.
- Use HMAC for internal services that already share a key. Use RSA signatures when the parties are strangers.
