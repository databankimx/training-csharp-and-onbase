# Samples.RazorPages

## What This Is

ASP.NET Core Razor Pages -- the other server-rendered pattern in ASP.NET Core, distinct enough from MVC that it warrants its own standalone sample rather than being folded into `Samples.MvcWebPortal.Core`. There's no `Controllers` folder here at all. Each page under `Pages/` is a self-contained unit with its own routing, its own `PageModel` code-behind class, and its own `OnGet`/`OnPost` handlers, all discovered automatically from the file's location.

Same ZIP code lookup as every other Samples project. One page (`Pages/Index.cshtml`) handles both the search form and the results -- the same job `Samples.MvcWebPortal.Core` split across two controller/view pairs.

**.NET 10 only.** True Razor Pages (with `PageModel` classes and dependency injection) has no genuine equivalent on `net48`. The closest classic ASP.NET has is ASP.NET Web Pages (WebMatrix-era, no `PageModel`, no DI, structurally different), not the same pattern.

---

## When to Use Razor Pages

Microsoft's current default recommendation for new server-rendered ASP.NET Core applications, especially page-focused, CRUD-style UIs where a page and its logic naturally belong together. MVC (`Samples.MvcWebPortal.Core`) remains a reasonable choice when the Controllers + Views separation is already familiar, or when multiple views genuinely share one controller's logic in a meaningful way.

---

## How Razor Pages Works

Routing is file-path-based. `Pages/Index.cshtml` maps to `/` automatically -- no `RouteConfig`, no `MapControllerRoute`. The `PageModel` class handles requests and exposes properties the view binds to:

```csharp
// Pages/Index.cshtml.cs
public class IndexModel(LocationLookupContext db) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string ZipCode { get; set; }

    public List<ZipCode> Results { get; set; } = [];

    public async Task OnGetAsync()
    {
        if (!string.IsNullOrWhiteSpace(ZipCode))
        {
            Results = await db.ZipCodes
                .Where(z => z.ZipCode1 == ZipCode)
                .ToListAsync();
        }
    }
}
```

`[BindProperty(SupportsGet = true)]` binds the `zipCode` query string value directly to the property -- no method parameter needed. The view accesses `Model.Results` directly:

```html
@page
@model IndexModel
<form method="get">
    <input asp-for="ZipCode" />
    <button type="submit">Search</button>
</form>
@foreach (var row in Model.Results) { ... }
```

`@page` at the top of the `.cshtml` is what makes it a Razor Page rather than an MVC view.

---

## Creating a Razor Pages Project

### Visual Studio

**File > New > Project**, search "ASP.NET Core Web App", select the one labeled "Razor Pages" (not "Model-View-Controller"), click Next, choose .NET version, click Create. The scaffolding creates `Pages/`, `Pages/Shared/_Layout.cshtml`, and `Program.cs` with `AddRazorPages()` and `MapRazorPages()` already wired.

To add a new page: right-click `Pages/` > **Add > Razor Page**. Choose "Razor Page - Empty" for a blank `PageModel`, or a scaffolded CRUD page if you have a DbContext set up.

To add EF Core: Manage NuGet Packages > install `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Tools`. Register the `DbContext` in `Program.cs`:

```csharp
builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("LocationLookupDatabase")));
```

### VS Code

```powershell
dotnet new razor -n MyRazorApp -f net10.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet run
```

Add pages under `Pages/`. Each page is a `.cshtml` file paired with a `.cshtml.cs` `PageModel` class.

---

## MVC vs. Razor Pages: The Practical Difference

| MVC (`Samples.MvcWebPortal.Core`) | Razor Pages (`Samples.RazorPages`) |
|---|---|
| `Controllers/` + `Views/` folders | `Pages/` folder only |
| Route → Controller → Action → View | Route → Page (file path) → `OnGet`/`OnPost` |
| `return View(model)` | Properties on `PageModel`, bound directly |
| Good when one controller serves many views | Good when page and its logic belong together |
| Familiar from classic ASP.NET MVC | Microsoft's current recommendation for new work |

---

## Running This Project

1. Point `appsettings.json`'s `LocationLookupDatabase` at a SQL Server instance.
2. Press F5 or `dotnet run`.
3. Enter a ZIP code and click Search. Results appear on the same page without a URL change.

---

## Related Projects

- `Samples.MvcWebPortal.Core` -- ASP.NET Core MVC, Controllers + Views, for direct comparison.
- `Samples.MvcWebPortal` -- classic ASP.NET MVC 5.
- `Samples.WebForms` -- the oldest server-side rendering model, postback-based.
