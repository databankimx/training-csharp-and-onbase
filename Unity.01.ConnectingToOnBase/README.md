# Unity.01.ConnectingToOnBase

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.
>
> **This project's Connect logic is a draft, still under active collaborative review** — see `LectureNotes.md`'s "Draft, Not Final" note before treating anything here as settled.

## What This Is

Session connection and disconnection logic for the OnBase Unity API, built on `Unity.00.CommonFunctionality`'s `ServiceLocation`/`IdpSettings`/`AuthenticationMode`. Covers all four `AuthenticationMode` values, session-ID reconnection (independent of `AuthenticationMode`), automatic session failover, and obtaining a Hyland Identity Provider (IdP) access token when one isn't supplied directly.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `HelperClasses/OnBase/SessionManagement.cs` | `Connect()`/`Disconnect()`, session reconnect and failover |
| `HelperClasses/OnBase/IdpAuthentication.cs` | Obtains an access token from the Hyland IdP |

---

## Related Samples

- **`Unity.00.CommonFunctionality`** — `ServiceLocation`/`IdpSettings`/`AuthenticationMode`, the config model this project consumes.
- **`Unity.SimpleButBadExample`** — the "before" picture: hardcoded credentials instead of any of this.
