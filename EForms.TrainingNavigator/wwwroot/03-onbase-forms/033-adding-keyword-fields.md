# Adding Keyword Fields

Building on the naming requirements from the previous lesson, this covers the different HTML input types you'll actually use to capture keyword values, depending on what kind of value a keyword expects.

## Text Values

Plain `<input type="text">`, named with either the by-name or by-ID `OBKey_` format from the previous lesson.

> **Auto-numbering note:** If a keyword is configured for auto-numbering in OnBase, the number is incremented whenever the form is submitted - **whether or not the user actually saves it**. A cancelled or failed save still consumes a number, which can leave gaps in the sequence. This is an OnBase configuration behavior, not something the form itself controls.

## Limited-Choice Values: Radio Buttons and Checkboxes

For a keyword with a small, fixed set of valid values, radio buttons or checkboxes are a natural fit - they prevent the user from entering anything invalid in the first place. Both still follow the same `OBKey_`/`OBKey__###_#` naming rules as a text field; the `value` attribute on each option is what actually gets submitted as the keyword's value.

## Limited-Choice Values: Select Lists

A `<select>` works the same way - each `<option>`'s `value` is the keyword value that gets submitted.

## Dataset-Backed Select Lists: `OBDataset`

For a keyword configured against an OnBase **Dataset** (a managed list of valid values maintained in OnBase itself, rather than hardcoded into the form), use `OBDataset__###_#` instead of `OBKey__###_#` as the field's `name`. The `<select>`'s own `<option>`s can be left mostly empty in the HTML - OnBase populates the actual dataset values dynamically at runtime, rather than you hand-coding every possible value into the form.

## Long Values: `<textarea>`

For a keyword that can hold a large amount of text, a `<textarea>` works exactly like any other keyword field - same naming rules apply.

## Non-Keyword Fields

A field's `name` doesn't have to start with `OBKey`/`OBDataset` at all - an ordinary field with any other name is simply **not** mapped to a keyword, and is available to your own form logic (JavaScript, or downstream processing) without ever being written to OnBase. 

**Non-keyword fields are not supported on virtual e-forms, user forms, or custom-query forms** - those form types have their own restrictions on what fields you can add, which the corresponding later lessons in this chapter cover.

## Input Types OnBase Doesn't Support

Not every HTML5 input type from the Fundamentals chapter works as a keyword field. The following types are not (yet) supported by OnBase, and silently fall back to behaving like `type="text"`:

`color`, `datetime-local`, `file`, `image`, `month`, `number`, `range`, `search`, `time`, `week`

Stick to the types demonstrated in this lesson (plus `date`, `email`, `tel`, and `url`, which OnBase does support) for keyword fields.

