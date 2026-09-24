# Radio Buttons and Checkboxes

## Checkboxes

Each checkbox is independent - any number can be checked at once. Because there's no built-in grouping relationship, give each one both a unique `name` and a unique `id`:

```html
<input type="checkbox" name="cb_1" id="cb_1" value="1">
```

## Radio Buttons

Radio buttons work the opposite way - only one option in a group can be selected at a time. That grouping is defined by giving every radio button in the group the **same** `name` (with each still needing its own unique `id`):

```html
<input type="radio" name="rad" id="rad_1" value="1">
<input type="radio" name="rad" id="rad_2" value="2">
```

## Both Need `value`

Unlike a text input, the user can't type into a checkbox or radio button - so both require an explicit `value` attribute; without one, there's nothing meaningful to submit when the field is checked.

## `checked`

Adding the `checked` attribute pre-selects that checkbox or radio button when the form loads.

## `<fieldset>` and `<legend>`

`<fieldset>` logically (and usually visually) groups related form controls together; `<legend>` provides that group's own title, rendered inline with the fieldset's border by default.
