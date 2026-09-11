# Lecture Notes: RestApi.02.AccessingTaxonomy

## Every ID Is a String, Not a `long`

Unity API's `DocumentTypeGroup.ID`/`DocumentType.ID`/etc. are all `long`. Every corresponding id in `document-api.json`'s own schemas is typed `"string"`. This project's DTOs follow the REST API's own typing throughout (`string Id`), not Unity's. The "is this numeric-looking, so treat it as an id rather than a name" heuristic (`long.TryParse`) is kept in the lookup methods purely as a convenience for callers who might pass either, the id itself is still used as a string in the actual request either way.

---

## GetDocumentTypeKeywordGroupsAsync: Real Resolution Work, Not a Straight Port

`GET /document-types/{id}/keyword-type-groups` (confirmed directly against the spec's own `KeywordTypeGroupOnDocumentType`/`KeywordTypeOnDocumentType` schemas) only returns the *structure* — which keyword type ids belong to which group, plus whether a group has an id at all (an omitted `id` means Standalone; this is the API's own documented convention, not an inference). It does **not** include each group's name/storage type, or each keyword type's name/data type — those need separate calls to `/keyword-type-groups` and `/keyword-types`.

This matches what the Working with Keywords guide describes as "up to four calls to fully render keywords." `GetDocumentTypeKeywordGroupsAsync` does that resolution internally: one call for structure, then two batched calls (`GET /keyword-type-groups?id=X&id=Y...` and `GET /keyword-types?id=X&id=Y...`, both supporting multiple ids per request) to resolve everything referenced, combining it all into one `List<DocumentTypeKeywordGroup>`. Callers get one populated result, the same convenience Unity API's own `docType.KeywordRecordTypes` property gives for free via the SDK — the resolution work just has to happen explicitly here instead of being hidden inside the SDK.

---

## Unity Forms *Do* Have a REST Equivalent — On a Different API

**Correction to earlier documentation in this project**: a first pass at this file claimed Unity Forms had no REST API counterpart at all, based on `document-api.json` alone. That was wrong, not a confirmed gap, just incomplete research — Unity Form Templates and instances are real REST API concepts, documented in `forms-api.json`, a genuinely separate OpenAPI spec from the Document Management API.

The two APIs share a `{server}` but differ in their `{product}` path segment (`onbase/core` vs `onbase/forms`), confirmed directly: every OnBase REST API is served by the same server and honors the same Bearer token, so no second IdP token exchange is needed. What wasn't confirmed is whether the two APIs' session cookies are scoped to be shared — see `RestApi.01.ConnectingToOnBase`'s own `LectureNotes.md` for how `SessionManagement.GetFormsHttpClient()` handles that defensively (sharing the same underlying `HttpClientHandler`/`CookieContainer` as the main client, working correctly either way).

`GetUnityFormTemplatesAsync`/`GetUnityFormTemplateAsync` route through `InitializeForms()` (which resolves to `SessionManagement.GetFormsHttpClient()`, not the usual `GetHttpClient()`), the one place in this project where a method needs a *different* connected client than everything else. `GetUnityFormTemplateAsync`'s id-or-name lookup also works slightly differently from this class's other single-item lookups: the Forms API's `GET /unity-form-templates/{id}` endpoint only accepts an id directly, with no `systemName` query filter on it the way the Document Management API's list endpoints have, so a non-numeric-looking value here lists every template and matches by name/system name client-side instead of via a server-side filter.

E-Forms (a related but distinct concept, also in `forms-api.json`) are deliberately out of scope: `Unity.TestHarness`'s own Taxonomy page only ever looked up Unity Forms, never E-Forms, and this port's scope is an identical feature set, not an expanded one.

`UnityFormTemplate` deliberately doesn't model `formFieldDefinitions` (a polymorphic hierarchy of Calculated/NestedTable/Repeater/ValueField definitions, each with its own discriminated schema) — out of scope for a name/id lookup panel, which is all `Unity.TestHarness`'s own Unity Form lookup ever showed.

---

## No Unity Form *Instance* Retrieval Wired Up Yet

`forms-api.json` also documents `GET /unity-forms/{documentId}` (an actual Unity Form instance's field data for a given document), not just template metadata. Not implemented here: `Unity.02.AccessingTaxonomy`'s own `GetUnityForm` only looks up templates by name, matching what `Unity.TestHarness`'s Taxonomy page needs. If a later page needs actual form instance data, that's a natural addition to this file when the need arises, not a gap to worry about now.

---

## Method Count Is Smaller Than Unity.02's, on Purpose

Several of Unity.02's overloads exist purely to accept a `Document` *or* a `DocumentType` parameter (Unity API objects that carry their own document type reference, e.g. `GetKeywordGroupTypes(names, doc, app)` vs `GetKeywordGroupTypes(names, docType, app)`). That distinction doesn't apply here — there's no `Document` object with an embedded `DocumentType` reference, only string ids. Every method here takes plain string ids directly. The functional surface (what you can look up) is the same; the overload sprawl isn't reproduced, since it existed to serve a Unity-API-specific convenience that has no REST equivalent to preserve.

---

## Query String Encoding: Uri.EscapeDataString, Not HttpUtility.UrlEncode

`System.Web.HttpUtility` is a .NET-Framework/ASP.NET-classic namespace, not part of modern .NET's default surface (unlike the `net48` projects elsewhere in this training set, where it's implicitly available). Every query string value here is percent-encoded via `Uri.EscapeDataString` instead, which is available in the base class library on `net10.0` with no extra package reference needed.
