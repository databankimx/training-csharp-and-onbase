# Unity.00.CommonFunctionality

## What This Is

The foundation every other project in the OnBase Unity API training set is built on. No connection logic lives here -- that's `Unity.01`. What lives here is the infrastructure everything else depends on: configuration models that describe how to connect, enumerations that name the options, secrets management that keeps credentials out of source code, and a self-contained exception type that lets the whole track stand alone.

This is deliberately a class library with no `Program.cs` and no entry point. It produces a `.dll`, not an `.exe`. Its only job is to be referenced.

---

## Why a Separate Track From CSharp.SharedLibrary

The entire `Unity.*.*` track avoids any `ProjectReference` to `CSharp.SharedLibrary`. This means any single project -- or the whole set -- can be lifted out and handed to a client, studied independently, or dropped into a completely separate codebase without dragging this solution's C# training material along. Self-contained is a deliberate design constraint, not an oversight. `DatabankException` in this project is a near-identical duplicate of `CSharp.SharedLibrary`'s version, and that duplication is intentional.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Configuration/ServiceLocation.cs` | OnBase connection settings, all four authentication modes plus session-ID reconnect |
| `Models/Configuration/OnBaseSettings.cs` | Top-level `<onBaseSettings>` config section |
| `Models/Configuration/DocPopSettings.cs` | DocPop link-generation settings |
| `Models/Configuration/IdpSettings.cs` | Hyland Identity Provider (IdP) settings for token acquisition |
| `Models/Enumerations/AuthenticationMode.cs` | The four Unity API authentication modes |
| `Models/Enumerations/FileFormat.cs` | OnBase file format IDs |
| `Models/Objects/DatabankException.cs` | This training track's own exception type |
| `HelperClasses/Extensions/RegistryExtensions.cs` | DPAPI-based decryption of registry-stored secrets |
| `HelperClasses/Extensions/TypeConversionExtensions.cs` | String -> `LicenseType` / file-type-ID conversions |

---

## The Four Authentication Modes

The Unity API exposes five `AuthenticationProperties`-derived types. Four of them map to `AuthenticationMode` members:

| `AuthenticationMode` | Unity API type | What it requires |
|---|---|---|
| `OnBaseCredentials` (default) | `OnBaseAuthenticationProperties` | `Username`, `Password` |
| `DomainCredentials` | `DomainAuthenticationProperties` | Nothing -- uses the Windows identity the process is already running as |
| `AccessToken` | `AccessTokenAuthenticationProperties` | `AccessToken`, or blank (obtained from the IdP at connect time via `IdpSettings`) |
| `SingleSignOn` | `SingleSignOnAuthenticationProperties` | `LicenseToken` |

The fifth Unity API type, `SessionIDAuthenticationProperties`, is deliberately **not** a member of `AuthenticationMode`. A session-ID reconnect isn't a way of establishing new credentials -- it's an attempt to resume a previous connection made via one of the four modes above. If reconnection fails and you need to fall back to a fresh session, `AuthenticationMode` still has to be holding one of those four real modes to know how to reconnect. `SessionId` is an independent optional property on `ServiceLocation` instead, and `Unity.01`'s `Connect()` handles the two separately.

---

## ServiceLocation: Configuration Schema

`ServiceLocation` is a `ConfigurationElement` -- it maps to an XML element in `App.config`. A complete `<serviceLocation>` for each mode:

**OnBaseCredentials:**
```xml
<serviceLocation
    applicationId="GUID-HERE"
    servicePath="http://onbase-server/AppServer/Service.asmx"
    dataSource="OnBaseDB"
    licenseType="Default"
    authenticationMode="OnBaseCredentials"
    username="enc:..." 
    password="enc:..." />
```

**DomainCredentials:**
```xml
<serviceLocation
    applicationId="GUID-HERE"
    servicePath="http://onbase-server/AppServer/Service.asmx"
    dataSource="OnBaseDB"
    licenseType="Default"
    authenticationMode="DomainCredentials" />
