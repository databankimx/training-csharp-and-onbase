# RestApi.01.ConnectingToOnBase

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Connection logic for the OnBase Document Management (REST) API training set: obtaining a Bearer token from the Hyland Identity Provider (IdP), establishing an OnBase session, keeping it alive, and disconnecting. The REST-API counterpart to `Unity.01.ConnectingToOnBase`.

Built on top of `RestApi.00.CommonFunctionality`'s configuration types. `net10.0`, self-contained (no `Unity.*.*` reference), async/await throughout — see `LectureNotes.md` for why that's a deliberate departure from Unity.01's synchronous style, not just a style preference.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `HelperClasses/OnBase/IdpAuthentication.cs` | Obtains a Bearer token from the Hyland IdP. Only the OAuth2 "password" grant is implemented (matching `AuthenticationMode.OnBaseCredentials`); saml/adfs/client_credentials are explicit stubs. |
| `HelperClasses/OnBase/SessionManagement.cs` | The core connection manager: `ConnectAsync()`, `DisconnectAsync()`, `GetHttpClient()` (used by `RestApi.02`–`04` for their own API calls), and a background heartbeat timer for `KeepAlive`. See `LectureNotes.md` for how this genuinely differs from Unity.01's own `SessionManagement` (no `Application` object, no SessionId-reconnect, cookie-based session state). |

---

## Related Samples

- **`Unity.01.ConnectingToOnBase`** — the Unity API counterpart this project mirrors in role. See `LectureNotes.md` for where and why the two diverge — the underlying session models aren't the same shape.
- **`RestApi.00.CommonFunctionality`** — the configuration types this project's connection logic is built on.
- **`RestApi.02.AccessingTaxonomy`** (planned) — the first consumer of `SessionManagement.GetHttpClient()`.
