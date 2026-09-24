# Seeing It Happen: Persistent XSS in the Notes Field

## Reflected vs. Persistent, Side by Side

The demo in `122-xss-test-links` showed reflected XSS: load a crafted URL, the payload runs, and that's it - reload without the URL, and it's gone. This demo is different, and needs no query string or preset buttons to show it: each frame below is the real page from the last two lessons, complete with its own **Add Note** button.

Type a note into the vulnerable frame containing `<script>alert('I injected JavaScript!');</script>`, click that frame's own **Add Note**, and the alert fires. Now click that frame's **Reload** button. It fires again - with nothing re-typed and no crafted link involved, because the note was saved to storage the first time and gets written straight back into the page, and re-executed, on every load from now on. Do the same thing in the fixed frame, and the note just displays as plain text, reload after reload.

## Why This Matters More in Practice

A reflected payload only affects the one person who clicks a specific crafted link. A stored payload sits in the data and runs for *everyone* who loads the page - every visitor, every session, indefinitely - until someone notices and removes it. That's what makes this category of bug, in real applications, generally considered more dangerous than reflected XSS: the attacker doesn't need to trick anyone into clicking anything after the note gets saved once.

## The Full Original Article

The complete original "Preventing Cross-Site Scripting (XSS)" article this whole chapter is based on is linked from `122-xss-test-links` - see that lesson for the link to the PDF.
