# Samples.RazorPages

## What This Is

A standalone demonstration of ASP.NET Core Razor Pages, built after an explicit decision that true Razor Pages has no genuine `net48` equivalent (the closest analog, "ASP.NET Web Pages," is a real but structurally different, much older technology), so this project exists on its own, net10.0 only, rather than as a paired sibling the way `Samples.MvcWebPortal`/`Samples.MvcWebPortal.Core` are. See `README.md` for the fuller when-to-use discussion.

---

## No `Controllers` Folder, Anywhere

Every other web sample in this training set (classic and Core alike) has a `Controllers` folder. This project doesn't, and can't meaningfully have one, Razor Pages routes by file location under `Pages/` directly (`Pages/Index.cshtml` → `/`), with each page's own `PageModel` code-behind class handling that page's logic. There's no `RouteConfig.cs`/`MapControllerRoute` call anywhere in this project either, `app.MapRazorPages()` in `Program.cs` is the entire routing configuration needed.

---

## One Page, Two States: A Genuine Structural Simplification

`Samples.MvcWebPortal`/`Samples.MvcWebPortal.Core` each needed two separate controller+view pairs to do this project's whole job: `HomeController`/`Views/Home/Index.cshtml` for the search form, `LocationLookupController`/`Views/LocationLookup/Index.cshtml` for the results. Here, `Pages/Index.cshtml.cs`'s `OnGetAsync()` simply checks whether `ZipCode` was bound:

```csharp
public async Task OnGetAsync()
{
    if (string.IsNullOrWhiteSpace(ZipCode)) return;

    HasSearched = true;
    Locations = await db.ZipCodes.Where(z => z.ZipCode1 == ZipCode).ToListAsync();
}
```

and the page itself conditionally renders the results table based on `Model.HasSearched`. One route, one file pair, both states. Worth noting this isn't a Razor Pages *requirement*, splitting into separate pages remains entirely possible and often reasonable for more complex scenarios, but the framework doesn't force the split the way MVC's controller-per-concern convention tends to.

---

## `[BindProperty(SupportsGet = true)]`

```csharp
[BindProperty(SupportsGet = true)]
public string? ZipCode { get; set; }
```

This is the idiomatic Razor Pages way to accept input from a `GET` request's query string directly onto a page property, no `OnGet(string zipCode)` parameter list needed the way `Samples.MvcWebPortal.Core`'s `LocationLookupController.Index(string zipCode)` required. `SupportsGet = true` is necessary specifically because `[BindProperty]` alone only binds on `POST` by default, a deliberate safety default (to avoid accidentally binding arbitrary query string values on every `GET` request across an application) that has to be explicitly opted into per-property.

---

## Try It Yourself

Run the project, search for a ZIP code, and watch the same page (same URL, `/?ZipCode=...`) render both the form and the results together. Compare `Pages/Index.cshtml.cs` directly against `Samples.MvcWebPortal.Core`'s two controllers, same underlying query, genuinely less ceremony here.
