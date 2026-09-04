# Unity.04.DocumentArchiving

## What This Is

Document storage built on `Unity.02.AccessingTaxonomy`/`Unity.03.DocumentRetrieval`. See `README.md` for the file breakdown.

---

## Two Genuinely Unfinished Areas, Preserved As-Is

Unlike everywhere else in this training set, these gaps were **not** fixed during the port, they need Unity API documentation this migration didn't have access to, and implementing them incorrectly would be worse than leaving them honest stubs.

**Repeater support.** `StoreNewUnityForm` and `UpdateUnityFormMetadata` both accept `request.Form.Repeaters` (`RepeaterInfo`, a real, fully-defined model), but neither actually adds those repeater rows to OnBase:

```csharp
foreach (var field in request.Form.Fields) props.AddField(field.Name, field.Value);

// TODO: Add repeater Items, see Training Notes above

return storage.StoreNewUnityForm(props);
```

**Form revision/rendition updates.** `UpdateEFormRevision`, `UpdateUnityFormRevision`, `UpdateEFormRendition`, and `UpdateUnityFormRendition` all throw `NotImplementedException` outright. The conventional-document equivalents (`UpdateRevision`/`UpdateRendition`'s main paths) are fully implemented; only the form-specific branches are stubs, the same class of intentional gap as `Unity.01`'s `SAML`/`ADFS` `IdpGrantType` stubs.

If you have the relevant Unity API documentation for OnBase repeater controls or form revision/rendition storage, these are good candidates to actually finish.

---

## `DocumentStorage`'s Constructor Doesn't Call `Initialize()`

```csharp
public DocumentStorage(Application app)
{
    App = app;
}
```

Unlike `OnBaseTaxonomy`/`Metadata` (both of which call their own `Initialize()` from their constructors), `DocumentStorage`'s constructor just assigns `App` directly, `Config`/`Metadata` stay `null` until the first public method call, each of which calls `Initialize(app)` itself before touching either. Functionally fine (nothing reads `Config`/`Metadata` before a public method runs), but worth noticing as a genuine inconsistency in the original construction pattern if you're comparing these three classes side by side.

---

## Try It Yourself

Call `CreateDocument(new NewDocumentRequest { DocumentType = "CON - Primary Document", Files = { @"C:\Temp\Sample.pdf" } })` against a connected session, then `DeleteDocument(new DeleteRequest { DocumentId = <the returned document's ID> })` to clean it back up.
