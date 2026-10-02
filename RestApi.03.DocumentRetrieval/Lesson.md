# RestApi.03.DocumentRetrieval

## What This Is

Document retrieval for the OnBase Document Management (REST) API: searching for documents, retrieving a single document's metadata/revisions/renditions, and downloading file content. The REST-API counterpart to `Unity.03.DocumentRetrieval`.

`net10.0`, async/await throughout.

---

## What's in This Project

| File | What it does |
|---|---|
| `Models/Objects/RetrievalModels.cs` | DTOs -- mostly mirroring Unity.03's shapes (string IDs), with `RetrievalRequest` deliberately richer than Unity's version |
| `HelperClasses/OnBase/DocumentRetrieval.cs` | Main retrieval class -- two-step query flow, metadata retrieval, file download |

---

## Queries Are Two Steps, Not One

Unity API's `query.ExecuteQueryResults(maxDocuments)` is one synchronous call. The REST API requires two:

```
POST /documents/queries
→ { "id": "query-123", ... }

GET /documents/queries/query-123/results
→ [ { "id": "456", "displayColumns": [...] }, ... ]
```

The query ID comes from the response body's `id` field, not the `Location` header -- simpler, avoids header parsing. `GetDocumentInfoAsync(RetrievalRequest)` wraps both steps internally, so callers still get one result list back in one call, even though two HTTP round-trips happen underneath.

---

## RetrievalRequest: Deliberately Richer Than Unity's

An explicit decision was made to expose the REST API's real query capability here, rather than capping `RetrievalRequest` to match Unity API's simpler shape.

**`Scope` + `Ids` replaces separate string fields:**
```csharp
// Unity's shape
string DocumentType;          // single type
List<string> DocumentTypes;   // multiple types
string CustomQuery;           // custom query

// REST shape
QueryScope Scope;             // CustomQuery / DocumentType / DocumentTypeGroup
List<string> Ids;             // one or more IDs for that scope
```

`DocumentTypeGroup` as a search scope is genuinely new -- Unity's `RetrievalRequest` never exposed group-level searching.

**`DateRanges` (plural):**
```csharp
// Unity: one date range per query
DateRange DateRange;

// REST: multiple OR'd date ranges per query (documentDateRangeCollection)
List<DateRange> DateRanges;
```

**Keywords carry `Operator` and `Relation`:**
```csharp
// Unity: flat KeywordInfo, implicitly Equal AND'd
class KeywordInfo { string Name; string Value; }

// REST: per keyword operator and relation
class KeywordInfo { string Name; string Value; string Operator; string Relation; }
// Operator: Equal/LessThan/GreaterThan/LessThanEqual/GreaterThanEqual/NotEqual/Literal
// Relation: And/Or/To
```

**`MaxResults`:** `document-api.json`'s `QueryInformation.maxResults`. Unity's `RetrievalRequest` never carried this.

**No `KeywordGroups`:** Unity's `RetrievalRequest` had a `KeywordGroups` list for MultiInstance-specific query records (via `AddQueryKeywordRecord`). The REST API's `queryKeywordCollection` is flat -- keyword group membership doesn't change how a keyword is queried.

---

## Display Columns: Requested Explicitly, Parsed by Position

`CreateQueryAsync` requests five fixed columns first:
```
DocumentId, DocumentName, DocumentTypeName, DocumentDate, ArchivalDate
```
Then one `Keyword` display column per common keyword type (resolved via `RestApi.02`'s `GetCommonKeywordTypesAsync`/`GetCustomQueryKeywordTypesAsync`), in a deterministic order.

`ParseDocumentResult` reads `displayColumns` back by position:
- Index 0 = document ID (already available from `item.id`)
- Index 1 = document name
- Index 2 = document type name
- Index 3 = document date
- Index 4 = archival date
- Index 5+ = keyword columns, in the same order they were requested

Since the request order is already known at parse time, a third `GET .../columns` call to look up what each index means is redundant.

---

## preferPdf: Accept Header, Not Provider Dispatch

Unity.03's `GetDocumentFile` dispatches to different retrieval provider objects based on file type, with `IsPdfConvertible` checking a hardcoded list of formats known (from Hyland's support table) to support PDF conversion.

The REST API uses HTTP content negotiation instead:

```http
GET /documents/{id}/revisions/{revId}/renditions/{rendId}/content
Accept: application/pdf
```

With `preferPdf: true`, `GetDocumentFileAsync` sends `Accept: application/pdf` first. If that request fails, it silently falls back to requesting the rendition's native format -- matching Unity's "silently ignored for unsupported file types" behavior, without needing a client-side whitelist.

---

## RevisionInfo Is Sparser -- the Data Genuinely Isn't There

Unity API's `RevisionInfo` carries `Comment` and `CreatedBy` at the revision level. The REST API's own `Revision` schema is just `id` + `revisionNumber`. Those fields appear on `Rendition` instead (`RenditionInfo.Comment`/`CreatedByUserId`). This is a structural difference between the two APIs' data models, not an oversight.

---

## KeywordGroup: Two ID Fields, Not One

An earlier version of `KeywordGroup` had a single `Id` property populated from the API's `groupId` field. This was incomplete -- the API distinguishes:

- `typeGroupId` -- which kind of group (shared by every instance of a MultiInstance group)
- `groupId` -- which specific instance (only present for MultiInstance groups; absent for SingleInstance)

Collapsing both into one `Id` meant SingleInstance groups, which have a `typeGroupId` but no `groupId`, ended up with no group identity captured. Fixed to `TypeGroupId`/`GroupId` as two separate properties. This fix was required by `RestApi.04.DocumentArchiving`'s keyword-collection payloads, which need `typeGroupId` to submit a new instance of a group (where `groupId` is necessarily unknown until the server assigns one).

---

## No DocPop/UnityPop Links

`GetDocumentLinks` doesn't exist here -- confirmed gap flagged in `RestApi.00.CommonFunctionality` and carried through consistently. `RestApi.TestHarness`'s Retrieval page omits those link buttons.

---

## Endpoints Used

| Endpoint | Purpose |
|---|---|
| `POST /documents/queries` | Create a document query |
| `GET /documents/queries/{id}/results` | Fetch query results |
| `GET /documents/{id}` | Get a single document's metadata |
| `GET /documents/{id}/revisions` | Get a document's revisions |
| `GET /documents/{id}/revisions/{revId}/renditions` | Get renditions for a revision |
| `GET /documents/{id}/revisions/{revId}/renditions/{rendId}/content` | Download file content |

---

## Takeaways

- Queries are two steps: `POST` to create (returns query ID), `GET` to fetch results.
- `RetrievalRequest` exposes the REST API's real query capability: `DocumentTypeGroup` scope, multiple date ranges, per-keyword operators and relations, `MaxResults`.
- Display columns are requested in a deterministic order and parsed by position -- no third call needed to resolve column metadata.
- `preferPdf` uses `Accept: application/pdf` content negotiation and falls back silently on failure -- no client-side whitelist.
- `RevisionInfo` has no `Comment`/`CreatedBy` in the REST API's `Revision` schema; those fields live on `RenditionInfo`.
- `KeywordGroup` has `TypeGroupId` (which kind) and `GroupId` (which instance) as separate fields -- not one collapsed `Id`.
- No DocPop/UnityPop links -- no documented REST equivalent.
