---
title: "Context Document Example: OnBase API"
chapter: 0
index: 5
dependencies: []
---

```csharp
using System;

internal static class Program
{
    // This step shows a representative excerpt from the DataBank OnBase API
    // context document - the kind of markdown file you would paste into an
    // agent session before asking it to write any Unity API code.
    //
    // The content is drawn from real DataBank reference files. Notice:
    //   - The entry point is named explicitly (Application.Connect, not "new Application").
    //   - Authentication modes are enumerated with the actual method names.
    //   - Session lifecycle rules (keepAlive, IsDisconnectEnabled) are stated clearly.
    //   - Threading and load-balancing constraints are called out as hard rules.
    //   - "What to avoid" names concrete mistakes seen in agent output and real bugs.
    //
    // Without this context, an agent will invent method names, omit disposal,
    // ignore the threading constraint, and produce code that compiles but fails at runtime.

    private static void Main()
    {
        var excerpt = """
            # OnBase Unity API - Agent Context Document

            ## Purpose
            The Hyland Unity API (Hyland.Unity) is the primary SDK for integrating .NET
            applications with OnBase. Use it for document retrieval, archiving, keyword
            access, workflow, and taxonomy queries. This document is authoritative. Do not
            invent types or method names not listed here. Ask if you need a detail not covered.

            ## Architecture
            Client-server over HTTP (SOAP or binary). The client holds a session-backed
            Application object; each request travels to the OnBase Application Server.

            - Not thread-safe. One Application object per thread. Each thread consumes a
              separate client license.
            - Load-balanced environments require sticky sessions. Do not disable the load
              balancer during troubleshooting without understanding the session impact.

            ## Entry Point
            Always connect through the static factory - never instantiate Application directly.

            ### NT Authentication (preferred for server-to-server)
            ```csharp
            var authProps = Application.CreateDomainAuthenticationProperties(
                serviceLocation.ServicePath,
                serviceLocation.DataSource);
            ```

            ### OnBase username/password
            ```csharp
            var authProps = Application.CreateOnBaseAuthenticationProperties(
                serviceLocation.ServicePath,
                serviceLocation.DecryptedUsername,
                serviceLocation.DecryptedPassword,
                serviceLocation.DataSource);
            ```

            ### Reconnect to an existing session
            ```csharp
            var authProps = Application.CreateSessionIDAuthenticationProperties(
                serviceLocation.ServicePath,
                sessionId,
                isDisconnectEnabled: !keepAlive);
            ```

            After building authProps, set the license type and connect:
            ```csharp
            authProps.LicenseType = serviceLocation.LicenseType;
            authProps.IsDisconnectEnabled = !serviceLocation.KeepAlive;

            // Optional: set Integration GUID identity
            if (!string.IsNullOrEmpty(serviceLocation.ApplicationId))
                authProps.IdentitySettings = authProps.CreateIdentitySettings(serviceLocation.ApplicationId);

            var app = Application.Connect(authProps);
            ```

            ## Session Lifecycle
            - `IsDisconnectEnabled = true`  - session ends when Disconnect() is called.
              Use for short-lived integrations.
            - `IsDisconnectEnabled = false` (KeepAlive) - session persists on the server
              after Disconnect(). Required for reconnecting with a stored session ID.
            - To disconnect a KeepAlive session, reconnect with `isDisconnectEnabled: true`
              first, then call Disconnect().
            - Application implements IDisposable. Always dispose it or call Disconnect().

            ## Key Types
            | Type                | How to obtain                                      |
            |---------------------|----------------------------------------------------|
            | Application         | Application.Connect(authProps) - root object       |
            | Core                | app.Core                                           |
            | DocumentQuery       | app.Core.CreateDocumentQuery()                     |
            | Document            | result of app.Core.GetDocumentList(query)          |
            | DocumentType        | app.Core.DocumentTypes[id]                         |
            | KeywordType         | app.Core.KeywordTypes[id]                          |
            | KeywordRecord       | document.KeywordRecords (collection)               |

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

            ## What to Avoid
            - Do not use `new Application(...)` - the constructor is not the entry point.
            - Do not share an Application instance across threads - the API is not thread-safe.
            - Do not forget disposal - undisposed connections exhaust the license pool.
            - Do not access `document.KeywordRecords[0]` without checking Count > 0 first.
            - Do not hold Application open across requests in a web context.
            - Do not catch the base Exception class - catch specific Hyland exceptions and
              wrap in a DataBank exception type with the original as innerException.
            - Do not use throw new Exception(...) - use approved DataBank exception types.

            ## Cancellation
            The Unity API is synchronous. Wrap in Task.Run for async contexts.
            Check CancellationToken.ThrowIfCancellationRequested() before and after
            long-running calls.
            """;

        Console.WriteLine(excerpt);
        Console.WriteLine();
        Console.WriteLine("--- How this changes agent output ---");
        Console.WriteLine();

        var before = new[]
        {
            "var app = new Application();                    // invented - does not exist",
            "app.Open(url, user, password);                  // invented - does not exist",
            "var docs = app.SearchDocuments(typeId);         // invented - does not exist",
            "// no disposal, no exception handling",
        };

        var after = new[]
        {
            "var authProps = Application.CreateDomainAuthenticationProperties(path, ds);",
            "authProps.LicenseType = LicenseType.Default;",
            "using var app = Application.Connect(authProps);",
            "var query = app.Core.CreateDocumentQuery();",
            "query.AddDocumentType(app.Core.DocumentTypes[docTypeId]);",
            "var results = app.Core.GetDocumentList(query);",
        };

        Console.WriteLine("Without the context document, an agent typically produces:");
        foreach (var line in before)
            Console.WriteLine($"  {line}");

        Console.WriteLine();
        Console.WriteLine("With the context document, the same agent produces:");
        foreach (var line in after)
            Console.WriteLine($"  {line}");

        Console.WriteLine();
        Console.WriteLine("The context document eliminates the most common failure modes");
        Console.WriteLine("before any code is written. That is the point.");
    }
}
```
