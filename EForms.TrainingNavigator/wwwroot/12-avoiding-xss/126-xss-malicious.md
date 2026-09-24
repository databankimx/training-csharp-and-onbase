# XSS Example (Malicious)

This page is identical to XSS Example (Vulnerable) - same page, same unsanitized handling of the `myData` query string parameter. The only difference is the notice at the top, and how you're meant to arrive here: not by typing a URL yourself, but by clicking a link.

## The Payload Decides Where You End Up

Everywhere else in this chapter, the demo payload is `<script>alert('I injected JavaScript!');</script>` - a harmless pop-up that makes the point without doing anything real. The email demo on the previous lesson uses a different payload instead:

```html
<script>window.location='126-xss-malicious.html';</script>
```

Same vulnerability, same reflected-and-executed mechanism - but instead of an alert box, it redirects your browser somewhere else entirely, without asking. That's a closer match to how this actually gets used: the attacker doesn't need you to see a message, they need your browser to end up somewhere they control. In a real attack, this page could have been a convincing fake login form, a malware download, or a page built to look exactly like the one you meant to visit - the notice at the top of this page is standing in for whatever that might have been.
