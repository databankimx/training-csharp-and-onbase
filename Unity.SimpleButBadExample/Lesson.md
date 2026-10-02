# Unity.SimpleButBadExample

## What This Is

The first project in the OnBase Unity API training set, and the only one that's deliberately bad. It performs a complete, working Unity API walkthrough -- connect, query, retrieve, update, upload, delete, disconnect -- all functional. But it's written the way a first attempt often is: one `Program.cs`, hardcoded credentials, no separation of concerns anywhere.

Every project after this one does the same work correctly. Keep this one in mind as the "before" picture.

---

## What's Wrong With It, Specifically

### 1. Hardcoded Credentials in Source Control

```csharp
private const string AppServer = "http://OnBaseTestVM/AppServer/Service.asmx";
private const string DataSource = "OnBaseTest";
private const string UserName = "MANAGER";
private const string Password = "password";
```

Compile-time constants. Baked directly into the compiled assembly. Committed to source control in plaintext. Anyone with read access to the repository (or a decompiler and the `.dll`) has the OnBase credentials.

`Unity.00.CommonFunctionality`'s `ServiceLocation`/`OnBaseSettings` reads this from an external XML config file instead, with the password optionally encrypted via a DPAPI-protected registry key. "What the code does" is genuinely separated from "what account it does it as."

### 2. One Method Doing Everything

`Main()` calls `Connect`, `RetrieveDocuments`, `ReportDocumentDetails`, `GetDocumentFile`, `UpdateDocumentKeyword`, `UploadDocumentRevision`, `UploadNewDocument`, and `DeleteDocument` in sequence -- all as `private static` methods on the same class. There's no way to:

- Reuse "just the connection logic" from another project
- Reuse "just the retrieval logic" from another project
- Test any single piece without pulling in everything else and a live OnBase connection

Compare `Unity.01.ConnectingToOnBase`'s `SessionManagement.cs`: connection logic and nothing else, reusable by any project that references it.

### 3. `ApplicationException`, Not `DatabankException`

Every `catch` block wraps in `ApplicationException`. Every other project in this training set uses `DatabankException` -- DataBank's own exception type, which lets calling code catch specifically for "something in a DataBank-authored library failed," distinct from exceptions that originate from .NET itself or from third-party code. Using the generic base type here loses that distinction entirely.

### 4. No Testability

`Connect()`, `RetrieveDocuments()`, and everything else are `private static` methods directly inside `Program`. None can be called, mocked, or verified from a test project without reflection tricks. The public, individually-referenceable methods in `Unity.00`-`04` can be exercised directly -- the structural difference that makes unit testing possible later in the training set.

---

## What It Gets Right

It does work. The underlying Unity API calls are correct. The sequence of operations -- connect, query, retrieve, modify, upload, delete, disconnect -- covers the same territory as the structured projects that follow. Reading it is a useful orientation to what the Unity API actually does at the call level, before the structure around those calls becomes the focus.

---

## How to Run

1. Update the constants at the top of `Program.cs` (`AppServer`, `DataSource`, `UserName`, `Password`, `DocTypeName`, etc.) to match a real OnBase test environment.
2. Press F5 (or `dotnet run`). The program pauses between each step.

---

## Reading It Alongside the Structured Projects

Open `Program.cs` here and `Unity.01.ConnectingToOnBase`'s `SessionManagement.cs` side by side. The connection call:

**Here:**
```csharp
var authProps = Application.CreateOnBaseAuthenticationProperties(AppServer, UserName, Password, DataSource);
var app = Application.Connect(authProps);
```

**Unity.01:**
```csharp
authProps = Application.CreateOnBaseAuthenticationProperties(
    ServiceLocation.ServicePath,
    ServiceLocation.DecryptedUsername,
    ServiceLocation.DecryptedPassword,
    ServiceLocation.DataSource);
return Application.Connect(authProps);
```

Same Unity API call. Credentials come from different places. The method is isolated, testable, and configurable in the structured version. That's the entire difference -- and it's worth noticing exactly where and why they diverge as you go through the rest of this training set.

---

## Takeaways

- Hardcoded credentials in source control are not acceptable in production. Read credentials from external, encrypted configuration.
- A single method doing everything can't be tested, reused, or reasoned about in isolation.
- `ApplicationException` tells callers nothing useful. Use a specific, recognizable exception type.
- The Unity API calls themselves are correct. The surrounding structure is the problem.
- This exists so the contrast against every subsequent project is concrete, not theoretical.
