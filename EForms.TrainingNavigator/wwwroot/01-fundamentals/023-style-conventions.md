# HTML5 Style Conventions

Consistent style makes HTML easier to read, review, and maintain across a team. This page (click each item on its own page to expand it) covers the conventions used throughout these lessons.

- Always declare `<!DOCTYPE html>` - we use the HTML5 specification exclusively.
- Use all lowercase for tags and attributes: `<span class="spacer">`, not `<SPAN CLASS="spacer">`.
- Close every container element: `<p>...</p>`, not `<p>...` left open.
- Don't use self-closing syntax for empty/object elements: `<br>`, not `<br />`.
- Always surround attribute values with quotes: `<span class="spacer">`, not `<span class=spacer>`.
- Don't put spaces around the `=` in an attribute: `<span class="spacer">`, not `<span class = "spacer">`.
- Avoid long lines - since HTML ignores line breaks in the source, keep lines under roughly 130 characters for readability.
- Indent child elements by four spaces per level.
- Include all required and recommended top-level elements: `<!DOCTYPE html>`, `<html lang="en-us">`, `<head>` (with `<meta charset="utf-8">` and `<title>` required, `<meta name="viewport" ...>` recommended), and `<body>`.
- Keep `<script>` and `<style>` tags inside `<head>` - never after `</head>`.

None of these affect how the page actually renders - a browser doesn't care about casing, quoting, or indentation. They exist purely for the humans who have to read and maintain the code afterward.
