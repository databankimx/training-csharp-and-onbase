# Samples.MvcWebPortal

## What This Is

A plain, server-rendered ASP.NET MVC 5 Razor web application, no logging, no filters beyond the stock `HandleErrorAttribute`, no custom config, the leanest of the classic web samples in this training set. See `README.md` for the fuller technology overview.

---

## No Serilog, No `DatabankException`

Unlike `Samples.AsmxWebService`, `Samples.WcfService`, and `Samples.MvcWebApi`, this project's original had **no logging or error-handling infrastructure at all**, no log4net, no custom filters, no `ApplicationException` usages anywhere in the code. There was nothing to convert, so nothing was added here that wasn't already present, keeping this project's scope proportionate to what it actually was. Worth contrasting directly against its siblings: not every classic ASP.NET project needs the same amount of infrastructure.

---

## A Real Bug Fixed: `DbContext` Never Disposed

```csharp
public class LocationLookupController : Controller
{
    private readonly ExternalDataEntities db = new ExternalDataEntities();

    public ActionResult Index(string zipCode)
    {
        var results = db.ZipCodes.Where(x => string.Equals(x.ZipCode1, zipCode)).ToList();
        return View(results);
    }
}
```

`db` is a `DbContext`, itself wrapping a real `SqlConnection`, created once per controller instance and never disposed anywhere in the original code. `Controller.Dispose()` (called automatically by the MVC framework at the end of each request) only disposes things it explicitly knows about, it has no idea a derived controller added a private `DbContext` field, so this connection was left for the garbage collector to eventually finalize rather than being released promptly after each request, a genuine resource leak under any real load. **Fixed** by overriding `Dispose(bool disposing)` to explicitly dispose `db`:

```csharp
protected override void Dispose(bool disposing)
{
    if (disposing) db.Dispose();
    base.Dispose(disposing);
}
```

Worth internalizing as a general pattern: any field a controller (or any class) constructs that implements `IDisposable` needs an explicit disposal path, the base class's own `Dispose()` never reaches into a derived class's own fields automatically.

---

## A Real Bug Fixed: Duplicate jQuery Load

```html
<!-- Views/Home/Index.cshtml -->
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
```

`_Layout.cshtml` already provides jQuery to every page via its own bundle/CDN reference. `Views/Home/Index.cshtml` additionally loaded a **second, different-version** copy of jQuery (`3.6.0`, versus whatever the layout's own bundle provided) directly from CDN, genuinely wasteful (downloading and parsing an entire second copy of the library) and a real risk: with two different jQuery versions loaded on the same page, which one `$` actually refers to depends on script execution order, a subtle, hard-to-debug source of behavior differences between pages. **Fixed** by removing the redundant `<script>` tag entirely, this page already has jQuery available from the layout.

---

## Bootstrap 3 → 5, Same Treatment as `Samples.MvcWebApi`

`.jumbotron` removed entirely, `navbar-toggle`/`data-toggle` renamed to their Bootstrap 5 equivalents, jQuery and Bootstrap now loaded from CDN instead of vendored NuGet-package files. See `Samples.MvcWebApi`'s own `LectureNotes.md` for the fuller explanation, the exact same migration applies here.

---

## Try It Yourself

Run the project, enter a ZIP code on the home page, and confirm the results table renders correctly, then open the browser's Network tab and confirm jQuery loads exactly once.
