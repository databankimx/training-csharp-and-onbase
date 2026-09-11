# Lecture Notes: RestApi.04.DocumentArchiving

## Scope: Conventional Documents Only, Deliberately

`Unity.TestHarness`'s own Archiving page never exercised e-form or Unity Form archiving — only conventional document Store New/Modify Metadata/Add Revision/Add Rendition/Delete. And separately: `document-api.json` exposes no write (POST/PUT) endpoints for either form concept at all — `forms-api.json` is entirely read-only (`GET /unity-form-templates`, `GET /unity-forms/{documentId}`). Both facts point the same direction, so this project doesn't attempt form archiving at all, rather than half-building something neither the target app needs nor the API supports.

Worth noting: `Unity.04.DocumentArchiving`'s own form-archiving support was already partially stubbed before this port even started (missing repeater support, `NotImplementedException` for form revision/rendition) — this isn't a new gap introduced by moving to REST, just one that was never relevant to begin with for this training set's actual app.

---

## No CanAddRevision/CanAddRendition — Confirmed, Not Guessed

Unity API's `CanAddRevision`/`CanAddRendition` are synchronous pre-flight checks (`doc.DocumentType.Revisable` + a `CanI` privilege check), used to enable/disable the Archiving page's own buttons before an attempt is even made. `document-api.json`'s `DocumentType` schema exposes neither `revisable` nor `renditionable` as a field at all. The only place that information surfaces anywhere in this API is *after* an upload attempt, in a `300 Multiple Choices` response's per-match `canAddAsRevision`/`canAddAsRendition` flags — there's no way to ask "could I?" without actually trying.

Confirmed decision: omit the pre-flight check entirely, rather than approximate it with something unconfirmed. `RestApi.TestHarness`'s own Add Revision/Add Rendition controls will need to always be enabled (once a document is loaded), with a disallowed attempt surfacing as an ordinary failed-request error rather than being blocked in the UI beforehand.

---

## File Uploads: A Real Three-Step Process, Handled Internally

The REST API's own upload flow (per the Upload and Archive Interactions guide): `POST /documents/uploads` to initiate each file (returns an upload id, part size, and part count), `PUT /documents/uploads/{id}?filePart=N` for each part, then the resulting upload ids are referenced in the actual archive/revision/rendition request. `StageUploadsAsync` handles all of this internally — `NewDocumentRequest.Files`/`UpdateDocumentRequest.Files` still just take local file paths, matching Unity API's own shape, so callers never see the REST-specific staging mechanics directly.

---

## keywordGuid: A Concurrency Token With No Unity API Equivalent

Every archive/revision/rendition/metadata-update request needs a `keywordGuid`, obtained from `GET /document-types/{id}/default-keywords` (a new document) or `GET /documents/{id}/keywords` (an existing one) immediately before submitting. This is a genuinely new concept: Unity API's `KeywordModifier`/`CreateStoreNewDocumentProperties` have no analogous "did the keyword schema change out from under you" concurrency guard at all. `BuildKeywordCollectionAsync` fetches it fresh on every call rather than caching it, since (per the Working with Keywords guide) it becomes stale immediately after any successful update elsewhere.

---

## 300 Multiple Choices: Surfaced Explicitly, Not Auto-Resolved

When a Revisable/Renditionable Document Type's new-document attempt matches existing documents, the server returns `300 Multiple Choices` with the matches and their own `canAddAsRevision`/`canAddAsRendition` flags — a disambiguation flow with no Unity API equivalent (Unity's own `DocumentStorage` doesn't have a "found multiple matches, now what" branch at all). `CreateDocumentAsync` here doesn't attempt to auto-pick a match and silently add a revision/rendition on the caller's behalf — it surfaces the situation as a clear, actionable `DatabankException` instead, pointing at `NewDocumentRequest.StoreAsNew = true` as the explicit way to force new-document storage. Actually handling the disambiguation (letting the caller choose a specific match) isn't implemented in this training set.

---

## typeGroupId vs groupId: The Fix That Rippled Back Into RestApi.03

Building this project's `keywordCollection` payloads required tracking a Keyword Type Group's `typeGroupId` (which kind of group) separately from its `groupId` (which specific instance) — a NEW group instance being archived has a `typeGroupId` but necessarily no `groupId` yet (the server assigns one). `RestApi.03.DocumentRetrieval`'s own `KeywordGroup` DTO originally collapsed both into one `Id` property, which only ever captured `groupId`, and so had no way to express this at all. See `RestApi.03`'s own `LectureNotes.md` for the fix (now `TypeGroupId`/`GroupId` as two separate properties) — this project's own archiving logic was the thing that surfaced the gap.
