# Adding a Favicon

## `<link rel="icon">`

A favicon is the small icon a browser shows in the tab, bookmarks bar, and history for a page. It's added with a `<link>` tag in the `<head>`, not visible content in the `<body>`:

```html
<link rel="icon" type="image/x-icon" href="images/Icon_Blue.ico" />
```

- `rel="icon"` tells the browser this link points to the page's icon
- `type` should match the actual image format - `.ico` files use `image/x-icon`; a `.png` favicon would use `image/png`, and so on
- `href` is the path to the icon file itself, relative or absolute like any other URL

If no favicon is specified, most browsers fall back to requesting `/favicon.ico` from the site's root automatically - explicitly linking one avoids relying on that fallback (and lets you use a different filename, format, or location).
