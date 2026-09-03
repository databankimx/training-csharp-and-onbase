# Unity.00.CommonFunctionality

## What This Is

The foundation everything else in the OnBase Unity API training set is built on. See `README.md` for the file-by-file breakdown.

---

## `AuthenticationMode`: Four Modes, Plus an Independent Reconnect Path

`ServiceLocation` originally had one bool, `UseNTAuthentication`, a plain OnBase username/password when `false`, Windows/NT authentication when `true`. That covers exactly two of the five concrete `Hyland.Unity.AuthenticationProperties`-derived types the Unity API actually exposes:

| `AuthenticationMode` member | Hyland.Unity type | Requires |
|---|---|---|
| `OnBaseCredentials` (default) | `OnBaseAuthenticationProperties` | `Username`, `Password` |
| `DomainCredentials` | `DomainAuthenticationProperties` | Nothing, NT authentication is purely the Windows identity the process is already running as, the Unity API takes no credentials of any kind for this mode |
| `AccessToken` | `AccessTokenAuthenticationProperties` | `AccessToken`, OR nothing (a token is instead obtained from the Hyland IdP at connect time, see `IdpSettings` below) |
| `SingleSignOn` | `SingleSignOnAuthenticationProperties` | `LicenseToken` |

The fifth Unity API type, `SessionIDAuthenticationProperties`, is deliberately **not** a member of this enum at all, `ServiceLocation.SessionId` is an independent, optional property instead. See `AuthenticationMode`'s own Training Notes (in `Models/Enumerations/AuthenticationMode.cs`) for why: a session-ID reconnect isn't a way of establishing new credentials, it's an attempt to resume a previous connection made via one of the four modes above, and treating it as a fifth mode made session-failover impossible to express without risking infinite recursion.

Each enum member is named to match its `Application.Create...AuthenticationProperties()` factory method directly, so when `Unity.01.ConnectingToOnBase` builds the actual `switch` over `AuthenticationMode`, the mapping from enum value to API call is obvious rather than something a reader has to go look up. `ServiceLocation.PostDeserialize()` validates only the fields the selected mode actually needs, an XML config missing `Username`/`Password` is perfectly valid when `AuthenticationMode` is `AccessToken`.

---

## `IdpSettings`: Obtaining a Token, Not Just Presenting One

`ServiceLocation.AccessToken` covers the case where a caller already has a Hyland Identity Provider (IdP) access token in hand. `IdpSettings` (a separate, sibling element under `OnBaseSettings`, alongside `ServiceLocation` and `DocPop`) covers the other case: **obtaining** one, via the IdP's own OAuth2 token endpoint. It holds `IdpUrl`, `IdpTenant`, `IdpClientId`, `IdpClientSecret` (with the same DPAPI/registry-encryption protection as every other secret in this training set, via `DecryptedIdpClientSecret`), `IdpScope` (default `"evolution"`), and `IdpGrantType` (default `"password"`). Unlike `ServiceLocation`, this whole element is optional on `OnBaseSettings`, a config that never uses `AuthenticationMode.AccessToken` shouldn't need to fill in placeholder IdP settings it will never use, so validation of "do we actually have everything needed" happens at runtime, in `Unity.01.ConnectingToOnBase`'s `IdpAuthentication.GetAccessToken()`, not as static config-schema validation here (which has no visibility into `ServiceLocation.AuthenticationMode`, a sibling element).

---

## Every Secret Gets the Same Protection

`Password`, `AccessToken`, `LicenseToken`, and (on `IdpSettings`) `IdpClientSecret` each have a matching `Decrypted...` property (`DecryptedPassword`, `DecryptedAccessToken`, `DecryptedLicenseToken`, `DecryptedIdpClientSecret`), all four built on the exact same mechanism: `RegistryExtensions.IsEncrypted()` checks whether the raw config value looks like `registry:HKLM\...,valueName`, and if so, `DecryptRegistryKey()` reads the actual secret from a DPAPI-encrypted registry value instead (populated ahead of time via `aspnet_setreg.exe`, see `Other Resources` at the solution root). Consuming code should always read the `Decrypted...` property, never the raw `ConfigurationProperty` directly, that raw value might be plaintext or might be an encrypted-registry-value reference, and the `Decrypted...` property is what handles either case transparently.

This is deliberately extended to cover **every** secret this class (and `IdpSettings`) can hold, not just the original `Password`, an access token, license token, or IdP client secret left sitting in plaintext in an XML file would undermine the whole point.

---

## Self-Contained, Deliberately

`DatabankException` here is a near-identical duplicate of `CSharp.SharedLibrary`'s own class, on purpose. This whole `Unity.*.*` track avoids any dependency on `CSharp.SharedLibrary`, so any single project (or the whole set) can be lifted out and handed to a client, studied independently, or reused in a completely separate codebase without dragging in this solution's other training material.

---

## Try It Yourself

Read through `ServiceLocation.cs`'s `PostDeserialize()` method and try writing a `<serviceLocation>` XML block for each of the four `AuthenticationMode` values, notice which attributes each one actually requires versus ignores. Then try adding a `<sessionId>` attribute alongside an `AuthenticationMode` of your choice, and trace through how `Unity.01.ConnectingToOnBase`'s `Connect()` would use it.
