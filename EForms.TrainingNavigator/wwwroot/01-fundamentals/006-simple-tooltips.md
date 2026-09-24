# Adding Tooltips

## `title="..."`

The `title` attribute can be added to almost any HTML element. In most browsers, hovering over the element for a moment shows its `title` text as a small tooltip:

```html
<img title="DataBank Icon" alt="DataBank Icon" src="..." />
```

Don't rely on `title` as your only way of conveying information, though - it's not accessible to keyboard-only or touch-screen users (there's nothing to "hover"), and screen readers handle it inconsistently. For images specifically, `alt` text is the one that matters for accessibility; `title` is just a nice-to-have supplemental tooltip.
