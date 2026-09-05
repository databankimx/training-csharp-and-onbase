# Unity.SimpleButBadExample

> **Looking for the full anti-pattern breakdown?** See `LectureNotes.md` in this folder.

## What This Is

The first project in the OnBase Unity API training set, and the only one that's deliberately **bad**. It performs a complete, working Unity API walkthrough, connect, query, retrieve, update, upload, delete, disconnect, all correct at a functional level. But it's written the way a first attempt often is: one giant `Main()` method, hardcoded credentials in source, and no separation of concerns anywhere.

Every project after this one (`Unity.00.CommonFunctionality` onward) does the same kind of work correctly. Keep this one in mind as you go, it's the "before" picture.

---

## What's in This Project

A single `Program.cs`. That's the point, everything (connection, querying, document retrieval, keyword updates, uploads, deletes) lives in one class with no structure separating those concerns.

---

## How to Run

1. Update the constants at the top of `Program.cs` (`AppServer`, `DataSource`, `UserName`, `Password`, `DocTypeName`, etc.) to match a real OnBase test environment.
2. Press F5 (or `dotnet run`). The program pauses between each step so you can follow along.

---

## Related Samples

- **`Unity.00.CommonFunctionality`** / **`Unity.01.ConnectingToOnBase`** — the correct way to structure connection management, configuration, and error handling for the same underlying task.

---

## `Hyland.Unity` Package

This project references `Hyland.Unity` 26.1.2 via the `DataBank GitHub` NuGet feed. That package bundles:

- `Hyland.Unity.dll`
- `Hyland.Types.dll`
- `Hyland.Applications.Web.Security.dll`
- `Security.Cryptography.dll`

These are Hyland's own licensed OnBase Unity API binaries, not DataBank's own code. DataBank mirrors them on its internal feed for convenience, but the underlying software is licensed per OnBase deployment: **your own copies should come from your own OnBase Application Server installation** (typically found under that server's Unity API redistribution folder), not assumed to be freely available just because a NuGet package happens to exist for them.
