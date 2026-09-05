# Unity.05.UnityScripts

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

Unity Script templates, one file per OnBase script hook point, organized into the same folder structure as the original: `AppEnabler`, `ClientSide`, `DataBankExtensions`, `DocComp`, `DocumentHooks`, `ECommerce`, `EnterpriseIntegrationServer`, `ExternalLookups`, `IndexingHooks`, `ScanningHooks`, `UnityForms`, `UnityScheduler`, `Workflow`, `Workview`.

**These are not standalone applications.** Each file is executed *by* an already-connected OnBase client, the `Application` object is handed in by that host, never obtained here. There's no `Program.cs`, no `Connect()`/`Disconnect()` call anywhere in this project.

Two files are fully-worked-out templates, worth studying directly: `Workflow/WorkflowScript.cs` and `DataBankExtensions/UsingTheExtensionsLibrary.cs` (the same pattern, plus registering a DataBank Extensions Library license hash). Both delegate their exception handling to `HelperLibrary.HandleException()`, a shared extension method (see the `HelperLibrary` folder), rather than duplicating the same diagnostics-logging/document-history logic in each one. Every other file is a minimal, deliberate stub, just the interface implementation and `throw new NotImplementedException();`, a real starting point for filling in one specific script.

---

## What's in This Project

| Folder | Hook point |
|---|---|
| `AppEnabler` | Application Enabler screen-scrape events |
| `ClientSide` | Client-side scripts (per-item, per-batch, global) |
| `DataBankExtensions` | Using the DataBank Extensions Library from a script |
| `DocComp` | Document Composition template events |
| `DocumentHooks` | Add/modify keywords, cross-reference, archive import/reindex/revision |
| `ECommerce` | Handling charges, custom data elements |
| `EnterpriseIntegrationServer` | Message broker enrichment, fault, response, normalization |
| `ExternalLookups` | External autofill keysets, external keyword datasets |
| `HelperLibrary` | Shared exception-handling extension method and a self-contained `DatabankException`, referenced by the two fully-worked templates |
| `IndexingHooks` | Every Scan Queue indexing event (ad hoc, keyword focus/blur, point-and-shoot, pre/post index, reindex, secondary index) |
| `ScanningHooks` | Barcode processing, custom processing, pre/post scan, sweep, input file list |
| `UnityForms` | Custom action button events |
| `UnityScheduler` | Scheduled script execution |
| `Workflow` | Workflow scripts, approval conditions/roles, Business Rules Engine |
| `Workview` | Every WorkView event (filters, create/save/delete/open, action buttons, external classes, application loading, document folders) |

---

## Related Samples

- **`Unity.01.ConnectingToOnBase`** — provides the `Application` object these scripts receive.

---

## `Hyland.Unity` Package

This project references `Hyland.Unity` 26.1.2 via the `DataBank GitHub` NuGet feed. That package bundles:

- `Hyland.Unity.dll`
- `Hyland.Types.dll`
- `Hyland.Applications.Web.Security.dll`
- `Security.Cryptography.dll`

These are Hyland's own licensed OnBase Unity API binaries, not DataBank's own code. DataBank mirrors them on its internal feed for convenience, but the underlying software is licensed per OnBase deployment: **your own copies should come from your own OnBase Application Server installation** (typically found under that server's Unity API redistribution folder), not assumed to be freely available just because a NuGet package happens to exist for them.
