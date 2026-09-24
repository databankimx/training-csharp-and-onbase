# Seeing It Happen: Vulnerable vs. Fixed, Side by Side

The two child lessons nested under this one show the vulnerable and fixed pages in isolation. This lesson puts them side by side in two iframes, both driven by the same `myData` value at once - click "Load Malicious Script" and watch the exact same payload run in the left frame and sit there as inert text in the right one. No cross-frame scripting is needed for this: each iframe just reads its own URL's query string independently, so the parent page only has to set both `src` attributes to the same value.

## How This Actually Reaches Someone

Typing a crafted URL into the address bar, like the demo above, isn't how this usually happens in practice - a reflected payload almost always arrives as an ordinary-looking link in an email instead. The "Download a Demo Email (.eml)" button generates a real email file on the spot, pointed at wherever this training happens to be running right now, and opens the door to a third child lesson: **XSS Example (Malicious)**.

That page is identical to XSS Example (Vulnerable) - same markup, same unsanitized `myData` handling. What's different is the payload in the email link itself: instead of popping an alert, it redirects the browser (via `window.location`) to that malicious page once it runs on the vulnerable page - a more honest picture of what this vulnerability actually enables. An attacker doesn't just get to show you a message, they get to decide where your browser ends up next.

## The Full Original Article

This entire chapter is based on an internal article, "Preventing Cross-Site Scripting (XSS)" - read it here:  
[Preventing Cross-Site Scripting.pdf](./12-avoiding-xss/Preventing%20Cross-Site%20Scripting.pdf)

The original article included a zip file with the complete, runnable example HTML pages (both the vulnerable and fixed versions) - this training set's own `120-xss-vulnerable.html` and `121-xss-fixed.html` lessons are built from that same example code.
