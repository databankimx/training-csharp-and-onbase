# Samples.MvcWebApi.Core.Common

## What This Is

Originally defined directly inside `Samples.MvcWebApi.Core`'s own `Models/Dtos.cs`. Extracted into this separate project once `Samples.MvcWebApi.Core.Client` needed the exact same shapes, exactly the same rationale (a second consumer existing) that justified `Samples.MvcWebApi.Common`'s own existence for the classic API group.

---

## `record` Types, Not Classes

`Samples.MvcWebApi.Common` uses plain mutable classes with `{ get; set; }` properties. This project uses C# `record` types instead, immutable by default, structural (value) equality, and a concise positional-parameter declaration syntax. Both approaches work fine for a simple DTO, `record` is the more idiomatic modern C# choice for this exact kind of "just a data shape" type, worth showing directly alongside the classic pattern rather than only describing the difference.
