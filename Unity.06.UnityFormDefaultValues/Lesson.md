# Unity.06.UnityFormDefaultValues

## What This Is

Generates a shared Unity Form URL with pre-populated field default values and an HMAC-SHA256 signature that authenticates them. This is the technique OnBase's Unity Form integrations use to let an external system hand a user a working, pre-filled form link -- the receiving form can verify the embedded values weren't tampered with because only holders of the signing key can produce a valid hash.

This is a standalone console application (`Program.cs`), unlike the class libraries that make up most of this training set.

---

## How It Works

```
FormFields  -->  CreateParameterString  -->  parameterString
                                                   |
Token  -->  GenerateHash(token, parameterBytes)  -->  hash
                                                          |
BaseUrl + parameterString + hash  -->  GenerateUrl  -->  URL  -->  browser
```

1. Build a parameter string from the field IDs and values you want pre-populated.
2. Sign the parameter string with an HMAC-SHA256 hash using the integration's signing key.
3. Append the hash to the URL as `&ufprehash=...`.
4. Open the URL in a browser -- the receiving Unity Form reads the field values and the hash, recomputes the hash from the values using its own copy of the key, and only pre-populates the fields if the hashes match.

---

## The URL Format

Each field to pre-populate becomes a `ufpre{fieldId}={value}` parameter:

```
https://onbase-server/AppServer/UnityForm.aspx?lcid=1033&formid=12345
  &ufprelawId=2
  &ufprehash=<HMAC-SHA256-signature>
```

Field IDs come from the Unity Form template configuration in OnBase Studio. The `ufpre` prefix is the OnBase convention for "this is a pre-populated default value parameter."

---

## The Code

```csharp
// Build the parameter string
private static string CreateParameterString(List<KeyValuePair<string, string>> fields)
{
    var builder = new StringBuilder();
    foreach (var field in fields)
    {
        builder.Append($"&ufpre{Uri.EscapeDataString(field.Key)}={Uri.EscapeDataString(field.Value)}");
    }
    return builder.ToString();
}
```

`Uri.EscapeDataString` percent-encodes each key and value. Field IDs and values containing `&`, `=`, or spaces would corrupt the URL without this.

```csharp
// Sign with HMAC-SHA256
private static string GenerateHash(string token, byte[] parameterBytes)
{
    byte[] tokenBytes = Convert.FromBase64String(token);
    using var hmac = new HMACSHA256(tokenBytes);
    var hashBytes = hmac.ComputeHash(parameterBytes);
    return Uri.EscapeDataString(Convert.ToBase64String(hashBytes));
}
```

`Token` is stored as a Base64-encoded byte array (matching how OnBase expects signing keys to be configured). The HMAC is computed over the UTF-8 bytes of the parameter string, then Base64-encoded and percent-encoded for safe URL embedding.

---

## Configuration

`App.config` holds both settings:

```xml
<appSettings>
  <add key="BaseUrl" value="https://onbase-server/AppServer/UnityForm.aspx?lcid=1033&amp;formid=12345" />
  <add key="Token" value="registry:HKLM\SOFTWARE\DataBank\...\ASPNET_SETREG,Token" />
</appSettings>
```

`Token` can be either:
- A plain Base64 string (local dev/testing only)
- A registry reference (`registry:HKLM\...`) pointing to a DPAPI-encrypted value

The code handles both transparently via `RegistryExtensions.IsEncrypted()`/`DecryptRegistryKey()` from `Unity.00.CommonFunctionality`:

```csharp
string rawToken = ConfigurationManager.AppSettings["Token"];
string token = rawToken.IsEncrypted() ? rawToken.DecryptRegistryKey() : rawToken;
```

This is the same DPAPI/registry-encryption mechanism `ServiceLocation.DecryptedPassword` and `IdpSettings.DecryptedIdpClientSecret` use. `RegistryExtensions` was changed from `internal` to `public` in `Unity.00.CommonFunctionality` specifically so this project could reuse it rather than reimplementing identical logic.

---

## Why Token Is a Real Secret

`Token` is an HMAC signing key. Whoever holds it can construct valid signed URLs for this integration. A URL with a valid hash will be trusted by the receiving Unity Form -- it will pre-populate fields with whatever values that URL contains. This is the signature that prevents tampering, but only from people who don't have the key. Treat the token the same way you'd treat a password.

`BaseUrl` and `Token` were originally hardcoded constants in the source file. Both moved to `App.config` during the port, with the DPAPI/registry encryption path available for `Token` specifically.

---

## How to Run

1. Update `App.config`'s `BaseUrl` to a real shared Unity Form URL from your OnBase environment.
2. Update `App.config`'s `Token` to the matching integration's signing key (plain for local testing, `registry:...` for anything resembling a real deployment).
3. Update `FormFields` in `Program.cs` to whatever field ID/value pairs you want pre-populated.
4. Press F5. The URL opens in the default browser.

---

## Try It Yourself

Configure a real `BaseUrl`/`Token` pair against your own OnBase environment, run the project, and confirm the target form opens with `FormFields`' values pre-populated. Then tamper with the generated URL's parameter values by hand (without regenerating the hash) and confirm the form correctly rejects them -- that's the entire point of the signature.

---

## Takeaways

- The `ufpre{fieldId}` parameter prefix is OnBase's convention for Unity Form pre-populated default values.
- `Uri.EscapeDataString` is required on both keys and values -- special characters in either corrupt the URL.
- `Token` is a Base64-encoded HMAC-SHA256 signing key. It's a real secret that belongs in encrypted configuration, not source code.
- The same `RegistryExtensions.IsEncrypted()`/`DecryptRegistryKey()` pattern from `Unity.00` handles the `Token` secret here too.
- The receiving Unity Form recomputes the hash from the field values using its own copy of the key. A tampered URL won't match.
