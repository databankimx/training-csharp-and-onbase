# Samples.MvcWebApi.Common

## What This Is

Shared request/response DTOs for the `Samples.MvcWebApi` group (server + `.Client` + `.WebClient`). See `README.md` for the fuller "why this project exists" discussion.

---

## Porting Notes

SDK-style class library. No bugs found in the original. No `DatabankException` usage, these are pure data-holding classes with nothing to throw from.

One thing worth preserving exactly as found: `LocationRequest.cs`'s original namespace declaration had unusual leading whitespace before `namespace Samples.MvcWebApi.Common` (four spaces of indentation before the keyword, inconsistent with every other file in the same project). Harmless, compiles identically either way, but normalized here to match the rest of the codebase's formatting, since there's no reason to faithfully preserve a whitespace quirk that carries no meaning.
