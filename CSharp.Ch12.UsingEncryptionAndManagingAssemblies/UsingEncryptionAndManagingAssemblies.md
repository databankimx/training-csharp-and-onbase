# Using Encryption and Managing Assemblies

## Introduction

This is the final lesson in the training set, and it covers two topics that don't actually depend on each other: **cryptography** (keeping data secret, proving it hasn't been tampered with, and proving who sent it) and **assembly management** (how .NET identifies, versions, and shares compiled code). They're grouped together because both matter enormously once software actually ships to real users, not because one builds on the other. Feel free to treat this as two shorter lessons back to back.

---

## Part 1: Using Encryption

### The Big Picture: Three Different Problems, Three Different Tools

Before looking at any code, it's worth being clear about what each technique actually solves, because they get confused constantly:

- **Encryption** (symmetric or asymmetric) makes data *unreadable* to anyone without the right key, and *reversible* back to the original by whoever has that key. Use it when you need to hide the *contents* of something and get them back later.
- **Hashing** produces a fixed-size fingerprint of data. It is **one-way**: there is no key, and no operation exists to turn a hash back into the original data. Use it to detect whether data changed, or to verify something matches, without needing to see the original.
- **Certificates** are a way of packaging a public key together with information about who it belongs to, and (usually) a trusted third party's signature vouching for that binding. Use them for identity: proving a server really is who it claims to be, or that code really was published by who it claims.

Mixing these up is one of the most common real-world security mistakes: hashing something you actually need to decrypt later, or encrypting a password instead of hashing it, or trusting a self-signed certificate the way you'd trust a CA-issued one. Keep the three purposes straight and a lot of confusion disappears.

---

### Symmetric Encryption: One Key, Two Directions

```csharp
using var aes = Aes.Create();
using var encryptor = aes.CreateEncryptor();
using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
using var writer = new StreamWriter(cryptoStream);
writer.Write(plaintext);
```

"Symmetric" means the exact same key both encrypts and decrypts. AES (Advanced Encryption Standard) is the standard modern choice, it's fast, it's been studied exhaustively by cryptographers for decades without a practical break being found, and .NET's `Aes` class gives you a solid, correctly-configured default (`Aes.Create()`) without you needing to choose a mode, padding scheme, or key size yourself.

**The one rule that matters most**: `Aes.Create()` generates a fresh, random `Key` and `IV` (Initialization Vector) every single time you call it. Never encrypt two different messages with the same Key+IV pair. In some cipher modes, doing so can let an attacker recover information about both plaintexts just by comparing the two ciphertexts, XORing them together can literally cancel out the encryption entirely for the parts where the messages overlap, without the attacker ever needing to break the key itself. This is a real, historically-exploited mistake (reused keystreams have broken real cryptographic systems), not a theoretical concern.

**The tradeoff symmetric encryption makes**: it's fast enough for large amounts of data (encrypting a multi-gigabyte file is entirely practical), but both the sender and receiver need to already possess the *same* secret key. How do you get that key to the other party in the first place, especially over a network an attacker might be watching? That's the "key distribution problem," and it's exactly what asymmetric encryption exists to solve.

---

### Asymmetric Encryption: Two Different Keys

```csharp
RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: false);
// Hand publicKey to literally anyone. They can encrypt with it.

using var rsaPublicOnly = RSA.Create();
rsaPublicOnly.ImportParameters(publicKey);
byte[] encrypted = rsaPublicOnly.Encrypt(plaintextBytes, RSAEncryptionPadding.OaepSHA256);

// Only the instance that STILL HAS the private key can decrypt.
byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
```

RSA (named for its three inventors, Rivest, Shamir, and Adleman) uses a *pair* of mathematically related keys: a public key, which can genuinely be shared with anyone, even posted publicly, and a private key, which must never leave the possession of whoever generated it. Anyone can use your public key to encrypt something addressed to you, but only your private key can decrypt it. This is what actually solves the key distribution problem: you never need to secretly transmit anything, the public key was never a secret in the first place.

**Why this doesn't replace AES entirely**: RSA is computationally expensive compared to AES (often 100-1000x slower for the same amount of data), and it has a hard mathematical limit on how much it can encrypt in one operation, roughly the key size in bytes minus some padding overhead (a 2048-bit RSA key can encrypt at most around 190 bytes with OAEP padding). You genuinely cannot RSA-encrypt a large file directly.

