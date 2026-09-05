# Unity.07.UsingDataBankExtensionsLibrary

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.
>
> **Missing content**: the original project bundled an entire DocFX-generated documentation site for `DBIMX.Unity.Extensions` (dozens of HTML/CSS/JS files under `Resources\SDK\`), tied to an old package version. Not carried over, see `LectureNotes.md` for why, and where to find current docs instead.
>
> **NuGet package availability**: `DBIMX.Extensions_unsigned.v25` is a DataBank-internal package, published to the `DataBank GitHub` NuGet feed. It's only available to internal DataBank employees with access to that feed, not a public package. `Hyland.Unity` is also consumed via that same feed here, but see this file's own "Hyland.Unity" section below for how that one differs.

## What This Is

A Workflow Unity Script demonstrating the `DBIMX.Unity.Extensions` library's convenience methods, `FindDocumentType`/`FindFileType`, `TryGetKeywordType`, keyword `Add`/`GetFirst`/`GetAll`/`GetFirstHavingValue`/`GetKeywordInstanceCount`, and `DeleteDocument`, alongside commented-out plain-Unity-API equivalents for comparison.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `WorkflowScript.cs` | The demo, storing, updating, and deleting a document using `DBIMX.Unity.Extensions` |
| `DatabankException.cs` | This project's own minimal, self-contained exception type |

---

## Related Samples

- **`Unity.SimpleButBadExample`**, **`Unity.00.CommonFunctionality`** — the license key here (`LicenseKey`) is a hardcoded "User-Editable Script Setting," matching the convention `Unity.05.UnityScripts`'s own fully-worked templates use for the same kind of value; this differs from `Unity.06.UnityFormDefaultValues`'s `Token`, which lives in `App.config` because that project is a standalone console app with a natural place to read configuration from, unlike a Unity Script hosted by an OnBase client.

---

## `Hyland.Unity`

This project also references `Hyland.Unity` 26.1.2 (also via the `DataBank GitHub` feed). That package bundles:

- `Hyland.Unity.dll`
- `Hyland.Types.dll`
- `Hyland.Applications.Web.Security.dll`
- `Security.Cryptography.dll`

These are Hyland's own licensed OnBase Unity API binaries, not DataBank's own code. DataBank mirrors them on its internal feed for convenience, but the underlying software is licensed per OnBase deployment: **your own copies should come from your own OnBase Application Server installation** (typically found under that server's Unity API redistribution folder), not assumed to be freely available just because a NuGet package happens to exist for them.
