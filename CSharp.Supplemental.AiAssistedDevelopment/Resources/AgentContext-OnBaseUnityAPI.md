---
title: "Agent Context: Hyland Unity API (OnBase)"
purpose: "Pre-instruction document for AI agents writing OnBase Unity API code at DataBank"
source: "Derived from DBIMX-Session-Management.md, Architecture.md, API-Basics.md"
---

# Agent Context: Hyland Unity API (OnBase)

Feed this document to an AI agent at the start of any session involving OnBase Unity API code.
Open with: *"The following describes the Hyland Unity API as used at DataBank. Treat it as
authoritative. Do not invent types or method names not listed here. Ask me if you need a
detail that is not covered."*

---

## What This API Is

The Hyland Unity API (`Hyland.Unity`) is the primary SDK for integrating .NET applications
with OnBase. Use it for document retrieval, archiving, keyword access, workflow, and taxonomy
queries. The API is client-server over HTTP (SOAP or binary format). The client holds a
session-backed `Application` object; every request travels to the OnBase Application Server.

---

## Hard Constraints

- **Not thread-safe.** One `Application` object per thread. Each thread consumes a separate
  client license. Do not share an Application instance across threads under any circumstances.
- **Load balancing requires sticky sessions.** The session is tied to a specific server.
  Do not disable the load balancer during troubleshooting without understanding the session impact.
- **Always dispose.** `Application` implements `IDisposable`. Undisposed connections exhaust
  the license pool. Always use `using` or call `Disconnect()` explicitly.

---

## Entry Point

**Never instantiate `Application` directly.** Always use the static factory:

### NT Authentication (preferred for server-to-server integrations)
```csharp
var authProps = Application.CreateDomainAuthenticationProperties(
    serviceLocation.ServicePath,
    serviceLocation.DataSource);
```

### OnBase username / password
```csharp
var authProps = Application.CreateOnBaseAuthenticationProperties(
    serviceLocation.ServicePath,
    serviceLocation.DecryptedUsername,
    serviceLocation.DecryptedPassword,
    serviceLocation.DataSource);
```

### Reconnect to an existing session (KeepAlive pattern)
```csharp
var authProps = Application.CreateSessionIDAuthenticationProperties(
    serviceLocation.ServicePath,
    sessionId,
    isDisconnectEnabled: !keepAlive);
```

### After building authProps, connect:
```csharp
authProps.LicenseType      = serviceLocation.LicenseType;
authProps.IsDisconnectEnabled = !serviceLocation.KeepAlive;

// Set Integration GUID identity if configured
if (!string.IsNullOrEmpty(serviceLocation.ApplicationId))
    authProps.IdentitySettings = authProps.CreateIdentitySettings(serviceLocation.ApplicationId);

var app = Application.Connect(authProps);
```

---

## Session Lifecycle

| Setting | Behaviour |
|---|---|
| `IsDisconnectEnabled = true` | Session ends when `Disconnect()` is called. Use for short-lived integrations. |
| `IsDisconnectEnabled = false` (KeepAlive) | Session persists on the server after `Disconnect()`. Required for reconnecting with a stored session ID. |

To disconnect a KeepAlive session, reconnect with `isDisconnectEnabled: true` first, then call `Disconnect()`.

If `AllowSessionFailover` is configured and reconnection fails, fall back to a new session via `ConnectNewSession()`.

---

## Key Types

| Type | How to obtain |
|---|---|
| `Application` | `Application.Connect(authProps)` - the root object for everything |
| `Core` | `app.Core` |
| `DocumentQuery` | `app.Core.CreateDocumentQuery()` |
| `Document` | Result of `app.Core.GetDocumentList(query)` |
| `DocumentType` | `app.Core.DocumentTypes[id]` |
| `KeywordType` | `app.Core.KeywordTypes[id]` |
| `KeywordRecord` | `document.KeywordRecords` (collection - check Count before indexing) |

---

## Common Patterns

### Document retrieval
```csharp
var query = app.Core.CreateDocumentQuery();
query.AddDocumentType(app.Core.DocumentTypes[docTypeId]);
query.AddKeyword(keywordType, value);
var results = app.Core.GetDocumentList(query);
foreach (var doc in results)
    Console.WriteLine(doc.ID);
```

### Keyword access
```csharp
foreach (var record in document.KeywordRecords)
    foreach (var kw in record.Keywords)
        Console.WriteLine($"{kw.KeywordType.Name}: {kw.Value}");
```

### Full connect / work / disconnect pattern
```csharp
try
{
    using var app = Application.Connect(authProps);
    var query = app.Core.CreateDocumentQuery();
    query.AddDocumentType(app.Core.DocumentTypes[docTypeId]);
    var results = app.Core.GetDocumentList(query);
    // process results
}
catch (SessionNotFoundException ex)
{
    throw new DatabankException("OnBase session not found - may have expired.", ex);
}
catch (UnityAPIException ex)
{
    throw new DatabankException("OnBase API error during document retrieval.", ex);
}
```

---

## Exception Handling

- Do **not** catch the base `Exception` class.
- Catch specific Hyland exception types (`SessionNotFoundException`, `UnityAPIException`, etc.).
- Wrap in an approved DataBank exception type with the original as `innerException`.
- Do **not** use `throw new Exception(...)` - this violates the DataBank standards policy (CS-3).

---

## Cancellation

The Unity API is synchronous. For async contexts, wrap calls in `Task.Run`. Check
`cancellationToken.ThrowIfCancellationRequested()` before and after long-running calls.

---

## What to Avoid

- `new Application(...)` - the constructor is not the entry point. It does not exist in this form.
- Sharing an `Application` instance across threads.
- Forgetting to dispose - license pool exhaustion is a real production problem.
- `document.KeywordRecords[0]` without checking `Count > 0` first.
- Holding `Application` open across requests in a web context.
- `throw new Exception(...)` - use DataBank exception types.
- Inventing method names that are not in this document - ask instead.
