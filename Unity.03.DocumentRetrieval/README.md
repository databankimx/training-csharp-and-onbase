# Unity.03.DocumentRetrieval

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Document retrieval: query hit-lists (metadata only), file content retrieval, DocPop/UnityPop link generation, and keyword/keyword-group lookup extension methods on `Document`.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `HelperClasses/OnBase/DocumentRetrieval.cs` | Query, retrieve, and link generation |
| `HelperClasses/OnBase/Metadata.cs` | Builds `Keyword`/`EditableKeywordRecord`/`QueryKeywordRecord` objects from plain data |
| `HelperClasses/OnBase/DocumentExtensions.cs` | Keyword/keyword-group lookup extension methods |
| `Models/Objects/` | Plain DTOs for document metadata, files, links, and query filters |

---

## Related Samples

- **`Unity.02.AccessingTaxonomy`** — provides the document type/keyword type lookups this project builds on.
- **`Unity.04.DocumentArchiving`** — the storage-side counterpart to this project's retrieval.

---

## `Hyland.Unity` Package

This project references `Hyland.Unity` 26.1.2 via the `DataBank GitHub` NuGet feed. That package bundles:

- `Hyland.Unity.dll`
- `Hyland.Types.dll`
- `Hyland.Applications.Web.Security.dll`
- `Security.Cryptography.dll`

These are Hyland's own licensed OnBase Unity API binaries, not DataBank's own code. DataBank mirrors them on its internal feed for convenience, but the underlying software is licensed per OnBase deployment: **your own copies should come from your own OnBase Application Server installation** (typically found under that server's Unity API redistribution folder), not assumed to be freely available just because a NuGet package happens to exist for them.
