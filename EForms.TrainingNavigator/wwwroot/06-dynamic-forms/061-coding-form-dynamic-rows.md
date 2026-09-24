# GL Coding Form - True Dynamic Rows

A more flexible alternative to the pseudo-dynamic approach: instead of pre-building every possible row, this version starts with just one and genuinely creates new DOM nodes - complete with their own real `OBKey__###_#` fields - each time the user clicks "Add Row".

## Why This Still Satisfies OnBase

It might seem like OnBase's requirement for keyword fields to "exist in the form" would rule this out - but even though OnBase cannot merge the existing values at load-time, we store the serialized row data in a hidden field (`mikgData`) and rebuild the table from that on every add/remove. The `OBKey_` fields are still present in the DOM when the form is submitted, so OnBase sees them and saves their values just fine.

## Building a Row

```javascript
function displayRow(rowNum, rowData) {
    $("#mikgBody").append(
        "<tr id=\"gl" + rowNum + "\" class=\"dyn-row\">" +
        "<td><input type=\"text\" name=\"OBKey__384_" + rowNum + "\" ... value=\"" + sanitizeText(rowData.kwPoNumber) + "\" /></td>" +
        ...
    );
}
```

Each new row gets its own delete button (`btnDeleteRow<N>`), wired up right after the row is inserted. Deleting a row doesn't just remove it - `removeRow()` calls `loadDynTable()` again afterward, which rebuilds every remaining row from scratch so the instance numbers stay sequential (1, 2, 3, ...) with no gaps, since a gap in MIKG instance numbers would leave the remaining data in a state OnBase (and this form's own validation) doesn't expect.

## The Hidden `mikgData` Field

Because rows can be added and removed freely, the script needs some way to remember exactly which rows exist and what they contain - both across a save/reload cycle, and to rebuild the table after a delete. `storeMikgData()` serializes the current rows to a JSON array, base64-encodes it, and stores it in a hidden `<input name="mikgData">`; `loadMikgData()` reverses that on load. This shadow copy is what makes `loadDynTable()`'s "rebuild everything" strategy possible without needing to inspect the actual `OBKey_` fields to figure out how many rows there were.

## Debug Panels

Unlike the pseudo-dynamic version, this lesson includes two visible (in `debugMode`) panels: a live JSON dump of the current `mikgData` array, and a running log of any caught errors. Genuinely useful while building or troubleshooting a dynamic-row form - you can watch the underlying data structure change in real time as you add, edit, and remove rows, rather than only inferring it from the visible fields.

## Invoice Approvals: Name Fields Editable, Dates Stamped on Save

As with the GL Coding Form (Validation and Computation) lesson earlier in this chapter, `kwRequestedBy` and `kwApprovedBy` are editable here rather than `readonly` - on a real OnBase form these would normally be `readonly`, since the name comes from the current OnBase user automatically, but this training demo has no real OnBase session driving that, so both stay editable to let you try out both the requester and approver flows yourself. The two Date fields stay `readonly`, and are only stamped with today's date by `populateApprovalDates()` once validation passes and their paired Name field actually has a value - never automatically on page load, and never overwriting a date that's already been recorded.

## Why Submitting the Form Doesn't Actually Submit Anywhere

The hidden `btnSaveAfterValidation` button really is a `type="submit"` - clicking it after validation passes triggers a genuine HTML form submission, POSTing to `action="#"`. That's fine inside OnBase, where a real endpoint handles the save, but this training file has no backend of its own. Depending on how the file happens to be hosted, letting that real POST go through can fail outright - for example, IIS's static file handler rejects a POST to a plain `.html` file with a 405 "Method Not Allowed" error. The script intercepts the form's `submit` event directly and shows a simple confirmation instead, so the demo behaves consistently no matter how (or where) the file is actually opened.
