# Block vs Inline Elements

Every HTML element falls into one of two basic display categories by default (which CSS's `display` property can always override).

## Block Elements

A block element:

- Starts on its own new line (forces a line break before it)
- Takes up the full width of its parent container by default
- Can contain other block elements and inline elements

`<p>`, `<div>`, `<h1>`-`<h6>`, `<ul>`/`<ol>`/`<li>`, `<table>`, and `<form>` are all block elements.

## Inline Elements

An inline element:

- Does **not** force a line break - it flows within the surrounding text
- Only takes up as much width as its own content needs
- Generally can't contain block-level elements

`<span>`, `<a>`, `<b>`/`<strong>`, `<i>`/`<em>`, and `<img>` are all inline elements.

## Deprecated Elements

This lesson's own reference lists include several struck-through tags (`<font>`, `<center>`, `<strike>`, `<big>`, etc.) - presentational tags from early HTML that are no longer part of the HTML5 specification. Their visual effects are all achievable with CSS instead, which is now the correct way to control them.

Click any tag in the lists on this lesson's own page to open its full reference documentation.
