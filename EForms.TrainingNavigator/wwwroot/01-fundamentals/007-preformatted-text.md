# Pre-Formatting Text

## `<pre>...</pre>`

Normal HTML collapses runs of whitespace (multiple spaces, tabs, line breaks) down to a single space when rendering. The `<pre>` ("preformatted") tag turns that off: whatever whitespace is in the source is preserved exactly as written, including indentation and line breaks.

To keep the fixed-width spacing meaningful, browsers render `<pre>` content in a monospace font by default (rather than the page's normal proportional font).

`<pre>` is commonly used for displaying blocks of code, ASCII art, or any other content where the exact spacing matters. If you need HTML-reserved characters like `<` or `>` to display literally inside a `<pre>` block (rather than being interpreted as a tag), escape them the same way you would anywhere else - `&lt;` and `&gt;`.
