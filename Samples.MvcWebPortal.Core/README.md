# Samples.MvcWebPortal.Core

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

The ASP.NET Core MVC sibling of `Samples.MvcWebPortal`: server-rendered HTML pages, Controllers + Views, structurally the same shape as the classic project, running on **.NET 10**. Kept as MVC rather than Razor Pages (ASP.NET Core's other server-rendered pattern) specifically to give a direct, apples-to-apples comparison against the classic project. Razor Pages is demonstrated separately in its own standalone sample, `Samples.RazorPages`.

---

## When to Use ASP.NET Core MVC

For server-rendered web applications where the Controllers + Views separation is already familiar (from classic ASP.NET MVC, or from other MVC-pattern frameworks) and you want to keep that structure. For genuinely new development with no such constraint, Razor Pages is Microsoft's current default recommendation, page-focused, less ceremony for simple CRUD-style pages, see `Samples.RazorPages`.

---

## What's Different From `Samples.MvcWebPortal`

- **EF Core Code-First**, not EF6 Database-First.
- **Dependency-injected `DbContext`**, not `new ExternalDataEntities()` constructed directly, no manual disposal needed anywhere (contrast against the real bug fixed in the classic project, a `DbContext` that was never disposed at all).
- **Genuinely async EF Core query** (`ToListAsync()`), not a blocking `.ToList()` call.
- **No hand-written JavaScript for the search form.** A plain HTML GET form does the whole job, ASP.NET Core's model binding matches the `zipCode` query string value to the action parameter automatically.
- **No `DatabankException`.** `CSharp.SharedLibrary` targets `net48`, incompatible with this project's `net10.0` target.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Controllers/HomeController.cs` | The search page |
| `Controllers/LocationLookupController.cs` | Runs the EF Core query, renders the results table |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |
| `Views/Home/Index.cshtml` | The ZIP code search form (plain HTML `<form>`, no JS) |
| `Views/LocationLookup/Index.cshtml` | The results table |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Press F5 (or `dotnet run`).
3. Enter a ZIP code and click Search.

---

## Related Samples

- **`Samples.MvcWebPortal`** — the classic ASP.NET MVC 5 sibling of this project, worth comparing directly.
- **`Samples.RazorPages`** — ASP.NET Core's other server-rendered pattern, genuinely different enough (no controllers, `PageModel` classes instead) to warrant its own standalone sample rather than folding it in here.
