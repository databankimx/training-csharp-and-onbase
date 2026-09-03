# Unity.01.ConnectingToOnBase

## What This Is

Connection management for the whole OnBase Unity API training set. See `README.md` for the file breakdown.

---

## Draft, Not Final

`SessionManagement.cs`'s `ConnectNewSession()` (the `switch` over `AuthenticationMode`) was built collaboratively, worked through issue by issue rather than written once and left alone. Two corrections worth knowing about, in case older reasoning about this file is floating around elsewhere:

- **`DomainCredentials` originally assumed** an overload of `CreateDomainAuthenticationProperties` accepting an alternate domain user's credentials. Confirmed against the actual Unity API signature: it only accepts `(url, datasource)`. NT authentication is purely the Windows identity the process is already running as; there's no Unity API mechanism for an alternate domain user at all (that would require OS-level impersonation performed before `Connect()`, a separate concern this training set doesn't currently implement).
- **A `SessionId` `AuthenticationMode` member was tried and abandoned.** Having `ConnectNewSession()` handle `SessionId` by calling `ReconnectExistingSession()` directly would cause infinite recursion the moment that reconnect failed with `AllowSessionFailover` enabled (fail → fall back to `ConnectNewSession()` → hit the same `SessionId` case again → fail again → ...). `ServiceLocation.SessionId` is an independent property instead; see `Connect()`'s own doc comment for how it actually combines with `AuthenticationMode`.

---

## Obtaining an Access Token, Not Just Presenting One

`ConnectNewSession()`'s `AccessToken` case calls `GetAccessTokenIfNeeded()`:

```csharp
private static string GetAccessTokenIfNeeded()
{
    if (!string.IsNullOrEmpty(ServiceLocation.DecryptedAccessToken))
        return ServiceLocation.DecryptedAccessToken;

    return IdpAuthentication.GetAccessToken(IdpSettings, ServiceLocation.DecryptedUsername, ServiceLocation.DecryptedPassword);
}
```

If `ServiceLocation.AccessToken` is already configured, it's used directly. If it's blank, `IdpAuthentication.GetAccessToken()` obtains one from the Hyland IdP instead, this is what lets `AuthenticationMode.AccessToken` work from nothing more than `IdpSettings` plus the same `Username`/`Password` `ServiceLocation` already carries for `OnBaseCredentials` mode.

---

## `IdpAuthentication.cs`: Corrections Over the Documented Sample

This is based on the Unity API documentation's own "Connecting with Hyland IdP" example (a `password`-grant OAuth2 token request), adapted with a few deliberate fixes:

1. **`FormUrlEncodedContent`, not a hand-built query string.** The documented sample builds the request body as a raw interpolated string and only afterward forces the `Content-Type` header to `application/x-www-form-urlencoded`, it never actually percent-encodes the individual values. A username, password, or client secret containing a `&`, `=`, or `%` would silently corrupt the request. `FormUrlEncodedContent` encodes each value correctly.
2. **`System.Text.Json`, not `Newtonsoft.Json`.** The documented sample uses `Newtonsoft.Json.Linq.JObject`, an extra NuGet dependency this training set has no other reason to take on, for a "read one field out of a JSON response" task `System.Text.Json` handles equally well.
3. **The documented sample's `catch` block references `Execption`** (a typo, wouldn't compile as written), corrected here to `DatabankException`, matching this training set's convention.

Only the `"password"` grant type is implemented, matching the one example the documentation actually provides. `"saml"`, `"adfs"`, and `"client_credentials"` are left as explicit `NotImplementedException` stubs rather than guessed at, their exact request shapes weren't derivable from the documentation reviewed for this training set.

---

## Try It Yourself

Configure `<idpSettings>` and set `<serviceLocation authenticationMode="AccessToken">` with no `accessToken` attribute, then step through `ConnectNewSession()` and watch `GetAccessTokenIfNeeded()` fall through to `IdpAuthentication.GetAccessToken()`. Then try a different `idpGrantType` value and confirm you land in one of the `NotImplementedException` stubs.
