# A Real-World Example: Accounting Notes (Fixed)

The exact same page as the previous lesson, with the same two changes from lessons 120/121 applied: `.html()` replaced with `.val()`, and note text passed through `sanitizeText()` before it's ever written anywhere - including before it's saved to storage.

```javascript
function sanitizeText(text) {
    // Escape the following characters (< > & ' ")
    text = text.replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/&/g, "&amp;")
        .replace(/'/g, "&#39;")
        .replace(/"/g, "&quot;");
    return text;
}

function addNoteText() {
    ...
    var updated = $("#notes").val() + ... + sanitizeText($("#noteText").val());
    $("#notes").val(updated);
    localStorage.setItem(storageKey, updated);
    ...
}
```

## Safe Even If Storage Isn't

On load, this version reads back whatever was last saved with `.val()`:

```javascript
var stored = localStorage.getItem(storageKey);
if (stored !== null) $("#notes").val(stored);
```

Notice what this doesn't do: it doesn't check whether the stored value looks safe before displaying it. It doesn't need to - `.val()` never asks the browser to parse its argument as markup, so reading back a value that arrived completely unsanitized is exactly as safe as reading back one that was properly escaped on the way in. You'll see this for yourself in the next lesson: both versions get fed the identical raw payload, straight into storage, with no escaping applied by that page at all.

That's the real lesson here. Safety comes from *how a page displays the data it's given*, not from trusting *where that data came from*. In a real application, plenty of unsafe data can end up in storage through paths that have nothing to do with this page's own `addNoteText()` function - a different page, a different form, an API, a migration script. A page that's only safe because it assumes its own inputs were clean isn't actually safe.

See the next lesson for a hands-on, side-by-side way to see both versions' persistence behavior at once.
