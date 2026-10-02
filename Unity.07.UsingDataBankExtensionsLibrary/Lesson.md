# Unity.07.UsingDataBankExtensionsLibrary

## What This Is

A Workflow Unity Script demonstrating the `DBIMX.Unity.Extensions` library's convenience methods, side by side with the equivalent plain Unity API calls they replace. The demo stores a new document, adds a keyword with automatic truncation, reads keyword values back, and deletes the document.

This is not a standalone application -- it's a Unity Script, executed by the OnBase workflow engine. The `Application` object is handed in by the host.

---

## NuGet Package Note

This project uses `DBIMX.Extensions_unsigned.v25` at `1.0.64` -- the unsigned variant targeting the v25 OnBase SDK. A `v26` release is expected on the `DataBank GitHub` feed; update the `PackageReference` in the `.csproj` once that's published.

Both `DBIMX.Extensions_unsigned.v25` and `Hyland.Unity` are consumed from the internal `DataBank GitHub` NuGet feed and are only available to DataBank employees with access to that feed.

---

## License Registration

```csharp
License.Register(LicenseKey);
```

Called once near the start of `InitializeScript`, before any other `DBIMX.Unity.Extensions` method is used. `LicenseKey` here is a hardcoded "User-Editable Script Setting," the conventional placement for values the script's configuring developer is expected to supply. This differs from `Unity.06.UnityFormDefaultValues`'s `Token`, which lives in `App.config` because that project is a standalone console app with a natural configuration file -- Unity Scripts hosted by OnBase clients have no such file.

---

## The Extension Methods, With Their Plain-API Equivalents

Every `DBIMX.Unity.Extensions` call in `ExtensionsDemo()` has its plain-Unity-API equivalent commented out directly alongside it. Read them together:

**Finding a document type:**
```csharp
var docType = unity.FindDocumentType("TST Document");
// var docType = theApp.Core.DocumentTypes.Find("TST Document");
```

**Finding a file type:**
```csharp
var fileType = unity.FindFileType("Text Report Format");
// var fileType = theApp.Core.FileTypes.Find("Text Report Format");
```

**Checking for a keyword type:**
```csharp
if (newDoc.DocumentType.TryGetKeywordType("Description", out keyType))
```
`TryGetKeywordType` follows the `TryParse` pattern -- returns `bool`, sets the `out` parameter on success -- rather than returning `null` on failure the way `Find` does. Same information, different failure-handling contract.

**Adding a keyword with automatic truncation:**
```csharp
keyMod.AddKeyword("Description", description, true);

// Without the extension method, you'd write:
// if (description.Length > keyType.DataLength)
//     description = description.Substring(0, (int)keyType.DataLength);
// keyMod.AddKeyword("Description", description);
```

The `true` parameter enables automatic truncation to the keyword type's configured max length. This is the one case where the extension method adds behavior beyond syntactic shortening -- the plain API equivalent requires writing the truncation check by hand every time.

**Reading keyword values:**
```csharp
var first  = newDoc.KeywordRecords.GetFirstKeyword("Description");
var all    = newDoc.KeywordRecords.GetAllKeywordValues("Description");
var having = newDoc.KeywordRecords.GetFirstKeywordHavingValue("Description", "MY DESCRIPTION");
int count  = newDoc.KeywordRecords.GetKeywordInstanceCount("Description");
```

**Deleting a document:**
```csharp
unity.DeleteDocument(newDoc.ID);
// theApp.Core.Storage.DeleteDocument(newDoc);
```

---

## Script Structure

Same `InitializeScript`/`HandleException`/`FinalizeScript` pattern as `Unity.05.UnityScripts`'s `WorkflowScript.cs`. The difference here is the `License.Register(LicenseKey)` call at the end of `InitializeScript` -- the Extensions Library must be registered before any of its methods are used:

```csharp
private void InitializeScript(Application app, WorkflowEventArgs args)
{
    unity = app;
    unity.Diagnostics.Level = unity.SystemProperties.IsProduction ? ProdDiagLevel : TestDiagLevel;
    unity.Diagnostics.WriteIf(Diagnostics.DiagnosticsLevel.Info, $"Start Script - {ScriptName}");
    wfArgs = args;
    wfArgs.ScriptResult = true;
    // ... clear error property, get doc ...
    License.Register(LicenseKey);
}
```

---

## A Third Local DatabankException

This project's own `DatabankException.cs` is a third copy of that type in the solution. Neither `Unity.00.CommonFunctionality` nor `Unity.05.UnityScripts` is a natural dependency for a project whose entire topic is demonstrating the DBIMX Extensions Library. The local copy keeps this project self-contained with no extraneous references.

---

## Why Documentation Wasn't Ported

The original project bundled a complete DocFX-generated static documentation site for `DBIMX.Unity.Extensions` under `Resources\SDK\` -- dozens of HTML, CSS, JS, and font files, tied to the `v21`/`1.0.25` package version the original referenced. That documentation was already stale when the package reference moved to `v25`/`1.0.64`, and will be stale again when it moves to `v26`. Rather than port a snapshot guaranteed to drift out of sync, check the NuGet package itself (XML doc comments are visible in IntelliSense) or DataBank's GHE for current documentation.

---

## Try It Yourself

Run this script against a real OnBase environment with a "TST Document" document type, a "Text Report Format" file type, and a "Description" keyword type configured. Step through `ExtensionsDemo()` watching each `DBIMX.Unity.Extensions` call and its commented-out plain-API equivalent side by side. Pay particular attention to `AddKeyword` with `true` -- that's the one that does something the plain API doesn't.

---

## Takeaways

- `License.Register(LicenseKey)` must be called before any `DBIMX.Unity.Extensions` method. Call it at the end of `InitializeScript`.
- The extension methods are syntactic shortcuts for most operations. `AddKeyword(..., truncate: true)` is the exception -- it adds automatic keyword-length enforcement.
- `TryGetKeywordType` follows the `TryParse` contract (bool return + out parameter) rather than returning null. Choose based on which failure-handling style you prefer.
- `LicenseKey` is a user-editable script constant, not an app.config value -- Unity Scripts have no config file.
- `v25` package currently; update to `v26` when available on the DataBank GitHub NuGet feed.
