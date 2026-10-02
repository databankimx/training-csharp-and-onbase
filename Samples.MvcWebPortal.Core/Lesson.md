# Samples.MvcWebPortal.Core

## What This Is

The ASP.NET Core MVC sibling of `Samples.MvcWebPortal`: server-rendered HTML pages, Controllers + Views, structurally the same shape as the classic project, running on .NET 10. Kept as MVC (rather than Razor Pages) specifically to give a direct, apples-to-apples comparison. Razor Pages is demonstrated separately in `Samples.RazorPages`.

---

## When to Use ASP.NET Core MVC

For server-rendered web applications where the Controllers + Views separation is familiar and wanted. For genuinely new development with no such constraint, Razor Pages is Microsoft's current default recommendation for server-rendered ASP.NET Core apps -- less ceremony for page-focused, CRUD-style UIs.

---

## Key Differences From Classic MVC 5

| Classic (`Samples.MvcWebPortal`) | ASP.NET Core (`Samples.MvcWebPortal.Core`) |
|---|---|
| EF6 Database-First (`.edmx`) | EF Core Code-First (`ZipCode.cs`) |
| `new ExternalDataEntities()` directly | DI-injected `LocationLookupContext` |
| Synchronous `.ToList()` | `await db.ZipCodes.ToListAsync()` |
| Hand-written JavaScript for form submission | Plain HTML `<form method="get">`, model binding handles the rest |
| `net48`, Windows/IIS only | `net10.0`, cross-platform |
| DbContext never disposed (real bug in original) | DI container disposes it automatically |

The "never disposed" bug from the classic version is fixed here automatically -- the DI container manages the `DbContext` lifetime and disposes it at the end of each request.

---

## How It Works

```csharp
// Program.cs
builder.Services.AddDbContext<LocationLookupContext>(options =>
    options.UseSqlServer(connectionString));

// Controllers/LocationLookupController.cs
public class LocationLookupController(LocationLookupContext db) : Controller
{
    public async Task<IActionResult> Index(string zipCode)
    {
        var results = await db.ZipCodes
            .Where(z => z.ZipCode1 == zipCode)
            .ToListAsync();
        return View(results);
    }
}
```

The form in `Views/Home/Index.cshtml` is a plain HTML GET form -- no JavaScript:

```html
<form method="get" asp-controller="LocationLookup" asp-action="Index">
    <input name="zipCode" />
    <button type="submit">Search</button>
</form>
```

---

## Creating an ASP.NET Core MVC Project

### Visual Studio

**File > New > Project**, search "ASP.NET Core Web App (Model-View-Controller)", click Next, choose .NET version, click Create. The scaffolding creates `Controllers/`, `Views/`, `Models/`, and `Program.cs`.

To add EF Core: right-click the project > Manage NuGet Packages, install `Microsoft.EntityFrameworkCore.SqlServer` and `Microsoft.EntityFrameworkCore.Tools`. Then scaffold or hand-write the `DbContext` and entity, register in `Program.cs`.

### VS Code

```powershell
dotnet new mvc -n MyMvcApp -f net10.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet run
```

---

## Running This Project

1. Point `appsettings.json`'s `LocationLookupDatabase` at a SQL Server instance.
2. Press F5 or `dotnet run`.

---

## Related Projects

- `Samples.MvcWebPortal` -- the classic ASP.NET MVC 5 sibling for direct comparison.
- `Samples.RazorPages` -- ASP.NET Core's other server-rendered pattern.
