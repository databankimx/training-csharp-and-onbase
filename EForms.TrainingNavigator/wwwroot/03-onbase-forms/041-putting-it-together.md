# Putting It All Together - a GL Coding Form

A capstone example, combining everything from this chapter into the shell of a realistic OnBase form: an invoice-coding form with three logical sections (`<header>` for invoice info, `<main>` for GL distribution, `<footer>` for accounting notes), each grouped in its own `<fieldset>`/`<legend>`.

## The Wrapper Page

`041-putting-it-together.html` itself is just a thin wrapper - it embeds the real form (`coding-form-basic.html`) in an `<iframe>`, the same pattern used to view any lesson in this navigator.

## A Basic, Working Starting Point

The "Invoice Information" and "GL Distribution" fieldsets are now populated with real keyword fields, applying the mapping patterns covered earlier in this chapter - including two rows of a Multi-Instance Keyword Group in the GL Distribution table. This is still deliberately basic, though: no validation, no computed totals, no ability to add more distribution rows dynamically. Look ahead to the GL Coding Form (Validation and Computation) lesson, in the Scripting and Validation chapter, to see this same form with all of that added.

## `coding-form.js`

The accompanying script is intentionally minimal: it hides the `.debug` section unless a `debug` flag is set to `true`, plus small `log()`/`logError()` helpers that write to both the browser console and (when `debug` is on) the visible Debug Information textarea. This is a lightweight, reusable debug-logging pattern worth reusing in your own forms during development - flip `debug = true` while building/testing, then back to `false` before deploying.
