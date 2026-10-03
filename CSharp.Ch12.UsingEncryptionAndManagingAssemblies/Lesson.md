# Chapter 12: Using Encryption and Managing Assemblies

## What This Is

Two genuinely separate topics sharing a chapter: cryptography (encryption, hashing, certificates) and assembly management (versioning, strong naming, the GAC). The common thread is "things that matter for shipping and securing real software," not a shared technical mechanism.

The three cryptographic tools here solve three different problems, and mixing them up is one of the most common real-world security mistakes:

- **Encryption** makes data unreadable to anyone without the right key, and reversible back to the original by whoever has it. Use it when you need to hide contents and recover them later.
- **Hashing** produces a fixed-size fingerprint. It is one-way: there is no key, and no operation exists to turn a hash back into original data. Use it to detect whether data changed, or to verify something matches.
- **Certificates** package a public key together with identity information and (usually) a trusted third party's signature vouching for that binding. Use them to prove who someone is.

A general note: .NET's cryptography APIs are deliberately designed so the algorithm classes (`Aes`, `RSA`, `SHA256`) are hard to misuse at the primitive level - but it's entirely possible to build something insecure on top of them. Reusing an IV, storing a key next to the data it protects, hashing a password without a salt. Every mini-program below notes the specific pitfall it's either demonstrating or avoiding.

---

## How to Write This Program

### Mini-Program 1: Symmetric Encryption (AES)

Clear `Main()` and write:

```csharp
const string plaintext = "The shipment arrives Tuesday at 3 PM.";

using var aes = Aes.Create();
// Aes.Create() already generates a cryptographically random Key and IV.
// Never reuse the same Key+IV pair to encrypt more than one message --
// doing so can leak information about the plaintext even without recovering the key.

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

Console.WriteLine($"Original:  {plaintext}");
Console.WriteLine($"Encrypted: {Convert.ToBase64String(encrypted)}");
Console.WriteLine($"Decrypted: {decrypted}");
Console.WriteLine("\nSymmetric encryption is fast and well-suited to bulk data, but both parties");
Console.WriteLine("need the SAME key. Getting that key to the other party securely is the");
Console.WriteLine("\"key distribution problem\" -- which asymmetric encryption solves.");

GenericFunctions.Pause();
```

Run it. The same `aes` instance encrypts and decrypts because it holds both the key and the IV. The encrypted bytes are unreadable without them. That's what "symmetric" means: same key both ways.

`CryptoStream` is the .NET mechanism for chaining a cipher onto any `Stream`. Writing to a `CryptoStream` wrapping a `MemoryStream` encrypts data as it flows through. The same pattern works with a `FileStream` for large files - see Mini-Program 3.

### Mini-Program 2: Asymmetric Encryption (RSA)

Clear `Main()` and write:

```csharp
const string plaintext = "Meet at the usual place.";
byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

using var rsa = RSA.Create();

// ExportParameters(false) exports only the PUBLIC key.
// A real sender would never possess the recipient's private key at all.
RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: false);

using var rsaPublicOnly = RSA.Create();
rsaPublicOnly.ImportParameters(publicKey);

// OaepSHA1 (not OaepSHA256) on net48 specifically: RSA.Create() returns an
// RSACryptoServiceProvider on this platform, which only supports OaepSHA1.
// Using SHA-1 for OAEP's internal padding scheme (not for signing or certificates)
// is still considered acceptable -- this is a platform constraint, not a security choice.
byte[] encrypted = rsaPublicOnly.Encrypt(plaintextBytes, RSAEncryptionPadding.OaepSHA1);

// Only the ORIGINAL "rsa" instance holds the private key.
byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA1);

Console.WriteLine($"Original:  {plaintext}");
Console.WriteLine($"Encrypted: {Convert.ToBase64String(encrypted)}");
Console.WriteLine($"Decrypted: {Encoding.UTF8.GetString(decrypted)}");
Console.WriteLine("\nRSA solves the key distribution problem -- the public key really can be public.");
Console.WriteLine("But RSA is slow and has a hard size limit (roughly the key size minus padding).");
Console.WriteLine("Real systems use RSA to encrypt a random AES key, then AES for the actual data.");
Console.WriteLine("That's \"hybrid encryption.\" See Supplemental.01 for RSA's other major use: signing.");

GenericFunctions.Pause();
```

Run it. The public-key-only instance can encrypt but not decrypt. Only the original instance, which retained the private key, can decrypt.

Note: `OaepSHA256` throws `CryptographicException` on classic .NET Framework because `RSA.Create()` returns `RSACryptoServiceProvider` - the CAPI-based provider - which only supports `OaepSHA1` for encryption. Modern .NET uses a different provider. This is a real platform difference, not a mistake in the code. SHA-1's known weaknesses are about collision attacks, which matter for signatures; OAEP uses its internal hash purely for padding, so `OaepSHA1` is still safe here.

### Mini-Program 3: Stream Encryption

