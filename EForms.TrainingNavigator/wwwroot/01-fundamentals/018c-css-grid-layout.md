# CSS Grid Layout

CSS Grid is a two-dimensional layout model, purpose-built for arranging content into rows and columns at once - unlike Flexbox, which only really handles one dimension (a single row or column) at a time.

## Setting Up a Grid Container

Any element becomes a grid container with `display: grid`. Its columns are defined with `grid-template-columns`:

```css
.layout {
    display: grid;
    grid-template-columns: 30% 70%;
}
```

## Named Grid Areas

Grid's most distinctive feature is `grid-template-areas`, which lets you lay out an entire page's regions in one place, almost like drawing a diagram directly in CSS:

```css
.layout {
    grid-template-areas:
        "header  header"
        "nav     article"
        "footer  footer";
}

header { grid-area: header; }
nav { grid-area: nav; }
article { grid-area: article; }
footer { grid-area: footer; }
```

Each element is assigned to a named area with `grid-area`, and the quoted rows in `grid-template-areas` show exactly which area sits where - `header` and `footer` each span both columns, while `nav` and `article` sit side by side.

## Making It Responsive

Redefining `grid-template-columns` and `grid-template-areas` inside a media query can restructure the whole layout at once - here, from two columns down to one, with every region simply stacked in order:

```css
@media (max-width: 600px) {
    .layout {
        grid-template-columns: 1fr;
        grid-template-areas:
            "header"
            "nav"
            "article"
            "footer";
    }
}
```

## Grid vs. Flexbox

They're not competitors so much as tools for different jobs: Grid is generally the better choice for an overall page layout (rows and columns together), while Flexbox tends to be simpler for laying out items within a single row or column - a navigation bar's links, or a row of cards, for example. It's common for a real page to use Grid for its overall structure and Flexbox for smaller pieces within it.
