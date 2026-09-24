# Using Tables

## Basic Structure

- `<table>...</table>` - the table itself
- `<tr>...</tr>` - a table row
- `<th>...</th>` - a header cell (bold/centered by default, semantically marks it as a header)
- `<td>...</td>` - a regular data cell

`<th>` and `<td>` are interchangeable in terms of what they can contain, but by convention `<th>` is used for the top (header) row so it can be styled and interpreted differently from the data rows below it.

## `<thead>`, `<tbody>`, `<tfoot>`

A table can optionally be split into three sections:

- `<thead>` - the header row(s)
- `<tbody>` - the main data rows
- `<tfoot>` - a footer row (e.g. totals), if needed

None of these are required - a simple table can just use `<tr>` rows directly inside `<table>` - but splitting them out makes it easier to style each section independently (e.g. giving `<thead>` its own background color) and helps browsers/assistive technology understand the table's structure.

## `<colgroup>` and `<col>`

`<colgroup>` groups `<col>` elements, one per column, letting you style an entire column (e.g. a fixed width) without repeating that style on every cell in that column.

## Merging Cells: `colspan` and `rowspan`

- `colspan="n"` - makes a cell span `n` columns
- `rowspan="n"` - makes a cell span `n` rows

When a cell is covered by another cell's `colspan`/`rowspan`, don't include a `<td>` for it at all - the spanning cell already accounts for that space, and adding an extra cell would throw off the rest of the row.

## `<caption>`

`<caption>` provides a title for the table, rendered above it by default. Like `<th>`, it's a semantic element (announced as the table's caption by screen readers), not just a styling convenience.
