# Samples.MvcWebPortal.Core

## What This Is

The ASP.NET Core MVC sibling of `Samples.MvcWebPortal`, targeting **.NET 10**. See `README.md` for the fuller technology overview and the decision to keep this project MVC-shaped (Controllers + Views) rather than Razor Pages.

---

## No `DatabankException`, Same Reasoning as `Samples.MvcWebApi.Core`

`CSharp.SharedLibrary` targets `net48`, incompatible with a `net10.0` project (one-directional compatibility, a `net48` library can't be referenced by modern .NET at all). Standard exceptions are used directly, no custom exception type, same as `Samples.MvcWebApi.Core`, and consistent with `Samples.MvcWebPortal`'s own original scope, that project never had `ApplicationException` usage to convert in the first place.

---

## `DbContext` Disposal: A Non-Issue Here

`Samples.MvcWebPortal`'s `LocationLookupController` constructed its `ExternalDataEntities` `DbContext` directly (`new ExternalDataEntities()`) and never disposed it anywhere, a genuine resource leak fixed in that project's own migration (see its `LectureNotes.md`). Here, `LocationLookupContext` is registered once in `Program.cs` via `builder.Services.AddDbContext<LocationLookupContext>(...)` and injected into the controller's primary constructor. The DI container owns its entire lifetime, including disposal, there's no manual disposal code to write or forget, the same class of bug simply isn't possible in this pattern.

---

## No JavaScript Needed for the Search Form

`Samples.MvcWebPortal`'s home page needed a hand-written jQuery click handler specifically to build a URL string (`Url.Action(...) + "/Index/" + zipCode`) and navigate to it. Here:

```html
<form method="get" asp-controller="LocationLookup" asp-action="Index">
    <input type="text" name="zipCode" value="75067" />
    <button type="submit">Search</button>
</form>
```

A plain HTML GET form submits as a query string (`?zipCode=75067`) on its own, no JavaScript at all, and ASP.NET Core MVC's model binding matches that query string value to `LocationLookupController.Index(string zipCode)`'s parameter automatically, it doesn't care whether a value arrived via a route segment or a query string, as long as the name matches. Worth noting this produces a slightly different URL shape than the classic project's route-segment style (`?zipCode=75067` vs. `/Index/75067`), both are equally valid; the point here is that the *simpler* one needed zero client-side code.

---

## Try It Yourself

Run the project, search for a ZIP code, and compare the URL in your browser's address bar against what `Samples.MvcWebPortal`'s equivalent search produces, then compare the total lines of code needed for the search form itself.