```
No username or password -- NT authentication uses whatever Windows identity the process is running as.

**AccessToken (with IdP):**
```xml
<serviceLocation
    applicationId="GUID-HERE"
    servicePath="http://onbase-server/AppServer/Service.asmx"
    dataSource="OnBaseDB"
    licenseType="Default"
    authenticationMode="AccessToken"
    username="enc:..."
    password="enc:..." />
```
Leave `accessToken` blank and pair with an `<idpSettings>` element -- the token is obtained at connect time.

**SingleSignOn:**
```xml
<serviceLocation
    applicationId="GUID-HERE"
    servicePath="http://onbase-server/AppServer/Service.asmx"
    dataSource="OnBaseDB"
    licenseType="Default"
    authenticationMode="SingleSignOn"
    licenseToken="enc:..." />
```

`PostDeserialize()` calls `Validate()` automatically when the config loads, enforcing that the right fields are present for whichever mode is configured. When building a `ServiceLocation` in code (e.g., from a settings UI) rather than loading from XML, call `Validate()` explicitly -- `PostDeserialize` only runs during XML deserialization.

---

## Secrets Management: DPAPI + Registry

Every secret that can appear in configuration (`Password`, `AccessToken`, `LicenseToken`, `IdpClientSecret`) follows the same pattern. The raw config value is either:

- A plaintext string (local dev/testing only)
- A registry reference: `registry:HKLM\SOFTWARE\DataBank\...\ASPNET_SETREG,password`

`RegistryExtensions.IsEncrypted()` detects which one by matching the `registry:...` pattern. `RegistryExtensions.DecryptRegistryKey()` reads the actual secret from the DPAPI-encrypted registry value (populated ahead of time via `aspnet_setreg.exe`). Every secret property has a matching `Decrypted...` property that handles either case transparently:

```csharp
public string DecryptedPassword
{
    get
    {
        decryptedPassword = Password.IsEncrypted()
            ? Password.DecryptRegistryKey()
            : Password;
        return decryptedPassword;
    }
}
```

Consuming code should **always** read the `Decrypted...` property, never the raw one. The raw value might be plaintext or might be a registry reference -- the `Decrypted...` property is what makes that transparent.

This pattern was deliberately extended to cover every secret in the class, not just `Password`. An access token or license token sitting in plaintext in an XML file would undermine the entire point.

---

## IdpSettings: Obtaining a Token, Not Just Presenting One

`ServiceLocation.AccessToken` handles the case where a caller already has a Hyland IdP token. `IdpSettings` handles the other case: obtaining one by calling the IdP's own OAuth2 token endpoint. It sits as a separate, optional sibling element under `<onBaseSettings>`:

```xml
<idpSettings
    idpUrl="https://idp.example.com"
    idpTenant="your-tenant"
    idpClientId="your-client-id"
    idpClientSecret="registry:..."
    idpScope="evolution"
    idpGrantType="password" />
```

Validation of whether the IdP settings are actually complete happens at runtime in `Unity.01`'s `IdpAuthentication.GetAccessToken()`, not here -- this class has no visibility into `ServiceLocation.AuthenticationMode`, a sibling element, so it can't know at deserialization time whether IdP settings will even be needed.

---

## RegistryExtensions Was Changed From `internal` to `public`

Originally `internal`, since only `ServiceLocation` and `IdpSettings` consumed it, both in the same assembly. Changed to `public` once `Unity.06.UnityFormDefaultValues` needed to reuse the same DPAPI/registry-encryption pattern for its own signing token, rather than reimplementing identical logic in a separate project.

---

## Takeaways

- `AuthenticationMode` has four members, not five. `SessionId` reconnection is handled separately as an independent property.
- Every secret has a `Decrypted...` property. Always use it, never the raw property.
- `PostDeserialize` runs `Validate()` automatically for XML-loaded configs. Call `Validate()` directly for configs built in code.
- `RegistryExtensions.IsEncrypted()` and `DecryptRegistryKey()` are the two extension methods backing all secret decryption.
- This project is self-contained by design. No reference to `CSharp.SharedLibrary`.
