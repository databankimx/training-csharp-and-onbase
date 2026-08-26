# Certificates Deep Dive

## Introduction

The main lesson created the simplest possible certificate. This lesson covers what real certificates actually contain and do: extensions that declare their purpose, the right way to export them for different audiences, the Windows certificate store, and how "trust" actually gets checked.

---

## Extensions: Declaring What a Certificate Is For

```csharp
new X509BasicConstraintsExtension(certificateAuthority: false, ...)
new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, ...)
```

**Basic Constraints** says whether a certificate is allowed to issue *other* certificates (a Certificate Authority) or not. **Key Usage** says which operations its key is allowed for, signing, encrypting, or both. Real certificates always declare this, it's what stops a stolen or misused certificate from being used for something it was never meant for.

---

## Two Export Formats, Two Very Different Audiences

```csharp
var pfx = cert.Export(X509ContentType.Pfx, "password");   // has the PRIVATE key
var cer = cert.Export(X509ContentType.Cert);               // public only, no password
```

A **PFX** file contains the private key and needs a password because of it. A **CER** file has only the public parts, safe to hand to anyone.

**The rule**: give the CER to anyone who just needs to verify or encrypt to you. Never give the PFX to anyone but the system that will actually act as you, whoever has it can fully impersonate that identity.

---

## The Certificate Store

```csharp
var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
store.Open(OpenFlags.ReadWrite);
store.Add(certificate);
```

Instead of managing loose certificate files, Windows keeps a proper store you can install into and search, by thumbprint or other details, no file path needed. `CurrentUser` doesn't need admin rights; `LocalMachine` usually does.

---

## How "Trust" Gets Checked

```csharp
var chain = new X509Chain();
bool valid = chain.Build(certificate);
// A self-signed cert reports: UntrustedRoot
```

Checking whether a certificate should be trusted means walking up from it through whoever issued it, then whoever issued *that*, until you reach something already trusted, or run out of chain. A self-signed certificate's chain is just itself, and since nothing vouches for it, it comes back "untrusted." This is exactly why browsers warn about self-signed certificates: there's genuinely nothing tying them to anything already trusted.

---

## Try It Yourself

Run `ValidatingACertificateChain()` and read the `ChainStatus` output. `UntrustedRoot` isn't an error in the code, it's the honest, correct answer for a self-signed certificate with no CA behind it.
