# Samples.WebForms

## What This Is

A fresh addition to `SampleProjects` (no legacy source to port), demonstrating ASP.NET Web Forms' genuinely distinct execution model. See `README.md` for the fuller when-to-use discussion and the "what makes this different" breakdown.

---

## Current DataBank Standards Applied From the Start

Since there was no existing code to convert, this project uses DataBank's current standards (Serilog, `DatabankException`) directly, no `log4net`-to-Serilog or `ApplicationException`-to-`DatabankException` migration story here, unlike the ported ASMX/WCF/MvcWebApi/MvcWebPortal projects. `Global.asax.cs` follows the exact same Serilog + `LoggingLevelSwitch` + `serilog.json` pattern already established in `Samples.AsmxWebService`, and `Default.aspx.cs`'s `btnSearch_Click` wraps its EF6 query in a `try`/`catch` that throws `DatabankException` on failure, logged via `Global.Logger`.

---

## No Core Sibling, Genuinely

Every other "classic" project in this training set (`Samples.AsmxWebService`, `Samples.WcfService`, `Samples.MvcWebApi`, `Samples.MvcWebPortal`) has, or will have, an ASP.NET Core sibling demonstrating the modern equivalent technology. Web Forms doesn't get one, not because it wasn't prioritized, but because **no ASP.NET Core equivalent exists**. Microsoft made this an explicit, public decision early in ASP.NET Core's development: Web Forms' entire model (postback, `ViewState`, the server control tree, the page lifecycle) doesn't map onto ASP.NET Core's request pipeline at all. `Samples.RazorPages` is the closest thing to "what replaced this," but it's a genuinely different pattern, not a port target.

---

## `ViewState`, Made Visible

Worth actually looking at once, in a browser: right-click the rendered `Default.aspx` page, View Source, and find the hidden `<input type="hidden" name="__VIEWSTATE" ...>` field, a large base64-encoded blob. That's where `txtZipCode`'s value (and every other server control's state) actually lives between postbacks. `Default.aspx.cs` never reads or writes this field directly, the `TextBox` control's own `.Text` property does that automatically as part of the page lifecycle, specifically what makes Web Forms feel almost stateful across what are, underneath, completely independent HTTP requests.

---

## Try It Yourself

Run the project, search a ZIP code, and watch the URL in your browser's address bar, it never changes, every interaction is a `POST` back to the same `Default.aspx`. Compare that directly against `Samples.MvcWebPortal.Core`'s search (a plain `GET` with a query string) or `Samples.RazorPages`' (also `GET`, same URL pattern, different underlying mechanism), a very visible, concrete way to see three different execution models solving the same problem.
