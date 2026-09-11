# Lecture Notes: RestApi.00.CommonFunctionality

## Scope: Identical Application, Different API

`RestApi.TestHarness` (planned) mirrors `Unity.TestHarness` exactly: same six pages (Connect, Taxonomy, Retrieval, Archiving, Settings, Help), same functionality, no Notes/Locks or other REST-API-only capability added. Where the two APIs genuinely can't do the same thing, that's called out explicitly below and in the affected class's own Training Notes, rather than silently dropped or awkwardly faked.

---

## net10.0, Not net48

Unlike the `Unity.*.*` track, nothing in the `RestApi.*.*` track depends on `Hyland.Unity`, a .NET-Framework-only assembly. The REST API is plain HTTP. There's no architectural reason to stay on net48 for this track, so it targets net10.0, with modern `HttpClient`/`System.Text.Json` patterns where relevant (connection logic, in `RestApi.01`, will use these; this project itself is mostly configuration types).

`System.Configuration.ConfigurationManager` (via its NuGet package, which supports modern .NET) is still used for the same App.config-style `ConfigurationSection`/`ConfigurationElement` pattern Unity.00 uses, so Settings' "Save to config" behavior can work the same way across both the Unity API and REST API versions of the harness. Secret protection uses `Microsoft.AspNetCore.DataProtection`'s `IDataProtector` rather than Unity.00's DPAPI-via-registry approach — see the dedicated section below.

---

## Self-Contained, Deliberately

The whole `RestApi.*.*` track avoids depending on the `Unity.*.*` track, the same reasoning `Unity.00.CommonFunctionality` itself documents for avoiding `CSharp.SharedLibrary`: so it can be studied, copied, or handed to a client entirely on its own. `DatabankException` is a self-contained duplicate of Unity.00's own version, not an oversight.

---

## Secret Protection: IDataProtector, Not DPAPI-via-Registry

Unity.00.CommonFunctionality's `RegistryExtensions` uses raw DPAPI (`CryptProtectData`/`CryptUnprotectData` via P/Invoke), with secrets stored in the registry via `aspnet_setreg.exe` and referenced from config as `"registry:key,value"`. Once this track was no longer tied to net48, there was no reason to keep that mechanism: `Microsoft.AspNetCore.DataProtection`'s `IDataProtector` is the actively-maintained, officially-recommended modern equivalent. Despite the ASP.NET Core-sounding package name, it works fine in any .NET app.

**Package split worth knowing about**: the base `Microsoft.AspNetCore.DataProtection` package only provides the `IDataProtectionProvider`/`IDataProtector` interfaces and the DI-container-based `AddDataProtection()` setup (for ASP.NET Core apps with a service collection to hang it off of). The standalone, non-DI factory method actually used here, `DataProtectionProvider.Create(DirectoryInfo)`, lives in a *separate* package, `Microsoft.AspNetCore.DataProtection.Extensions`. Referencing only the base package produces a `CS0103: The name 'DataProtectionProvider' does not exist in the current context` error, easy to misread as a missing `using` when it's actually a missing package reference. `RestApi.00.CommonFunctionality.csproj` references `.Extensions` directly (it pulls in the base package transitively).

`DataProtectionExtensions` (replacing what would otherwise be a `RegistryExtensions` port) is a genuine improvement, not just a rename:

1. **No more registry indirection.** The protected value IS the config value (a `"protected:"` prefix followed by the protected Base64 payload). No separate registry key to manage, no `aspnet_setreg.exe` to run, no read permissions to grant to an application pool identity.
2. **Encryption can happen from within the app now**, not just decryption. `aspnet_setreg.exe` was a separate, manually-run external tool; there was no way to encrypt a *new* secret from inside `Unity.TestHarness` itself. `RestApi.TestHarness`'s Settings page can genuinely protect secret fields before writing them to config, on Save — a capability the old mechanism never had.

Key storage is `%LOCALAPPDATA%\DataBank\RestApi.TestHarness\DataProtection-Keys`, a persistent, per-Windows-user location. This mirrors DPAPI's own user-profile-scoped nature: a protected value from this machine, for this Windows user, only decrypts on that same machine, for that same user, same as before.

One shared purpose string (`"RestApi.00.CommonFunctionality.SecretProtection"`) is used across every secret field in this project (Password, IdpClientSecret, AccessToken, LicenseToken), rather than one purpose per field. That's a deliberate simplification: nothing in this training set's threat model calls for separating them, and it keeps `ServiceLocation`/`IdpSettings` from needing to know the field name at protect/unprotect time.

---

## AuthenticationMode: Same Names, Different Meaning

