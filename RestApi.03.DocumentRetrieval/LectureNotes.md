# Lecture Notes: RestApi.03.DocumentRetrieval

## Queries Are Two-Step and Asynchronous, Not One Synchronous Call

Unity API's `query.ExecuteQueryResults(maxDocuments)` is one synchronous call. The REST API's query flow is genuinely two steps: `POST /documents/queries` creates the query and returns a query id (read from the response body's own `id` field, not the `Location` header — simpler, and avoids header parsing), then `GET /documents/queries/{queryId}/results` fetches the actual results. `GetDocumentInfoAsync(RetrievalRequest)` does both steps internally, so callers still get one result list back in one call, matching what `ExecuteQueryResults` felt like to use, even though two HTTP round-trips happen underneath.

---

## RetrievalRequest: Deliberately Richer, Not Capped to Unity's Shape

An explicit decision was made to expose the REST API's real query capability here, rather than artificially limit `RetrievalRequest` to match Unity API's simpler flat model:

- **`Scope` (a `QueryScope` enum) + `Ids`** replaces Unity's separate `DocumentType`/`DocumentTypes`/`CustomQuery` string fields, matching `document-api.json`'s own `QueryType` schema directly (`type`: `CustomQuery`/`DocumentType`/`DocumentTypeGroup`, `ids`: `string[]`). `DocumentTypeGroup` is a genuinely new search scope — Unity API's own `RetrievalRequest` never exposed searching by group at this layer at all.
- **`DateRanges` (plural)** replaces the single `DateRange`: the REST API's `documentDateRangeCollection` supports multiple OR'd ranges per query; Unity's `AddDateRange` only ever accepted one.
- **`Keywords` now carry `Operator`/`Relation`** (`Equal`/`LessThan`/`GreaterThan`/`LessThanEqual`/`GreaterThanEqual`/`NotEqual`/`Literal`, and `And`/`Or`/`To` respectively), matching `document-api.json`'s own `QueryKeyword` schema. Unity's flat `KeywordInfo` had neither — every keyword was implicitly "Equal, AND'd with everything else."
- **No `KeywordGroups`** (Unity's MultiInstance-specific query records, via `AddQueryKeywordRecord`): the REST API's `queryKeywordCollection` is flat, a keyword type's group membership doesn't change how it's queried. This is a simplification, not a gap — there's nothing the REST query model needs group membership *for* at query time.
- **`MaxResults`** is new: `document-api.json`'s own `QueryInformation.maxResults`. Unity's `RetrievalRequest` never carried this either (`ExecuteQueryResults` took `maxDocuments` as a separate call parameter).

This is a deliberate divergence from this whole training set's usual "identical feature set" goal for `RestApi.TestHarness` itself — the *library* exposes what the REST API can actually do; whether and how much of that richness the eventual Retrieval page's UI surfaces is a separate decision for when that page gets built.

---

## Display Columns: Requested Explicitly, Parsed by Position

`CreateQueryAsync` requests `DocumentId`/`DocumentName`/`DocumentTypeName`/`DocumentDate`/`ArchivalDate` first, then one `Keyword` display column per resolved common keyword type (via `RestApi.02`'s own `GetCommonKeywordTypesAsync`/`GetCustomQueryKeywordTypesAsync`) — the same five-plus-keywords shape Unity.03's own `GetKeywordColumns` builds. `ParseDocumentResult` then reads each result's `displayColumns` back by **position** (index 0 = id, already have it from `item.id` directly; 1 = name; 2 = type; 3 = documentDate; 4 = archivalDate; 5+ = the keyword columns, in the same order they were requested), rather than making a third call to `GET .../columns` to resolve what each index means. Since the request order is already known at parse time, that third call would be redundant.

---

## preferPdf: Accept Header, Not Provider Dispatch

Unity.03's own `GetDocumentFile` dispatches to a different retrieval provider object (`retrieval.PDF`/`.Image`/`.Text`/`.Native`) based on the rendition's file type, with `IsPdfConvertible` checking a hardcoded whitelist of `FileFormat` values known (from Hyland's own support table) to convert to PDF. There's no equivalent whitelist here: the REST API's content negotiation (an `Accept` header on the content request) lets the *server* decide what it can convert. `GetDocumentFileAsync` with `preferPdf: true` sends `Accept: application/pdf` first; if that request fails, it silently falls back to requesting the rendition's native format instead (matching Unity's own "silently ignored for unsupported file types" behavior), rather than pre-checking a whitelist client-side before ever making a request.

---

## RevisionInfo Is Sparser Than Unity's — the Data Genuinely Isn't There

Unity API's `RevisionInfo` carries `Comment`/`CreatedBy` at the revision level. The REST API's own `Revision` schema is just `id` + `revisionNumber` — no comment or creator on the revision itself. Those fields live on **`Rendition`** instead (`RenditionInfo.Comment`/`CreatedByUserId`), a genuine structural difference between the two APIs' data models, not a bug or an oversight in this port.

---

## GetDocumentLinks Has No REST Equivalent

No `DocumentLink`/DocPop/UnityPop support here at all — this was already a confirmed gap flagged in `RestApi.00.CommonFunctionality`'s own `ServiceLocation`, carried through consistently here rather than re-litigated per project.

---

## Correction: KeywordGroup Needed Two Id Fields, Not One

An earlier version of `KeywordGroup` had a single `Id` property, populated from the REST API's own `groupId` field. This was incomplete: the API actually distinguishes `typeGroupId` (which KIND of group — shared by every instance) from `groupId` (which specific INSTANCE — only present for Multi-Instance groups at all). Collapsing them into one `Id` meant Single-Instance groups, which have a `typeGroupId` but no `groupId`, ended up with no group identity captured whatsoever. Fixed to `TypeGroupId`/`GroupId` as two separate properties, matching the API's own two-field distinction; `GetDocumentInfoAsync(string id)`'s own parsing was updated to populate both. This surfaced while designing `RestApi.04.DocumentArchiving`'s keyword-collection payloads, which need `typeGroupId` to submit a NEW instance of a group (where `groupId` is necessarily still unknown, the server assigns one) — something the old single-`Id` shape had no way to express.
