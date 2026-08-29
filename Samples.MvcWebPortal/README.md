# Samples.MvcWebPortal

> **Looking for implementation details, bugs found, or migration notes?** See `LectureNotes.md` in this folder.

## What This Is

A plain ASP.NET MVC 5 Razor web application: server-rendered HTML pages, no API, no JSON, the classic "web portal" pattern. A home page takes a ZIP code and searches for matching locations via EF6 (Database-First) against the same `ZipCodes` table `Samples.MvcWebApi` uses, rendering the results as an HTML table.

---

## When to Use ASP.NET MVC 5

Only for existing classic ASP.NET applications, or genuine constraints ruling out ASP.NET Core. For new server-rendered web applications, ASP.NET Core MVC (or Razor Pages, or a full client-side framework backed by a Web API) is the modern default.

---

## Pros

- **Simple, well-understood pattern.** Controller returns a model, Razor view renders HTML, no extra moving parts.
- **No JSON/serialization concerns at all.** Everything is server-rendered HTML, there's no API contract to keep in sync with a client.
- **Native Entity Framework integration**, same as `Samples.MvcWebApi`.

## Cons

- **No client-side interactivity story beyond plain jQuery.** Every page navigation is a full server round-trip; there's no SPA-style partial updates without hand-rolling AJAX yourself.
- **Superseded by ASP.NET Core MVC/Razor Pages.** No further platform investment.
- **Tied to classic .NET Framework**, `System.Web`, IIS, no cross-platform story.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Controllers/HomeController.cs` | The search page |
| `Controllers/LocationLookupController.cs` | Runs the EF6 query, renders the results table |
| `LocationLookupModel.edmx` (+ generated files) | EF6 Database-First model, one entity (`ZipCode`) |
| `Views/Home/Index.cshtml` | The ZIP code search form |
| `Views/LocationLookup/Index.cshtml` | The results table |

---

## How to Run

1. Point `Web.config`'s `ExternalDataEntities` connection string at a real SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Enter a ZIP code and click Search.

---

## Related Samples

- **`Samples.MvcWebApi`** — the JSON API sibling of this project's own EF6 query, worth comparing directly: same table, same query, one returns JSON, the other renders HTML.
