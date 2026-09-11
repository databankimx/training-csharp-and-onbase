# RestApi.02.AccessingTaxonomy

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Taxonomy access for the OnBase Document Management (REST) API training set: Document Type Groups, Document Types, Keyword Type Groups/Types, Custom Queries, and File Types. The REST-API counterpart to `Unity.02.AccessingTaxonomy`.

Built on `RestApi.01.ConnectingToOnBase`'s `SessionManagement.GetHttpClient()`. `net10.0`, async/await throughout, no extra NuGet packages (`System.Net.Http`/`System.Text.Json` are both in the base class library).

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Models/Objects/TaxonomyModels.cs` | POCOs matching `document-api.json`'s own schemas (`DocumentTypeGroup`, `DocumentType`, `KeywordTypeGroup`, `KeywordType`, `CustomQuery`, `FileType`), plus `DocumentTypeKeywordGroup`, this project's own resolved combination type — see `LectureNotes.md`. |
| `HelperClasses/OnBase/OnBaseTaxonomy.cs` | The main taxonomy access class. Same functional surface as Unity.02's own `OnBaseTaxonomy`, with a smaller method count (no `Document`/`DocumentType`-overload sprawl — every method takes plain string ids/names) and one genuinely new capability, `GetDocumentTypeKeywordGroupsAsync`, which resolves keyword group/type metadata across multiple API calls into one populated result. Also includes Unity Form Template lookups (`GetUnityFormTemplatesAsync`/`GetUnityFormTemplateAsync`), routed through a separate Forms API connection — see `LectureNotes.md`. |

---

## Related Samples

- **`Unity.02.AccessingTaxonomy`** — the Unity API counterpart this project mirrors in role. See `LectureNotes.md` for the genuine differences (multi-call keyword resolution, string ids, Unity Form lookups on a separate Forms API connection).
- **`RestApi.01.ConnectingToOnBase`** — supplies the connected `HttpClient` this project's calls go through.
- **`RestApi.03.DocumentRetrieval`** (planned) — the next consumer of this project's Document Type/Custom Query lookups.
