# Cross-Site Scripting (XSS) - Vulnerable Example

Cross-site scripting (XSS) happens when a page takes data it didn't fully control - a URL parameter, user input, anything from outside the application - and inserts it into the DOM in a way that lets the browser interpret it as HTML or JavaScript instead of as plain text.

## Reflected XSS via the URL

```javascript
var urlParams = new URLSearchParams(window.location.search);
if (urlParams.has("myData")) {
    // This is XSS-vulnerable, because we're updating the DOM with the raw incoming data
    $("#myData").html(urlParams.get("myData"));
}
```

This page reads a `myData` value straight out of its own URL's query string and writes it into the page with `.html()`. Since `.html()` parses its argument as markup rather than treating it as plain text, a URL like:

```
120-xss-vulnerable.html?myData=<script>alert('I injected JavaScript!');</script>
```

causes the browser to execute that script the moment the page loads - no user interaction beyond opening the link. This is called **reflected** XSS: the malicious payload lives in the link itself (often disguised or shortened), and the vulnerable page "reflects" it back as executable code. It's the classic mechanism behind a phishing email containing a link to a legitimate-looking site.

## The Fix, and Seeing It Happen

See the next lesson for the fix (sanitizing the value before it's written into the DOM), and the lesson after that for a hands-on, side-by-side way to see both versions' behavior at once, along with the full original article this whole chapter is based on.
