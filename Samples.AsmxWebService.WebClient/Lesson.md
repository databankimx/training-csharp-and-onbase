# Samples.AsmxWebService.WebClient

## What This Is

A plain HTML/JavaScript page calling `Samples.AsmxWebService` directly from the browser via jQuery `$.ajax()`. No proxy class, no build step -- three buttons, request/response JSON displayed on the page.

---

## When to Use This Pattern

Only when a browser needs to call an existing ASMX service and a server-side proxy isn't an option. The same "maintenance only" framing applies here as to ASMX itself.

---

## How It Works

ASMX can accept and return JSON when decorated with `[ScriptMethod]` (already applied to every method on the service side). A browser can POST to the endpoint directly:

```javascript
$.ajax({
    type: "POST",
    url: serviceUrl + "/LookupLocation",
    data: JSON.stringify({ request: { ZipCode: zipCode } }),
    contentType: "application/json; charset=utf-8",
    dataType: "json",
    success: function(result) {
        var data = result.d;  // ASMX always wraps the response in { d: ... }
    }
});
```

The `result.d` unwrapping is mandatory -- ASMX wraps every JSON response in a `d` property as an older security measure. Forgetting this is one of the most common ASMX AJAX debugging sessions.

---

## Creating a Similar Project

This is a static web project, not a compiled application. It's hosted here as a minimal ASP.NET Web Application to get IIS Express, but the HTML/JS/CSS files themselves are the entire product.

### Visual Studio

**File > New > Project**, "ASP.NET Web Application (.NET Framework)", "Empty" template. Add an HTML file, a JS file, and a CSS file. No controller, no model, no code-behind needed.

### VS Code

Create an empty folder with an HTML file. Use VS Code's Live Server extension (or any static file server) to serve it during development. The same files can be hosted by any web server in production.

---

## Running This Project

1. Start `Samples.AsmxWebService` (F5 in Visual Studio) and leave it running.
2. Run this project (F5). Click "Ping Service", "Test Service", or "Lookup Location".

---

## Related Projects

- `Samples.AsmxWebService` -- the service this page calls.
- `Samples.AsmxWebService.Client` -- a compiled .NET console client for comparison.
