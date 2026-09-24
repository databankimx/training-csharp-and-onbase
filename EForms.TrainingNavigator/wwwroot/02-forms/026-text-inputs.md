# Text Form Inputs

## Common Attributes on `<input type="text">`

- `autofocus` - moves the cursor to this field automatically when the page loads
- `autocomplete="off"` - tells the browser not to offer previously-entered values for this field
- `maxlength` - caps how many characters can be entered
- `value` - pre-populates a default value
- `readonly` - the value is shown but can't be edited (and, unlike `disabled`, **is** still submitted with the form)
- `disabled` - the field is greyed out, uneditable, and **not** submitted with the form at all
- `placeholder` - light, non-submitted hint text shown only while the field is empty
- `required` - marks the field as required; supported inconsistently across browsers, and **not honored by OnBase forms at all**, so don't rely on it there

## Labels

```html
<label for="lblInput">Labeled Input:</label>
<input type="text" name="lblInput" id="lblInput">
```

A `<label>`'s `for` attribute should match the input's `id`. This isn't just cosmetic - it lets screen readers announce the label when the field receives focus, and clicking the label text focuses (or toggles, for checkboxes/radios) the associated input.

## Hidden and Password Fields

- `type="hidden"` - not rendered at all, but still submitted with the form; used to pass data the user doesn't need to see or edit
- `type="password"` - renders like a text field, but masks the characters typed

## `<textarea>`

A multi-line, scrollable text box. `cols`/`rows` set its size in character columns/rows, but CSS `width`/`height` are generally preferred today - sizing by character count doesn't account for different fonts or font sizes the way pixel/CSS units do.
