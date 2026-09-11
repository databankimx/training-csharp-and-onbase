# RestApi.04.DocumentArchiving

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Document archiving for the OnBase Document Management (REST) API training set: storing new documents, updating metadata/revisions/renditions, and deleting documents. The REST-API counterpart to `Unity.04.DocumentArchiving`, **scoped to conventional documents only** — no e-form/Unity Form archiving, matching what `Unity.TestHarness`'s own Archiving page actually exercises, and matching that `document-api.json` exposes no write endpoints for either form concept at all.

Built on `RestApi.01`–`03`. `net10.0`, async/await throughout.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Objects/ArchivingModels.cs` | `NewDocumentRequest`/`UpdateDocumentRequest`/`DeleteRequest`. Reuses `RestApi.03`'s own `KeywordInfo`/`KeywordGroup`. `DeleteRequest` has no `PurgeDocument` — confirmed no REST equivalent exists. |
| `Models/Enumerations/UpdateType.cs` | Metadata/Revision/Rendition — no `StorageType` (no form-archiving scope to distinguish). |
| `HelperClasses/OnBase/DocumentStorage.cs` | The main archiving class. Handles the REST API's three-step upload staging and `keywordGuid` concurrency token internally, so its own public methods still just take local file paths and plain keyword lists. No `CanAddRevision`/`CanAddRendition` — confirmed no REST pre-flight equivalent exists. See `LectureNotes.md`. |

---

## Related Samples

- **`Unity.04.DocumentArchiving`** — the Unity API counterpart this project mirrors in role (for conventional documents). See `LectureNotes.md` for the genuine differences.
- **`RestApi.02.AccessingTaxonomy`** / **`RestApi.03.DocumentRetrieval`** — supply the document type/keyword lookups this project's archiving logic depends on.
