# Select Lists

## `<select>` / `<option>`

`<select>` creates a drop-down list; each `<option>` inside it is one choice. Add `selected` to an `<option>` to pre-select it when the form loads:

```html
<select id="select" name="select">
    <option value="1">One</option>
    <option value="2" selected="selected">Two</option>
</select>
```

The `size` attribute controls how many options are visible at once without opening the dropdown - set high enough, it turns the control into a scrollable list box instead of a traditional dropdown.

## `<optgroup>`

Groups related `<option>`s under a shared, non-selectable heading (`label`), useful for organizing a long list into categories:

```html
<optgroup label="Odd Numbers">
    <option value="1">One</option>
    <option value="3">Three</option>
</optgroup>
```

## `<select>` vs. `<datalist>`

Both offer a list of choices, but they behave differently:

- `<select>` **restricts** the user to exactly the listed options - nothing else can be submitted.
- `<datalist>` (paired with a text `<input>` via its `list` attribute) only **suggests** values as the user types; they can still type something not on the list.

An `<option>`'s `value` attribute is optional when the displayed text and the submitted value should be identical - omitting it just submits the option's own visible text.
