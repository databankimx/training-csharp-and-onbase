# Using Colors

CSS colors can be specified several different ways:

- **Named colors** - e.g. `tomato`, `bisque` (HTML/CSS supports 140 standard names, browsable in the table on this lesson's own page)
- **RGB** - red/green/blue values from 0-255: `rgb(255, 99, 71)`
- **Hexadecimal** - the same RGB values, as hex: `#FF6347`
- **HSL** - hue (0-360 degrees), saturation (0-100%), lightness (0-100%): `hsl(9, 100%, 64%)`

## Alpha (Opacity)

Each of RGB, hex, and HSL supports a fourth value for opacity/transparency, from fully transparent to fully opaque:

- `rgba(255, 99, 71, 0.5)` - alpha as a 0-1 decimal
- `#FF634780` - alpha as an extra two hex digits (`00`-`FF`) appended to the color
- `hsla(9, 100%, 64%, 0.5)` - same idea as RGBA, with HSL

## Foreground vs. Background

Color applies differently depending on which CSS property it's set on:

- `color` - the foreground (text) color
- `background-color` - the background behind the element

Both can be set on the same element at once.