`AuthenticationMode` keeps Unity API's four member names (`OnBaseCredentials`/`DomainCredentials`/`AccessToken`/`SingleSignOn`) for UI/feature parity with `RestApi.TestHarness`'s Settings page. But the REST API's auth model is fundamentally different: everything goes through the Hyland Identity Provider (IdP) via OAuth2, there's no direct equivalent of Unity API's four separate `AuthenticationProperties`-derived types. Each member maps to a different OAuth2 grant type:

- **`OnBaseCredentials`** → OAuth2 "password" grant. The only mode implemented. Closest functional match to what this mode does in the Unity API version: credentials entered directly, no browser redirect.
- **`DomainCredentials`** → stubbed. No documented Hyland IdP grant type equivalent to NT/Windows-integrated authentication was found in the SDK documentation reviewed for this training set.
- **`AccessToken`** → stubbed, even though it would be trivial to implement (a user-supplied token needs no grant flow at all, it's used directly as the Bearer token). Stubbed anyway for consistency with the other non-`OnBaseCredentials` modes.
- **`SingleSignOn`** → stubbed. Maps conceptually to "Authorization Code with PKCE", the grant type the Authentication guide recommends for SSO-capable user-facing apps. Not implemented: it requires a browser-based redirect flow, a bigger addition than a straight credential swap.

---

## ServiceLocation: What Changed, and Why

Adapted from `Unity.00.CommonFunctionality`'s own `ServiceLocation`, but genuinely reshaped:

- **`ApiServerUrl`** replaces `ServicePath`: a single combined base URL (matching the OpenAPI spec's own `{protocol}://{server}/{product}` servers block), the same "one string" approach `ServicePath` itself took.
- **`LicenseType` and `ApplicationId`** (the Unity Integration GUID) are dropped entirely. Neither concept appears anywhere in the Document Management API documentation reviewed for this training set (Getting Started, Authentication, Document Retrieval, Creating and Executing Document Queries, Upload and Archive Interactions, Working with Keywords). OnBase licensing for REST API access appears to be handled server-side, transparent to the client.
- **`SessionId`/`AllowSessionFailover` are dropped — a genuine feature gap, not an oversight.** The REST API's session is carried by the `Cookie.Session.OnBase.Hyland` cookie the Document Management API itself issues on the first authenticated request, not a value the client can supply up front to reconnect to a specific prior session the way Unity API's SessionID authentication properties allow. There is no documented REST equivalent of `Unity.TestHarness`'s "Reconnect to Session ID" feature. `RestApi.TestHarness`'s own Connect page will need to omit that control rather than fabricate a mechanism that isn't documented anywhere.
- **`KeepAlive` is kept, but its meaning shifts.** Unity API's `KeepAlive` means "don't disconnect this session when this connection ends" (`IsDisconnectEnabled = false`), a one-time connection property. Here it means "send an explicit heartbeat call periodically", since a REST API session's cookie expires after 5 minutes of inactivity regardless (per the Authentication guide's own Heartbeat section) — keeping a session alive is an ongoing runtime behavior, not something to configure at connect time. The actual heartbeat timer will live in `RestApi.01.ConnectingToOnBase`'s `SessionManagement`.

---

## RestApiSettings: No DocPop

`Unity.00.CommonFunctionality`'s `OnBaseSettings` has a `DocPopSettings` element, used by `Unity.TestHarness`'s Retrieval page for "Open DocPop"/"Open UnityPop" links. Nothing in the Document Management API documentation reviewed for this training set mentions a DocPop or UnityPop equivalent. `RestApiSettings` omits it entirely — another genuine, worth-flagging feature gap, matching `ServiceLocation`'s own SessionId omission. `RestApi.TestHarness`'s Retrieval page will need to omit those two link buttons.

---

## Source Material

The REST API research behind this project's design came from six pages saved from the Hyland SDK Portal (`sdk.onbase.com`, login-gated, `robots.txt`-blocked for automated access — hence saved copies rather than live fetches) — Getting Started, Authentication & the Document Management API, Document Retrieval, Creating and Executing Document Queries, Upload and Archive Interactions, and Working with Keywords on Documents — plus the `document-api.json` OpenAPI 3.0 spec itself (46 endpoints, 113 schemas). Key architectural takeaways that shaped this project specifically:

- The Document Management API lives on a separate component (the OnBase API Server), not the Application Server the Unity API connects through.
- Getting a bearer token from the IdP does not itself create an OnBase session or consume a license — the first authenticated request to the Document Management API does that, returning the session cookie described above.
- Uploads and partial content retrieval go through a separate Temp File Storage service — relevant to `RestApi.04.DocumentArchiving`, not this project, but worth noting here as it shaped the overall scoping conversation.
