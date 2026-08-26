# Chapter 12 Supplemental 01: Digital Signatures Deep Dive

## What This Is

The main lesson used RSA for encryption, hiding a message's contents. This Supplemental covers RSA's other major job: signing, proving a message really came from the holder of a specific private key, and that it wasn't altered after signing. The two use RSA's key pair in genuinely opposite roles, worth getting straight before anything else here.

---

## The Core Distinction: Opposite Key Roles

```
ENCRYPTING: encrypt with RECIPIENT's public key  ->  decrypt with RECIPIENT's private key
SIGNING:    sign with SENDER's private key       ->  verify with SENDER's public key
```

"Public key encrypts, private key decrypts" is only true for encryption. For signing it flips entirely: the *private* key signs, and the *public* key verifies. Same key pair, opposite roles depending on which operation is happening. This is a genuinely common point of confusion worth internalizing directly rather than pattern-matching "public key = safe to share, does the safe operation" onto both cases; a signed message is still completely readable by anyone, signing proves authorship and integrity, it does nothing at all to hide contents.

---

## A Basic Sign/Verify Round Trip

```csharp
byte[] signature = sender.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
bool isValid = verifier.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
```

Under the hood, `SignData()` hashes the data (SHA-256 here) and then encrypts *that hash* with the private key; `VerifyData()` independently hashes the data it received, decrypts the signature with the public key to recover the original hash, and checks the two match. Both sides need to agree on the same hash algorithm and padding scheme, mismatching either produces a failed verification even for otherwise-correct data and signature.

---

## Two Ways Verification Fails, Both Demonstrated

```csharp
// Data changed after signing:
bool tamperedIsValid = rsa.VerifyData(tamperedData, signature, ...);   // false

// Signature changed (corrupted or forged):
bool isValid = rsa.VerifyData(data, tamperedSignature, ...);           // false
```

Either the original data or the signature itself being altered, even by a single byte, breaks verification. Both have to arrive exactly as they were at signing time. Worth noticing this gives a signature real value as an integrity check even independent of who signed it: any tampering in transit, accidental corruption or deliberate forgery, is detectable.

---

## HMAC: A Faster, Symmetric Alternative

```csharp
using var hmac = new HMACSHA256(sharedSecretKey);
byte[] mac = hmac.ComputeHash(data);
```

HMAC (Hash-based Message Authentication Code) achieves a similar goal, proving data wasn't tampered with, using a shared secret key and a hash function instead of RSA's asymmetric key pair. It's considerably faster than an RSA signature and needs no certificate or key-pair infrastructure at all. The tradeoff: **both parties need to already share the same secret key**, exactly the key distribution problem symmetric encryption has (see the main lesson). RSA signatures sidestep that problem entirely, since verifying only ever needs the sender's *public* key, never a secret both sides had to somehow agree on in advance.

The practical rule: reach for HMAC when both sides already share a key (an internal service-to-service API with a pre-shared secret, for instance), reach for RSA signatures when the verifier and signer are genuine strangers with no pre-existing relationship (verifying a downloaded file was really published by who it claims, for instance).

---

## A Note on Padding: `Pkcs1` vs. `Pss`

Every signature example here uses `RSASignaturePadding.Pkcs1`, the older, universally-supported padding scheme. `RSASignaturePadding.Pss` is the more modern, generally preferred choice where available, but support for it varies more across providers and platforms than `Pkcs1` does (a real, concrete example of exactly this class of platform difference is documented in the main lesson's `LectureNotes.md`, where `RSAEncryptionPadding.OaepSHA256` turned out to be unsupported by classic .NET Framework's default RSA provider). Worth checking directly against whatever platform you're actually targeting before committing to `Pss` in real code, rather than assuming it's universally available.
