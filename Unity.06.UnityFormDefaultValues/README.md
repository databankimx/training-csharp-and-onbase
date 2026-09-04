# Unity.06.UnityFormDefaultValues

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.
>
> **Missing file**: the original project's companion `Internal - Unity Form Default Values.pdf` wasn't ported (no way to copy binary files between the source and destination trees with the tools available). Copy it over manually from the original `developer-training-bb` source if you want it here.

## What This Is

Generates a shared Unity Form URL with pre-populated field default values and an HMAC signature authenticating them, the technique OnBase's Unity Form integrations use to let an external system hand a user a working, pre-filled form link.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Program.cs` | Builds the parameter string, signs it, generates the final URL |
| `App.config` | `BaseUrl`/`Token`, externalized from what were originally hardcoded constants |

---

## How to Run

1. Update `App.config`'s `BaseUrl` to a real shared Unity Form URL from your own OnBase environment.
2. Update `App.config`'s `Token` to the matching integration's signing key (either plain, for local testing, or `registry:HKLM\...,Token` for anything resembling a real deployment, see `LectureNotes.md`).
3. Update `Program.cs`'s `FormFields` to whatever field ID/value pairs you want pre-populated.
4. Press F5 (or `dotnet run`). The generated URL opens in your default browser.

---

## Related Samples

- **`Unity.00.CommonFunctionality`** — provides the `RegistryExtensions` DPAPI/registry-encryption this project reuses for its `Token`.
