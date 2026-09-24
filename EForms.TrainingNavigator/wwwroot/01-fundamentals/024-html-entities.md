# HTML Entities and Special Characters

Some characters can't be typed directly into HTML, either because they have a special meaning in HTML itself, or because they're not easily typed on a standard keyboard.

## Why Entities Are Needed

- Characters like `<`, `>`, and `"` have special meaning in HTML syntax, so they must be escaped to display literally rather than being interpreted as markup.
- Accented, foreign-language, and currency characters (`á`, `€`, `α`) aren't on a standard US keyboard.
- Many useful symbols (`⇒`, `♠`, `©`) have no keyboard key at all.

## Entity Syntax

There are three equivalent ways to write an entity:

- **Named entity** - `&lt;` renders as `<`
- **Decimal numeric reference** - `&#60;` also renders as `<`
- **Hexadecimal numeric reference** - `&#x3C;` also renders as `<`

Named entities (where one exists) are generally the most readable choice; numeric references work for any Unicode character, including many that have no named entity at all.

## Reference Pages

This lesson links out to a set of dedicated reference pages, each a browsable table of entities for one category - common entities, currency symbols, math symbols, accented letters, Greek letters, arrows, other symbols, and emojis. These are reference material to search when you need a specific character, not something to read start to finish.
