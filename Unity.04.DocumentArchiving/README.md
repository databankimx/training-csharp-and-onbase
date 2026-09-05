# Unity.04.DocumentArchiving

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Document creation, update, and deletion: conventional documents, e-forms, and Unity Forms, new documents/revisions/renditions, metadata updates, and delete/purge.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `HelperClasses/OnBase/DocumentStorage.cs` | Create, update, and delete documents |
| `Models/Objects/` | Request DTOs for each operation (`NewDocumentRequest`, `UpdateDocumentRequest`, `DeleteRequest`, etc.) |
| `Models/Enumerations/` | `StorageType`, `UpdateType` |

**Two areas are intentionally incomplete**, matching the original codebase: OnBase repeater-row support (the model exists, the Unity API wiring doesn't), and e-form/Unity Form revision and rendition updates (all throw `NotImplementedException`). See `LectureNotes.md` for exactly where.

---

## Related Samples

- **`Unity.02.AccessingTaxonomy`** / **`Unity.03.DocumentRetrieval`** — provide the lookups and keyword-building logic this project builds on.

---

## `Hyland.Unity` Package

This project references `Hyland.Unity` 26.1.2 via the `DataBank GitHub` NuGet feed. That package bundles:

- `Hyland.Unity.dll`
- `Hyland.Types.dll`
- `Hyland.Applications.Web.Security.dll`
- `Security.Cryptography.dll`

These are Hyland's own licensed OnBase Unity API binaries, not DataBank's own code. DataBank mirrors them on its internal feed for convenience, but the underlying software is licensed per OnBase deployment: **your own copies should come from your own OnBase Application Server installation** (typically found under that server's Unity API redistribution folder), not assumed to be freely available just because a NuGet package happens to exist for them.
