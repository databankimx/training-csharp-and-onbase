---
title: "Agent Context: DataBank C# Coding Standards"
purpose: "Pre-instruction document for AI agents working on DataBank C# projects"
source: "Derived from Custom-Dev-Developer-Guide.md and SonarQube-CSharp-Developers-Guide.md"
---

# Agent Context: DataBank C# Coding Standards

Feed this document to an AI agent at the start of any session involving DataBank C# code.
Open with: *"The following describes the coding standards you must follow for all C# code
in this project. Treat them as hard requirements, not suggestions."*

---

## Non-Negotiable Rules (Checker Failures = Build Fails)

These will cause the CI standards-policy check to fail. Do not generate code that violates them.

### Exceptions
- **Never** use `throw new Exception(...)` or `throw new ApplicationException(...)`.
- Use specific, meaningful exception types. For DataBank projects, use the approved packages:
  - `Databank.NetCore.Exceptions` for net6 through net9
  - `Databank.Exceptions` for .NET Framework and everything else
- Always preserve the original exception as `innerException` when adding context.

### Tests
- Use **NUnit**. Always.
- **xUnit and MSTest are forbidden.** Do not reference them, do not generate tests using them.
- Every test project must contain at least one `[Test]`, `[TestCase]`, or `[TestFixture]` attribute.

### Async
- Return `Task` or `Task<T>`. Use `async void` only for event handlers.
- **Never** block with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
- Use `await Task.Delay(delay, token)` instead of `Thread.Sleep`.

### Secrets and Connection Strings
- **Never** hardcode connection strings, passwords, API keys, or any credentials in source code.
- Load from configuration, `IOptions<T>`, or a secret store.

### Cryptography
- **Never** use MD5, SHA-1, DES, 3DES, or RC2 for security purposes.
- Use SHA-256 or stronger for hashing. Use AES (preferably `AesGcm`) for encryption.

### Dependency Policy (REPO-4)
- All third-party packages **must** allow closed-source commercial use.
- MIT, Apache 2.0, BSD, MS-PL: acceptable.
- GPL, LGPL, AGPL: **not acceptable** for closed-source work.
- Unknown license: reject until clarified.

---

## Required Conventions (Standards Checker Warnings)

These generate warnings in CI. Treat them as required unless there is a documented reason to keep them.

### Copyright Header
Every `.cs` file must begin with the DataBank copyright region:

```csharp
#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion
```

### Logging
- Do not use `Console.WriteLine` in service or library code.
- If the project references a third-party logging library, also reference the approved DataBank
  logging package: `Databank.NetCore.Logging` (net6-net9) or `Databank.Logging` (everything else).

### Pragma Suppressions
- Every `#pragma warning disable` must have a trailing comment explaining why.
- Do not use `NOSONAR` comments anywhere in the repository - CI scans for them and fails the build.

### NuGet References
- Use `PackageReference` in `.csproj`. Do not use `<HintPath>` references to local DLLs.

---

## SonarQube Quality Gate

The DataBank SonarQube instance runs in MQR mode. The **Databank Way** quality gate
requires **zero new issues** of any severity on new code. This covers Security, Reliability,
and Maintainability. There is no threshold below which a new issue is acceptable.

Key rules behind the Sonar way C# profile:
- Enable nullable reference types (`<Nullable>enable</Nullable>`). Fix warnings; do not suppress them.
- Validate arguments at public API boundaries (`ArgumentNullException.ThrowIfNull`).
- Dispose `IDisposable` and `IAsyncDisposable` deterministically with `using` or `await using`.
- Use parameterized SQL. Never concatenate input into a query string.
- Pass a `CancellationToken` through all I/O, database, HTTP, and long-running calls.
- Get `HttpClient` from `IHttpClientFactory`. Do not instantiate one per request.
- Never use `BinaryFormatter`.
- Do not use `TypeNameHandling.All` in Newtonsoft.Json for untrusted data.

---

## Merge Requirements

Code is not done until it has cleared all of the following:

1. Committed to DataBank GitHub Enterprise source control.
2. Pull request opened and reviewed by a **team lead or senior developer**.
3. All CI scans passing:
   - Standards policy checker (no FAILs; no unresolved WARNs without documented reason)
   - SonarQube (quality gate: Databank Way - zero new issues)
   - Snyk (no new vulnerabilities introduced)
