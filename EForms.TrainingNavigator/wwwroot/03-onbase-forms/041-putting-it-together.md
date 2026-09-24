# Putting It All Together - a GL Coding Form

A capstone example, combining everything from this chapter into a realistic OnBase invoice-coding form: three logical sections (`<header>` for invoice info, `<main>` for GL distribution, `<footer>` for accounting notes), each grouped in its own `<fieldset>`/`<legend>`.

## A Basic, Working Starting Point

The "Invoice Information" and "GL Distribution" fieldsets are populated with real keyword fields, applying the mapping patterns covered earlier in this chapter - including two rows of a Multi-Instance Keyword Group in the GL Distribution table. This is still deliberately basic: no validation, no computed totals, no ability to add more distribution rows dynamically. Look ahead to the GL Coding Form (Validation and Computation) lesson, in the Scripting and Validation chapter, to see this same form with all of that added.

## Save, Reset, and Cancel

This form uses the `OBBtn_Save` / `btnReset` / `OBBtn_Cancel` naming convention covered in OnBase Form Buttons, and the same shared submit-preview behavior (`scripts/lessons.js`) used throughout this chapter: clicking Save intercepts the form's submit and shows exactly what would be posted to OnBase, in the same popup already seen on the other lessons in this chapter. Clicking Cancel confirms and resets the form instead of submitting it.

## Styling

This page pulls in DataBank's own branding stylesheets (`databank-colors.css`, `databank-icons.css`, `databank-logos.css`) alongside the shared `coding-form-base.css` and this chapter's own `coding-form.css` - the same look used by the other GL Coding Form lessons later in the training, rather than the simpler `lessons.css` styling used elsewhere in this chapter. The `#data-modal` popup's own styling is copied directly into `coding-form.css` rather than pulling in all of `lessons.css`, since that stylesheet's body/font rules would otherwise conflict with this page's look.
