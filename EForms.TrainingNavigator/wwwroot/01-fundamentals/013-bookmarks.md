# Using Bookmarks

An in-page bookmark links to a specific location further down the same page (or a different page), rather than to a different resource entirely.

## Creating a Bookmark Target

Any element with an `id` attribute becomes a jump target:

```html
<div id="chapter1">...</div>
```

## Linking to a Bookmark

An `<a href="#id">` linking to that same `id` (prefixed with `#`) jumps the page to that element when clicked:

```html
<a href="#chapter1">Chapter 1</a>
```

This is commonly used to build a table of contents at the top of a long page, letting readers jump straight to the section they want:

```html
<ul>
    <li><a href="#chapter1">Chapter 1</a></li>
    <li><a href="#chapter2">Chapter 2</a></li>
</ul>
```

## Linking to a Bookmark on a Different Page

The same `#id` syntax works appended to a full URL too, not just within the current page: `href="other-page.html#chapter1"` opens `other-page.html` and jumps straight to the element with `id="chapter1"` on it.
