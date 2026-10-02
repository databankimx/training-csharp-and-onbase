# Samples.WebForms

## What This Is

ASP.NET Web Forms, the oldest server-side UI technology in this training set, part of .NET's very first release (2002). It demonstrates a genuinely different execution model from everything else here: **postback**. A button click submits the entire page back to itself, the framework re-runs the page lifecycle to figure out what happened, and re-renders the page. There's no routing to a distinct action method, no separate URL -- the page is both the form and its own handler.

Same ZIP code lookup as every other Samples project, backed by EF6 against the same `ZipCodes` table.

---

## When to Use Web Forms

Only for existing Web Forms applications. **There is no ASP.NET Core equivalent** -- a permanent, deliberate Microsoft decision made early in ASP.NET Core's development. The postback model, `ViewState`, the server control tree, and the page lifecycle don't map onto ASP.NET Core's request pipeline at all. If you're maintaining a Web Forms application, you're maintaining it in classic ASP.NET Framework indefinitely, or rewriting it in MVC, Razor Pages, or Blazor.

---

## How Web Forms Works

Every interactive control (like `btnSearch`) posts the entire page back to its own URL. `Page_Load` runs first on every request (use `!IsPostBack` to skip initial setup on postbacks), then the button's event handler fires:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    if (!IsPostBack) { /* first load only */ }
}

protected void btnSearch_Click(object sender, EventArgs e)
{
    string zip = txtZipCode.Text; // already restored from ViewState automatically
    var results = /* EF6 query */;
    gridResults.DataSource = results;
    gridResults.DataBind();
}
```

`txtZipCode.Text` is read directly with no re-binding code -- ASP.NET already restored the textbox's value from the hidden `__VIEWSTATE` field before the event handler ran. This is what makes Web Forms feel almost stateful across what are, underneath, completely independent HTTP requests.

**Make it visible:** right-click the rendered page in a browser, View Source, find `<input type="hidden" name="__VIEWSTATE" ...>`. That large base64-encoded blob is where every server control's state lives between postbacks.

The URL in the address bar never changes. Every interaction is a `POST` to the same `.aspx` URL. Compare this directly against `Samples.MvcWebPortal.Core` (a `GET` with a query string) or `Samples.RazorPages` (also `GET`) -- three different execution models, same task.

---

## Creating a Web Forms Project

### Visual Studio

**File > New > Project**, search "ASP.NET Web Application (.NET Framework)", click Next, select the "Web Forms" template, click Create. The scaffolding creates `Site.Master` (the shared layout), `Default.aspx` (the home page), and `Global.asax`.

To add a new page: right-click the project > **Add > New Item > Web Form with Master Page**. The `.aspx` markup and `.aspx.cs` code-behind are created together.

To add a server control to the form: drag from the Toolbox in Design view, or type the tag directly in Source view (`<asp:TextBox runat="server" ID="txtZipCode" />`). The `runat="server"` attribute is what makes a tag a server control -- without it, it's just plain HTML that the server ignores.

### VS Code

Web Forms requires the classic ASP.NET hosting model and Visual Studio's design-time tooling. Create the project in Visual Studio. VS Code can edit the `.aspx` and `.aspx.cs` files for simple changes, but the project scaffolding, server control IntelliSense, and designer support require Visual Studio.

---

## Running This Project

1. Point `Web.config`'s `ExternalDataEntities` connection string at a SQL Server instance with a `ZipCodes` table.
2. Press F5 (IIS Express).
3. Enter a ZIP code and click Search. Watch the URL -- it never changes.

---

## Pros and Cons

**Pros:** Familiar to Windows Forms / event-driven developers. `ViewState` makes controls feel stateful without manual rebinding. Rich built-in server controls (`GridView`, `DetailsView`, etc.) with full property models.

**Cons:** Postback makes every interaction a full page reload. `ViewState` bloats page size. One server form per page. No ASP.NET Core equivalent -- no migration path except a rewrite. No modern DI or middleware story.

---

## Related Projects

- `Samples.MvcWebPortal` -- classic ASP.NET MVC 5, same server-side rendering, different model.
- `Samples.MvcWebPortal.Core` -- ASP.NET Core MVC, GET-based, DI-aware.
- `Samples.RazorPages` -- ASP.NET Core's page-focused server-rendered pattern.
