# Using Images

## `<img>`

The `<img>` tag embeds an image. It's an empty/self-closing tag (no content, no closing tag) with a few key attributes:

- `src` - where the image comes from
- `alt` - text shown if the image can't be loaded, and read aloud by screen readers; always include this
- `width` / `height` - the image's rendered dimensions

## Absolute vs. Relative URLs

`src` can point to an image anywhere:

- **Absolute URL** - a full web address, pointing to an image hosted elsewhere: `src="https://cdn-ilcdbbj.nitrocdn.com/.../Header-Logo-DB.png"`
- **Relative URL** - a path relative to the current page's own location, pointing to an image bundled alongside your HTML: `src="images/html.png"`

## Missing Images

If the file at `src` can't be found, the browser shows the `alt` text in its place instead of a broken image. This is another reason `alt` text is always worth including - it's your fallback, not just an accessibility nicety.

## Base64-Encoded Images

An image can also be embedded directly in the `src` attribute as base64-encoded text, using a `data:` URI, rather than referencing an external file at all:

```html
<img src="data:image/svg+xml;base64,..." />
```

This avoids a separate file/network request, at the cost of bloating the HTML itself - generally only worth it for small icons or images that must always be available even if other assets fail to load.

## Image Maps

An `<img>` can be linked to a `<map>` (via the `usemap` attribute) to make specific regions of the image clickable as their own hyperlinks:

```html
<img src="images/workplace.jpg" alt="Workplace" usemap="#workmap">
<map name="workmap">
    <area shape="rect" coords="34,44,270,350" alt="Computer" href="https://www.google.com/search?q=computer">
    <area shape="circle" coords="337,300,44" alt="Coffee" href="https://www.google.com/search?q=coffee">
</map>
```

Each `<area>` defines one clickable region, using a `shape` (`rect`, `circle`, or `poly`) and its matching `coords`. A `poly` shape isn't limited to simple polygons - its `coords` can define as many vertices as needed to trace an irregular outline.
