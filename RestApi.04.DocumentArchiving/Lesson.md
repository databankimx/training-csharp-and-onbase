# RestApi.04.DocumentArchiving

## What This Is

Document archiving for the OnBase Document Management (REST) API: storing new documents, updating metadata/revisions/renditions, and deleting documents. The REST-API counterpart to `Unity.04.DocumentArchiving`, scoped to conventional documents only.

`net10.0`, async/await throughout.

---

## What's in This Project

| File | What it does |
|---|---|
| `Models/Objects/ArchivingModels.cs` | `NewDocumentRequest`/`UpdateDocumentRequest`/`DeleteRequest`, reusing `RestApi.03`'s `KeywordInfo`/`KeywordGroup` |
| `Models/Enumerations/UpdateType.cs` | `ModifyMetadata`/`AddRevision`/`AddRendition` -- no `StorageType` (no form-archiving scope) |
| `HelperClasses/OnBase/DocumentStorage.cs` | Main archiving class -- three-step upload staging and `keywordGuid` concurrency token handled internally |

---

## Scope: Conventional Documents Only

`Unity.TestHarness`'s Archiving page never exercised e-form or Unity Form archiving -- only conventional documents. And separately, `document-api.json` exposes no write endpoints for either form concept at all -- `forms-api.json` is entirely read-only. Both facts point the same direction: this project doesn't attempt form archiving.

`Unity.04.DocumentArchiving`'s own form-archiving support was already partially stubbed before this port (missing repeater support, `NotImplementedException` for form revision/rendition) -- this isn't a new gap, just one that was never relevant to this training set's actual app.

---

## File Uploads: A Real Three-Step Process

Where Unity API's `StoreNewDocument` took local file paths directly, the REST API requires staging uploads first:

```
1. POST /documents/uploads
   → { "id": "upload-abc", "partSize": 5242880, "partCount": 1 }

2. PUT /documents/uploads/upload-abc?filePart=0
   (request body = file bytes for this part)

3. Reference upload IDs in the archive/revision/rendition request body
```

`StageUploadsAsync` handles all three steps internally. `NewDocumentRequest.Files`/`UpdateDocumentRequest.Files` still just take local file paths, matching Unity API's own shape, so callers never see the REST-specific staging mechanics.

Large files are split into parts -- `partSize` from the initiation response determines the chunk size for each `PUT`. All parts for all files must be staged before any archive request is made.

---

## keywordGuid: A Concurrency Token With No Unity Equivalent

Every archive/revision/rendition/metadata-update request requires a `keywordGuid`, obtained immediately before submitting:

```
GET /document-types/{id}/default-keywords   → keywordGuid (for new documents)
GET /documents/{id}/keywords                → keywordGuid (for existing documents)
```

This is a genuinely new concept -- Unity API's `KeywordModifier` and `CreateStoreNewDocumentProperties` have no analogous concurrency guard. The `keywordGuid` proves that the keyword schema hasn't changed between when you fetched the keyword metadata and when you're submitting new keyword values.

`BuildKeywordCollectionAsync` fetches it fresh on every call rather than caching it -- per the Working with Keywords guide, it becomes stale immediately after any successful update.

---

## 300 Multiple Choices: Surfaced, Not Auto-Resolved

When a document type is configured as Revisable or Renditionable and a new document upload matches existing documents, the server returns `300 Multiple Choices` with the matches and their `canAddAsRevision`/`canAddAsRendition` flags. This is a disambiguation flow with no Unity API equivalent.

`CreateDocumentAsync` does not attempt to auto-pick a match and silently add a revision/rendition -- it surfaces the situation as a clear `DatabankException` instead, pointing at `NewDocumentRequest.StoreAsNew = true` as the explicit way to force new-document storage rather than revision/rendition matching.

This is also the only place in the REST API where `canAddAsRevision`/`canAddAsRendition` appear. There's no pre-flight equivalent to Unity API's `CanAddRevision`/`CanAddRendition` (which checked `doc.DocumentType.Revisable` synchronously before any upload attempt). `RestApi.TestHarness`'s Add Revision/Add Rendition controls always appear enabled once a document is loaded -- a disallowed attempt surfaces as a failed request rather than being blocked in the UI beforehand.

---

## No PurgeDocument

`Unity.04.DocumentArchiving`'s `PurgeDocument` (permanent deletion, bypasses recycle bin) has no confirmed REST equivalent in `document-api.json`. `DeleteRequest` here maps to `DELETE /documents/{id}` only.

---

## typeGroupId vs groupId: The Fix That Rippled Back Into RestApi.03

Building keyword-collection payloads here required tracking a keyword type group's `typeGroupId` (which kind of group) separately from its `groupId` (which specific instance). A new group instance being submitted has a `typeGroupId` but no `groupId` yet -- the server assigns one. `RestApi.03.DocumentRetrieval`'s `KeywordGroup` DTO originally collapsed both into one `Id` property, which could never express this. The fix (two separate properties `TypeGroupId`/`GroupId`) was driven by what this project needed. See `RestApi.03`'s Lesson for the full context.

---

## Keyword Collection Payload Shape

The `keywordCollection` payload sent with archive/update requests:

```json
{
  "keywordGuid": "obtained-from-GET-endpoint",
  "keywordCollection": [
    {
      "typeGroupId": "group-type-id",
      "keywords": [
        { "typeId": "keyword-type-id", "value": "keyword-value" }
      ]
    },
    {
      "typeGroupId": "multi-instance-group-type-id",
      "keywords": [
        { "typeId": "kw-type-id", "value": "instance-1-value" }
      ]
    },
    {
      "typeGroupId": "multi-instance-group-type-id",
      "groupId": "existing-instance-id",
      "keywords": [
        { "typeId": "kw-type-id", "value": "updated-instance-value" }
      ]
    }
  ]
}
```

A MultiInstance group instance being added has `typeGroupId` but no `groupId`. An existing instance being updated has both. `typeGroupId` is what distinguishes which kind of group; `groupId` is what identifies which specific row within a MultiInstance group.

---

## Endpoints Used

| Endpoint | Purpose |
|---|---|
| `POST /documents/uploads` | Initiate file upload staging |
| `PUT /documents/uploads/{id}?filePart=N` | Upload one file part |
| `POST /documents` | Store a new document |
| `PUT /documents/{id}/keywords` | Update document metadata |
| `POST /documents/{id}/revisions` | Add a new revision |
| `POST /documents/{id}/revisions/{revId}/renditions` | Add a new rendition |
| `DELETE /documents/{id}` | Delete a document |
| `GET /document-types/{id}/default-keywords` | Get `keywordGuid` for new documents |
| `GET /documents/{id}/keywords` | Get `keywordGuid` for existing documents |

---

## Takeaways

- File uploads are three steps: initiate, upload parts, reference upload IDs in the archive request. `StageUploadsAsync` handles this internally.
- `keywordGuid` is a concurrency token required on every keyword-bearing request. Fetch it fresh immediately before submitting -- it goes stale on any successful update.
- `300 Multiple Choices` surfaces when a Revisable/Renditionable document type matches existing documents. Surface it as an actionable exception; don't auto-resolve.
- No pre-flight `CanAddRevision`/`CanAddRendition` -- those flags only appear in a `300 Multiple Choices` response.
- `typeGroupId` = which kind of group; `groupId` = which specific MultiInstance instance. New instances have only `typeGroupId`; existing instances have both.
- No `PurgeDocument` -- no confirmed REST equivalent.
- Scope is conventional documents only -- `document-api.json` has no write endpoints for forms.
