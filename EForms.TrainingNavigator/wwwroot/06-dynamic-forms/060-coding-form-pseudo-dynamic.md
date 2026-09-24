# GL Coding Form - Pseudo-Dynamic Rows

The first of three approaches to letting a user add and remove GL distribution rows on a form. This one is built around a real OnBase constraint: keyword fields need to physically exist in the form's HTML for OnBase to map them, so this version simply **pre-builds all 10 possible rows** in the static markup, and uses JavaScript only to show and hide them.

## The Core Idea

Every row (`kwPoNumber1` through `kwPoNumber10`, and so on) already exists in the HTML from the moment the page loads - nothing is ever created or destroyed. `loadDynTable()` hides every `.dyn-row`, then re-shows exactly as many as have actual data in them (row 1 always shows; any later row shows if any of its MIKG fields is non-blank):

```javascript
function loadDynTable() {
    $(".dyn-row").hide();
    rows = 1;
    for (var r = 1; r <= maxRows; r++) {
        // row 1 always shows; later rows show only if they already have data
    }
}
```

`addRow()` and `removeRow()` are correspondingly simple - just `show()`/`hide()` on the next row in sequence, with `removeRow()` also clearing that row's values so no stale data lingers if the user later re-adds a row.

## Trade-offs

- **Simple** - no HTML string-building, no re-numbering logic, nothing to keep in sync.
- **Hard-capped** - `maxRows` (10 here) is a real ceiling; there's no way to add an 11th row without changing the HTML itself.
- **All the fields exist from page load** - meaning OnBase's requirement that keyword fields be present in the form is satisfied trivially, by construction, rather than needing any special handling at save time.

## A Development Convenience: `testMode`

Set `testMode = true` and the form auto-populates itself with sample data (`fillTestData()`) on load and after every reset - handy while actively developing or demoing a form, since you're not stuck re-typing the same values every time you reload the page. Just remember to set it back to `false` before deploying the form for real use.

## Invoice Approvals: Name Fields Editable, Dates Stamped on Save

As with the GL Coding Form (Validation and Computation) lesson earlier in this chapter, `kwRequestedBy` and `kwApprovedBy` are editable here rather than `readonly` - on a real OnBase form these would normally be `readonly`, since the name comes from the current OnBase user automatically, but this training demo has no real OnBase session driving that, so both stay editable to let you try out both the requester and approver flows yourself. The two Date fields stay `readonly`, and are only stamped with today's date by `populateApprovalDates()` once validation passes and their paired Name field actually has a value - never automatically on page load, and never overwriting a date that's already been recorded.

## Why Submitting the Form Doesn't Actually Submit Anywhere

The hidden `btnSaveAfterValidation` button really is a `type="submit"` - clicking it after validation passes triggers a genuine HTML form submission, POSTing to `action="#"`. That's fine inside OnBase, where a real endpoint handles the save, but this training file has no backend of its own. Depending on how the file happens to be hosted, letting that real POST go through can fail outright - for example, IIS's static file handler rejects a POST to a plain `.html` file with a 405 "Method Not Allowed" error. The script intercepts the form's `submit` event directly and shows a simple confirmation instead, so the demo behaves consistently no matter how (or where) the file is actually opened.

See the next two lessons for approaches without the hardcoded row cap.
