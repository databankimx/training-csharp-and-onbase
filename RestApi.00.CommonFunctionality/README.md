# RestApi.00.CommonFunctionality

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The foundational library for the OnBase Document Management (REST) API training set — models and configuration types only, no connection logic (that's `RestApi.01.ConnectingToOnBase`). The REST-API counterpart to `Unity.00.CommonFunctionality`, built to produce an **identical application** (`RestApi.TestHarness`) to `Unity.TestHarness`, just talking to the REST API instead of the Unity API — same feature set, same six pages, no scope expansion.

`net10.0` (not `net48`): unlike the `Unity.*.*` track, nothing here depends on `Hyland.Unity` (a .NET-Framework-only assembly), the REST API is plain HTTP, so there's no architectural reason to stay on net48.

Deliberately self-contained, the same way `Unity.00.CommonFunctionality` avoids depending on `CSharp.SharedLibrary`: no reference to the `Unity.*.*` track at all, even though several types here are close counterparts to ones there.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Enumerations/AuthenticationMode.cs` | Same four member names as Unity API's own version, for UI/feature parity, but mapped to REST/OAuth2 grant-type concepts. Only `OnBaseCredentials` (the "password" grant) is currently implemented; the other three are documented, stubbed placeholders. |
| `Models/Configuration/ServiceLocation.cs` | Connection settings for the OnBase API Server. Genuinely reshaped from Unity's version, not just renamed — see `LectureNotes.md` for what's dropped (SessionId reconnect, LicenseType, ApplicationId) and what shifted meaning (KeepAlive). |
| `Models/Configuration/IdpSettings.cs` | Hyland Identity Provider (IdP) settings — URL, tenant, client ID/secret, scope, grant type. A close port of Unity's own version. |
| `Models/Configuration/RestApiSettings.cs` | The top-level `ConfigurationSection` wrapper, the REST equivalent of `OnBaseSettings`. No DocPop element — see `LectureNotes.md`. |
| `Models/Objects/DatabankException.cs` | Self-contained duplicate, matching Unity.00's own precedent. |
| `HelperClasses/Extensions/DataProtectionExtensions.cs` | Secret protect/unprotect via Microsoft.AspNetCore.DataProtection's `IDataProtector` — the modern replacement for Unity.00's DPAPI-via-registry (`aspnet_setreg.exe`) approach, adopted once this track was no longer tied to net48. Values are stored as a `"protected:"`-prefixed payload directly in config, no registry involved. |

---

## Related Samples

- **`Unity.00.CommonFunctionality`** — the Unity API counterpart this project mirrors in role, not in every implementation detail. See `LectureNotes.md` for where and why they diverge.
- **`RestApi.01.ConnectingToOnBase`** (planned) — connection logic (OAuth2 token acquisition, session cookie management, heartbeat/disconnect) built on top of the configuration types here.