Clear `Main()` and write:

```csharp
string plainPath     = Path.Combine(Path.GetTempPath(), $"ch12-plain-{Guid.NewGuid():N}.txt");
string encryptedPath = Path.Combine(Path.GetTempPath(), $"ch12-encrypted-{Guid.NewGuid():N}.bin");
string decryptedPath = Path.Combine(Path.GetTempPath(), $"ch12-decrypted-{Guid.NewGuid():N}.txt");

try
{
    File.WriteAllText(plainPath, "This file's contents never exist in memory as one plaintext blob.");

    using var aes = Aes.Create();

    using (var src = File.OpenRead(plainPath))
    using (var dst = File.Create(encryptedPath))
    using (var cs = new CryptoStream(dst, aes.CreateEncryptor(), CryptoStreamMode.Write))
        src.CopyTo(cs);

    using (var src = File.OpenRead(encryptedPath))
    using (var cs = new CryptoStream(src, aes.CreateDecryptor(), CryptoStreamMode.Read))
    using (var dst = File.Create(decryptedPath))
        cs.CopyTo(dst);

    Console.WriteLine($"Encrypted file: {new FileInfo(encryptedPath).Length} bytes");
    Console.WriteLine($"Decrypted contents: {File.ReadAllText(decryptedPath)}");
}
finally
{
    foreach (var path in new[] { plainPath, encryptedPath, decryptedPath })
        if (File.Exists(path)) File.Delete(path);
}

GenericFunctions.Pause();
```

Run it. The plaintext never exists as a single in-memory byte array - it flows from the source file through the cipher and into the destination file a chunk at a time. For a multi-gigabyte file this is the difference between needing to hold the whole thing in memory and needing only a buffer.

### Mini-Program 4: Hashing

Clear `Main()` and write:

```csharp
const string original = "password123";
const string tampered = "password124";

// SHA256.HashData() (the static convenience method) was only added in .NET 5+.
// SHA256.Create() + ComputeHash() is the net48-compatible equivalent.
using var sha256 = SHA256.Create();
byte[] originalHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(original));
byte[] tamperedHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tampered));

// Convert.ToHexString() is also .NET 5+. BitConverter.ToString() minus its "-" separators
// is the net48-compatible equivalent.
string originalHex = BitConverter.ToString(originalHash).Replace("-", "");
string tamperedHex = BitConverter.ToString(tamperedHash).Replace("-", "");

Console.WriteLine($"\"{original}\" -> {originalHex}");
Console.WriteLine($"\"{tampered}\" -> {tamperedHex}");
Console.WriteLine("\nChanging ONE character produced a completely different hash.");
Console.WriteLine("This is the avalanche effect -- it's what makes hashing useful for detecting tampering.");
Console.WriteLine("\nImportant distinction from encryption: there is no \"unhash\" operation.");
Console.WriteLine("And hashing a plain password directly (as shown here) is NOT correct password storage.");
Console.WriteLine("See Supplemental.02.PasswordHashingDoneRight for why, and what to do instead.");

GenericFunctions.Pause();
```

Run it. One character different in the input, completely different output. The avalanche effect is the property that makes hashing useful for integrity checking.

### Mini-Program 5: Creating and Inspecting a Certificate

Clear `Main()` and write:

```csharp
using var rsa = RSA.Create(2048);

var request = new CertificateRequest(
    "CN=CSharp.Ch12.Demo, O=DataBank IMX Training",
    rsa,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);

// A self-signed certificate is its own issuer. A real-world certificate is instead
// signed by a separate Certificate Authority (CA). The whole reason a browser trusts
// a website's certificate is that a CA it already trusts vouched for it.
// A self-signed certificate has no such third party backing it.
using X509Certificate2 certificate = request.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddDays(-1),
    DateTimeOffset.UtcNow.AddYears(1));

Console.WriteLine($"Subject:    {certificate.Subject}");
Console.WriteLine($"Issuer:     {certificate.Issuer} (matches Subject -- it's self-signed)");
Console.WriteLine($"Thumbprint: {certificate.Thumbprint}");
Console.WriteLine($"Valid:      {certificate.NotBefore:d} to {certificate.NotAfter:d}");
Console.WriteLine($"Has private key: {certificate.HasPrivateKey}");

GenericFunctions.Pause();
```

Run it. The thumbprint is a hash of the whole certificate, used to identify it uniquely. `HasPrivateKey` is true because we created this certificate right here - a certificate loaded from a CER file (public only) would be false.

---

## Part 2: Managing Assemblies

### Mini-Program 6: Inspecting Assembly Versions

Clear `Main()` and write:

