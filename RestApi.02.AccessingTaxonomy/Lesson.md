# RestApi.02.AccessingTaxonomy

## What This Is

Taxonomy access for the OnBase Document Management (REST) API: Document Type Groups, Document Types, Keyword Type Groups/Types, Custom Queries, File Types, and Unity Form Templates. The REST-API counterpart to `Unity.02.AccessingTaxonomy`.

`net10.0`, async/await throughout, no extra NuGet packages -- `System.Net.Http` and `System.Text.Json` are both in the base class library.

---

## What's in This Project

| File | What it does |
|---|---|
| `Models/Objects/TaxonomyModels.cs` | DTOs matching `document-api.json`'s own schemas, plus `DocumentTypeKeywordGroup` -- this project's own resolved combination type |
| `HelperClasses/OnBase/OnBaseTaxonomy.cs` | Taxonomy lookups -- same functional surface as Unity.02's version, smaller method count, one new method for resolved keyword group metadata |

---

## Every ID Is a String, Not a `long`

Unity API's `DocumentTypeGroup.ID`, `DocumentType.ID`, etc. are all `long`. Every corresponding id in `document-api.json`'s schemas is typed `"string"`. This project's DTOs use `string Id` throughout, not `long`. The "numeric-looking string gets treated as an id" heuristic from Unity.02 is kept as a caller convenience -- the id is still used as a string in the actual request either way.

---

## Taxonomy Endpoints

The Document Management API endpoints this project calls:

| Endpoint | Unity.02 equivalent |
|---|---|
| `GET /document-type-groups` | `App.Core.DocumentTypeGroups` |
| `GET /document-type-groups/{id}` | `DocumentTypeGroups.Find(id)` |
| `GET /document-types` | `App.Core.DocumentTypes` |
| `GET /document-types/{id}` | `DocumentTypes.Find(id)` |
| `GET /document-types/{id}/keyword-type-groups` | `docType.KeywordRecordTypes` |
| `GET /keyword-type-groups` | `App.Core.KeywordRecordTypes` |
| `GET /keyword-types` | `App.Core.KeywordTypes` |
| `GET /custom-queries` | `App.Core.CustomQueries` |
| `GET /file-types` | `App.Core.FileTypes` |

Multiple ids per request are supported on list endpoints: `GET /keyword-type-groups?id=X&id=Y&id=Z`. Query string values are encoded with `Uri.EscapeDataString`, not `HttpUtility.UrlEncode` -- `System.Web.HttpUtility` isn't available on `net10.0` without adding a package reference.

---

## GetDocumentTypeKeywordGroupsAsync: Real Resolution Work

This is the biggest difference from Unity.02. Unity API's `docType.KeywordRecordTypes` hands you fully-populated objects for free -- the SDK resolves them. The REST API doesn't:

`GET /document-types/{id}/keyword-type-groups` returns only the structure -- which keyword type IDs belong to which group, and whether a group has an ID (omitted `id` means Standalone, per the API's own convention). It does NOT include each group's name/storage type, or each keyword type's name/data type.

`GetDocumentTypeKeywordGroupsAsync` does the resolution internally -- what the Working with Keywords guide describes as "up to four calls to fully render keywords":

1. `GET /document-types/{id}/keyword-type-groups` -- get the structure
2. `GET /keyword-type-groups?id=X&id=Y...` -- resolve all group metadata (batched)
3. `GET /keyword-types?id=X&id=Y...` -- resolve all keyword type metadata (batched)
4. Combine into `List<DocumentTypeKeywordGroup>`

Callers get one populated result back, the same convenience Unity API's own `docType.KeywordRecordTypes` gives for free.

`DocumentTypeKeywordGroup` is this project's own combination DTO -- no `document-api.json` schema for this exact shape, since it's the result of combining three separate API responses.

---

## Unity Forms: A Different API, Same Token

An earlier version of this file claimed Unity Forms had no REST equivalent -- that was incomplete research. Unity Form Templates and instances are real REST concepts, documented in `forms-api.json`, a separate OpenAPI spec from the Document Management API.

The two APIs share a `{server}` but differ in `{product}` path segment (`onbase/core` vs `onbase/forms`). Both use the same Bearer token -- no second IdP exchange needed. Whether their session cookies are scoped to be shared wasn't confirmed; `RestApi.01`'s `SessionManagement.GetFormsHttpClient()` handles this defensively by sharing the same `HttpClientHandler`/`CookieContainer`.

`GetUnityFormTemplatesAsync`/`GetUnityFormTemplateAsync` route through `InitializeForms()`, which resolves to `SessionManagement.GetFormsHttpClient()` -- the one place in this project where a method needs a different connected client than everything else.

`GetUnityFormTemplateAsync`'s ID-or-name lookup works slightly differently: `forms-api.json`'s `GET /unity-form-templates/{id}` only accepts an ID directly, with no `systemName` query filter. A non-numeric name lists all templates and matches client-side instead.

E-Forms are deliberately out of scope -- `Unity.TestHarness`'s Taxonomy page only looked up Unity Forms, never E-Forms.

---

## Smaller Method Count Than Unity.02

Several of Unity.02's overloads exist to accept a `Document` OR a `DocumentType` parameter -- Unity API objects that carry their own document type reference. That distinction doesn't apply here: there's no `Document` object with an embedded `DocumentType` reference, only string IDs. Every method here takes plain string IDs directly.

`GetCommonKeywordTypesAsync` and `GetCustomQueryKeywordTypesAsync` are present -- both needed by `RestApi.03` to build display columns for search results -- but the overload sprawl from `GetKeywordGroupType(name, doc, app)` vs `GetKeywordGroupType(name, docType, app)` doesn't exist here.

---

## Takeaways

- Every taxonomy ID is a `string`, not a `long`. The "is numeric" heuristic is kept for caller convenience.
- `GET /document-types/{id}/keyword-type-groups` returns structure only -- no group/keyword names or types. `GetDocumentTypeKeywordGroupsAsync` resolves those in two additional batched calls.
- Unity Forms are in `forms-api.json`, not `document-api.json`. Same server, same Bearer token, separate base URL.
- `GetFormsHttpClient()` from `RestApi.01` is used for all Forms API calls -- the only method category in this class using a different client.
- `Uri.EscapeDataString` for query string encoding -- `HttpUtility.UrlEncode` isn't available on `net10.0` without an extra package.
- Method count is smaller than Unity.02 because there are no `Document`/`DocumentType` overloads -- only string IDs.
