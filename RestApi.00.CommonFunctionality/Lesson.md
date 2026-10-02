# RestApi.00.CommonFunctionality

## What This Is

The foundational library for the OnBase Document Management (REST) API training set -- configuration models and secret protection, no connection logic. The REST-API counterpart to `Unity.00.CommonFunctionality`, shaped to produce an identical application (`RestApi.TestHarness`) to `Unity.TestHarness`, just talking to the REST API instead of the Unity API.

`net10.0`, not `net48`. Nothing here depends on `Hyland.Unity` (a .NET-Framework-only assembly). The REST API is plain HTTP.

Deliberately self-contained -- no reference to the `Unity.*.*` track. `DatabankException` here is a local copy, matching `Unity.00`'s own precedent.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Configuration/ServiceLocation.cs` | Connection settings for the OnBase API Server -- reshaped from Unity's version, not just renamed |
| `Models/Configuration/IdpSettings.cs` | Hyland IdP settings -- close port of Unity's own version |
| `Models/Configuration/RestApiSettings.cs` | Top-level `<restApiSettings>` config section -- no DocPop element |
| `Models/Enumerations/AuthenticationMode.cs` | Same four names as Unity's version, mapped to OAuth2 grant types |
| `Models/Objects/DatabankException.cs` | Self-contained duplicate |
| `HelperClasses/Extensions/DataProtectionExtensions.cs` | Secret protect/unprotect via `Microsoft.AspNetCore.DataProtection` |

---

## The Document Management API

The OnBase Document Management API lives on the **API Server**, a separate component from the Application Server the Unity API connects through. Its base URL pattern: `{protocol}://{server}/{product}`, e.g. `https://onbase-server/apiserver/onbase/core`.

The OnBase REST surface is actually several separate APIs -- Document Management, Forms, Admin, Workflow -- each with its own `{product}` path segment, all served by the same server, all honoring the same Bearer token. `ServiceLocation` carries two base URLs:

```xml
<serviceLocation
    apiServerUrl="https://onbase-server/apiserver/onbase/core"
    formsApiUrl="https://onbase-server/apiserver/onbase/forms"
    authenticationMode="OnBaseCredentials"
    username="protected:..."
    password="protected:..." />
```

`formsApiUrl` is optional -- only needed by `RestApi.02.AccessingTaxonomy`'s Unity Form methods.

---

## AuthenticationMode: Same Names, Different Meaning

Four members matching Unity's enum exactly, for UI/feature parity with the Settings page. But the REST API's auth model is fundamentally different -- everything goes through the Hyland Identity Provider (IdP) via OAuth2. Each member maps to an OAuth2 grant type:

| Member | OAuth2 grant type | Status |
|---|---|---|
| `OnBaseCredentials` | "password" (Resource Owner Password Credentials) | Implemented |
| `DomainCredentials` | No documented IdP equivalent found | Stubbed |
| `AccessToken` | Token used directly as Bearer (no grant flow) | Stubbed for consistency |
| `SingleSignOn` | Authorization Code with PKCE | Stubbed -- requires browser redirect |

Only `OnBaseCredentials` is currently implemented. The others throw `NotImplementedException` in `RestApi.01.ConnectingToOnBase`'s `SessionManagement`.

---

## ServiceLocation: What Changed From Unity's Version

Adapted from `Unity.00.CommonFunctionality`'s `ServiceLocation`, but genuinely reshaped:

**Dropped:**
- `LicenseType` and `ApplicationId` -- neither appears in the Document Management API documentation. Licensing appears server-side, transparent to the client.
- `SessionId`/`AllowSessionFailover` -- a genuine feature gap. The REST API's session is carried by a `Cookie.Session.OnBase.Hyland` cookie the API issues on first authenticated request. There's no documented way to reconnect to a specific prior session the way Unity API's `SessionIDAuthenticationProperties` allows. `RestApi.TestHarness`'s Connect page omits the "Reconnect to Session ID" control.

