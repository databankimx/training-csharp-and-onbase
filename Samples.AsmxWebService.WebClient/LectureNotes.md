# Samples.AsmxWebService.WebClient

## What This Is

A plain HTML/CSS/JavaScript page that calls `Samples.AsmxWebService` directly from the browser via jQuery's `$.ajax()`, no server-side code of any kind, no build step for the client itself. This is the second of two consumption styles demonstrated for this service (`Samples.AsmxWebService.Client` being the first, a compiled .NET console app using a generated proxy).

---

## Porting Notes

**The original project format was itself the thing worth noting**: this was an old-style ASP.NET "Web Site" project, directory-based, no `.csproj` at all, opened in Visual Studio via "Open Web Site" rather than "Open Project". That format predates MSBuild-based tooling entirely and integrates poorly with a modern solution (no real build step, an awkward `packages.config`-in-a-`Bin`-folder NuGet convention, no project references). Converted here to a proper, lightweight Web Application Project instead, matching `Samples.AsmxWebService`'s own pattern, so it opens, builds, and F5-runs normally as part of this solution.

Because there's no C# code to compile in this project at all (pure static content), it needs none of the `PackageReference`/`<system.codedom>` machinery the ASMX service project needed, that was present in the original `Web.config` but was already unused dead configuration even there, a leftover from whatever Visual Studio template originally scaffolded the project.

jQuery bumped from 2.2.4 to 3.7.1 (matching this training set's front-end library modernization elsewhere); nothing in this file used any jQuery 3.x-removed API, so the upgrade was a drop-in version bump.

---

## A Real Bug Fixed: `.http()` Is Not a Real jQuery Method

```javascript
error: function (request, status, error) {
    ...
    $("#responseJson").http("\n" + error);   // bug: not a real jQuery method
    ...
}
```

`.http()` doesn't exist on a jQuery object, calling it throws a genuine `TypeError` at runtime. Because this line sits inside a `try`/`catch` in `callWebService()`, that `TypeError` was silently swallowed by the `catch` block rather than visibly breaking anything, meaning: whenever the web service call itself actually failed, the error message was logged to the browser console (via `writeLogEntry`) but the response panel on the page stayed blank instead of showing the failure, exactly the situation where a user would most want to see *something* on screen. **Fixed** to `.html("\n" + error)`, matching the success handler's own pattern for writing into that same panel.

---

## A Second Real Bug Fixed: `<textarea>` Doesn't Reliably Re-render on `.html()`

The original request/response panels were `<textarea>` elements, and both the success and error handlers in `callWebService()` call `.html(...)` to populate them. That's a real, separate problem from the `.http()` typo above: once a browser has parsed a `<textarea>`, what it actually *displays* is driven by the element's `.value` property, not its `innerHTML`, so calling `.html()` on it after the page has loaded does not reliably update what's visibly shown in current browsers, even though the underlying DOM content technically changed. In other words, even with the `.http()` typo fixed, the response panel likely still wouldn't have visibly updated on error in a modern browser, and depending on the exact engine, may not have reliably updated on *success* either.

**Fixed** as part of the visual redesign below by switching both panels from `<textarea>` to `<pre>` elements. A `<pre>`'s rendered content *is* its `innerHTML`, so `.html()` updates it correctly and predictably, and semantically it's the right element regardless, these panels were always read-only output, never something the user was meant to type into. No JavaScript changes were needed for this fix, `$("#requestJson")`/`$("#responseJson")` still resolve to the same elements by `id`, `.html()`, `.addClass()`, and `.hasClass()` all work identically on a `<pre>`.

---

## Visual Redesign

The page is a raw SOAP/JSON wire-protocol test console, so the redesign leans into that directly rather than reaching for generic page styling: a dark graphite "service console" shell, monospace type (JetBrains Mono) for anything code- or protocol-related, and a single amber accent used the way a status light is used, not a marketing gradient. The three operations are laid out as numbered cards (a genuine, meaningful sequence here, each one is a distinct SOAP operation being tested), and the request/response panels are styled like a browser DevTools Network inspector, complete with a status dot on each pane that turns red when `#responseJson` carries the `.error` class, reusing the exact same class jQuery was already toggling, no JavaScript changes needed for that either.

---

## Try It Yourself

Run `Samples.AsmxWebService` first (F5, IIS Express), then run this project. Click each button in turn and watch the Request/Response panes populate. Open the browser's developer console too, `writeLogEntry()` logs every step there as well.
