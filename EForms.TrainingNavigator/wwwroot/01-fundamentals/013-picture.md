# Using the `<picture>` Tag

## `<picture>...</picture>`

`<picture>` lets you offer several image sources and let the browser pick the best one for the current viewport, rather than forcing every device to load the same image regardless of screen size:

```html
<picture>
    <source media="(min-width:800px)" srcset="images/big-viewport.png">
    <source media="(min-width:400px)" srcset="images/small-viewport.png">
    <img alt="DataBank Logo" src="images/tiny-viewport.png">
</picture>
```

Each `<source>` provides a `media` condition (a CSS media query) and its own `srcset` (the image to use when that condition matches). The browser evaluates them in order and uses the first one that matches.

The trailing `<img>` tag is required - it's the fallback used if none of the `<source>` conditions match, and it's also what actually renders the image in browsers that don't support `<picture>` at all. Its own `src`/`alt` attributes work exactly like a normal `<img>`.

This is primarily about serving an appropriately-sized image for the viewport (saving bandwidth on small screens), which is a different problem from responsive *layout* (covered in the CSS layout lessons) - the two are often used together, but `<picture>` only controls which image file loads.
