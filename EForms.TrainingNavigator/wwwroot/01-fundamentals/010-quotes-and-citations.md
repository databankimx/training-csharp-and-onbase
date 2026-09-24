# Quotes and Citations

A handful of tags exist specifically for quoting, citing, and annotating text, each with its own semantic meaning:

- `<abbr title="...">` - marks an abbreviation/acronym; the `title` attribute supplies the full expansion, shown as a tooltip
- `<address>` - contact information for the page's author or the content it's near (physical address, not necessarily an email)
- `<cite>` - the title of a referenced work (a book, article, organization, etc.), typically rendered in italics
- `<blockquote cite="...">` - a longer, block-level quotation from another source; the `cite` attribute (optional) holds a URL to the source, though it isn't displayed
- `<q>` - a short, inline quotation; browsers typically add quotation marks around it automatically
- `<bdo dir="ltr|rtl">` - overrides the text direction (bidirectional override), useful for languages that read right-to-left

## `<blockquote>` vs. `<q>`

Use `<blockquote>` for a quotation substantial enough to be set apart as its own block (often with indentation); use `<q>` for a short quotation that flows inline within a sentence.
