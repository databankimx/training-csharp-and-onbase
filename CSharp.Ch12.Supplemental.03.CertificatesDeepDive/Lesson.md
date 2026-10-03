# Chapter 12 Supplemental 03: Certificates Deep Dive

## What This Is

The main lesson built the simplest possible self-signed certificate and read a handful of its properties. This project covers what real certificates actually contain and do: adding the extensions that tell a relying party what a certificate is for, exporting and importing in both private-key and public-only formats, using the Windows certificate store, and validating a certificate chain - including seeing exactly what "untrusted" looks like for a self-signed certificate.

What's being extended here is the gap between a minimal certificate (main lesson) and a production-quality one (this project). A minimal certificate has a subject and a validity window. A real certificate also has extensions that constrain its use, exists in a managed store rather than a loose file, and can be chain-validated by a relying party. Each mini-program adds one of those layers and explains why it matters.

---

## How to Write This Program

### Mini-Program 1: Adding Certificate Extensions

Clear `Main()` and write:

```csharp
using var rsa = RSA.Create(2048);
var request = new CertificateRequest(
    "CN=CSharp.Ch12.ExtensionsDemo",
    rsa,
    HashAlgorithmName.SHA256,
    RSASignaturePadding.Pkcs1);

// Basic Constraints: is this certificate allowed to ISSUE other certificates (a CA),
// or is it an end-entity certificate only? Browsers refuse to trust a certificate chain
// where a non-CA certificate tries to sign another certificate.
request.CertificateExtensions.Add(
    new X509BasicConstraintsExtension(
        certificateAuthority: false,
        hasPathLengthConstraint: false,
        pathLengthConstraint: 0,
        critical: true));

// Key Usage: which cryptographic operations is this certificate's key actually allowed
// for. Keeping signing and encryption separate limits the damage if a key is compromised.
request.CertificateExtensions.Add(
    new X509KeyUsageExtension(
        X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
        critical: true));

using X509Certificate2 cert = request.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddDays(-1),
    DateTimeOffset.UtcNow.AddYears(1));

Console.WriteLine($"Subject: {cert.Subject}");
Console.WriteLine("\nExtensions:");
foreach (var ext in cert.Extensions)
    Console.WriteLine($" - {ext.Oid.FriendlyName}: {ext.Format(multiLine: false)}");

GenericFunctions.Pause();
```

Run it. Two extensions appear: Basic Constraints and Key Usage.

The `critical: true` flag means a relying party that doesn't understand an extension must reject the certificate entirely rather than ignoring it - important for security-critical constraints like these. A `critical: false` extension can be safely ignored by older software; a `critical: true` extension cannot.

### Mini-Program 2: Exporting and Importing

Clear `Main()` and write:

```csharp
using var rsa = RSA.Create(2048);
var request = new CertificateRequest("CN=CSharp.Ch12.ExportDemo", rsa,
    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 original = request.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

// PFX (PKCS#12): includes the PRIVATE key, password-protected.
// Use this format when moving a certificate somewhere that still needs to prove ownership.
byte[] pfxBytes = original.Export(X509ContentType.Pfx, "P@ssw0rd123!");

// CER: PUBLIC key and metadata only, no password, no private key.
// Hand this out to anyone who needs to verify signatures or encrypt data to this identity.
byte[] cerBytes = original.Export(X509ContentType.Cert);

Console.WriteLine($"PFX: {pfxBytes.Length} bytes (includes private key, password-protected)");
Console.WriteLine($"CER: {cerBytes.Length} bytes (public key and metadata only, no password)");

using var fromPfx = new X509Certificate2(pfxBytes, "P@ssw0rd123!");
using var fromCer = new X509Certificate2(cerBytes);

Console.WriteLine($"\nRe-imported from PFX -- has private key: {fromPfx.HasPrivateKey}");
Console.WriteLine($"Re-imported from CER -- has private key: {fromCer.HasPrivateKey}");
Console.WriteLine("\nGive the CER to anyone who verifies your signatures or encrypts data to you.");
Console.WriteLine("Give the PFX to nobody but the system that will USE this identity.");
Console.WriteLine("Anyone holding the PFX can impersonate this certificate's owner entirely.");

GenericFunctions.Pause();
```

Run it. The private key survives a PFX round-trip but is absent from a CER round-trip.

The format choice is a security decision, not just a technical one. PFX files require careful custody - treat them like private keys, because they contain one. CER files can be handed out freely.

### Mini-Program 3: Using the Windows Certificate Store

