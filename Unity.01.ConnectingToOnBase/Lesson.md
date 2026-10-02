# Unity.01.ConnectingToOnBase

## What This Is

Connection and disconnection management for OnBase sessions. This is the first project in the Unity API training set that actually touches `Hyland.Unity` at runtime. Everything before this is configuration; this is where `Application.Connect()` happens.

Two classes: `SessionManagement` (connect, disconnect, reconnect) and `IdpAuthentication` (obtain a token from the Hyland IdP when one isn't supplied directly).

---

## The Connection Flow

`Connect()` follows a deliberate decision tree:

```
Connect() called
  |
  +-- Is ServiceLocation.SessionId configured?
  |     YES --> ReconnectExistingSession(sessionId, keepAlive)
  |               |
  |               +-- Reconnect succeeds --> return Application
  |               +-- Reconnect fails
  |                     |
  |                     +-- AllowSessionFailover? YES --> ConnectNewSession()
  |                     +-- AllowSessionFailover? NO  --> throw
  |
  +-- No SessionId --> ConnectNewSession()
                         |
                         +-- switch(AuthenticationMode)
                               OnBaseCredentials   --> Application.CreateOnBaseAuthenticationProperties(...)
                               DomainCredentials   --> Application.CreateDomainAuthenticationProperties(...)
                               AccessToken         --> Application.CreateAccessTokenAuthenticationProperties(...)
                               SingleSignOn        --> Application.CreateSingleSignOnAuthenticationProperties(...)
```

The key insight: `ConnectNewSession()` never touches `SessionId`. The session-ID path is checked in `Connect()` first, before `ConnectNewSession()` is even considered. This avoids the infinite-recursion trap that an earlier draft fell into when `SessionId` was a fifth `AuthenticationMode` member (fail to reconnect -> fall back to `ConnectNewSession()` -> hit the same `SessionId` case -> fail again -> repeat).

---

## The Four Authentication Modes in Code

```csharp
case AuthenticationMode.OnBaseCredentials:
    authProps = Application.CreateOnBaseAuthenticationProperties(
        ServiceLocation.ServicePath,
        ServiceLocation.DecryptedUsername,
        ServiceLocation.DecryptedPassword,
        ServiceLocation.DataSource);
    break;

case AuthenticationMode.DomainCredentials:
    authProps = Application.CreateDomainAuthenticationProperties(
        ServiceLocation.ServicePath,
        ServiceLocation.DataSource);
    break;

case AuthenticationMode.AccessToken:
    authProps = Application.CreateAccessTokenAuthenticationProperties(
        ServiceLocation.ServicePath,
        GetAccessTokenIfNeeded(),
        ServiceLocation.DataSource);
    break;

case AuthenticationMode.SingleSignOn:
    authProps = Application.CreateSingleSignOnAuthenticationProperties(
        ServiceLocation.ServicePath,
        ServiceLocation.DataSource);
    authProps.LicenseToken = ServiceLocation.DecryptedLicenseToken;
    break;
```

`DomainCredentials` takes only the URL and data source -- confirmed against the actual Unity API. There is no overload accepting a domain username or password. NT authentication is purely the Windows identity the process is already running as. If you need to connect as a different Windows user, that requires OS-level impersonation before this call, which is a separate concern this training set doesn't address.

---

## KeepAlive and IsDisconnectEnabled

```csharp
authProps.IsDisconnectEnabled = !ServiceLocation.KeepAlive;
```

`IsDisconnectEnabled = false` (i.e., `KeepAlive = true`) means the session survives past `app.Disconnect()` and can be reconnected later by session ID. This is what makes `ReconnectExistingSession` viable -- the session has to have been originally established with `KeepAlive = true` for a reconnect to succeed. A session established with `IsDisconnectEnabled = true` is gone the moment `Disconnect()` is called.

Disconnecting a kept-alive session requires a special step:

```csharp
// Before we can disconnect a maintained session, we have to reconnect
// in disconnectable mode
if (!app.IsDisconnectEnabled)
    app = ReconnectExistingSession(app.SessionID, false);
app.Disconnect();
```

The session is briefly reconnected with `IsDisconnectEnabled = true` before calling `Disconnect()`. Without this, `Disconnect()` on a kept-alive session doesn't actually release the license.

---

## LicenseType

```csharp
authProps.LicenseType = ServiceLocation.LicenseType;
```

Set after creating authentication properties for any mode. Common values: `Default` (standard named user), `QueryMetering` (concurrent use for query-heavy workflows), `Enterprise` (per the OnBase license agreement). The value here must match what's licensed in the OnBase environment -- using the wrong license type fails at connect time.

---

## Unity Integration GUID

```csharp
if (!string.IsNullOrEmpty(ServiceLocation.ApplicationId))
    authProps.IdentitySettings = authProps.CreateIdentitySettings(ServiceLocation.ApplicationId);
```

The Application GUID is configured in OnBase Studio under Unity Integrations. Setting it here lets OnBase log which application is performing which operations, useful for auditing. It's optional -- `ApplicationId` being blank skips this entirely.

---

## GetAccessTokenIfNeeded

```csharp
private static string GetAccessTokenIfNeeded()
{
    if (!string.IsNullOrEmpty(ServiceLocation.DecryptedAccessToken))
        return ServiceLocation.DecryptedAccessToken;

    return IdpAuthentication.GetAccessToken(
        IdpSettings,
        ServiceLocation.DecryptedUsername,
        ServiceLocation.DecryptedPassword);
}
```

When `AuthenticationMode` is `AccessToken`:
- If `ServiceLocation.AccessToken` is configured directly, use it.
- If blank, call the Hyland IdP's token endpoint using `IdpSettings` plus the existing `Username`/`Password` credentials.

This means `AccessToken` mode can work two ways: hand it a token you already obtained, or configure `IdpSettings` and let it obtain one.

---

## IdpAuthentication

Based on the Hyland Unity API documentation's own "Connecting with Hyland IdP" example, with three deliberate corrections:

**1. `FormUrlEncodedContent` instead of a raw string.** The documented sample built the request body as an interpolated string (`$"grant_type={...}&username={...}&..."`). A username, password, or client secret containing `&`, `=`, or `%` would silently corrupt the request. `FormUrlEncodedContent` percent-encodes each value correctly:

```csharp
var formValues = new List<KeyValuePair<string, string>>
{
    new("grant_type", idpSettings.IdpGrantType),
    new("username", username),
    new("password", password),
    new("scope", idpSettings.IdpScope),
    new("client_id", idpSettings.IdpClientId),
    new("client_secret", idpSettings.DecryptedIdpClientSecret),
    new("tenant", idpSettings.IdpTenant)
};
using var request = new HttpRequestMessage(HttpMethod.Post, idpSettings.IdpUrl)
{
    Content = new FormUrlEncodedContent(formValues)
};
```

**2. `System.Text.Json` instead of `Newtonsoft.Json`.** The documented sample used `JObject`, an extra NuGet dependency for a "read one field out of a JSON response" task:

```csharp
using var document = JsonDocument.Parse(responseBody);
if (!document.RootElement.TryGetProperty("access_token", out var token))
    throw new DatabankException("IdP response did not contain an access_token");
return token.GetString();
```

**3. The documented sample had a typo** (`Execption`) in its catch block that wouldn't compile. Corrected to `DatabankException`.

Only the `"password"` grant type is implemented. `"saml"`, `"adfs"`, and `"client_credentials"` throw `NotImplementedException` explicitly rather than silently failing -- their exact request shapes weren't derivable from the documentation available when this was written.

---

## Static Constructor Pattern

```csharp
static SessionManagement()
{
    var settings = (OnBaseSettings)ConfigurationManager.GetSection(OnBaseSettings.SectionName);
    ServiceLocation = settings.ServiceLocation;
    IdpSettings = settings.IdpSettings;
}
```

The static constructor loads configuration from `App.config` on first access. The properties are public and settable, so a settings UI can replace them in-memory without modifying `App.config`. See `Unity.TestHarness`'s `SettingsViewModel` for exactly this usage.

---

## Takeaways

- `Connect()` checks for a configured `SessionId` first, before considering `AuthenticationMode`. This prevents the infinite-reconnect loop that a `SessionId` enum member would have caused.
- `DomainCredentials` takes only URL and data source. No credentials of any kind -- the Windows identity is used as-is.
- `KeepAlive = true` means `IsDisconnectEnabled = false` on `AuthenticationProperties`, and means you must reconnect with `false` before you can truly disconnect.
- `GetAccessTokenIfNeeded()` handles both having a token already and needing to obtain one from the IdP.
- `FormUrlEncodedContent` is the correct way to build an OAuth2 token request body. A raw interpolated string doesn't encode special characters.
- The `"password"` grant type only. Other grant types are explicit stubs.
