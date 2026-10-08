---
title: "Certificates Deep Dive - Extensions, Export, Store, Chain Validation"
chapter: 12
index: 1
dependencies: []
---

```csharp
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

internal static class Program
{
    private static void Main()
    {
        // --- Extensions ---
        // A minimal certificate (main lesson) has just a subject and a validity window.
        // Real certificates also declare what they're for via extensions.
        // critical: true means a relying party that doesn't understand the extension
        // MUST reject the certificate rather than ignoring it.
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=CSharp.Ch12.ExtensionsDemo",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        // Basic Constraints: is this certificate allowed to ISSUE other certificates (CA)?
        // false = end-entity only. Browsers refuse chains where a non-CA signs another cert.
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(
            certificateAuthority: false, hasPathLengthConstraint: false,
            pathLengthConstraint: 0, critical: true));

        // Key Usage: which operations is this key allowed for?
        // Keeping signing and encryption separate limits damage if a key is compromised.
        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
            critical: true));

        using X509Certificate2 cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));

        Console.WriteLine("=== Certificate Extensions ===");
        foreach (var ext in cert.Extensions)
            Console.WriteLine($"  {ext.Oid.FriendlyName}: {ext.Format(multiLine: false)}");

        // --- Export formats ---
        // PFX (PKCS#12): includes private key, password-protected.
        //   Guard like a private key -- whoever holds it can impersonate the owner entirely.
        // CER: public key and metadata only, no password, freely distributable.
        byte[] pfx = cert.Export(X509ContentType.Pfx, "P@ssw0rd123!");
        byte[] cer = cert.Export(X509ContentType.Cert);

        using var fromPfx = new X509Certificate2(pfx, "P@ssw0rd123!");
        using var fromCer = new X509Certificate2(cer);

        Console.WriteLine($"\n=== Export Formats ===");
        Console.WriteLine($"PFX ({pfx.Length} bytes) -- has private key after import: {fromPfx.HasPrivateKey}");
        Console.WriteLine($"CER ({cer.Length} bytes) -- has private key after import: {fromCer.HasPrivateKey}");

        // --- Windows certificate store ---
        // How applications manage certificates without juggling loose files.
        // CurrentUser\My ("Personal") needs no admin -- unlike LocalMachine stores.
        // Find by thumbprint at runtime rather than needing a file path.
        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadWrite);
        Console.WriteLine($"\n=== Certificate Store ===");
        try
        {
            store.Add(cert);
            Console.WriteLine($"Added: thumbprint {cert.Thumbprint}");
            var found = store.Certificates.Find(
                X509FindType.FindByThumbprint, cert.Thumbprint, validOnly: false);
            Console.WriteLine($"Found by thumbprint: {found.Count} certificate(s).");
        }
        finally
        {
            store.Remove(cert);
            Console.WriteLine("Removed -- demo cleaned up after itself.");
        }

        // --- Chain validation ---
        // Replicates what a browser does on every HTTPS connection.
        // UntrustedRoot is the expected, correct result for any self-signed certificate:
        // the issuer (itself) is not in the machine's trusted root store.
        // A cert issued by a real CA in the trusted root store produces isValid: true.
        using var chain = new X509Chain();
        bool isValid = chain.Build(cert);
        Console.WriteLine($"\n=== Chain Validation ===");
        Console.WriteLine($"Chain valid: {isValid}  <-- false is correct for self-signed");
        foreach (var s in chain.ChainStatus)
            Console.WriteLine($"  {s.Status}: {s.StatusInformation.Trim()}");
        Console.WriteLine("UntrustedRoot = no CA vouches for this cert. Same check a browser makes.");
    }
}
```
