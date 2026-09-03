# Unity.02.AccessingTaxonomy

## What This Is

Taxonomy lookup helpers built on the connected `Application` object `Unity.01.ConnectingToOnBase` produces. See `README.md` for the file breakdown.

---

## "Find by ID or Name" Is a Recurring Pattern, Worth Recognizing

Nearly every lookup method in `OnBaseTaxonomy.cs` follows the same shape:

```csharp
return long.TryParse(name, out long id)
    ? App.Core.DocumentTypeGroups.Find(id)
    : App.Core.DocumentTypeGroups.Find(name);
```

If the caller-supplied string parses as a number, it's treated as an ID; otherwise it's treated as a name. This means every lookup method in this class accepts either interchangeably, worth knowing before assuming a "name" parameter only ever takes a literal name.

---

## Three Genuine Bugs Fixed From the Original

- **`GetDocumentTypes(string groupName, Application app = null)`**'s `catch` block originally just logged to the console and re-threw the raw exception (`Console.WriteLine(e); throw;`), the only method in this entire class that didn't wrap failures in `DatabankException`. Fixed to match every other method here (and every other "good" project in this training set).
- **`GeKeywordType`** (missing a "t") is now **`GetKeywordType`**. This wasn't just a cosmetic typo, fixing the name makes it a genuine, correctly-named overload of the existing `GetKeywordType(string, DocumentType, Application)`, taking a `Document` instead, exactly mirroring the `GetKeywordGroupType(string, Document, ...)`/`GetKeywordGroupType(string, DocumentType, ...)` overload pair that already exists a few methods above it. The misspelling meant this overload was effectively invisible to any caller who'd naturally reach for `GetKeywordType` and expect it to accept a `Document`.
- **`GetFileType(List<string> files, ...)`** threw a plain `ApplicationException` for mismatched file extensions, inconsistent with this training set's `DatabankException` standard (the same anti-pattern `Unity.SimpleButBadExample` was built specifically to illustrate). Fixed.

---

## Try It Yourself

Call `GetDocumentType("CON - Primary Document")` and `GetDocumentType("16")` (or whatever numeric ID that document type actually has in your environment) against the same connection, and confirm both return the same `DocumentType`.
