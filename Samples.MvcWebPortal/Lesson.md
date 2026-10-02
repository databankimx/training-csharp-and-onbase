# Samples.MvcWebPortal

## What This Is

A classic ASP.NET MVC 5 Razor web application: server-rendered HTML pages, no API, no JSON. A home page takes a ZIP code and POSTs it to a controller action that runs an EF6 query against the same `ZipCodes` table, then renders the results as an HTML table in the response. The browser sees only page reloads, not API calls.

---

## When to Use ASP.NET MVC 5

Only for existing classic ASP.NET applications, or genuine constraints ruling out ASP.NET Core. For new server-rendered web applications, ASP.NET Core MVC or Razor Pages is the modern default.

---

## How ASP.NET MVC 5 Works

The framework maps URL routes to controller action methods. Each action either renders a Razor `.cshtml` view or redirects:

```csharp
// Controllers/LocationLookupController.cs
public ActionResult Index(string zipCode)
{
    using var db = new ExternalDataEntities();
    var results = db.ZipCodes
        .Where(z => z.ZipCode == zipCode)
        .ToList();
    return View(results);
}
```

The view (`Views/LocationLookup/Index.cshtml`) receives the model and renders HTML:

```html
@model IEnumerable<ZipCode>
<table>
@foreach (var row in Model) {
    <tr><td>@row.City</td><td>@row.State</td></tr>
}
</table>
```

No JSON, no JavaScript required. Every interaction is a full server round-trip.

---

## Creating an ASP.NET MVC 5 Project

### Visual Studio

**File > New > Project**, search "ASP.NET Web Application (.NET Framework)", click Next, select "MVC" template, click Create. The scaffolding creates `Controllers/`, `Views/`, `Models/`, `App_Start/RouteConfig.cs`, and a starting `HomeController`.

To add a new controller: right-click `Controllers/` > Add > Controller > "MVC 5 Controller with views, using Entity Framework" for a scaffolded CRUD controller, or "MVC 5 Controller - Empty" for a blank one.

### VS Code

ASP.NET MVC 5 requires the classic `System.Web` hosting model. Create the project in Visual Studio. VS Code can edit the files but the project scaffolding must be created in Visual Studio first.

---

## Running This Project

1. Point `Web.config`'s `ExternalDataEntities` connection string at a SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Enter a ZIP code and click Search.

---

## Pros and Cons

**Pros:** Simple, well-understood pattern. No API contract to maintain. Native EF integration.

**Cons:** Every interaction requires a full server round-trip. No client-side interactivity beyond jQuery. Superseded by ASP.NET Core. Tied to Windows/IIS.

---

## Related Projects

- `Samples.MvcWebPortal.Core` -- the ASP.NET Core MVC sibling.
- `Samples.MvcWebApi` -- the JSON API alternative to server-rendered HTML for the same lookup.
- `Samples.WebForms` -- a very different server-side rendering model using postback.
