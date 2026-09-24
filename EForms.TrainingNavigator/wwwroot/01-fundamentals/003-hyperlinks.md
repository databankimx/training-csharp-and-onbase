# Basic Hyperlinks

## HTML Attributes

This is the first lesson where you're seeing **attributes** used outside the `<head>` section, so it's worth pausing on the concept itself before looking at hyperlinks specifically.

An attribute provides extra information about an element, written as a `name="value"` pair inside the element's opening tag:

```html
<tagname attribute1="value1" attribute2="value2">Content</tagname>
```

A few things worth knowing about attributes in general:

- Attributes live inside the opening tag itself, separated from the tag name (and from each other) by spaces. They never appear in the closing tag.
- Attribute values are conventionally wrapped in double quotes. Single quotes also work, but double quotes are the standard convention.
- An element can have as many attributes as it needs, in any order.
- Some attributes are **boolean** - their presence alone turns a behavior on, with no value needed (you'll see examples like `required` and `readonly` in later lessons on forms). Most attributes, though, take a specific value, like the ones below.

The `<a>` tag below uses two attributes, `href` and `target`, to configure what the link does - that's the pattern to watch for throughout the rest of this training: an element defines *what kind* of thing it is, and its attributes configure the specifics of *how* it behaves.

## `<a href="...">...</a>`

The anchor tag creates a hyperlink. The `href` attribute holds the destination URL, and the text (or other content) between the opening and closing tags is what the user actually clicks:

```html
<a href="http://www.databankimx.com">This is a hyperlink</a>
```

## Opening Links in a New Tab

By default, clicking a link navigates away from the current page. Adding `target="_blank"` opens the link in a new tab instead, leaving the current page open:

```html
<a href="http://www.databankimx.com" target="_blank">This link opens in a new tab</a>
```

## `<br>`

The `<br>` tag inserts a single line break. Unlike most HTML elements, it's an empty/self-closing tag - it has no content and no closing tag.
