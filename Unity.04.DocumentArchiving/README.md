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
