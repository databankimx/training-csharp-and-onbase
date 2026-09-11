# RestApi.03.DocumentRetrieval

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Document retrieval for the OnBase Document Management (REST) API training set: searching for documents, retrieving a single document's metadata/revisions/renditions, and downloading file content. The REST-API counterpart to `Unity.03.DocumentRetrieval`.

Built on `RestApi.01`/`RestApi.02`. `net10.0`, async/await throughout.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Objects/RetrievalModels.cs` | DTOs mostly mirroring Unity.03's own shapes (string ids, no `DocumentLink`/DocPop). `RetrievalRequest` is the deliberate exception — genuinely reshaped to expose the REST API's real query capability (per-keyword operators/relations, multiple OR'd date ranges, a `DocumentTypeGroup` search scope) rather than capped to Unity's simpler shape. See `LectureNotes.md`. |
| `HelperClasses/OnBase/DocumentRetrieval.cs` | The main retrieval class. `GetDocumentInfoAsync(RetrievalRequest)` handles the REST API's two-step query flow (create, then fetch results) internally. `preferPdf` uses `Accept` header content negotiation instead of Unity's provider dispatch. No `GetDocumentLinks` (DocPop/UnityPop) — a confirmed gap. |

---

## Related Samples

- **`Unity.03.DocumentRetrieval`** — the Unity API counterpart this project mirrors in role. See `LectureNotes.md` for the genuine differences.
- **`RestApi.02.AccessingTaxonomy`** — supplies Document Type/Custom Query lookups and common-keyword resolution this project's search logic depends on.
- **`RestApi.04.DocumentArchiving`** (planned) — the next consumer of this project's document/revision lookups.
