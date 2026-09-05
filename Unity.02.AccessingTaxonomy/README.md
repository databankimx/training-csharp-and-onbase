# Unity.02.AccessingTaxonomy

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Lookup methods for OnBase's "taxonomy" objects, document type groups, document types, keyword group (record) types, keyword types, custom queries, file types, and Unity form templates. Every lookup accepts either a name or a numeric ID, and works with an existing connected `Application` or one supplied per-call.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `HelperClasses/OnBase/OnBaseTaxonomy.cs` | The full set of taxonomy lookup methods |

---

## Related Samples

- **`Unity.01.ConnectingToOnBase`** — provides the connected `Application` this project's methods operate on.
- **`Unity.03.DocumentRetrieval`** / **`Unity.04.DocumentArchiving`** — use taxonomy lookups (document types, keyword types) as part of retrieving and storing documents.

---

## `Hyland.Unity` Package

This project references `Hyland.Unity` 26.1.2 via the `DataBank GitHub` NuGet feed. That package bundles:

- `Hyland.Unity.dll`
- `Hyland.Types.dll`
- `Hyland.Applications.Web.Security.dll`
- `Security.Cryptography.dll`

These are Hyland's own licensed OnBase Unity API binaries, not DataBank's own code. DataBank mirrors them on its internal feed for convenience, but the underlying software is licensed per OnBase deployment: **your own copies should come from your own OnBase Application Server installation** (typically found under that server's Unity API redistribution folder), not assumed to be freely available just because a NuGet package happens to exist for them.
