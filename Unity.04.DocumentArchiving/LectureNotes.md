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

## `UpdateKeywordGroups` Now Replaces MIKG Instances, Not Just Adds

`KeywordModifier` has no way to update an existing MultiInstance keyword record in place, the way `modifier.UpdateKeyword(oldKeyword, newKeyword)` does for single-instance/standalone keywords. The original code (unchanged when this was first ported) only ever called `modifier.AddKeywordRecord(record)` for MultiInstance groups, with a comment acknowledging as much ("We can only add MIKG records, not overwrite"). Caught via `Unity.TestHarness`: editing a value in an existing MIKG instance and saving added a NEW instance alongside the old, unedited one, rather than replacing it.

Fixed by removing the document's existing instances of a MultiInstance group FIRST (matched by `KeywordRecordType.ID` against the request's `KeywordGroup.Id`), then adding the request's instances, effectively treating a MultiInstance group in an update request as "this group's full, new set of instances" rather than "instances to append." This means a Modify Metadata caller (like `Unity.TestHarness`) should always resend the COMPLETE set of instances it wants a MultiInstance group to end up with, including any unedited ones, not just the one it changed, they'd otherwise be deleted along with the one being replaced.

**Worth confirming**: the removal call uses `modifier.RemoveKeywordRecord(existingRecord)`, a best-guess method name matching the `AddKeywordRecord`/`AddKeyword`/`UpdateKeyword` convention already used elsewhere in this class, not directly confirmed against Unity API documentation while writing this fix. If it doesn't compile, that's the name to correct.

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