**What real systems actually do**: "hybrid encryption." Generate a random, one-time-use AES key, encrypt the actual data with that AES key (fast, no size limit), then encrypt *just the AES key itself* with RSA (small, well within RSA's size limit) and send both pieces together. The recipient uses their RSA private key to decrypt the small AES key, then uses that AES key to decrypt the actual data. This is exactly how HTTPS/TLS works under the hood, and it's worth recognizing that pattern when you see it, it's everywhere in real-world secure systems.

RSA has a second major job beyond encryption entirely: digital signatures, proving a message really came from the holder of a specific private key and wasn't altered in transit. That's covered in depth in `CSharp.Ch12.Supplemental.01.DigitalSignaturesDeepDive`.

---

### Stream Encryption: When the Data Doesn't Fit in Memory

```csharp
using var sourceStream = File.OpenRead(plainPath);
using var destinationStream = File.Create(encryptedPath);
using var cryptoStream = new CryptoStream(destinationStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
sourceStream.CopyTo(cryptoStream);
```

Everything shown for symmetric encryption above worked with a `MemoryStream`, meaning the *entire* plaintext and the *entire* ciphertext both existed as byte arrays in memory at once. That's completely fine for a short string, but it falls apart for a 10 GB video file, you'd need 10+ GB of free memory just to hold the data being processed.

`CryptoStream` solves this by wrapping *any* other stream, here, a `FileStream` reading from disk, and encrypting or decrypting data as it flows through, one chunk at a time, rather than requiring the whole thing up front. `sourceStream.CopyTo(cryptoStream)` reads a buffer's worth of plaintext bytes from the file, `CryptoStream` encrypts just that buffer, and writes the encrypted result to the destination file, then repeats, using a small, constant amount of memory regardless of how large the file actually is. This is the standard technique for encrypting anything that's too large to comfortably hold in RAM, which in practice means most real files.

---

### Hashing: A Fingerprint, Not a Lock

```csharp
using var sha256 = SHA256.Create();
byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
```

A cryptographic hash function takes an input of any size and produces a fixed-size output (SHA-256 always produces exactly 256 bits / 32 bytes, no matter whether the input was one byte or one gigabyte). Three properties make it useful:

1. **Deterministic**: the same input always produces the same hash.
2. **One-way**: there is no practical way to work backward from a hash to figure out what input produced it (this is different from encryption, where decryption is the entire point).
3. **Avalanche effect**: changing even one bit of the input produces a wildly different, unpredictable-looking hash. Hashing `"password123"` and `"password124"` produces two hashes that share essentially nothing in common, even though the inputs differ by one character.

That third property is what makes hashing useful for **integrity checking**: if you hash a file when you send it, and the recipient hashes the same file when they receive it, matching hashes mean the file is byte-for-byte identical, and even a single flipped bit (from corruption or tampering) would produce a completely different hash, making the mismatch obvious.

**The password mistake, spelled out**: it might seem like hashing a password before storing it (instead of storing the password itself) solves the "what if my database leaks" problem. It helps, but naive hashing (exactly what this lesson demonstrates, `sha256.ComputeHash()` on the raw password) is genuinely dangerous in practice, for reasons covered in full in `CSharp.Ch12.Supplemental.02.PasswordHashingDoneRight`: fast hash algorithms like SHA-256 are *designed* to be fast, which is exactly the wrong property for password storage, an attacker with a leaked database can try billions of guesses per second against a fast hash. Real password storage needs a deliberately *slow*, purpose-built algorithm, plus a per-user random salt, both covered in that Supplemental.

---

### Certificates: Binding a Public Key to an Identity

```csharp
var request = new CertificateRequest(
    "CN=CSharp.Ch12.Demo, O=DataBank IMX Training",
    rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

using X509Certificate2 certificate = request.CreateSelfSigned(notBefore, notAfter);
```

A digital certificate (specifically, an X.509 certificate, the standard format) bundles together a public key, information about who that key belongs to (the "Subject", encoded as a Distinguished Name, `CN=` for Common Name, `O=` for Organization, and other fields), a validity period, and a digital signature over all of that, proving the bundle hasn't been tampered with.

**The critical question for any certificate**: who signed it? A **self-signed** certificate (exactly what this lesson creates) is signed by its own private key, it vouches for itself. That's fine for local development, testing, or situations where you personally control both ends and can verify the certificate out-of-band. It is fundamentally *not* fine for proving identity to a stranger, anyone can generate a self-signed certificate claiming to be anyone (a self-signed certificate claiming `CN=google.com` is trivial to create and proves absolutely nothing).

Real-world trust comes from **Certificate Authorities (CAs)**: organizations (like Let's Encrypt, DigiCert, or others) whose own certificates are pre-installed and trusted by operating systems and browsers. When a CA signs your certificate, they're vouching, with their own already-trusted signature, that they verified you actually control the domain or identity the certificate claims. That's the entire chain of trust that makes HTTPS work: your browser trusts the CA, the CA vouches for the website's certificate, so your browser trusts the website's certificate too, transitively.

`certificate.HasPrivateKey` in this lesson's output will be `true`, since `CreateSelfSigned()` generated the certificate from a key pair we still hold. In a real deployment, you'd typically only distribute the *public* portion of a certificate widely; the private key stays wherever it was generated, exactly the same principle as asymmetric encryption's public/private key split above.

---

## Part 2: Managing Assemblies

### What an Assembly Actually Is

Every compiled .NET output, a `.dll` or `.exe`, is an **assembly**: a unit of deployment and versioning that bundles compiled IL code together with a **manifest**, metadata describing the assembly itself (its name, version, culture, referenced assemblies, and more). This is what makes reflection (covered back in Chapter 8) even possible, the manifest is genuinely part of the file, inspectable at runtime, not documentation living somewhere else.

### Assembly Versions

```csharp
AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
Console.WriteLine(assemblyName.Version);   // e.g. 1.0.0.0
```

Every assembly carries a four-part version number: **Major.Minor.Build.Revision**. .NET itself doesn't enforce any particular meaning for these numbers, but most teams follow some version of semantic versioning conventions: Major bumps for breaking changes, Minor bumps for new-but-backward-compatible features, and Build/Revision for bug fixes and patches. This version number is what makes it possible for tooling (and, for strong-named assemblies, the runtime itself) to reason about compatibility between different builds of the same library.

### Strong Naming: A Full, Unique Identity

```csharp
byte[] publicKeyToken = assemblyName.GetPublicKeyToken();
bool isStrongNamed = publicKeyToken != null && publicKeyToken.Length > 0;
```

An ordinary assembly is identified essentially by its file name, "MyLibrary.dll". That's fragile: nothing stops two completely unrelated projects from both producing a file called "MyLibrary.dll", and if both end up on the same machine, there's no reliable way to tell them apart or guarantee the right one gets loaded.

**Strong naming** fixes this by signing the assembly with a private key (traditionally stored in an `.snk` file), which embeds a cryptographic public key token into the assembly's identity. A strong-named assembly's *full* identity is four things together: simple name + version + culture + public key token. Because the public key token is derived from a private key only the actual publisher possesses, it's essentially impossible for someone else to accidentally (or deliberately) produce an assembly with the exact same full identity. This full identity is what two genuinely important capabilities depend on:

- **Side-by-side versioning**: version 1.0.0.0 and version 2.0.0.0 of the exact same library (same simple name, same public key token) can both be installed and loaded on the same machine at the same time, because their *full* identities (which include the version number) are different. Each application that references the library gets the specific version it was actually built and tested against, rather than potentially being silently upgraded (or downgraded) to whatever version happens to be present.
- **Safe placement in the GAC**, covered next.

Most application projects (like nearly everything in this training set) are *not* strong-named, and don't need to be, strong naming matters specifically for shared libraries that need a globally unique, verifiable identity, which is why the demo above expects to find `isStrongNamed` is `false`.

### The Global Assembly Cache (GAC)

The GAC is a special, machine-wide folder (historically `C:\Windows\assembly` or `C:\Windows\Microsoft.NET\assembly`) where strong-named assemblies can be installed once and shared by every application on that machine, rather than each application needing its own private copy sitting next to its `.exe`. Because the GAC can hold multiple versions of the same-named assembly simultaneously (side-by-side versioning again), and each application's own configuration specifies exactly which version it was built against, this used to be the standard way Microsoft (and many third parties) distributed shared, common libraries.

**Worth knowing this has fallen out of favor**: modern .NET development, especially anything using NuGet, has largely moved away from machine-wide shared assemblies entirely. Each application gets its own, self-contained set of exact package versions restored specifically for it, no machine-wide state to coordinate, no risk of one application's dependency update accidentally affecting another's. The GAC is still genuinely relevant for classic .NET Framework applications, like the ones throughout this entire training set, and for certain low-level framework assemblies, but it's a technique worth understanding historically and for legacy-system work rather than reaching for in new development.

---

## Try It Yourself

Run the project and read through the console output section by section, in particular, compare `UsingSymmetricEncryption()`'s and `UsingAsymmetricEncryption()`'s output side by side: notice both successfully round-trip a message, but think through what would actually be required to get the AES example's key safely to a second party, versus what the RSA example required (nothing secret to transmit at all, just the public key). That practical difference is the entire reason both techniques exist side by side rather than one simply replacing the other.
