"use strict";

// The extra script that makes this page different from 120-xss-vulnerable.html: it announces
// that the visitor didn't get here on purpose - they were redirected by a script that ran on
// the vulnerable page, exactly the kind of thing a real attack payload could do instead of
// just popping an alert.
$(document).ready(function () {
    $("#redirectNotice").html(
        "<h3>You Were Just Redirected</h3>" +
        "<p>You didn't navigate here on purpose - the link you clicked ran a script that sent your " +
        "browser to this page automatically, without asking first. In a real attack, this page could " +
        "have been anything: a convincing fake login form, a malware download, or a page built to look " +
        "exactly like the one you meant to visit.</p>"
    );
});
