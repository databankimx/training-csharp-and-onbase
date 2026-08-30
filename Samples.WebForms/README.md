# Samples.WebForms

> **Looking for implementation details or notes?** See `LectureNotes.md` in this folder.

## What This Is

ASP.NET Web Forms, the *oldest* server-side UI technology in this entire training set, part of .NET's very first release (2002). It demonstrates a genuinely different execution model from everything else here: **postback**, not request-per-action. A button click submits the entire page back to itself, and the framework re-runs the page's lifecycle (`Page_Load`, then the relevant control's event handler) to figure out what happened and re-render, rather than routing to a distinct action method or page handler.

This sample looks up city/county/state by ZIP code, the same domain every other sample in this training set uses, backed by EF6 against the same `ZipCodes` table structure.

---

## When to Use Web Forms

Only for existing Web Forms applications. **There is no ASP.NET Core equivalent at all**, a permanent, deliberate Microsoft decision (not an oversight, not "coming later"), so there's no meaningful migration path except a genuine rewrite in MVC, Razor Pages, or Blazor. If you're maintaining a Web Forms application today, you're maintaining it in classic ASP.NET Framework indefinitely, or rewriting it.

---

## What Makes This Genuinely Different

- **Postback.** Every interactive control (like `btnSearch` here) posts the *entire page* back to its own URL. There's no separate route, action method, or page handler the way MVC/Razor Pages/Web API have, `Default.aspx.cs`'s `Page_Load` and `btnSearch_Click` together *are* the entire request-handling logic for this page.
- **Automatic `ViewState`.** `txtZipCode.Text` is read directly in `btnSearch_Click` with **no** re-binding code anywhere, ASP.NET already restored the textbox's value from a hidden, encoded `__VIEWSTATE` field before that method even ran. Compare this against `Samples.RazorPages`' explicit `[BindProperty(SupportsGet = true)]` or `Samples.MvcWebPortal.Core`'s query-string parameter, both of which require you to say, in code, how a value gets from the request into your page.
- **One server form per page.** An entire Web Forms page can have exactly one `<form runat="server">` element (see `Site.Master`), every postback-capable control anywhere on the page shares it.
- **Server controls with a full property model.** `<asp:GridView>`, `<asp:TextBox>`, `<asp:Button>` etc. aren't just HTML tags, they're server-side objects with their own properties, methods, and events, rendered to HTML at the very end of the page lifecycle.

---

## What's in This Project

| Path | Purpose |
|---|---|
| `Site.Master` / `.master.cs` | The master page, Web Forms' equivalent of a shared layout |
| `Default.aspx` / `.aspx.cs` | The search form and results, combined, using postback |
| `Models/` | EF6 Database-First model (same `ZipCode` entity as `Samples.MvcWebPortal`) |
| `serilog.json` | Log sink configuration |

---

## How to Run

1. Point `Web.config`'s `ExternalDataEntities` connection string at a real SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Enter a ZIP code and click Search, watch the same page reload with results.

---

## Related Samples

- **`Samples.MvcWebPortal`** / **`Samples.MvcWebPortal.Core`** / **`Samples.RazorPages`** — the technologies that succeeded Web Forms, worth comparing the execution models directly.