**Changed:**
- `ServicePath` → `ApiServerUrl` -- the REST API's full base URL directly.
- `KeepAlive` -- kept, but its meaning shifts. Unity's `KeepAlive` means `IsDisconnectEnabled = false`, a one-time connect-time flag. Here it means "send an explicit heartbeat call periodically," because the REST API session cookie expires after 5 minutes of inactivity regardless of anything set at connect time.

**Added:**
- `FormsApiUrl` -- the Forms API's separate base URL, no Unity equivalent.

**Secret protection:** `DecryptedPassword`/`DecryptedAccessToken`/`DecryptedLicenseToken` now use `DataProtectionExtensions.IsProtected()`/`Unprotect()` instead of `RegistryExtensions.IsEncrypted()`/`DecryptRegistryKey()`.

---

## DataProtectionExtensions: Modern Secret Protection

Replaces `Unity.00`'s `RegistryExtensions` (DPAPI via P/Invoke, secrets in registry via `aspnet_setreg.exe`). The modern equivalent is `Microsoft.AspNetCore.DataProtection`'s `IDataProtector` -- despite the ASP.NET Core name, it works in any .NET app.

```csharp
// Protect a secret for storage in config
string protected = "mypassword".Protect();   // "protected:CfDJ8..."

// Check whether a config value is already protected
bool isProtected = configValue.IsProtected();

// Unprotect when reading
string plain = configValue.Unprotect();
```

Two real improvements over `aspnet_setreg.exe`:

**1. No registry indirection.** The protected value is the config value itself (a `"protected:"` prefix followed by Base64 payload). No registry key to create, no `aspnet_setreg.exe` to run, no read permissions to grant to an application pool identity.

**2. Encryption can happen from inside the app.** `aspnet_setreg.exe` was a separate external tool -- there was no way to encrypt a new secret from inside `Unity.TestHarness` itself. `RestApi.TestHarness`'s Settings page can genuinely protect secret fields on Save.

Key storage: `%LOCALAPPDATA%\DataBank\RestApi.TestHarness\DataProtection-Keys` -- per-Windows-user, matching DPAPI's own user-profile scope. A protected value from this machine, for this Windows user, only decrypts on that same machine for that same user.

**Package note:** `DataProtectionProvider.Create(DirectoryInfo)` lives in `Microsoft.AspNetCore.DataProtection.Extensions`, not the base `Microsoft.AspNetCore.DataProtection` package. Referencing only the base package produces `CS0103: The name 'DataProtectionProvider' does not exist in the current context` -- easy to misread as a missing `using` when it's actually a missing package reference.

One shared purpose string (`"RestApi.00.CommonFunctionality.SecretProtection"`) is used for all secrets. Separating by field name would add complexity with no threat-model benefit for this training set.

---

## RestApiSettings: No DocPop

`OnBaseSettings` in Unity has a `<docPopSettings>` element. Nothing in the Document Management API documentation has a DocPop or UnityPop equivalent -- `RestApiSettings` omits it, and `RestApi.TestHarness`'s Retrieval page omits those link buttons.

---

## Takeaways

- The Document Management API lives on the API Server, not the Application Server.
- The OnBase REST surface is multiple separate APIs at different `{product}` path segments, all on the same server, all sharing a Bearer token.
- `AuthenticationMode` keeps Unity's four names but maps them to OAuth2 grant types. Only `OnBaseCredentials` (the "password" grant) is implemented.
- `SessionId`/`AllowSessionFailover` are dropped -- no documented REST equivalent for reconnecting to a specific prior session.
- `KeepAlive` means heartbeat timer here, not `IsDisconnectEnabled`.
- `DataProtectionExtensions` replaces `RegistryExtensions`. Protected values live directly in config as `"protected:..."` strings; no registry involved.
- `DataProtectionProvider.Create()` requires the `.Extensions` package, not just the base package.
