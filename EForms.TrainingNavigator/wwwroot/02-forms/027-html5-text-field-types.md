# HTML5 Text Field Types

HTML5 added a number of specialized `<input>` types beyond plain `text`. Where a browser (or OnBase) doesn't support a given type, it falls back to rendering it as a plain `type="text"` field instead - the form still works, it just loses the specialized UI and any built-in validation for that type.

- `email` - validates a basic email shape; add `multiple` to accept a comma-separated list of addresses
- `search` - styled as a search box (often with a built-in clear button)
- `tel` - no format validation (phone formats vary too much worldwide), but triggers a numeric keypad on mobile
- `url` - validates a basic URL shape
- `number` - a numeric spinner; `min`/`max`/`step` constrain the allowed range and increment
- `range` - a slider; same `min`/`max`/`step` attributes as `number`. Pairing it with a `<datalist>` (via the `list` attribute) adds visible tick marks - not supported in every browser (Firefox notably doesn't show them)
- `date` / `month` / `week` / `time` / `datetime-local` - native date/time pickers; `date` also supports `min`/`max` to constrain the selectable range
- `color` - a native color picker
- `file` - a file upload control

## `<datalist>`

A `<datalist>` provides a set of suggested values for a paired input (linked via the input's `list` attribute matching the datalist's own `id`). Unlike `<select>`, it doesn't restrict the input to only those values - the user can still type anything else.
