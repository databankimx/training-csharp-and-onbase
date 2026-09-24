# Headings and Paragraphs

## Heading Levels

HTML provides six levels of heading, `<h1>` through `<h6>`, with `<h1>` being the largest/most important and `<h6>` the smallest/least important:

```html
<h1>This is heading size 1</h1>
<h2>This is heading size 2</h2>
<h3>This is heading size 3</h3>
<h4>This is heading size 4</h4>
<h5>This is heading size 5</h5>
<h6>This is heading size 6</h6>
```

Headings should be used to reflect the actual structure of your content (an `<h1>` for the page title, `<h2>` for major sections, `<h3>` for subsections within those, and so on), not just picked for their visual size. Skipping levels (e.g. jumping from `<h1>` straight to `<h4>`) is technically legal but considered poor practice, since it breaks the logical outline of the page for screen readers and other tools that rely on heading structure to navigate.

## Paragraphs

The `<p>` tag defines a paragraph. Each `<p>` renders with space before and after it automatically - you don't need `<br>` tags to separate paragraphs from one another.
