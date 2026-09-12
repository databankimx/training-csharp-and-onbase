# Lecture Notes: RestApi.01.ConnectingToOnBase

## A Genuinely Different Session Model, Not a Like-for-Like Port

Unity API's `Application` object directly represents an active session — one object, held for the session's lifetime, passed around explicitly. The REST API has no equivalent single object. Instead:

- A **Bearer token** (obtained from the Hyland IdP) proves *identity*.
- A **session cookie** (`Cookie.Session.OnBase.Hyland`), issued by the Document Management API itself on the *first* authenticated request, proves an active *session* and is what actually consumes a license.
- Both have to travel together on every subsequent request. Obtaining a token does **not** by itself create a session or consume a license — this is easy to get wrong by assuming the token alone is "being connected."

`SessionManagement.ConnectAsync()` reflects this two-step reality explicitly: it obtains the token, then makes one deliberate request (`GET document-type-groups`, chosen for being lightweight and read-only, the same spirit as `Unity.TestHarness`'s own "Test API Availability" ping) specifically to establish the session and capture the cookie.

---

## The Cookie Is Handled by HttpClientHandler, Not Manually

Rather than parsing `Set-Cookie` headers and reattaching them to every later request by hand, `ConnectAsync()` builds the `HttpClient` with an `HttpClientHandler` whose `CookieContainer` has `UseCookies = true`. This captures the session cookie automatically on the connect request and resends it automatically on every later request through the same client — exactly what a browser does, and far less error-prone than hand-rolled cookie juggling.

---

## No SessionId Reconnect

Unity.01's `Connect()` checks `ServiceLocation.SessionId` first, before ever looking at `AuthenticationMode`: if a session ID is configured, a reconnect is attempted before falling back to establishing a brand new session. There is no REST equivalent here, and this isn't an oversight — see `RestApi.00.CommonFunctionality`'s own `ServiceLocation`/`LectureNotes.md` for why: the session cookie *is* the session, not a value the client can supply up front to reconnect to a specific prior one. `ConnectAsync()` here always establishes a brand new session; there's no "try reconnecting, fail over to new" branch to mirror.

---

## KeepAlive Drives an Actual Background Heartbeat

Unity API's `KeepAlive` is a one-time connect-time flag (`IsDisconnectEnabled = false`) — set once, and the session simply *can* be reconnected to later, indefinitely, no ongoing behavior required. The REST API's session cookie expires after 5 minutes of inactivity regardless of anything set at connect time (per the Authentication guide's own Heartbeat section). So here, `KeepAlive` instead starts an actual `System.Threading.Timer` (`StartHeartbeat`/`StopHeartbeat`), firing `POST session/heartbeat` every 4 minutes — comfortably under the 5-minute idle expiry, not right at the edge of it. Heartbeat failures are swallowed rather than thrown (there's no caller on the other end of a timer callback to handle an exception); if the session has genuinely died, the next real API call will fail loudly and visibly instead, which is a better place for that failure to surface than a silent background timer.

---

## Async/Await, Not `.Result`-Blocking Calls

Unity.01's `IdpAuthentication`/`SessionManagement` are entirely synchronous, matching Unity API's own SDK, which is synchronous by design (a SOAP/WCF client with a purely synchronous surface). This project has no such constraint: an HTTP token request, a connect request, a heartbeat, are all exactly the I/O-bound work async/await exists for, and `RestApi.TestHarness`'s own UI code (WPF, same as `Unity.TestHarness`) can `await` these calls without blocking the UI thread the way Unity API's own synchronous `Application.Connect()` sometimes needed `Task.Run` wrapping to avoid. `IdpAuthentication.GetAccessTokenAsync`/`SessionManagement.ConnectAsync`/`DisconnectAsync` are all genuinely async, not synchronous methods with an `Async` suffix slapped on.

---

## GetHttpClient(): The Handoff Point for RestApi.02–04

Where Unity.02–04's helper classes take a `Hyland.Unity.Application app` parameter (the connected session object, passed in from wherever `SessionManagement.Connect()` returned it), this project's counterparts will call `SessionManagement.GetHttpClient()` instead, to get the same fully-configured (Bearer token header + session cookie via `CookieContainer`) `HttpClient` every Document Management API call needs. One connected client, shared by reference, rather than each caller managing its own authentication.

---

## GetFormsHttpClient(): A Second API, Same Server, Same Token

The OnBase REST surface turned out to be several separate APIs, not one — Document Management, Forms, Admin, Workflow, and more, each with its own base URL differing only in the `{product}` path segment (confirmed against `forms-api.json`'s own `servers` block: `onbase/core` vs `onbase/forms`, same `{server}`). Confirmed directly: every one of these is served by the same server and honors the same Bearer token, so no second IdP exchange is needed to talk to the Forms API.

What *wasn't* confirmed either way is whether the Forms API's session cookie is scoped broadly enough to be shared with the Document Management API's, on the same host. Rather than guess, `GetFormsHttpClient()`'s `HttpClient` is built sharing the exact same underlying `HttpClientHandler`/`CookieContainer` as `GetHttpClient()`'s own — constructed with `disposeHandler: false` on both, so disposing either client doesn't take the shared handler out from under the other, with `sharedHandler` itself disposed once, explicitly, in `DisconnectAsync()`. This costs nothing if the two APIs turn out to have fully independent sessions (each API's own first request still establishes its own session normally), and works automatically for free if the server does scope the cookie broadly enough to cover both.

---

## Correction: SessionManagement Is No Longer Static

This class was originally `static` (process-wide, one shared session for the whole application), matching how `RestApi.00`'s own `ServiceLocation` documents Settings as an admin-style, global concept. That was a reasonable read for `RestApi.TestHarness` (a single-user desktop app), but wrong once `RestApi.TestHarness.Web` entered the picture: a web app has multiple concurrent users, and a shared, process-wide session would mean every visitor shares the same IdP token and the same OnBase session — one user's disconnect (or their session simply timing out) would silently break every other user's session too. An explicit, confirmed requirement ("every user should require their own IdP token, not a shared one") drove converting this class to an ordinary instance class instead.

Two constructors now serve the two different contexts this library supports:
- The parameterless constructor still auto-loads `ServiceLocation`/`IdpSettings` from the XML config file (`App.config`), unchanged from the old static constructor's own behavior — still the right approach for `RestApi.TestHarness`, a single-user desktop app with one shared settings file. `RestApi.TestHarness`'s own `ConnectionViewModel` owns one `SessionManagement` instance for the app's whole lifetime, the same effective lifecycle the static class used to have.
- `SessionManagement(ServiceLocation, IdpSettings)` takes them explicitly, for `RestApi.TestHarness.Web`: one instance per user session, built from shared `appsettings.json`-bound infrastructure configuration (server/IdP URLs) plus that specific user's own credentials, not one instance for every visitor.

This rippled into RestApi.02-04 too: their own helper classes previously fell back to a static `SessionManagement.GetHttpClient()`/`GetFormsHttpClient()` when no `HttpClient` was explicitly supplied to a call. That fallback is gone entirely now (there's no longer a single global "the" SessionManagement to fall back to) — every call into `OnBaseTaxonomy`/`DocumentRetrieval`/`DocumentStorage` now requires an explicit `HttpClient`, sourced from whichever `SessionManagement` instance the caller owns. See each of those projects' own Training Notes for the corresponding change, and `RestApi.TestHarness`'s own `LectureNotes.md` for how every one of its view models was updated to pass `connection.GetHttpClient()` (or `connection.Session.GetFormsHttpClient()`) explicitly everywhere that fallback used to paper over the need.

This class also implements `IDisposable` now (it didn't need to as a static class, nothing ever tore it down): ASP.NET Core's DI container disposes a scoped service automatically at the end of each request/session lifetime, releasing the underlying `HttpClientHandler`/`HttpClient` instances rather than leaking them. `Dispose()` does NOT call `DisconnectAsync()` itself (disposal must stay synchronous and shouldn't make a final network call as a side effect of teardown) — callers that want a clean server-side disconnect still call `DisconnectAsync()` explicitly beforehand.
