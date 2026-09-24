# Emoji Entities

Emoji can be inserted into HTML using numeric character references, the same mechanism covered in the earlier entity lessons - `&#128512;` produces a grinning face emoji. Many emoji are actually short sequences of several codepoints joined together (sometimes with a zero-width joiner, `&zwj;`), which is what makes family emoji, profession icons, and flag variants possible from a fairly small set of base characters.

## Why This Lesson Doesn't Include a Table

Unlike the smaller reference sets covered elsewhere (arrows, math symbols, Greek letters), the full emoji set runs into the thousands once every sequence and combination is counted. A complete reference table at that scale is roughly 1.5 MB and can be slow enough to load and render that it's worth avoiding inside an embedded viewer or a page a trainee wasn't expecting to be that heavy.

The complete table still exists - it's `024j-emojis-full-reference.html`, in this same folder - and is linked from the lesson page with a clear size and performance warning. Open it in its own browser tab if you need to look something up, rather than through a tool that renders multiple lessons' content in one view.
