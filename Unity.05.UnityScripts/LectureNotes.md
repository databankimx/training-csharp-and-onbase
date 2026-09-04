# Unity.05.UnityScripts

## What This Is

Unity Script templates, one file per OnBase script hook point, exact original folder structure preserved throughout. See `README.md` for the full folder-by-folder breakdown.

---

## Two Fully-Worked Templates, Worth Copying From

`Workflow/WorkflowScript.cs` and `DataBankExtensions/UsingTheExtensionsLibrary.cs` are the only files here that aren't bare stubs. Both share the same structure, worth internalizing as the pattern to reuse when actually filling in any of the other 55 files:

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

`InitializeScript` sets diagnostics verbosity based on `SystemProperties.IsProduction` (verbose in test, warning-only in production), clears any stale error property, and logs which document is being processed. `FinalizeScript` logs script completion. `DataBankExtensions/UsingTheExtensionsLibrary.cs` adds exactly one thing on top of `Workflow/WorkflowScript.cs`: `License.Register(ExtensionsHash)`, called once near the start of `InitializeScript`, before any other DataBank Extensions Library method is used.

---

## `HelperLibrary`: One Shared Exception Handler, Not Two Copies

Both fully-worked templates used to each carry their own private `HandleException` method, identical inline logic duplicated in two places. `Templates/HelperLibrary/HelperLibrary.cs` now holds that logic once, as an extension method on `Exception`:

```csharp
public static void HandleException(this Exception ex, Application app,
    WorkflowEventArgs wfArgs = null, string errorProperty = "UnityError",
    bool writeToDocHistory = false, Document doc = null)
```

It walks the **full** `InnerException` chain (not just the outermost message), logging each level to the diagnostics console, and, on the innermost/root exception, writes the error to both the workflow property bag (if `wfArgs` is supplied) and the document history (if `writeToDocHistory` is true and `doc` is supplied). Both templates now call `ex.HandleException(...)` from their `catch` block instead of maintaining their own copy.

`HelperLibrary.cs` also defines its own minimal `DatabankException`, a **third** copy of this type across the whole solution (`CSharp.SharedLibrary`'s, `Unity.00.CommonFunctionality`'s, and now this one). Not an oversight: `Unity.05.UnityScripts` deliberately has no `ProjectReference` to `Unity.00.CommonFunctionality` (these templates are meant to be individually copied out of this project entirely, into a client's own codebase, so they shouldn't drag in a dependency on the rest of this training set), so `InitializeScript`/`FinalizeScript`'s `throw new DatabankException(...)` calls need a locally-defined type to throw. The `HelperLibrary` folder itself is a good example of the pattern the code comment inside it describes: "library scripts can be referenced by any Unity script and do not require implementation of specific interfaces like `IWorkflowScript`, they are meant to provide utility functions and shared logic."

---

## A Real Bug, Fixed: `PostArchiveRevision.cs`

The original file implemented `IDocumentReindexPostArchiveEventScript`, the exact same interface as `PostArchiveReindex.cs` sitting right next to it, despite this file's own name describing a **revision** event, not a reindex event. Corrected to `IDocumentRevisionPostArchiveEventScript` (with `DocumentRevisionPostArchiveEventArgs` as the matching event args type, inferred from this API's own consistent `I{Feature}Script`/`{Feature}EventArgs` naming pattern, worth confirming against the actual Unity API docs if it doesn't compile as-is in your environment).

---

## Every Other File: A Deliberate Blank Canvas

55 of these 57 files are minimal on purpose: the correct interface, the correct method signature(s), and `throw new NotImplementedException();`. This isn't unfinished work, it's the actual starting point a developer would use: pick the one script hook you need, copy that one file (or its shape) into a real project, replace the exception with real logic (following `WorkflowScript.cs`'s pattern above for structure), and leave the other 56 alone. Using all 57 at once, in one project, was never the intent, this project exists to be **browsed and copied from**, not run as-is.

---

## Try It Yourself

Pick any file in `DocumentHooks/` or `IndexingHooks/`, and rewrite its `OnItemExecute` method following `WorkflowScript.cs`'s `InitializeScript`/`HandleException`/`FinalizeScript` pattern (adjusted for that hook's own event args type), notice how little actually changes between hook points once you're following the same structure.
