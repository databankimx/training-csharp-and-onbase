# A Complete Example: the `dbStylize` Plugin

A real, working plugin that applies everything from the previous lesson - the IIFE wrapper, private scope, chainability, and an options object with defaults - to a genuinely useful task: applying one of DataBank's brand color/icon/logo templates to a page with a single call.

> **Note on scope:** the color, icon, and logo system this plugin applies is a working subset sufficient for this lesson. DataBank's full branding system - complete font files, every logo/icon variant, and the canonical source of truth for all of it - is covered in its own dedicated chapter later on.

## Calling It

```javascript
$(document).dbStylize("cloud", { "h2": "italic underline" });
```

- First argument: which template to apply (`databank`, `cloud`, `energy`, `government`, `gold`, `coral`) - defaults to `"databank"` if omitted or unrecognized.
- Second argument: an options object of `{ selector: "css class(es)" }` pairs, letting the caller add extra styling beyond what the template itself defines.

## How the Templates Work

Each template (`db-stylize-templates.js`) is a plain object mapping a handful of semantic roles - `text`, `emph`, `icon`, `logo`, `area` - to the actual CSS classes that realize them for that brand variant:

```javascript
"cloud": {
    text: "db-navy db-white-bg",
    emph: "db-blue db-white-bg",
    icon: "db-cloud-icon-navy",
    logo: "db-cloud-logo-navy",
    area: "db-navy-box"
}
```

The plugin itself never hardcodes a color - it just looks up whichever template was requested and applies its classes. Adding a seventh brand variant later means adding one more object to `templates`, not touching the plugin's logic at all.

## Merging Options Over the Template

```javascript
var template = templates[styleName.toLowerCase()];
for (var templateProp in template) {
    settings[templateProp] = settings[templateProp] ?? template[templateProp];
}
```

The nullish coalescing operator (`??`) fills in each template value only where the caller's own `settings` didn't already supply one - so a caller's explicit option always wins, and the template fills every gap they left. This is a slightly more manual version of the same idea as `$.extend()` from the previous lesson, needed here because the "defaults" are chosen dynamically (by which template was requested) rather than being one fixed object.

## Mapping Roles to Real Selectors

The `switch` statement is where the semantic role names (`text`, `emph`, `icon`, `logo`, `area`) get connected to actual CSS selectors in the page:

```javascript
case "text":
case ".text":
    sel = "body, td, .text";
    settings[settingsProp] += " db-font";
    break;
```

Both `"text"` and `".text"` are accepted as equivalent - a small bit of forgiveness for however a caller happens to write their options object. Anything not matching one of the known roles falls through to the `default` case and is treated as a literal CSS selector, letting a caller extend styling to elements the template system doesn't know about by name (like this lesson's own `{ "h2": "italic underline" }` option).

## What Changed From the Original Draft

If you compare this to a rough early draft of the same idea, two real bugs are fixed here: an unquoted string that would have thrown a runtime error (`db-font` used as a bare identifier instead of `"db-font"`), and undeclared loop variables that leaked into the global scope. Both are exactly the kind of mistake the private-scope and IIFE patterns from the previous lesson exist to prevent - and exactly why it's worth writing plugins that way from the start, rather than retrofitting it once something goes wrong.
