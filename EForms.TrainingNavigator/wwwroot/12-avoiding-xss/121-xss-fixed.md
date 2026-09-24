# Cross-Site Scripting (XSS) - Fixed Example

The exact same page as the previous lesson, with one change: the value from the URL is passed through a `Sanitize()` function before it's written into the DOM.

```javascript
function Sanitize(value) {
    // Escape the following characters (< > & ' ")
    value = value.replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/&/g, "&amp;")
        .replace(/'/g, "&#39;")
        .replace(/"/g, "&quot;");
    return value;
}
```

```javascript
if (urlParams.has("myData")) {
    var value = Sanitize(urlParams.get("myData"));
    // This is not XSS-vulnerable, because we've sanitized the incoming data before updating the DOM
    $("#myData").html(value);
}
```

## Why This Works

Escaping `<` to `&lt;` and `>` to `&gt;` means a payload like `<script>alert('...')</script>` no longer looks like a tag to the browser at all - it renders as the literal, visible text `<script>alert('...')</script>` instead of being parsed as markup and executed. The `&`, `'`, and `"` escapes close related gaps: an unescaped `&` can accidentally form a different valid entity than intended, and unescaped quotes can let injected content break out of an attribute value it's been placed inside.

## The Simpler Alternative: Don't Use `.html()` at All

Escaping is the right fix here specifically because this lesson's `#myData` span needs to render as part of an existing structure. But in many cases, the real fix is even simpler: use `.val()` or `.text()` instead of `.html()` in the first place. Both treat their argument as plain text unconditionally, with no escaping required at all, because they never ask the browser to parse it as markup to begin with. Reach for `.html()` only when you genuinely need to insert markup; reach for `.text()`/`.val()` (or their vanilla-JS equivalent, `.textContent`) for everything else, and most of this category of bug never has a chance to happen.

See the next lesson for a hands-on, side-by-side way to see both versions' behavior at once using the exact same crafted URL, along with the full original article this whole chapter is based on.
