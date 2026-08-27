# Ch12 Textbook Code: Chapter 12 (Encryption Samples)

## What This Is

The textbook's own combined Chapter 12 sample project: five standalone encryption/hashing/data-protection demos, one per file, organized into `Symmetric/`, `Asymmetric/`, `Hashing/`, and `ProtectingData/` folders. Unlike `CSharp.Ch08.TextbookCode.Chapter8`/`CSharp.Ch09.TextbookCode.Chapter9` (empty `Main()`, dead reference-only code), this project's `Main()` genuinely calls all five `Run()` methods directly, one after another, this is real, executed code, which is exactly why the bugs below were worth fixing rather than just documenting.

---

## A Real Bug: `TripleDESSample`'s Mislabeled Output

```csharp
public static void Run() {
    string input = "Data to be TrippleDES Encrypted!";
    ...
    Console.WriteLine("Symmetric AesManaged");   // bug: this is the TripleDES sample!
```

`TripleDESSample.Run()` printed `"Symmetric AesManaged"`, the exact same header `AesManagedSample.Run()` prints, clearly copy-pasted from that file and never updated to describe this one. Genuinely confusing when running the project end to end, the output for the TripleDES demo claimed to be showing AES. **Fixed** to print `"Symmetric TripleDES"` instead.

---

## A Real Bug: `RSASample` Leaves Permanent Key Material Behind

```csharp
using (var rsa = new RSACryptoServiceProvider(cspParams)) {
    rsa.PersistKeyInCsp = true;   // writes a REAL, permanent key to the Windows CSP store
    ...
    rsa.Clear();                  // does NOT remove it, PersistKeyInCsp is still true
}
```

`PersistKeyInCsp = true` tells the underlying Cryptographic Service Provider to write this key pair to Windows' actual key store, keyed by `"MyKeyContainer"`, and that write survives the process exiting entirely, `rsa.Clear()` releases the in-memory handle but does *not* remove the persisted container. The original download never cleaned this up: running this sample would leave real, permanent RSA key material sitting on whoever's machine ran it, growing by one (identically-named, silently overwritten) container every single run, indefinitely.

**Fixed** by adding `CleanupPersistedKeyContainer()`, called at the end of `Run()`:

```csharp
static void CleanupPersistedKeyContainer(string containerName)
{
    var cspParams = new CspParameters { KeyContainerName = containerName };
    using (var rsa = new RSACryptoServiceProvider(cspParams))
    {
        rsa.PersistKeyInCsp = false;   // setting this to false BEFORE Clear()/disposal
        rsa.Clear();                   //   tells the CSP to actually DELETE the container
    }
}
```

Worth internalizing the pattern generally: setting `PersistKeyInCsp = false` on a handle that's opened against an *existing* persisted container, then clearing/disposing it, is the standard, documented way to remove that container again. This matters well beyond this one sample: any code that persists a CSP key container for real (not just as a demo) needs an equally deliberate cleanup path, or it accumulates key material on every machine it ever ran on.

---

## A Real Bug: `ProtectDataSample` Printed `"System.Byte[]"` Instead of the Actual Data

```csharp
Console.WriteLine("Protected: {0}", encrypted);   // encrypted is a byte[]
```

`string.Format`/`Console.WriteLine`'s `{0}` placeholder calls `.ToString()` on whatever's passed in, and `byte[].ToString()` returns the unhelpful `"System.Byte[]"`, not any representation of the actual bytes. Every *other* sample in this project correctly wraps its byte array output in `Convert.ToBase64String()` before printing it; this one simply didn't. **Fixed** to match the other four samples' convention.

---

## Worth Knowing, Not a Bug: `SHA1Sample.cs`'s Naming Doesn't Match Its Content

```csharp
// file: Hashing/SHA1Sample.cs
public class SHASample {
    static string ComputeHash(string input) {
        HashAlgorithm sha = SHA256.Create();   // NOT SHA1!
```

The file is named `SHA1Sample.cs`, and the class inside is named generically `SHASample`, but the actual algorithm used is `SHA256`, not SHA-1 at all. This is *not* a functional bug worth fixing, SHA-256 is the objectively stronger, more correct choice (SHA-1 is cryptographically broken and shouldn't be used for new work), it's purely a leftover naming inconsistency: the sample was very likely written or renamed at some point without the file/class names being updated to match. Left exactly as downloaded, worth recognizing if you're ever navigating this codebase by file name and land somewhere unexpected.
