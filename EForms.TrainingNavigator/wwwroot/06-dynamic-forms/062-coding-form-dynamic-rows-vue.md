# GL Coding Form - Dynamic Rows With Vue.js

The same true-dynamic-rows idea as the previous lesson, rebuilt with Vue.js instead of manual jQuery DOM manipulation - a look at what changes when a reactive framework does the rendering instead of hand-written string concatenation.

## Declarative Rendering With `v-for`

Instead of building row HTML as a string, the row template lives directly in the page's own markup, repeated once per item in a `lines` array:

```html
<tr v-for="(line, index) in lines">
    <td><input v-bind:name="'OBKey__384_' + index" v-model="line.poNumber"></td>
    ...
</tr>
```

`v-model` two-way binds each input to a property on that row's object - type into the field, and `line.poNumber` updates automatically; change `line.poNumber` in code, and the field updates too. Adding or removing a row is just mutating the `lines` array (`lines.push(...)`, `lines.splice(index, 1)`) - Vue handles adding or removing the actual DOM elements itself.

> **Validation note:** `v-for`, `v-bind`, `v-model`, and the other Vue attributes won't pass standard HTML validation, and the `for`/`id` values they reference don't exist yet at the time a validator reads the raw source. This is expected - none of these attributes exist in the final rendered HTML at all; Vue consumes and removes them when it processes the template.

## A Custom Component for Currency Input: `my-currency-input`

The most interesting piece of this lesson is a small custom Vue component that solves a real UX problem: you want a currency field to *display* formatted (`$1,234.56`) but you don't want that formatting fighting the user while they're actively typing in it.

```javascript
computed: {
    displayValue: {
        get: function () {
            if (this.isInputActive) return this.value.toString();  // raw while focused
            return formatAsCurrency(this.value);                    // formatted once blurred
        },
        set: function (modifiedValue) {
            var newValue = parseFloat(modifiedValue.replace(/[^\d.]/g, ""));
            this.$emit('input', isNaN(newValue) ? 0 : newValue);
        }
    }
}
```

A computed property's `get`/`set` pair lets the component show one thing and store another: focused, the user sees and edits the raw number; blurred, they see the formatted currency string - all without any manual DOM manipulation on this lesson's part, just declaring the behavior and letting Vue react to it.

One Vue-specific detail worth knowing: a component can't reassign its own `value` prop directly (props flow one direction, parent to child) - instead it `$emit`s an `input` event with the new value, which `v-model` on the parent's side picks up and applies. This event-emission pattern is how data flows back *up* out of a Vue component, mirroring how it flows *down* through props.

## Same Persistence Strategy as the Plain-JS Version

Like the previous lesson, this one serializes its row data to base64-encoded JSON in a hidden field (`#glLines`) - but only on save (`saveDynamicRows()`, called from `validateRequiredFields()`), rather than after every single add/remove. Vue's own reactive `lines` array is the live source of truth while the form is open; the hidden field only needs to reflect it at the moment of submission.

## Comparing All Three Approaches

| | Pseudo-Dynamic | True Dynamic Rows (jQuery) | True Dynamic Rows (Vue) |
|---|---|---|---|
| **Row limit** | Hard-capped (`maxRows`) | None | None |
| **How rows are created** | Pre-built in static HTML, shown/hidden | Built as HTML strings, appended to the DOM | Declared once as a template, repeated via `v-for` |
| **Code to add a row** | One `show()` call | ~20 lines of string-built HTML plus event binding | One `lines.push(...)` |
| **Renumbering on delete** | Not needed (fixed slots) | Manual - rebuilds the whole table | Automatic - Vue re-renders from the array |
| **Extra dependency** | None beyond jQuery | None beyond jQuery | Vue.js |
| **Best fit** | A form with a known, small maximum number of rows, where simplicity matters most | A form needing real flexibility without introducing a new framework | A form with enough dynamic complexity (like the currency component here) that a reactive framework's benefits outweigh the added dependency |

None of these is universally "correct" - the pseudo-dynamic version's simplicity is a genuine advantage when 10 rows is truly always enough, and the jump to Vue is only worth it once a form's dynamic behavior gets complex enough that manual DOM manipulation becomes the harder path, not the easier one.

## Invoice Approvals: Name Fields Editable, Dates Stamped on Save

As with the other two lessons in this chapter, `kwRequestedBy` and `kwApprovedBy` are editable here rather than `readonly` - on a real OnBase form these would normally be `readonly`, since the name comes from the current OnBase user automatically, but this training demo has no real OnBase session driving that, so both stay editable to let you try out both the requester and approver flows yourself. The two Date fields stay `readonly`, and are only stamped with today's date by `populateApprovalDates()` once validation passes and their paired Name field actually has a value - never automatically on page load, and never overwriting a date that's already been recorded.

## Why Submitting the Form Doesn't Actually Submit Anywhere

The hidden `btnSaveAfterValidation` button really is a `type="submit"` - clicking it after validation passes triggers a genuine HTML form submission, POSTing to `action="#"`. That's fine inside OnBase, where a real endpoint handles the save, but this training file has no backend of its own. Depending on how the file happens to be hosted, letting that real POST go through can fail outright - for example, IIS's static file handler rejects a POST to a plain `.html` file with a 405 "Method Not Allowed" error. The script intercepts the form's `submit` event directly and shows a simple confirmation instead, so the demo behaves consistently no matter how (or where) the file is actually opened.

