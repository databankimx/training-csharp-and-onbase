# Samples.RazorPages

> **Looking for implementation details or porting notes?** See `LectureNotes.md` in this folder.

## What This Is

ASP.NET Core Razor Pages, the *other* server-rendered pattern in ASP.NET Core, distinct enough from MVC (see `Samples.MvcWebPortal.Core`) that it's demonstrated as its own standalone sample rather than folded into that project. No `Controllers` folder exists here at all: each page under `Pages/` is a self-contained unit, its own routing, its own `PageModel` code-behind class, discovered automatically from its file location.

**.NET 10 only, no classic ASP.NET sibling.** True Razor Pages (`PageModel` classes, `OnGet`/`OnPost` handlers, dependency injection) has no genuine equivalent on `net48`. The closest thing classic ASP.NET has is "ASP.NET Web Pages" (WebMatrix-era, `.cshtml` files with inline code, no `PageModel`, no DI), a real but much older and structurally different technology from around 2010, not the same pattern despite both using `.cshtml` and the word "Razor."

---

## When to Use Razor Pages

Microsoft's current default recommendation for new server-rendered ASP.NET Core applications, especially page-focused, CRUD-style UIs where a page and its logic naturally belong together. MVC (`Samples.MvcWebPortal.Core`) remains a reasonable choice when the Controllers + Views separation is already familiar, or when many views genuinely share one controller's logic.

---

## What's Different From `Samples.MvcWebPortal.Core`

- **No `Controllers` folder.** Routing is file-path-based: `Pages/Index.cshtml` is `/`, automatically, no `RouteConfig`/`MapControllerRoute` to configure.
- **One page, two states.** `Pages/Index.cshtml` handles *both* the search form and the results table, the same job `Samples.MvcWebPortal.Core` needed two separate controller+view pairs for (`HomeController` and `LocationLookupController`).
- **`[BindProperty(SupportsGet = true)]`** binds the `ZipCode` query string value directly to a page property, no method parameter needed.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Pages/Index.cshtml` / `.cshtml.cs` | The search form and results, combined |
| `Pages/Shared/_Layout.cshtml` | Shared page chrome |
| `Models/ZipCode.cs` | EF Core entity (Code-First) |
| `Data/LocationLookupContext.cs` | EF Core `DbContext` |

---

## How to Run

1. Point `appsettings.json`'s `LocationLookupDatabase` connection string at a real SQL Server instance.
2. Press F5 (or `dotnet run`).
3. Enter a ZIP code and click Search, the results appear on the same page.

---

## Related Samples

- **`Samples.MvcWebPortal.Core`** — ASP.NET Core's other server-rendered pattern (Controllers + Views), worth comparing directly.
- **`Samples.MvcWebPortal`** — the classic ASP.NET MVC 5 sibling.
