# Unity.00.CommonFunctionality

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

The foundational library for the entire OnBase Unity API training set: configuration models (`ServiceLocation`, `OnBaseSettings`, `DocPopSettings`), a self-contained `DatabankException`, and extension methods for registry-based secret decryption and Unity API type conversions. No connection logic lives here, that's `Unity.01.ConnectingToOnBase`, which references this project.

Deliberately **self-contained**: this whole `Unity.*.*` track does not reference `CSharp.SharedLibrary`, so it can be studied, copied, or handed to a client entirely on its own.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Configuration/ServiceLocation.cs` | OnBase connection settings, all five Unity API authentication modes |
| `Models/Configuration/OnBaseSettings.cs` | Top-level `<onBaseSettings>` config section |
| `Models/Configuration/DocPopSettings.cs` | DocPop link settings |
| `Models/Enumerations/AuthenticationMode.cs` | The five Unity API authentication modes |
| `Models/Enumerations/FileFormat.cs` | OnBase file format IDs |
| `Models/Objects/DatabankException.cs` | This training track's own exception type |
| `HelperClasses/Extensions/RegistryExtensions.cs` | DPAPI-based decryption of registry-stored secrets |
| `HelperClasses/Extensions/TypeConversionExtensions.cs` | String → `LicenseType`/file-type-ID conversions |

---

## Related Samples

- **`Unity.01.ConnectingToOnBase`** — uses `ServiceLocation`/`AuthenticationMode` to actually connect.
- **`Unity.SimpleButBadExample`** — the "before" picture: hardcoded credentials instead of any of this.
