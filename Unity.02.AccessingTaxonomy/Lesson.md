# Unity.02.AccessingTaxonomy

## What This Is

Lookup methods for every category of OnBase "taxonomy" object: document type groups, document types, keyword group (record) types, keyword types, custom queries, file types, and Unity Form templates. Everything here takes an already-connected `Application` object and wraps it in a class instance -- no connection logic lives here.

---

## OnBase Taxonomy Hierarchy

Understanding the hierarchy is prerequisite to using the lookup methods:

```
Application
  └── DocumentTypeGroups          (collections of related document types)
        └── DocumentTypes         (each document type configured in OnBase)
              └── KeywordRecordTypes   (groups of keyword fields)
                    ├── RecordType.StandAlone   (the pseudo-group for non-grouped keywords)
                    ├── RecordType.SingleInstance (one set of values per document)
                    └── RecordType.MultiInstance  (multiple rows; each row is one "instance")
                          └── KeywordTypes        (individual keyword field definitions)
  └── KeywordTypes                (all keyword types, system-wide)
  └── CustomQueries               (saved searches defined in OnBase)
  └── FileTypes                   (OnBase file format definitions)
  └── UnityFormTemplates          (Unity Form template definitions)
```

---

## The "ID or Name" Pattern

Every lookup in this class follows the same shape:

```csharp
return long.TryParse(name, out long id)
    ? App.Core.DocumentTypes.Find(id)
    : App.Core.DocumentTypes.Find(name);
```

If the caller passes a string that parses as a number, it's treated as an ID. Otherwise it's treated as a name. This means every method in this class accepts either form interchangeably -- `GetDocumentType("16")` and `GetDocumentType("CON - Primary Document")` both work, resolving to the same object when 16 is that document type's ID.

This is worth knowing before assuming a "name" parameter only ever takes a literal name.

---

## Document Type Groups and Types

```csharp
// Get all groups
List<DocumentTypeGroup> all = taxonomy.GetDocumentTypeGroups();

// Get specific groups by name or ID
List<DocumentTypeGroup> specific = taxonomy.GetDocumentTypeGroups(new[] { "Contracts", "15" });

// Get a single group
DocumentTypeGroup group = taxonomy.GetDocumentTypeGroup("Contracts");

// Get all document types in a group
List<DocumentType> typesInGroup = taxonomy.GetDocumentTypes("Contracts");

// Get all document types (system-wide)
List<DocumentType> allTypes = taxonomy.GetDocumentTypes(null);

// Get a single document type
DocumentType docType = taxonomy.GetDocumentType("CON - Primary Document");
```

---

## Keyword Group Types and Keyword Types

Keyword Record Types (keyword groups) are associated with document types, not the application globally. When `docType` is supplied, only keyword types assigned to that document type are returned:

```csharp
// All keyword group types for a document type
List<KeywordRecordType> groups = taxonomy.GetKeywordGroupTypes(null, docType);

// Specific keyword group types for a document type
List<KeywordRecordType> specific = taxonomy.GetKeywordGroupTypes(new[] { "Contact Info" }, docType);

// Keyword types for a document type (from all its groups)
List<KeywordType> kwTypes = taxonomy.GetKeywordTypes(null, docType);

// A single keyword type
KeywordType kwType = taxonomy.GetKeywordType("Document Date", docType);
```

Without a `docType`, lookups search system-wide (`App.Core.KeywordTypes`, `App.Core.KeywordRecordTypes`).

---

## StandAlone vs. Named Groups

Every `DocumentType.KeywordRecordTypes` collection includes a `StandAlone` pseudo-group (`RecordType.StandAlone`) holding all keyword types that don't belong to a named group. `SplitKeywordGroups` separates these into two lists so callers don't have to implement that check themselves:

```csharp
var (namedGroups, standaloneTypes) = taxonomy.SplitKeywordGroups(docType);
```

This distinction matters for display (standalone keywords appear without a group header), for search (adding standalone keywords to a query is simpler than building a group structure), and for storage (standalone and grouped keywords are added to `KeywordModifier` differently).

---

## MultiInstance Keyword Groups

`RecordType.MultiInstance` groups can have multiple independent sets of values on a single document -- each "instance" is a separate row of keyword values. This is different from `RecordType.SingleInstance` (one set of values only) and `RecordType.StandAlone` (no group structure at all).

The distinction matters for both retrieval and archiving. For retrieval, see `Unity.03`'s `GetKeywordColumns` discussion of why MultiInstance groups are excluded from hit-list display columns. For archiving, see `Unity.04`'s `UpdateKeywordGroups` discussion of how existing instances are replaced rather than appended.

---

## Common Keyword Types Across Multiple Document Types

When searching across multiple document types simultaneously, only keyword types shared by every selected type can meaningfully be used as search criteria. `GetCommonKeywordTypes` computes that intersection:

```csharp
List<KeywordType> common = taxonomy.GetCommonKeywordTypes(new[] { docType1, docType2 });
```

This is what `Unity.TestHarness`'s multi-document-type search uses to decide which search fields to display.

---

## File Types

```csharp
// From a file extension string ("PDF", "TIF", "DOC")
FileType fileType = taxonomy.GetFileType("PDF");

// From a numeric ID
FileType fileType = taxonomy.GetFileType(2L);

// From a list of files (validates all files have the same extension)
FileType fileType = taxonomy.GetFileType(new List<string> { "a.pdf", "b.pdf" });
```

The file list overload throws `DatabankException` if the files don't all share the same extension -- OnBase requires a single file type per document.

---

## Three Bugs Fixed From the Original

- **`GetDocumentTypes(string groupName, ...)`**: the `catch` block originally logged to the console and rethrew the raw exception, inconsistent with every other method in this class. Now wraps in `DatabankException`.
- **`GetKeywordType(string, Document, ...)`**: was named `GeKeywordType` (missing the second `t`), making it effectively invisible to any caller expecting the correct spelling and creating a misleading asymmetry with `GetKeywordGroupType`.
- **`GetFileType(List<string>, ...)`**: threw `ApplicationException` for mismatched extensions. Changed to `DatabankException` to match this training set's convention throughout.

---

## Takeaways

- Every lookup accepts either a name or a numeric ID as a string.
- `docType` is optional on most methods. Without it, lookups are system-wide.
- `SplitKeywordGroups` separates standalone keywords from named groups -- a distinction that affects display, search, and storage in different ways.
- MultiInstance keyword groups allow multiple rows of values per document. SingleInstance and StandAlone do not.
- `GetCommonKeywordTypes` finds keyword types shared across multiple document types, useful for multi-type search UIs.
- `GetFileType(List<string>)` validates that all files in a list share the same extension before looking up the type.
