# Chapter 12: Using Encryption and Managing Assemblies

## What This Is

The final chapter covers two genuinely separate topics sharing a chapter: cryptography (encryption, hashing, certificates) and assembly management (versioning, strong naming, the GAC). The connecting thread is "things that matter for shipping and securing real software," not a shared technical mechanism, worth reading the two halves as two distinct lessons rather than expecting them to build on each other.

A general note worth internalizing before any of the code: .NET's cryptography APIs are deliberately hard to misuse in ways that weaken the underlying algorithm, but it's entirely possible to build something insecure *on top of* correctly-used APIs (reusing an IV, storing a key next to the data it protects, hashing a password with no salt). Every method below calls out the specific pitfall it's actively avoiding.

---

## Symmetric Encryption: One Key, Both Directions

```csharp
using var aes = Aes.Create();   // generates a random Key and IV automatically
using var encryptor = aes.CreateEncryptor();
using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
```

`Aes.Create()` already generates a cryptographically random `Key` and `IV` for you, worth knowing explicitly rather than assuming it's your job. The one rule genuinely worth internalizing: **never reuse the same Key+IV pair to encrypt more than one message**. Doing so can leak real information about the plaintext (in some modes, XORing two ciphertexts encrypted under the same key+IV cancels the keystream out entirely) even without ever recovering the key itself.

Symmetric encryption's tradeoff: fast, well-suited to bulk data, but both parties need the *same* key, the "key distribution problem", how do you get that key to the other party without an attacker intercepting it too?

---

## Asymmetric Encryption: Different Keys for Encrypt and Decrypt

```csharp
RSAParameters publicKey = rsa.ExportParameters(includePrivateParameters: false);
// ... hand publicKey to anyone, they encrypt with it ...
byte[] decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA1);   // only the ORIGINAL holder can decrypt
```

This is what actually solves the key distribution problem: the public key genuinely can be made public, anyone can use it to encrypt something, but only the matching private key (which never has to leave its owner's possession) can decrypt it. The tradeoff: RSA is slow and has a hard size limit on what it can encrypt directly, roughly the key size in bytes minus padding overhead. Real systems combine both: use RSA to encrypt a random, one-time-use AES key, then use that AES key for the actual bulk data, "hybrid encryption." See `CSharp.Ch12.Supplemental.01.DigitalSignaturesDeepDive` for RSA's other major job, proving who sent something, not just hiding what it says.

**A real platform gotcha, hit while testing**: this originally used `RSAEncryptionPadding.OaepSHA256`, and threw `CryptographicException: Specified padding mode is not valid for this algorithm` the moment it ran. On classic .NET Framework specifically, `RSA.Create()` returns an `RSACryptoServiceProvider`, the legacy, CAPI-based provider, which only supports `OaepSHA1` (or plain `Pkcs1`) padding for encryption, `OaepSHA256` isn't implemented by that specific provider at all. Modern .NET's `RSA.Create()` returns a different, more capable provider that *does* support `OaepSHA256` directly, a genuine, real difference between the two platforms, not a mistake in the original code. **Fixed** by switching both the encrypt and decrypt calls to `RSAEncryptionPadding.OaepSHA1`. Worth being precise about why this is still fine: SHA-1's known weaknesses are about hash *collisions*, which matter enormously for digital signatures and certificates (where a forged collision could let someone substitute a different, malicious document undetected), but OAEP uses its internal hash purely as part of the padding/masking scheme, not as a signature, so SHA-1 there doesn't carry the same risk. Using SHA-1 for a certificate or a signature, by contrast, genuinely would be a real weakness, see `CSharp.Ch12.Supplemental.01.DigitalSignaturesDeepDive` and `CSharp.Ch12.Supplemental.03.CertificatesDeepDive`.

---

## Stream Encryption: Encrypting Data You Can't Fit in Memory

```csharp
using var sourceStream = File.OpenRead(plainPath);
using var destinationStream = File.Create(encryptedPath);
using var cryptoStream = new CryptoStream(destinationStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
sourceStream.CopyTo(cryptoStream);
```

The main difference from the symmetric encryption example above: `CryptoStream` here chains directly onto a `FileStream` instead of a `MemoryStream`. That means data gets encrypted a chunk at a time as it's read from the source and written to the destination, never sitting entirely in memory as one giant plaintext byte array. For a multi-gigabyte file, this is the difference between a small, constant memory footprint and trying (and likely failing) to load the entire file into RAM first.

---

## Hashing: One-Way, Not Encryption

```csharp
using var sha256 = SHA256.Create();
byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
```

Worth being precise about the distinction: there is no "unhash" operation, hashing is fundamentally one-directional. Changing a single character in the input (`"password123"` → `"password124"`) produces a *completely different* hash, the "avalanche effect", exactly what makes hashing useful for detecting tampering: comparing two hashes tells you whether the underlying data changed at all, without either party needing to see the original data both times.

This lesson deliberately hashes a plain password directly, purely to illustrate the avalanche effect, and explicitly flags that this is **not** how real systems should store passwords. See `CSharp.Ch12.Supplemental.02.PasswordHashingDoneRight` for why plain hashing (even with a strong algorithm like SHA-256) is genuinely dangerous for passwords specifically, and what to do instead.

Worth knowing about the API choice here too: `SHA256.HashData(byte[])`, a newer, more convenient static method, only exists starting in .NET 5+. This project targets net48 (classic .NET Framework, like the rest of this training set), so the older, instance-based `SHA256.Create()` + `ComputeHash()` pattern is what's actually available, and what's used here.

---

## Certificates: A Real, Working Self-Signed Certificate

```csharp
var request = new CertificateRequest("CN=...", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 certificate = request.CreateSelfSigned(notBefore, notAfter);
```

`CertificateRequest` (available since .NET Framework 4.7.2) creates a genuine, real `X509Certificate2` at runtime, no external tooling, no pre-existing `.pfx` file needed for this demo to work. Worth understanding what "self-signed" actually means here: the certificate is its own issuer. Contrast this against a real-world website certificate, which is instead signed by a separate, trusted Certificate Authority (CA); the entire reason a browser trusts a site's certificate is that a CA it *already* trusts vouched for it. A self-signed certificate has no such third party backing it, which is exactly why browsers warn loudly about them, useful for local development and testing, never for anything a stranger needs to trust.

---

## Assembly Versions, Strong Naming, and the GAC

```csharp
AssemblyName assemblyName = Assembly.GetExecutingAssembly().GetName();
Console.WriteLine(assemblyName.Version);              // the four-part version number
Console.WriteLine(assemblyName.GetPublicKeyToken());   // empty unless strong-named
```

A strong-named assembly is signed with a private key (traditionally an `.snk` file), giving it a full identity beyond just its file name: simple name + version + culture + public key token, *together*. This is what makes two genuinely useful things possible: **side-by-side versioning** (version 1.0 and version 2.0 of the same-named library, both installed and loadable at once, since their full identities differ even though their simple names match) and safe placement in the **GAC** (Global Assembly Cache), a machine-wide store for assemblies meant to be shared across multiple applications, rather than each application shipping its own private copy.

Worth knowing this is considerably less common in modern .NET than it was in classic .NET Framework: NuGet-based, per-application dependency management (each app gets its own copy of exactly the packages and versions it needs, no machine-wide shared state to reason about) has largely superseded the GAC for new development. Still genuinely relevant for classic .NET Framework applications, like the ones throughout this entire training set, and for certain framework-level assemblies.
