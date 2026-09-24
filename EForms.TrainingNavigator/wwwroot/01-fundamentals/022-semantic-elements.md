# Semantic Elements

## Semantic vs. Syntactic Elements

Some HTML elements only describe how content should be *rendered* - `<div>` and `<span>` are the classic examples, generic containers with no meaning of their own. **Semantic** elements go further: they also describe the *intent* or *role* of the content they contain, both to the browser and to any developer reading the markup.

`<main>` and `<div class="main">` render identically by default, but only `<main>` actually tells you (and any tool parsing the page - search engines, screen readers, browser extensions) that this is the page's main content.

## Accessibility Benefits

Semantic elements aren't just for developers reading the markup - screen readers and other assistive technology use them to build a page's "landmark" structure, letting a user jump directly between regions like `<nav>`, `<main>`, and `<header>` instead of tabbing through the whole page linearly. A page built entirely from `<div>`s has no such structure to navigate by; the same page built with semantic elements is far more usable for someone who can't see the visual layout.

## The Semantic Layout Elements

- `<header>` - introductory content for a page or a section
- `<nav>` - a block of navigation links
- `<main>` - the page's primary, unique content (should appear once per page)
- `<section>` - a distinct, thematically-grouped section of content
- `<article>` - a self-contained piece of content that could stand on its own (a blog post, a news story)
- `<aside>` - content tangentially related to the surrounding content (a sidebar, a pull quote)
- `<figure>` / `<figcaption>` - a self-contained piece of media (usually an image) plus its caption, grouped together
- `<footer>` - closing content for a page or a section

These are all block-level and can be nested inside each other in whatever combination reflects the actual structure of your content - this lesson's own page nests `<article>` inside `<section>` inside `<main>`, for example.

## Other Semantic Tags

Beyond layout, a few inline/small elements carry semantic meaning too:

- `<time>` - a specific date/time (this lesson's own page uses it to display the current time)
- `<mark>` - highlighted/relevant text
- `<details>` / `<summary>` - a native, built-in collapsible disclosure widget, no JavaScript required
