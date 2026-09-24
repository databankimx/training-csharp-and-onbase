# Form Buttons

There are two different HTML elements you can use to make a button, and several relevant type/behavior variations:

## `<button>`

A generic button element, not inherently tied to submitting or resetting anything. Its default `type` is actually `submit` when placed inside a `<form>` - a common source of accidental form submissions if you meant it to be a plain button. This lesson's own click handler prevents the default action either way, so it doesn't demonstrate that particular pitfall directly, but it's worth knowing.

## `<input type="...">`

- `type="button"` - a plain button with no built-in behavior of its own; still included in the form data on submit (unlike `<button>`, which submits the *button's* own name/value only if it's the one clicked)
- `type="reset"` - resets every field in the form back to the values it had when the page loaded (not to empty)
- `type="submit"` - submits the form to its `action` URL

## Multiple Submit Buttons

A form can have more than one submit button, each optionally overriding the form's own `method`/`action` for that specific submission via `formmethod`/`formaction`:

```html
<input type="submit" value="SUBMIT 2" formmethod="get" formaction="#">
```

This is useful when a form needs more than one possible outcome from the same page - e.g. "Save Draft" vs. "Submit for Approval," each posting to a different handler.
