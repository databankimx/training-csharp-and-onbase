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

## Try It Yourself

Call `GetDocumentInfo(new RetrievalRequest { DocumentType = "CON - Primary Document" })` against a connected session and inspect the returned `DocumentInfo` list, then call `GetDocumentLinks(...)` with the same request and compare the shape of the results, one returns metadata, the other returns POP links, same underlying query machinery (`MakeDocumentQuery`) either way.
