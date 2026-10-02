# Samples.MvcWebApi.WebClient

## What This Is

A browser-based test console for `Samples.MvcWebApi`, using jQuery `$.ajax()` for JSON POST calls and direct URL navigation for GET-style calls. Includes a link to the API's Swagger documentation.

---

## When to Use This Pattern

Whenever a browser needs to call a Web API 2 endpoint. For modern projects, the Core sibling (`Samples.MvcWebApi.Core.WebClient`) uses the browser's native `fetch()` with no jQuery dependency at all -- worth comparing both.

---

## How It Works

```javascript
$.ajax({
    type: "POST",
    url: apiUrl + "/api/locationlookup",
    data: JSON.stringify({ ZipCode: zipCode }),
    contentType: "application/json",
    success: function(result) {
        // result is the deserialized LocationResponse, no .d wrapper (unlike ASMX)
    }
});
```

No `result.d` unwrapping needed here -- that's specific to ASMX. Web API returns the response object directly.

---

## Creating a Similar Project

Same pattern as the other web clients: a minimal ASP.NET Web Application to host static files under IIS Express.

### Visual Studio

**File > New > Project**, "ASP.NET Web Application (.NET Framework)", "Empty" template. Add an HTML file and a JS file.

### VS Code

Create the HTML/JS files and serve them with VS Code's Live Server extension or any static file server.

---

## Running This Project

1. Start `Samples.MvcWebApi` (F5 in Visual Studio) and leave it running.
2. Run this project (F5). Use the operation cards and the Swagger link.

---

## Related Projects

- `Samples.MvcWebApi` -- the API this page calls.
- `Samples.MvcWebApi.Core.WebClient` -- the modern `fetch()`-based sibling.
