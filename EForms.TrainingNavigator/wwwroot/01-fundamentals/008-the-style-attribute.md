# Using the `style` Attribute

## `style="..."`

Nearly any HTML element accepts a `style` attribute, which applies CSS directly to that one element, inline, without needing a separate stylesheet:

```html
<h1 style="color: tomato">Colored Text</h1>
```

Multiple declarations are separated by semicolons:

```html
<p style="font-family: Helvetica, sans-serif; font-size: 36pt; text-align: center">Centered Big Text</p>
```

A style set this way applies to the element it's on, and (for properties like `font-family` that are inherited by default) to its descendants as well - that's why setting `style` on `<body>` affects the whole page's default font unless a child element overrides it.

## Inline Styles vs. Stylesheets

Inline `style` attributes are convenient for quick experiments and one-off overrides, but for real projects, prefer a `class` attribute plus rules in a `.css` file (external or in a `<style>` block in the `<head>`) instead. Reasons:

- **Reuse** - one CSS rule can style every element with a given class, instead of repeating the same `style` text everywhere
- **Maintainability** - changing a look site-wide means editing one stylesheet rule, not hunting down every inline `style` attribute
- **Specificity** - inline styles override almost everything else by default, which can make styles from a shared stylesheet hard to override predictably

These lessons use `style` attributes directly in places purely to keep each example self-contained and easy to read in isolation - not as a recommendation for how to write production forms.
