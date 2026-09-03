# Unity.SimpleButBadExample

## What This Is

The intentionally-bad starting point for the OnBase Unity API training set. See `README.md` for how to run it. This file exists to name, specifically, everything wrong with it, so the contrast against every project that follows is concrete rather than vague.

---

## Hardcoded Credentials, in Source Control

```csharp
private const string AppServer = "http://OnBaseTestVM/AppServer/Service.asmx";
private const string DataSource = "OnBaseTest";
private const string UserName = "MANAGER";
private const string Password = "password";
```

These are compile-time constants, meaning they're baked directly into the compiled assembly and committed to source control in plain text. Anyone with read access to the repository (or a decompiler and the `.dll`) has the OnBase credentials. `Unity.00.CommonFunctionality`'s `ServiceLocation`/`OnBaseSettings` classes read this from an external XML configuration file instead, with the password itself optionally encrypted via a registry key (see `ServiceLocation.DecryptedPassword`), genuinely separating "what the code does" from "what account it does it as."

---

## One Method Doing Everything

`Main()` calls `Connect`, `RetrieveDocuments`, `ReportDocumentDetails`, `GetDocumentFile`, `UpdateDocumentKeyword`, `UploadDocumentRevision`, `UploadNewDocument`, and `DeleteDocument`, in sequence, all as `private static` methods on the same class. There's no way to reuse "just the connection logic" or "just the retrieval logic" anywhere else, and no way to unit test any single piece of it without dragging in everything else in the file, plus a live OnBase connection. Compare this against `Unity.01.ConnectingToOnBase`'s `SessionManagement.cs`, which contains connection logic and nothing else, reusable by any project that references it.

---

## `ApplicationException`, Not `DatabankException`

Every `catch` block here wraps its exception in a plain `ApplicationException`. Every other project in this training set uses `DatabankException` (from `CSharp.SharedLibrary`) instead, DataBank's own exception type, which gives calling code a consistent, recognizable exception to catch specifically for "something in a DataBank-authored library failed," distinct from exceptions that originate from .NET itself or from third-party code. Using the generic base type here loses that distinction entirely.

---

## No Testability, Anywhere

Because `Connect()`, `RetrieveDocuments()`, and everything else are `private static` methods directly inside `Program`, none of them can be called, mocked, or verified from a test project at all without reflection tricks. `Unity.00.CommonFunctionality`/`Unity.01.ConnectingToOnBase`'s public, instance-based (or at least individually-referenceable) methods can be exercised directly, the structural difference that makes NUnit test suites possible later in this training set at all.

---

## Try It Yourself

Read through `Program.cs` once, then open `Unity.01.ConnectingToOnBase`'s `SessionManagement.cs` side by side. Same underlying Unity API calls, genuinely different structure, and it's worth noticing exactly where and why they diverge as you go through the rest of this training set.
