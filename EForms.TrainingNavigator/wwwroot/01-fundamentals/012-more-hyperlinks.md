# More Hyperlinks

Beyond a plain text link, a hyperlink's clickable content can be almost anything.

## Image Links

Wrapping an `<img>` in an `<a>` makes the whole image clickable:

```html
<a href="https://www.databankimx.com" target="_blank">
    <img src="images/Icon_Blue.svg" alt="..." title="DataBank IMX Website">
</a>
```

## Email Links (`mailto:`)

An `href` starting with `mailto:` opens the visitor's default email client with a new message pre-addressed to that address. Query-string-style parameters after a `?` can pre-fill other fields too:

- `mailto:someone@example.com` - just the recipient
- `mailto:someone@example.com?cc=other@example.com` - adds a CC
- `mailto:someone@example.com?subject=Hello` - pre-fills the subject
- `mailto:someone@example.com?subject=Hello&body=Hi%20there!` - subject and body together

Note the values are URL-encoded (`%20` for a space, `&` separating each parameter) the same way any other URL query string would be.

## Links That Aren't `<a>` Tags

A hyperlink doesn't have to be an `<a>` tag at all. A `<button>` with a small JavaScript `onclick` handler can navigate the browser just as well:

```html
<button onclick="window.location='https://www.databankimx.com';">DataBank IMX Website</button>
```

And conversely, an `<a>` tag can be styled with CSS to look and behave visually like a button (padding, background color, no underline) while still being a real, semantically correct hyperlink underneath - generally the better choice over the JavaScript button approach above, since it keeps working with JavaScript disabled, right-click "open in new tab," etc.