```csharp
Assembly currentAssembly = Assembly.GetExecutingAssembly();
AssemblyName assemblyName = currentAssembly.GetName();

Console.WriteLine($"Name:      {assemblyName.Name}");
Console.WriteLine($"Version:   {assemblyName.Version}");
Console.WriteLine($"Full name: {currentAssembly.FullName}");
Console.WriteLine("\nThe four-part version (Major.Minor.Build.Revision) is a .NET convention.");
Console.WriteLine("Teams commonly apply semantic versioning ideas within it: Major for breaking");
Console.WriteLine("changes, Minor for new-but-compatible features, Build/Revision for patches.");
Console.WriteLine(".NET itself doesn't enforce that meaning -- the convention is just that.");

GenericFunctions.Pause();
```

Run it. The version comes from `[assembly: AssemblyVersion(...)]` in `AssemblyInfo.cs`.

### Mini-Program 7: Understanding Strong Naming

Clear `Main()` and write:

```csharp
Assembly current = Assembly.GetExecutingAssembly();
byte[] token = current.GetName().GetPublicKeyToken();
bool isStrongNamed = token != null && token.Length > 0;

Console.WriteLine($"Is this assembly strong-named: {isStrongNamed}");
Console.WriteLine("\nThis project isn't strong-named (most application projects aren't --");
Console.WriteLine("only shared libraries that specifically need it usually are).");
Console.WriteLine("\nA strong-named assembly is signed with a private key (.snk file), giving it");
Console.WriteLine("a unique identity: name + version + culture + public key token, all together.");
Console.WriteLine("That full identity is what makes side-by-side versioning and safe GAC");
Console.WriteLine("placement possible. See Supplemental.04 for real, concrete examples.");

GenericFunctions.Pause();
```

Run it. The answer is `False` - expected. Application projects rarely need strong naming.

### Mini-Program 8: Understanding the GAC

Clear `Main()` and write:

```csharp
Console.WriteLine("The GAC (Global Assembly Cache) is a machine-wide store for assemblies meant");
Console.WriteLine("to be shared across multiple applications, rather than each application");
Console.WriteLine("shipping its own private copy.");
Console.WriteLine();
Console.WriteLine("Only STRONG-NAMED assemblies can go in the GAC, specifically because the GAC");
Console.WriteLine("needs to tell apart multiple versions of an assembly with the SAME simple name.");
Console.WriteLine("That's \"side-by-side versioning\": version 1.0 and 2.0 of the same-named");
Console.WriteLine("library can both live in the GAC simultaneously, and each application loads");
Console.WriteLine("whichever it was built against.");
Console.WriteLine();
Console.WriteLine("Worth knowing this is less common in modern .NET than in classic .NET Framework.");
Console.WriteLine("NuGet-based per-application dependency management has largely superseded the GAC");
Console.WriteLine("for new development. Still relevant for .NET Framework and framework-level assemblies.");

GenericFunctions.Pause();
```

---

## Try It Yourself

Run the project and compare `UsingSymmetricEncryption()` and `UsingAsymmetricEncryption()` side by side: both successfully round-trip a message, but think through what would actually be required to get the AES example's key safely to a second party versus what the RSA example required (nothing secret to transmit - just the public key). That practical difference is the entire reason both techniques exist side by side rather than one replacing the other.

---

## Summary: Three Cryptographic Tools, Three Problems

| Tool | Direction | Key model | Use for |
|---|---|---|---|
| Symmetric (AES) | Encrypt/decrypt | Same key both ways | Bulk data, fast, private channel |
| Asymmetric (RSA) | Encrypt/decrypt | Public encrypts, private decrypts | Key exchange, small payloads |
| Hashing (SHA-256) | One-way only | None | Integrity checking, fingerprinting |
| Digital signatures | One-way, verifiable | Private signs, public verifies | Authenticity (see Supplemental.01) |
| Certificates (X.509) | Identity binding | Public key + identity + issuer | Trust establishment |

---

## Takeaways

- `Aes.Create()` generates a cryptographically random key and IV automatically. Never reuse the same Key+IV pair.
- Symmetric encryption is fast but requires both parties to have the same key.
- Asymmetric encryption (RSA) solves key distribution - the public key can be public. Real systems use hybrid encryption.
- `CryptoStream` chains a cipher onto any `Stream`, encrypting data in flight without loading it all into memory.
- Hashing is one-way - there's no "unhash." The avalanche effect makes it useful for integrity checking.
- Hashing a plain password directly with SHA-256 is not correct password storage. See Supplemental.02.
- Certificates bind an identity to a public key, vouched for by an issuer. Self-signed certificates have no third-party backing.
- Assembly version is a .NET convention; .NET doesn't enforce semantic versioning within it.
- Strong naming gives an assembly a full verifiable identity beyond just a file name.
- The GAC requires strong naming so it can distinguish versions of same-named assemblies.

---

## Also in Chapter 12

1. `CSharp.Ch12.Supplemental.01.DigitalSignaturesDeepDive`
2. `CSharp.Ch12.Supplemental.02.PasswordHashingDoneRight`
3. `CSharp.Ch12.Supplemental.03.CertificatesDeepDive`
4. `CSharp.Ch12.Supplemental.04.StrongNamingAndTheGacDeepDive`
