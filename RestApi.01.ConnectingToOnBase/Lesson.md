# RestApi.01.ConnectingToOnBase

## What This Is

Connection logic for the OnBase Document Management (REST) API: obtaining a Bearer token from the Hyland IdP, establishing an OnBase session, keeping it alive with a background heartbeat, and disconnecting. The REST-API counterpart to `Unity.01.ConnectingToOnBase`.

`net10.0`, self-contained (no `Unity.*.*` reference), async/await throughout.

---

## A Genuinely Different Session Model

Unity API's `Application` object directly represents an active session -- one object, held for the session's lifetime, passed around explicitly. The REST API has no equivalent single object. Instead, two separate things combine to form "being connected":

| Thing | What it proves | How it travels |
|---|---|---|
| **Bearer token** | Identity (who you are) | `Authorization: Bearer <token>` header |
| **Session cookie** (`Cookie.Session.OnBase.Hyland`) | Active session (consumes a license) | Cookie header, managed by `HttpClientHandler.CookieContainer` |

Critically: **obtaining a Bearer token does not create an OnBase session or consume a license.** The session is created by the first authenticated request to the Document Management API, which issues the session cookie in its response. This is easy to get wrong by assuming the token alone represents "being connected."

`ConnectAsync()` reflects this two-step reality explicitly: obtain the token, then make one deliberate request (`GET /document-type-groups`, chosen as lightweight and read-only) specifically to establish the session and capture the cookie.

---

## Cookie Management: HttpClientHandler, Not Manual Parsing

Rather than parsing `Set-Cookie` response headers and reattaching them to every later request by hand, the `HttpClient` is built with an `HttpClientHandler` that has `UseCookies = true`. This captures the session cookie automatically on the connect request and resends it automatically on every subsequent request through the same client -- exactly what a browser does.

```csharp
var handler = new HttpClientHandler { UseCookies = true };
var client = new HttpClient(handler, disposeHandler: false);
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
```

The `disposeHandler: false` is load-bearing when two `HttpClient` instances share one `HttpClientHandler` (see `GetFormsHttpClient` below).

---

## IdpAuthentication: Getting the Bearer Token

Only the OAuth2 "password" grant is implemented, matching `AuthenticationMode.OnBaseCredentials`. The token request:

```
POST {IdpUrl}/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=password
&username={username}
&password={password}
&scope={scope}
&client_id={clientId}
&client_secret={clientSecret}
&tenant={tenant}
```

Response body contains `access_token`, extracted with `System.Text.Json`:

```csharp
using var document = JsonDocument.Parse(responseBody);
document.RootElement.TryGetProperty("access_token", out var tokenElement);
return tokenElement.GetString();
```

`FormUrlEncodedContent` handles percent-encoding of all values. The same fix as `Unity.01.ConnectingToOnBase` -- a raw interpolated string would silently corrupt a username, password, or client secret containing `&`, `=`, or `%`.

`DomainCredentials`/`AccessToken`/`SingleSignOn` throw `NotImplementedException` explicitly, matching the pattern in `Unity.01`.

---

## KeepAlive: An Actual Background Heartbeat

Unity API's `KeepAlive` is a one-time connect-time flag (`IsDisconnectEnabled = false`). The REST API session cookie expires after 5 minutes of inactivity, regardless. So `KeepAlive` here starts an actual `System.Threading.Timer` sending `POST /session/heartbeat` every 4 minutes:

```csharp
private void StartHeartbeat()
{
    heartbeatTimer = new Timer(
        async _ => await SendHeartbeatAsync(),
        null,
        dueTime: TimeSpan.FromMinutes(4),
        period: TimeSpan.FromMinutes(4));
}
```

4 minutes is comfortably under the 5-minute idle expiry without sitting right at the edge. Heartbeat failures are swallowed -- if the session has genuinely died, the next real API call will fail loudly and visibly, which is a better place for that failure to surface than a silent background timer.

---

## No SessionId Reconnect

