# Samples.WcfService.WebClient

## What This Is

A browser-based test console for `Samples.WcfService`'s REST/JSON endpoint (`webEndpoint`, `webHttpBinding`), demonstrating two calling styles the WCF REST endpoint supports:

- **JSON POST** via `$.ajax()` for `TestService`/`LookupLocation`.
- **Plain GET** via URL navigation for the `...Rest` variants (`TestServiceRest`/`LookupLocationRest`), where parameters are part of the URL path -- no JavaScript required, a link works just as well.

---

## When to Use This Pattern

When a browser needs to call a WCF service's REST endpoint directly. The GET-style REST URLs are worth knowing about specifically -- they work without scripting, useful for testing, for embedding in a plain `<a href>`, or for any consumer that can't run JavaScript.

---

## How It Works

The JSON POST call:

```javascript
$.ajax({
    type: "POST",
    url: serviceUrl + "/LookupLocation",
    data: JSON.stringify({ request: { ZipCode: zipCode } }),
    contentType: "application/json",
    success: function(result) { /* result is a plain JSON object, no .d wrapper like ASMX */ }
});
```

The GET/REST call is just navigation: `window.open(serviceUrl + "/LookupLocationRest/" + zipCode)`. No JavaScript plumbing at all on the client side.

---

## Creating a Similar Project

Same as `Samples.AsmxWebService.WebClient` -- this is a static web project hosted in a minimal ASP.NET shell for IIS Express.

### Visual Studio

**File > New > Project**, "ASP.NET Web Application (.NET Framework)", "Empty" template. Add `index.html`, a JS file, and a CSS file. Enable CORS on the WCF service side (already handled in `Samples.WcfService`'s `Global.asax.cs`).

### VS Code

Create an HTML file and serve it with VS Code's Live Server extension. Configure CORS on the service if needed.

---

## Running This Project

1. Start `Samples.WcfService` (F5 in Visual Studio) and leave it running.
2. Run this project (F5). Use the "JSON" and "REST" buttons on each card.

---

## Related Projects

- `Samples.WcfService` -- the service this page calls.
- `Samples.WcfService.Client` -- .NET console client via `ChannelFactory<T>` (SOAP endpoint).
