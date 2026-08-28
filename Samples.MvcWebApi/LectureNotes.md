# Samples.MvcWebApi

## What This Is

ASP.NET Web API 2, mixing classic MVC (a home page) with genuinely RESTful API controllers, backed by EF6 Database-First. See `README.md` for the fuller technology overview.

---

## Areas/HelpPage Dropped

The original shipped **two** interactive API documentation UIs for the same API: the classic Web API "HelpPage" (`Areas/HelpPage`, ~30 mostly Microsoft-scaffolded MVC/Razor files) and Swashbuckle/Swagger. Genuinely redundant, and per an explicit decision made before porting this project, HelpPage was dropped entirely, keeping only Swashbuckle, the more modern, actively maintained choice. `SwaggerConfig.cs` was kept essentially unchanged (still genuine Swashbuckle configuration), just with its comment updated to explain the HelpPage removal.

---

## Serilog Replaces log4net, `DatabankException` Replaces `ApplicationException`

Same treatment as `Samples.AsmxWebService`/`Samples.WcfService`: log sink configuration lives in `serilog.json`, minimum level is driven at runtime from a `debugMode` setting (a plain `appSettings` entry here, this project has no custom `<serviceSettings>` config section the way the ASMX/WCF samples do), and `Global.asax.cs`'s `Application_Start` wires `Log.Logger` into `ErrorHandling`, `LogFilter`, and `ExceptionFilter` all at once.

No `ApplicationException` usages were found in the actual compiled code paths of this project (the controllers rely entirely on filters for error handling, with the equivalent try/catch code left commented out for comparison, see `README.md`'s Pros section). `CSharp.SharedLibrary` is still referenced for consistency and future use.

---

## Bootstrap 3 → 5: A Real Markup Migration, Not Just a Version Bump

Bootstrap 5 **removed** `.jumbotron` entirely (not renamed, gone), and renamed `navbar-toggle` → `navbar-toggler`, `data-toggle`/`data-target` → `data-bs-toggle`/`data-bs-target`, and the three-`<span class="icon-bar">` hamburger icon → a single `navbar-toggler-icon` span. Bumping the version number alone would have produced a broken, unstyled page. **Fixed** by actually rewriting `Views/Shared/_Layout.cshtml`'s navbar markup and `Views/Home/Index.cshtml`'s jumbotron-replacement (`p-5 mb-4 bg-light rounded-3` utility classes reproduce the same visual block) to genuine Bootstrap 5 syntax.

While rewriting `Views/Home/Index.cshtml` anyway, its content was changed from the generic, never-customized "ASP.NET is a free web framework..." MVC scaffolding placeholder text to a real description of this actual sample and its three operations, worth knowing this is a content change beyond the Bootstrap migration itself, not something forced by the version bump.

---

## Bootstrap/jQuery/Modernizr: CDN Instead of Vendored Packages

The original NuGet packages for `bootstrap`, `jQuery`, and `Modernizr` only ever worked by physically copying files into `Scripts`/`Content` under the legacy `packages.config` installation model, a mechanism `PackageReference` does not replicate (it restores assembly references, not arbitrary content files). Rather than hand-vendoring these libraries into the project, `Views/Shared/_Layout.cshtml` now loads Bootstrap and jQuery from CDN directly. Modernizr was dropped entirely, per the earlier decision to remove it project-wide, negligible value in any browser still receiving updates.

---

## A Real Bug Found: Two Dead, Orphaned Connection Strings

```xml
<add name="LocationDatabase" connectionString="metadata=res://*/Model1.csdl|..." />
<add name="LocationDataEntities" connectionString="metadata=res://*/LocationDbModel.csdl|..." />
<add name="LocationLookupDatabase" connectionString="metadata=res://*/LocationLookupModel.csdl|..." />
```

The original `Web.config` had **three** EF connection strings. Only `LocationLookupDatabase` matches anything actually in this project, `LocationLookupModel.edmx`, and the `DbContext`'s own `base("name=LocationLookupDatabase")` call. The other two reference EDMX resource names (`Model1.csdl`, `LocationDbModel.csdl`) that don't exist anywhere in this codebase, leftover configuration from an earlier version of this sample that was renamed at some point and never cleaned up. **Fixed** by removing both dead entries, keeping only the one genuinely used. All three also pointed at a real internal test server and credential pair, genericized here regardless.

---

## `DocumentationFile` Now Generated in Both Configurations

The original only set `<DocumentationFile>` (which Swashbuckle reads for operation descriptions) in the `Debug` configuration, meaning a `Release` build would silently lose every Swagger description. Added to `Release` too, a small, genuine improvement rather than a faithful-but-flawed port.

---

## Three Real Build Errors Found and Fixed

**`ConfigurationManager` ambiguity.** `Microsoft.Extensions.Configuration` (needed for `serilog.json`) defines its own `ConfigurationManager` class, genuinely ambiguous against `System.Configuration`'s once both namespaces are `using`-imported in the same file. The exact same gotcha already hit (and fixed) in `Samples.WcfService`'s `ExampleWebService.svc.cs`, missed here in `Global.asax.cs` the first time through. **Fixed** by fully qualifying `System.Configuration.ConfigurationManager.AppSettings[...]`.

**`AutoGenerateBindingRedirects` wasn't reaching this project.** It's set `true` solution-wide in `Directory.Build.props`, and that was assumed sufficient (it appeared to work for `Samples.AsmxWebService`/`Samples.WcfService`). This project hit a build warning explicitly asking for the property to be set anyway, evidently that inheritance doesn't reliably reach every legacy web project. **Fixed** by setting `<AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>` directly in this project's `.csproj` too, rather than relying on inheritance.

**Unresolved `Antlr3.Runtime`/`WebGrease` version conflicts.** The original `packages.config` explicitly pinned both (`Antlr 3.5.0.2`, `WebGrease 1.6.0`), and that pinning was dropped when modernizing to `PackageReference`, on the assumption NuGet would resolve the versions automatically. It didn't: `Microsoft.AspNet.Mvc`/`WebPages` transitively pull in an *older* Antlr (`3.4.1.9004`, via Razor) and an older WebGrease (`1.5.2`), while `Microsoft.AspNet.Web.Optimization` wants newer versions of both, a genuine, unresolved conflict between two different parts of the dependency graph, not something PackageReference resolves on its own. **Fixed** by adding both back as explicit `<PackageReference>` entries at the original's own pinned versions, which is exactly what an explicit pin is for: telling NuGet which version wins when two dependencies disagree.

Along the way, the build warnings also revealed the actual resolved version of `System.Net.Http.Formatting` is `6.0.0.0`, not the `5.3.0.0` originally guessed to match the WebApi package version (the same class of package-version-vs-assembly-version mismatch already seen with Serilog in `Samples.AsmxWebService`). `Web.config`'s binding redirect was corrected to match.

---

## Try It Yourself

Run the project, browse to `/swagger`, and try `LocationLookup` interactively, Swagger's "Try it out" button lets you execute a real request against the running API and see the actual response, including a genuine database round-trip through EF6.
