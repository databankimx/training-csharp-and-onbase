# GL Coding Form - Validation and Computation

A complete, realistic invoice-coding form that puts the previous two lessons' fundamentals to work: required-field checks, date format validation, currency formatting, a running total that must reconcile against the invoice amount, and a small notes feature - all driven from one save button rather than the form's native `submit` event.

## Driving Validation from a Button, Not `submit`

```javascript
$("#btnSave").on("click", function () {
    if (validateRequiredFields() && validateDates() && validateCurrency() && validateMikgRows() && validateTotal()) {
        populateApprovalDates();
        $("#btnSaveAfterValidation").click();
    }
});
```

The visible "Save" button is a plain `type="button"` - it only *looks* like the real submit control. The actual OnBase-recognized submit button (`name="OBBtn_Save"`) is hidden (`class="hidden"`) and only gets clicked programmatically once every validation function returns `true`. This is why the form's own `submit` event handler is commented out in the source rather than used directly - driving everything from one click handler keeps the order of validation explicit and easy to follow, rather than relying on event bubbling.

## The Validation Functions

- **`validateRequiredFields()`** - checks every field marked `data-required="true"`, highlights the first empty one, and stops there rather than listing every problem at once.
- **`validateDates()`** - checks `.date` fields against a `MM/DD/YYYY` regex, clearing and re-selecting an invalid value.
- **`validateMikgRows()`** - enforces "all or nothing" on each GL distribution row (a Multi-Instance Keyword Group): if any one of PO Number/GL Code/GL Amount/GL Description is filled in, all four must be.
- **`validateCurrency()`** - normalizes every `.currency` field to a consistent formatted value (or clears it if it evaluates to zero).
- **`validateTotal()`** - sums the GL distribution line amounts and confirms they equal the invoice amount, refusing to save otherwise.

## Live Currency Formatting and Running Total

On `blur`, every `.currency` field is reformatted through `formatAsCurrency()`, and the distribution lines' running total is recalculated and compared against the invoice amount - turning the total field green (`.ok`) when they match, red (`.attention`) when they don't. This gives the user live feedback well before they ever click Save.

```javascript
function formatAsCurrency(number) {
    if (typeof Intl.NumberFormat !== "undefined") {
        var formatter = new Intl.NumberFormat(culture, { style: "currency", currency: money });
        return formatter.format(currencyToFloat(number));
    }
    return formatMoney(currencyToFloat(number));
}
```

`Intl.NumberFormat` is the modern, locale-aware way to format currency; `formatMoney()` is kept as a manual fallback for the rare case it's unavailable.

## The Notes Feature, Safely

Clicking "Add Note" opens a small modal (`#note-modal`); its text is appended to the read-only notes `<textarea>` using `.val()` (setting the *value*, not the markup) and passed through `sanitizeText()` first:

```javascript
function sanitizeText(text) {
    return text.replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/&/g, "&amp;")
        .replace(/'/g, "&#39;")
        .replace(/"/g, "&quot;");
}
```

This combination - `.val()` instead of `.html()`, plus escaping the handful of HTML-significant characters before storing user-entered text - is exactly what keeps this feature safe from script injection. It's worth remembering precisely, since a nearly-identical form exists (`123-coding-form-xss-vulnerable`, in the Avoiding XSS chapter) that changes just these two things and becomes exploitable.

## Auto-Filling the Requester Name, and Stamping Approval Dates on Save

On load, the Requester Confirmation name fills itself in automatically, but only if it's still empty:

```javascript
if ($("#kwRequestedBy").val() === "") $("#kwRequestedBy").val($("#propUserRealName").val());
```

`$("#propUserRealName")` is a hidden field carrying `OBProperty_CurrentUserRealName` - the OnBase property covered back in the OnBase Properties lesson. Checking for an empty value first matters because this same form gets reopened for approval later; without that check, reopening it would silently overwrite whoever originally requested it with whoever happens to be viewing it now.

> **Why the Name fields aren't `readonly` here:** on a real OnBase form, `kwRequestedBy` and `kwApprovedBy` would normally be `readonly`, since the name comes automatically from the current OnBase user rather than being typed in. In this training demo, though, there's no actual OnBase session driving who's "logged in" - so both fields are left editable, letting you type in a name yourself to try out both the requester and approver flows without needing to actually switch OnBase users in between.

The Date fields are different: both are `readonly`, and neither one fills in on page load at all. Instead, once every validation check has passed - right before the hidden submit button is clicked - `populateApprovalDates()` stamps each date, but only for a Name/Date pair that actually has a name recorded, and only if that date isn't already set:

```javascript
function populateApprovalDates() {
    if ($("#kwRequestedBy").val() !== "" && $("#kwRequestDate").val() === "") {
        $("#kwRequestDate").val(new Date().toLocaleDateString(culture));
    }
    if ($("#kwApprovedBy").val() !== "" && $("#kwApprovedDate").val() === "") {
        $("#kwApprovedDate").val(new Date().toLocaleDateString(culture));
    }
}
```

This keeps a date field meaningful as an actual record of *when that approval step happened*, rather than just *when the page happened to load* - and the "only if empty" check means saving the form a second time never overwrites an approval date that's already been recorded.

## `jQuery UI`

This lesson pulls in jQuery UI (`$(".date").datepicker()`) purely for its date-picker widget on the invoice date and any other `.date` field - a small addition worth knowing exists if you need a calendar picker rather than requiring users to type dates by hand.

## Debug Logging

Every function in this script wraps its logic in `try`/`catch` and reports through `traceLog()`/`errorLog()` rather than scattering raw `console.log()`/`alert()` calls throughout:

```javascript
var debugMode = true;
var debugAlert = false;

function traceLog(message) {
    if (!debugMode) return;
    log(message);
}

function errorLog(message) {
    log("*** ERROR ***\n" + message);
}

function log(message) {
    if (typeof console !== "undefined") console.log(message);
    if (debugAlert) alert(message);
}
```

Two flags control the behavior: `debugMode` gates routine trace messages (set it `false` once a form is finished and ready for production, and every `traceLog()` call goes silent with no code changes needed elsewhere), while `debugAlert` optionally routes messages through `alert()` as well as the console - useful in exactly the situation the Control Flow lesson called out: testing a form live inside OnBase, where the console is never reachable but an `alert()` still is.
