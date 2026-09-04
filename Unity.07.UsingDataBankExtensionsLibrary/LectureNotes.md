# Unity.07.UsingDataBankExtensionsLibrary

## What This Is

A demo of `DBIMX.Unity.Extensions`'s convenience methods against a real store/update/delete flow. See `README.md` for the file breakdown.

---

## `DBIMX.Extensions_unsigned.v25` 1.0.64, Deliberately, For Now

This project targets the **unsigned** `v25` package at **1.0.64** specifically, not the `v26` package used elsewhere in this training set. A `v26` release of `DBIMX.Extensions_unsigned` is expected on the `DataBank GitHub` feed; update the `PackageReference` in `Unity.07.UsingDataBankExtensionsLibrary.csproj` once that's published.

---

## Extension Methods vs. the Plain Unity API, Side by Side

`ExtensionsDemo()` keeps the plain Unity API equivalent commented out directly next to each `DBIMX.Unity.Extensions` call it replaces:

```csharp
var docType = unity.FindDocumentType("TST Document");
// var docType = theApp.Core.DocumentTypes.Find("TST Document");
```

Worth reading both lines together for each call in the method: the extension methods aren't doing anything the plain API couldn't already do, they're shorter, and in `AddKeyword`'s case, genuinely add behavior (automatic truncation to the keyword type's configured max length) that would otherwise need to be written out by hand every time, as the method's own commented-out manual-truncation equivalent shows.

---

## Documentation Wasn't Carried Over, and Why

The original project bundled a complete DocFX-generated static documentation site for `DBIMX.Unity.Extensions` under `Resources\SDK\`, dozens of HTML, CSS, JS, and font files, tied to the old `v21`/`1.0.25` package version it referenced. That documentation would already have been stale the moment this project's package reference moved to `v25`/`1.0.64`, and will be stale again once it moves to `v26`. Rather than port a snapshot that's guaranteed to drift out of sync with whatever package version is actually referenced, check the NuGet package itself (most packages of this kind ship their own XML doc comments, visible directly in IntelliSense) or DataBank's GHE for current documentation.

---

## A Third Local `DatabankException`

Neither `Unity.00.CommonFunctionality` nor `Unity.05.UnityScripts` is a natural dependency for this project, the DBIMX Extensions library is its own topic, unrelated to either. `DatabankException.cs` here is a small, self-contained copy, matching the same reasoning both of those other projects already apply to their own local copies.

---

## Try It Yourself

Run this script against a real OnBase environment with a "TST Document" document type, a "Text Report Format" file type, and a "Description" keyword type configured, and step through `ExtensionsDemo()` watching each `DBIMX.Unity.Extensions` call and its commented-out plain-API equivalent side by side.
