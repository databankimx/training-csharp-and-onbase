# CSS Flexbox Layout

Flexbox is a layout model purpose-built for arranging items in a single row or column, distributing space between them - a more direct tool for this job than repurposing `float`.

## Setting Up a Flex Container

Any element becomes a flex container with `display: flex`. Its direct children automatically become flex items, laid out in a row by default:

```css
section {
    display: flex;
}
```

## Sizing Items with `flex`

The `flex` property controls how much of the container's available space each item takes, relative to its siblings:

```css
nav { flex: 1; }
article { flex: 3; }
```

Here, `article` gets three times as much space as `nav` - the numbers are proportions, not fixed sizes, so they scale automatically as the container's width changes.

## Vendor Prefixes

The `-webkit-` and `-ms-` prefixed versions of these properties exist for older browser compatibility. Modern browsers only need the unprefixed `flex`/`flex-direction`, but the prefixed versions are harmless to include alongside them for broader support.

## Making It Responsive

Setting `flex-direction: column` inside a media query switches the layout from a row to a stacked column on small screens:

```css
@media (max-width: 600px) {
    section { flex-direction: column; }
}
```
