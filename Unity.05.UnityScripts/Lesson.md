# Unity.05.UnityScripts

## What This Is

Unity Script templates, one file per OnBase script hook point, organized into the same folder structure you'd use in a real OnBase project. These are not standalone applications -- each file implements a specific interface that the OnBase client calls into. There's no `Program.cs`, no `Connect()`/`Disconnect()` call anywhere in this project. The `Application` object is handed in by the OnBase host.

**This project is meant to be browsed and copied from, not run as-is.** Pick the hook point you need, copy that file into a real project, and replace the `throw new NotImplementedException()` with real logic.

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
| `HelperLibrary` | Shared exception-handling extension method and a self-contained `DatabankException` |
| `IndexingHooks` | Every Scan Queue indexing event |
| `ScanningHooks` | Barcode processing, custom processing, pre/post scan, sweep, input file list |
| `UnityForms` | Custom action button events |
| `UnityScheduler` | Scheduled script execution |
| `Workflow` | Workflow scripts, approval conditions/roles, Business Rules Engine |
| `Workview` | Every WorkView event |

---

## The Two Fully-Worked Templates

`Workflow/WorkflowScript.cs` and `DataBankExtensions/UsingTheExtensionsLibrary.cs` are the only files here that aren't bare stubs. Both share the same structure:

```csharp
public void OnWorkflowScriptExecute(Application app, WorkflowEventArgs args)
{
    try
    {
        InitializeScript(app, args);
        // Add your code here
    }
    catch (Exception ex)
    {
        ex.HandleException(unity, wfArgs, ErrorProperty, LogErrorToDocHistory, doc);
    }
    finally
    {
        FinalizeScript();
    }
}
```

**`InitializeScript`:** sets diagnostics verbosity based on `SystemProperties.IsProduction` (verbose in test, warning-only in production), clears any stale error property, and logs which document is being processed.

**`FinalizeScript`:** logs script completion.

**`HandleException`:** defined in `HelperLibrary.cs` as an extension method on `Exception`. Walks the full `InnerException` chain, logging each level, and on the root exception writes the error to the workflow property bag (if `wfArgs` is supplied) and the document history (if `writeToDocHistory` is true and `doc` is supplied).

`DataBankExtensions/UsingTheExtensionsLibrary.cs` adds exactly one thing: `License.Register(ExtensionsHash)`, called once near the start of `InitializeScript`, before any other DataBank Extensions Library method is used.

---

## Using InitializeScript and FinalizeScript

The diagnostics verbosity logic is what makes test environments easier to debug. In OnBase, `SystemProperties.IsProduction` reflects whether the current environment is flagged as production. Verbose logging in test environments lets you trace exactly what a script is doing without shipping debug noise to production users.

The "clear stale error property" step matters because a workflow property from a previous execution might still hold an error message if the item was requeued after a prior failure. Clearing it at the start ensures the current execution's result is always fresh.

---

## HelperLibrary

`HelperLibrary.cs` holds the shared exception handler and a minimal `DatabankException`. Both templates originally carried identical private `HandleException` methods. Moving the logic to a shared extension method means a change to the error-handling behavior only needs to happen once.

The `DatabankException` here is a third copy of that type in this solution (alongside `CSharp.SharedLibrary` and `Unity.00.CommonFunctionality`). It's deliberate: these templates are meant to be individually copied into client codebases with no dependency on the rest of this training set. A locally-defined exception type keeps each copied file self-contained.

---

## Every Other File Is a Deliberate Stub

55 of the 57 files look like this:

```csharp
public class PostArchiveReindex : IDocumentReindexPostArchiveEventScript
{
    public void OnPostArchiveEvent(Application app, DocumentReindexPostArchiveEventArgs args)
    {
        throw new NotImplementedException();
    }
}
```

This is intentional. The correct interface, the correct method signature, and a `NotImplementedException` as a placeholder. This is the starting point a developer would actually use. Copy the file, fill in the method body following `WorkflowScript.cs`'s pattern, delete the rest.

---

## One Real Bug Fixed: PostArchiveRevision.cs

The original `PostArchiveRevision.cs` implemented `IDocumentReindexPostArchiveEventScript` -- the exact same interface as `PostArchiveReindex.cs` sitting right next to it, despite this file's name describing a revision event. Corrected to `IDocumentRevisionPostArchiveEventScript` with `DocumentRevisionPostArchiveEventArgs`, following the API's consistent `I{Feature}Script`/`{Feature}EventArgs` naming pattern.

---

## Try It Yourself

Pick any file in `DocumentHooks/` or `IndexingHooks/`. Rewrite its `OnItemExecute` (or equivalent) method following `WorkflowScript.cs`'s `InitializeScript`/`HandleException`/`FinalizeScript` pattern, adjusting for that hook's own event args type. Notice how little actually changes between hook points once you're following the same structure -- the event args type and the method name differ; the surrounding scaffolding is identical.

---

## Takeaways

- These are not standalone applications. The `Application` object is handed in by the OnBase host at runtime.
- Two files are fully worked out (`WorkflowScript.cs`, `UsingTheExtensionsLibrary.cs`). Copy these patterns into real projects.
- `HelperLibrary.HandleException` walks the full exception chain and writes to both workflow properties and document history. Use it from the `catch` block of every script.
- 55 files are minimal stubs by design. Pick the one you need, copy it, fill it in.
- `License.Register(ExtensionsHash)` must be called before any DataBank Extensions Library method is used. Call it near the start of `InitializeScript`.
