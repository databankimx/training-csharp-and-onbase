# Digital Signatures Deep Dive

## Introduction

The main lesson used RSA to hide a message. This lesson covers RSA's other big job: proving who sent a message and that nobody tampered with it along the way. It's a genuinely different use of the same key pair, with the roles flipped.

---

## The Big Idea: Keys Swap Roles

```
Encrypting: public key locks it, private key unlocks it
Signing:    private key signs it, public key checks it
```

This trips people up constantly, so it's worth memorizing directly: for encryption, the public key is what you use to protect something. For signing, it's the *private* key that does the signing, and the public key just checks the result. A signed message is still completely readable by anyone; signing doesn't hide anything, it just proves who wrote it and that it hasn't changed.

---

## Signing and Checking a Message

```csharp
byte[] signature = sender.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
bool isValid = verifier.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
```

The sender signs with their own private key. Anyone who has the sender's *public* key (which is meant to be shared freely) can then check that signature, without ever needing the private key themselves.

---

## What Breaks a Signature

Two things, both shown in this lesson:

1. **Changing the message** after it was signed, even one character.
2. **Changing the signature itself**, corrupted or forged.

Either one makes verification fail. Both the message and its signature have to arrive exactly as they were when signed.

---

## A Faster Alternative: HMAC

```csharp
using var hmac = new HMACSHA256(sharedSecretKey);
byte[] mac = hmac.ComputeHash(data);
```

If both sides already have a shared secret key (not a public/private pair, just one key both of you already know), HMAC does something similar to a signature, much faster, no certificates needed. The catch: you both need that shared key ahead of time, which brings back the same "how do we safely share a key" problem covered in the encryption lesson. RSA signatures don't have that problem, since verifying only ever needs a public key.

**Rule of thumb**: use HMAC when you already share a secret with the other side. Use an RSA signature when you don't, when you're verifying something from someone you have no prior shared secret with.

---

## Try It Yourself

Run `DetectingTamperedData()` and `DetectingTamperedSignature()` back to back and notice both fail verification, for different reasons, either half of a signed message being wrong is enough to break the whole thing.
