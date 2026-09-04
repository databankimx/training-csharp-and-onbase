# Unity.06.UnityFormDefaultValues

## What This Is

A standalone demo of OnBase's Unity Form default-value URL signing technique. See `README.md` for the how-to-run steps.

---

## `Token` Is a Real Secret, Treated Like One

`Token` is an HMAC-SHA256 signing key: `GenerateHash()` uses it to compute a tamper-evident hash over the pre-populated field values, and that hash is what lets the receiving Unity Form trust the values embedded in the URL actually came from a legitimate source, rather than being forged by anyone who guessed at the URL format. Whoever holds `Token` can construct valid signed URLs for this integration, exactly the kind of value that shouldn't sit hardcoded in source control.

This project now reuses `Unity.00.CommonFunctionality`'s `RegistryExtensions` directly:

```csharp
string token = rawToken.IsEncrypted() ? rawToken.DecryptRegistryKey() : rawToken;
```

The same `IsEncrypted()`/`DecryptRegistryKey()` pair `ServiceLocation.DecryptedPassword`/`DecryptedAccessToken` and `IdpSettings.DecryptedIdpClientSecret` already use, `registry:HKLM\...,Token` in `App.config` for a DPAPI-protected value, or a plain string for local testing. `RegistryExtensions` itself was changed from `internal` to `public` in `Unity.00.CommonFunctionality` specifically to make this reuse possible, it previously had no consumer outside that project's own assembly.

---

## Try It Yourself

Configure a real `BaseUrl`/`Token` pair against your own OnBase environment, run the project, and open the generated URL, confirm the target form actually opens with `FormFields`' values pre-populated. Then try tampering with the generated URL's parameter values by hand (without regenerating the hash) and confirm the form correctly rejects it, that's the whole point of the signature.
