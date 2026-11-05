# A Real-World Example: Accounting Notes (Vulnerable)

The same class of vulnerability from the first two lessons, but found in a form you've already worked with, and with a twist: this time it *persists*.

This is the notes section from the GL Coding Form covered in the Scripting and Validation chapter (see `044-coding-form-validation`), pulled out on its own - invoice header, GL distribution, and approvals all stripped away, leaving just the notes fieldset and its "Add Note" flow.

## Reflected vs. Persistent

Lessons 120-122 demonstrated **reflected** XSS: the payload lived in a crafted URL, and only ran when someone opened that specific link. This page demonstrates **persistent** (also called **stored**) XSS: the payload is saved - here, to the browser's `localStorage`, standing in for a database in a real application - and gets written back into the page, and re-executed, every single time the page loads. No crafted link required at all.

```javascript
$(document).ready(function () {
    var stored = localStorage.getItem(storageKey);
    // Whatever was last saved gets written straight back into the DOM with .html() - if a
    // malicious note was ever saved, it re-executes on every single page load, not just once.
    if (stored !== null) $("#notes").html(stored);
    ...
});

function addNoteText() {
    ...
    var updated = $("#notes").html() + ... + $("#noteText").val();
    $("#notes").html(updated);
    localStorage.setItem(storageKey, updated);
    ...
}
```

Both the initial page load and every new note share the same mistake: raw text goes straight into `.html()`, and that same raw text goes straight into storage, with nothing sanitized anywhere in between. Type a note containing `<script>alert('I injected JavaScript!');</script>` and it doesn't just run once - reload the page, and it runs again. Close the tab and come back tomorrow, and it still runs.

## Why This Matters More Than the Reflected Version

A reflected payload only affects the one person who clicks a specific crafted link. A stored payload sits in the data and runs for *everyone* who loads the page - every visitor, every session, indefinitely - until someone notices and removes it. That's what makes this category of bug, in real applications, generally considered more dangerous than reflected XSS: the attacker doesn't need to trick anyone into clicking anything after the initial note gets saved once.

## The Fix

See the next lesson for the fixed version - the same `sanitizeText()` and `.val()` changes from lessons 120/121, applied here - and the lesson after that for a hands-on, side-by-side comparison of both versions' persistence behavior.
