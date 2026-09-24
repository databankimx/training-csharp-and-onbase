# Text Formatting

HTML has a handful of tags dedicated to formatting a run of text within a paragraph. In most cases, prefer CSS over these for purely visual styling - but several of these tags carry real semantic meaning beyond their default appearance, which CSS alone can't express.

## Bold: `<b>` vs. `<strong>`

Both render bold by default, but they mean different things:

- `<b>` - bold text with **no** extra importance (e.g. a keyword in a definition list)
- `<strong>` - text of **strong importance**; screen readers may announce it with extra emphasis

## Italic: `<i>` vs. `<em>`

Same pattern:

- `<i>` - italic text with no extra emphasis (e.g. a technical term, a foreign phrase, a book title)
- `<em>` - text with **stress emphasis** - meaningfully changes the sentence if read aloud with emphasis

## Other Formatting Tags

- `<small>` - side comments, fine print, legal disclaimers (renders smaller by default)
- `<mark>` - highlighted/marked text, e.g. search results matching a query
- `<del>` - text marked as deleted (renders with strikethrough)
- `<ins>` - text marked as inserted (renders underlined)
- `<sub>` - subscript text (e.g. the 2 in H<sub>2</sub>O)
- `<sup>` - superscript text (e.g. the 2 in x<sup>2</sup>)

`<del>`/`<ins>` are often used together to show a tracked change (what was removed and what replaced it), rather than just deleting old text outright.