Clear `Main()` and write:

```csharp
using var rsa = RSA.Create(2048);
var request = new CertificateRequest("CN=CSharp.Ch12.StoreDemo", rsa,
    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 cert = request.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

// CurrentUser\My ("Personal") is the standard store for a user's own certificates.
// No administrator privileges needed, unlike LocalMachine stores.
using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadWrite);

try
{
    store.Add(cert);
    Console.WriteLine($"Added certificate with thumbprint {cert.Thumbprint} to CurrentUser\\My.");

    // Find it back by thumbprint -- how a real application would locate a specific,
    // already-installed certificate at runtime rather than needing a file path.
    var found = store.Certificates.Find(
        X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false);
    Console.WriteLine($"Found {found.Count} matching certificate(s) by thumbprint.");
}
finally
{
    // Clean up: don't leave a throwaway demo certificate sitting in a real user's store.
    store.Remove(cert);
    Console.WriteLine("\nRemoved the demo certificate -- cleaning up after itself.");
}

GenericFunctions.Pause();
```

Run it. The certificate goes in, can be found by thumbprint, and is removed cleanly.

The certificate store is how Windows applications manage certificates without juggling loose files. ASP.NET finds its HTTPS certificate here, code signing tools find their signing certificate here, and client authentication certificates live here. The thumbprint is the stable identifier - it's a hash of the whole certificate, unique and immutable.

`validOnly: false` in `Find` returns certificates regardless of validity period or trust status. `validOnly: true` would filter to only those that are currently valid and chain to a trusted root.

### Mini-Program 4: Validating a Certificate Chain

Clear `Main()` and write:

```csharp
using var rsa = RSA.Create(2048);
var request = new CertificateRequest("CN=CSharp.Ch12.ChainDemo", rsa,
    HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
using X509Certificate2 cert = request.CreateSelfSigned(
    DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

using var chain = new X509Chain();
bool isValid = chain.Build(cert);

Console.WriteLine($"Chain valid: {isValid}");
Console.WriteLine("\nChain status:");
foreach (var status in chain.ChainStatus)
    Console.WriteLine($" - {status.Status}: {status.StatusInformation.Trim()}");

Console.WriteLine("\nA self-signed certificate's chain reports UntrustedRoot -- expected.");
Console.WriteLine("Its own issuer (itself) isn't in the machine's trusted root store.");
Console.WriteLine("This is exactly the check a browser performs on every HTTPS connection.");
Console.WriteLine("That's why self-signed certificates trigger a warning page rather than");
Console.WriteLine("being silently accepted: the chain genuinely doesn't terminate at anything");
Console.WriteLine("the system already trusts.");

GenericFunctions.Pause();
```

Run it. `isValid` is `false`, `UntrustedRoot` appears in the chain status. This is the expected, correct answer for a self-signed certificate.

`X509Chain.Build()` walks the issuer chain from the presented certificate up to a root, verifying each link's signature and checking validity period, revocation status, and trust. A certificate issued by a real CA in the trusted root store would produce `isValid: true` and an empty `ChainStatus`.

---

## Try It Yourself

Run `ValidatingACertificateChain()` and read the `ChainStatus` output. `UntrustedRoot` is not an error in the code - it's the honest, correct answer for a self-signed certificate with no CA behind it. This is exactly what a browser reports when you navigate to a site with a self-signed certificate.

---

## Summary: Minimal vs. Real Certificate

| Feature | Main lesson | This project |
|---|---|---|
| Extensions | None | Basic Constraints + Key Usage |
| Constrained use | No | Yes - `critical: true` enforces it |
| Private key export | Not shown | PFX (password-protected) |
| Public-only export | Not shown | CER (freely distributable) |
| Storage | In-memory only | Windows certificate store |
| Chain validation | Not shown | `X509Chain.Build()` - reports `UntrustedRoot` |

---

## Takeaways

- Extensions declare what a certificate is for. Basic Constraints says whether it can sign other certificates. Key Usage says which operations its key is allowed for.
- `critical: true` means relying parties that don't understand an extension must reject the certificate.
- PFX includes the private key and requires a password. CER is public key only and needs no password.
- Give the CER to verifiers. Guard the PFX like a private key - because it contains one.
- The Windows certificate store is how applications manage certificates without file paths at runtime.
- `X509Chain.Build()` replicates the trust check a browser performs on every HTTPS connection.
- `UntrustedRoot` is the expected, correct result for any self-signed certificate not in the trusted root store.
