# CSS Float Layout

The `float` property was originally designed to let text wrap around an image, but was widely repurposed (before Flexbox/Grid existed) to build entire page layouts by floating whole sections side by side.

## The Basic Pattern

Two elements both given `float: left` sit next to each other, each taking up a percentage of the available width:

```css
nav { float: left; width: 30%; }
article { float: left; width: 70%; }
```

## The Clearfix Problem

Floated elements are taken out of normal document flow - their parent container doesn't "see" their height, which can visually collapse the layout. The `::after` pseudo-element trick in this lesson's own CSS is a common fix (a "clearfix"):

```css
section::after {
    content: "";
    display: table;
    clear: both;
}
```

This need for a workaround (rather than something built into float layout itself) is exactly why Flexbox and Grid are generally preferred for layout today - they don't have this problem at all.

## Making It Responsive

The `@media (max-width: 600px)` block switches both columns to `width: 100%` on small screens, stacking them vertically instead of side by side. See the Responsive Design lesson for more on media queries.
