# Unity.04.DocumentArchiving

## What This Is

Document creation, update, and deletion: conventional documents, e-forms, and Unity Forms, with support for new documents, new revisions, new renditions, metadata-only updates, and delete/purge operations.

---

## What's in This Project

| File | What it does |
|---|---|
| `DocumentStorage.cs` | Create, update, and delete documents |
| `Models/Objects/NewDocumentRequest.cs` | DTO for storing a new document |
| `Models/Objects/UpdateDocumentRequest.cs` | DTO for updating an existing document |
| `Models/Objects/DeleteRequest.cs` | DTO for deleting or purging a document |
| `Models/Enumerations/StorageType.cs` | `ConventionalDocument`, `EForm`, `UnityForm` |
| `Models/Enumerations/UpdateType.cs` | `ModifyMetadata`, `AddRevision`, `AddRendition` |

---

## Creating a New Document

```csharp
var request = new NewDocumentRequest
{
    DocumentType = "CON - Primary Document",
    Files = new List<string> { @"C:\Temp\Contract.pdf" },
    Keywords = new List<KeywordInfo>
    {
        new KeywordInfo { Name = "Vendor Name", Value = "Acme Corp" },
        new KeywordInfo { Name = "Document Date", Value = "2026-01-15" }
    }
};

Document stored = storage.CreateDocument(request, app);
// stored.ID is now the OnBase document ID
```

For multiple pages (a multi-page TIFF, for example), add each file path to `Files`. All files must share the same extension -- `OnBaseTaxonomy.GetFileType(List<string>)` validates this.

The stored document's ID is immediately available on the returned `Document` object -- the Unity API updates the document object with its assigned ID after storage, similar to EF's behavior after `SaveChanges()`.

---

## Updating Metadata

```csharp
var request = new UpdateDocumentRequest
{
    DocumentId = 12345,
    UpdateType = UpdateType.ModifyMetadata,
    Keywords = new List<KeywordInfo>
    {
        new KeywordInfo { Name = "Vendor Name", Value = "Updated Vendor" }
    }
};

storage.UpdateDocument(request, app);
```

Keyword modification uses `Application.Core.GetDocumentModifier(doc)`, which returns a `KeywordModifier`. Individual keywords are updated with `modifier.UpdateKeyword(oldKeyword, newKeyword)`. `SaveKeywords()` commits the changes. Nothing persists until `SaveKeywords()` is called.

---

## Adding Revisions and Renditions

```csharp
// New revision (a new version of the document)
var revisionRequest = new UpdateDocumentRequest
{
    DocumentId = 12345,
    UpdateType = UpdateType.AddRevision,
    Files = new List<string> { @"C:\Temp\ContractV2.pdf" }
};

// New rendition (an alternate format for the same version)
var renditionRequest = new UpdateDocumentRequest
{
    DocumentId = 12345,
    UpdateType = UpdateType.AddRendition,
    Files = new List<string> { @"C:\Temp\Contract.tif" }
};
```

Revisions represent a new version of the document content. Renditions represent the same document version in a different file format. Both require the document type to be configured for revisability / rendition support in OnBase.

---

## Updating MultiInstance Keyword Groups

The original code only ever called `modifier.AddKeywordRecord(record)` for MultiInstance groups, which caused a well-documented problem: editing an existing MIKG instance and saving added a NEW instance alongside the old unedited one, rather than replacing it.

The fix treats a MultiInstance group in an update request as "this group's complete new set of instances." Existing instances of the group are removed first (matched by `KeywordRecordType.ID`), then the request's instances are added:

```csharp
// Remove existing instances of this group type
foreach (var existingRecord in modifier.OriginalKeywords.FindKeywordRecord(groupType))
    modifier.RemoveKeywordRecord(existingRecord);

// Add the new instances
foreach (var instance in requestGroup.Instances)
{
    var newRecord = modifier.AddKeywordRecord(recordType);
    foreach (var kwInfo in instance.Keywords)
        // ... add individual keywords
}
```

**Important consequence:** when updating a MultiInstance group, send the complete set of instances you want the document to end up with -- including any unedited ones. Instances not included in the request will be deleted along with the replaced ones.

---

## Deleting Documents

```csharp
// Soft delete (moves to recycle bin)
storage.DeleteDocument(new DeleteRequest { DocumentId = 12345 }, app);

// Purge (permanent, bypasses recycle bin)
storage.PurgeDocument(new DeleteRequest { DocumentId = 12345 }, app);
```

---

## Two Intentionally Incomplete Areas

Unlike everywhere else in this training set, two areas were NOT finished during the port -- the necessary Unity API documentation wasn't available to implement them correctly, and implementing them incorrectly would be worse than leaving them as honest stubs.

**OnBase Repeater Support.** `StoreNewUnityForm` and `UpdateUnityFormMetadata` accept `request.Form.Repeaters` (`RepeaterInfo`, a fully-defined model), but neither actually adds repeater rows to OnBase. The code has a `// TODO: Add repeater Items` comment at exactly that point. The model is ready; the Unity API wiring isn't.

**Form Revision and Rendition Updates.** `UpdateEFormRevision`, `UpdateUnityFormRevision`, `UpdateEFormRendition`, and `UpdateUnityFormRendition` all throw `NotImplementedException` explicitly. The conventional-document equivalents are fully implemented; only the form-specific branches are stubs.

If you have the relevant Unity API documentation for OnBase repeater controls or form revision/rendition storage, these are the right places to finish the implementation.

---

## Constructor Note

`DocumentStorage`'s constructor assigns `App` directly without calling `Initialize()`:

```csharp
public DocumentStorage(Application app)
{
    App = app;
}
```

`Config` and `Metadata` remain `null` until the first public method call, each of which calls `Initialize(app)` itself. This works -- nothing reads `Config`/`Metadata` before a public method runs -- but it's a genuine inconsistency with `OnBaseTaxonomy` and `Metadata`, both of which call their own `Initialize()` from their constructors. Worth knowing if you're comparing the three classes side by side.

---

## Takeaways

- `CreateDocument` returns the stored `Document` object with its assigned ID immediately available.
- Keyword updates use `KeywordModifier`. `SaveKeywords()` commits; nothing persists before it.
- For MultiInstance keyword groups in an update, send the complete desired set of instances. Existing instances are removed and replaced, not merged.
- Revisions = new version of content. Renditions = same version, different format.
- Repeater support and form revision/rendition updates are explicit `NotImplementedException` stubs, not oversights.
- `DocumentStorage`'s constructor doesn't call `Initialize()` -- `Config`/`Metadata` are initialized lazily on first method call.