`Unity.01.ConnectingToOnBase`'s `Connect()` checks `ServiceLocation.SessionId` first, before considering `AuthenticationMode`, and attempts to reconnect to a prior session. There is no REST equivalent -- the session cookie is the session, and there's no documented way to supply a prior session identifier to reconnect to. `ConnectAsync()` here always establishes a brand new session.

---

## GetFormsHttpClient: Two APIs, One Token, Shared Cookie Handling

OnBase's REST surface is multiple APIs -- the Forms API lives at `onbase/forms` on the same server, honored by the same Bearer token. Whether the Document Management API's session cookie also covers the Forms API wasn't confirmed from the documentation. Rather than guess:

```csharp
// sharedHandler is the same HttpClientHandler used by the main client
var formsClient = new HttpClient(sharedHandler, disposeHandler: false);
formsClient.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", token);
formsClient.BaseAddress = new Uri(ServiceLocation.FormsApiUrl);
```

Both clients share the same `HttpClientHandler`/`CookieContainer`, with `disposeHandler: false` on both so neither disposes the shared handler. `sharedHandler` is disposed once, explicitly, in `DisconnectAsync()`. This costs nothing if the two APIs have fully independent sessions (each API's first authenticated request still establishes its own session normally) and works automatically for free if the server does scope the cookie broadly enough.

---

## Instance Class, Not Static -- and Why

This class was originally `static` (process-wide shared session), which is fine for a single-user desktop app. Converting to an instance class was necessary once `RestApi.TestHarness.Web` entered scope -- a web app has multiple concurrent users, and a shared static session would mean one user's disconnect silently breaks everyone else's session.

Two constructors serve the two contexts:

```csharp
// Desktop app: auto-loads from App.config, one instance for the app's lifetime
public SessionManagement() { ... }

// Web app: explicit settings per user session, one instance per browser session
public SessionManagement(ServiceLocation serviceLocation, IdpSettings idpSettings) { ... }
```

`IDisposable` is also implemented now (as a static class nothing tore it down). `Dispose()` does NOT call `DisconnectAsync()` -- disposal must stay synchronous and shouldn't make a network call as a side effect. Callers that want a clean server-side disconnect call `DisconnectAsync()` explicitly beforehand.

---

## GetHttpClient(): The Handoff Point for RestApi.02-04

Where `Unity.02-04`'s helper classes take a `Hyland.Unity.Application app` parameter, this project's counterparts call `SessionManagement.GetHttpClient()`:

```csharp
// Unity style
var taxonomy = new OnBaseTaxonomy(app);

// REST style
var taxonomy = new OnBaseTaxonomy(sessionManagement.GetHttpClient());
```

One connected client -- Bearer token header already set, session cookie automatically sent via `CookieContainer` -- shared by every caller that gets it from the same `SessionManagement` instance. No caller manages its own authentication.

---

## Async/Await Throughout -- Not `.Result` Blocking

Unity API's SDK is synchronous by design. This project has no such constraint: token requests, connect requests, heartbeats are all exactly the I/O-bound work async/await exists for. `WPF`'s UI thread can `await` these calls without blocking, without `Task.Run` wrapping.

`GetAccessTokenAsync`, `ConnectAsync`, `DisconnectAsync` are genuinely async. Methods don't block on `.Result` anywhere.

---

## Takeaways

- Obtaining a Bearer token does not create an OnBase session. The session is created by the first authenticated request, which returns the session cookie.
- `HttpClientHandler` with `UseCookies = true` manages cookie capture and resend automatically.
- `KeepAlive` drives an actual 4-minute background timer sending `POST /session/heartbeat`. Failures are swallowed; the next real API call surfaces a dead session instead.
- No SessionId reconnect -- there's no documented REST equivalent of reconnecting to a prior session by ID.
- The Forms API shares the same server and Bearer token. Cookie scope isn't confirmed, so both clients share one `HttpClientHandler` defensively.
- Instance class with two constructors: parameterless for desktop (loads from App.config), explicit parameters for web (one instance per user session).
- `Dispose()` does not call `DisconnectAsync()`. Call `DisconnectAsync()` explicitly first if a clean server-side disconnect is needed.
