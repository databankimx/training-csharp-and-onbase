# Responsive Page Design

Responsive design means a page adapts to whatever screen/viewport size it's being shown on, rather than assuming one fixed size.

## `<meta name="viewport">`

Every page in these lessons includes this tag, and it's required for responsive design to work at all on mobile devices:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0">
```

Without it, mobile browsers render the page at a fixed desktop-like width (often 980px) and zoom it out to fit, rather than actually laying it out at the device's own width. `width=device-width` tells the browser to use the device's real width; `initial-scale=1.0` sets the initial zoom level to 100%.

## Scaling Images Responsively

- `width: 50%` - the image always renders at 50% of its container's width, scaling both up and down as the container resizes
- `max-width: 75%` - the image never exceeds 75% of its container's width, but also never scales *up* past its own natural size (avoiding a blurry, stretched-out image on a very wide screen)
- No `width`/`max-width` at all - the image renders at its natural pixel size regardless of the viewport, and can overflow a narrow screen

`max-width` is generally the safer default for responsive images, since it avoids both overflow on small screens and unnatural upscaling on large ones.

## Media Queries

Beyond simple percentage sizing, CSS `@media` queries let you apply entirely different styles at different viewport widths - see the CSS Float Layout, CSS Flexbox Layout, and CSS Grid Layout lessons for examples of using `@media (max-width: ...)` to restructure a whole layout on small screens.
