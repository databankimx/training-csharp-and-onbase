---
title: "X.509 Certificates"
chapter: 12
index: 2
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
        // --- Create a self-signed certificate ---
        // A self-signed certificate is its own issuer -- it has no CA backing it.
        // A real-world certificate is signed by a Certificate Authority the relying
        // party already trusts. That's why browsers trust HTTPS certificates:
        // a CA they ship with vouched for the binding between the site's identity
        // and its public key. A self-signed certificate has no such vouching.
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest(
            "CN=CSharp.Ch12.Demo, O=DataBank IMX Training",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        using X509Certificate2 cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(1));

        Console.WriteLine("=== Self-Signed Certificate ===");
        Console.WriteLine($"Subject:    {cert.Subject}");
        Console.WriteLine($"Issuer:     {cert.Issuer}  <-- matches Subject (self-signed)");
        Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
        Console.WriteLine($"Valid:      {cert.NotBefore:d} to {cert.NotAfter:d}");
        Console.WriteLine($"Has private key: {cert.HasPrivateKey}");

        // --- Chain validation ---
        // X509Chain.Build() replicates what a browser does on every HTTPS connection:
        // walk the issuer chain, verify each signature, check validity and trust.
        // UntrustedRoot is the expected, correct result for a self-signed certificate --
        // its issuer (itself) is not in the machine's trusted root store.
        using var chain = new X509Chain();
        bool isValid = chain.Build(cert);
        Console.WriteLine($"\n=== Chain Validation ===");
        Console.WriteLine($"Chain valid: {isValid}  <-- false is correct for self-signed");
        foreach (var status in chain.ChainStatus)
            Console.WriteLine($"  {status.Status}: {status.StatusInformation.Trim()}");
        Console.WriteLine("UntrustedRoot is expected -- no CA vouches for this certificate.");
        Console.WriteLine("This is exactly the check that produces a browser warning page.");

        // --- Export formats ---
        // PFX (PKCS#12): includes the private key, password-protected.
        //   Use when moving a certificate to a system that needs to PROVE ownership.
        //   Guard it like a private key -- because it contains one.
        // CER: public key and metadata only, no password.
        //   Hand out freely to anyone who needs to verify signatures or encrypt to you.
        byte[] pfx = cert.Export(X509ContentType.Pfx, "P@ssw0rd123!");
        byte[] cer = cert.Export(X509ContentType.Cert);

        using var fromPfx = new X509Certificate2(pfx, "P@ssw0rd123!");
        using var fromCer = new X509Certificate2(cer);

        Console.WriteLine($"\n=== Export Formats ===");
        Console.WriteLine($"PFX: {pfx.Length} bytes -- re-imported, has private key: {fromPfx.HasPrivateKey}");
        Console.WriteLine($"CER: {cer.Length} bytes -- re-imported, has private key: {fromCer.HasPrivateKey}");
        Console.WriteLine("Give CER to verifiers. Guard PFX like a private key.");
    }
}
```
