# Unity.03.DocumentRetrieval

## What This Is

Document retrieval built on `Unity.02.AccessingTaxonomy`'s lookups. See `README.md` for the file breakdown.

---

## `Hyland.Applications.Web.Security` Is Bundled Inside `Hyland.Unity`

`CreateDocPopLink()` uses `ChecksumCreator` from the `Hyland.Applications.Web.Security` namespace, generating a signed checksum for DocPop URLs (so the receiving page can verify the link wasn't tampered with). The original project referenced this as a separate local DLL (`Hyland.Applications.Web.Security.dll`, alongside `Hyland.Unity.dll` itself), but it's actually bundled inside the `Hyland.Unity` NuGet package, no separate `PackageReference` needed.

---

## `DocumentExtensions.cs` Got Its Missing Conventions Back

This was the one file in the entire original codebase with no copyright header, no `Using Directives` region, and no `Source Code Information` footer, everywhere else in this training set has all three. Added here purely for consistency; the actual extension-method logic is otherwise a faithful port.

---

## No `Unity.01.ConnectingToOnBase` Reference, Deliberately

The original `.csproj` referenced `Unity.01.ConnectingToOnBase`, but nothing in this project's actual code touches that namespace, `Application` objects are always passed in as parameters (from whatever code already connected), never obtained here. Dropped as an unnecessary dependency. Worth noticing as a general habit: a `ProjectReference` that isn't backed by an actual `using` somewhere is dead weight, and it's worth periodically checking whether every reference in a `.csproj` is still earning its place.

---

## `DocumentFile.Base64Content`: A Computed Property, Not a Second Field

The original `DocumentFile` class had a comment noting a base64-string content option "would" be included in a real-world example, never actually implemented. `Base64Content` is that option now, added as:

```csharp
public string Base64Content => Content == null ? null : Convert.ToBase64String(Content);
```

Deliberately a **computed** property, not a second settable field alongside `Content`. If both were independently settable, nothing would stop them from disagreeing with each other (someone sets `Content` to a new byte array but forgets to update `Base64Content`, or vice versa), a genuine, easy-to-introduce bug. Computing it from `Content` on every read means there is only ever one real source of truth, `Base64Content` is just a different encoding of the same bytes.

---

## `RetrievalRequest.DocumentTypes` (Plural): Multi-Type Search in One Query

`DocumentQuery.AddDocumentType` can be called more than once to search across several document types at once, but `RetrievalRequest`/`MakeDocumentQuery` originally only ever exposed a single `DocumentType` string, no way to search multiple types in one query. `DocumentTypes` (plural, a `List<string>`) is that missing capability, added once `Unity.TestHarness` needed a genuine multi-select document type search. `DocumentType` (singular) is kept for backward compatibility, and takes precedence only when `DocumentTypes` is empty. Display columns are added per selected type (mirroring the exact same pattern the Custom Query path already used for `customQuery.DocumentTypes`, itself already spanning multiple document types with potentially-overlapping keyword types, so this isn't introducing a new risk).

---

## `GetDocumentFile(Rendition, ...)`: a Specific-Rendition Overload, and `preferPdf`

`GetDocumentFile(Document doc, ...)` was originally hardcoded to `doc.DefaultRenditionOfLatestRevision`, no way to retrieve a *specific* revision/rendition a caller had already selected (e.g., `Unity.TestHarness`'s document viewer, letting a user browse a document's full revision/rendition history). `GetDocumentFile(Rendition rendition, ...)` is that missing overload, `GetDocumentFile(Document, ...)` now just delegates to it using the default rendition, so the file-type-switch logic isn't duplicated between the two.

Both overloads (plus the `long id` ones) also take a `preferPdf` flag: when true and the rendition's file type supports it, retrieval goes through `retrieval.PDF.GetDocument(rendition)` instead of the type's native provider, per Hyland's own PDFDataProvider/ImageDataProvider support table. `IsPdfConvertible(fileTypeId)` is the public check backing this, built from `Unity.00.CommonFunctionality`'s own `FileFormat` enum (`Text`, `Image`, `Pcl`, `Word`, `Excel`, `Pdf`), **not** a hand-typed list of IDs. The support table also lists AFP and DJDE as PDF-convertible, but neither has a corresponding `FileFormat` member here, so they're deliberately left out rather than guessed at; `preferPdf` is silently ignored (falls through to native retrieval) for any file type not in the confirmed set, rather than attempting the PDF provider and letting the Unity API itself fail on an unsupported format.

---

## `GetKeywordColumns` Skips MultiInstance Groups, Deliberately

Adding a MultiInstance keyword group's Keyword Types as display columns on a `DocumentQuery` causes OnBase to return one result row **per instance** of that group on a document, not one row per document, a flat display-column result set has no way to represent "this document has three values for this column." A document with two instances of a multi-instance keyword group showed up twice in `Unity.TestHarness`'s search results before this was caught: same document, same ID, appearing as two separate rows. `GetKeywordColumns` now skips `RecordType.MultiInstance` groups entirely when building display columns, only StandAlone and SingleInstance keyword columns are added. This only affects what a hit-list SEARCH shows, not what's actually available: `GetDocumentInfo(Document)` (used once a document is selected, not during the search itself) still reads the full, real multi-instance data straight from `doc.KeywordRecords`.

---

## `CreateDocPopLink`'s Checksum Was Never Actually Optional

`ChecksumCreator`'s own `CreateChecksum()` throws ("either 'ChecksumKey' or 'ChecksumValue' unavailable") if asked to compute a checksum with a blank seed. `CreateDocPopLink` had gotten the OPTIONALITY half right, `DocPopChecksumSeed` being blank correctly skipped APPENDING `&chksum=...` to the URL, but the checksum was still being CALCULATED unconditionally one line above, so any environment without a configured `DocPopChecksumSeed` (a genuinely common case, checksums are an optional tamper-check, not a requirement) threw a `DatabankException` wrapping that `ApplicationException` on every single `GetDocumentLink`/`GetDocumentLinks` call, caught by `Unity.TestHarness` selecting ANY search result. Fixed by skipping the `ChecksumCreator` call itself, not just its use in the URL, when the seed is blank.

---

## Try It Yourself

Call `GetDocumentInfo(new RetrievalRequest { DocumentType = "CON - Primary Document" })` against a connected session and inspect the returned `DocumentInfo` list, then call `GetDocumentLinks(...)` with the same request and compare the shape of the results, one returns metadata, the other returns POP links, same underlying query machinery (`MakeDocumentQuery`) either way.
