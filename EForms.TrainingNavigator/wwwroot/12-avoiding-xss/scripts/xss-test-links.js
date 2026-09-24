"use strict";

$(document).ready(function () {
    $("#btnLoadSafe").on("click", function () {
        loadValue("something safe");
    });

    $("#btnLoadScript").on("click", function () {
        loadValue("<script>alert('I injected JavaScript!');</script>");
    });

    $("#btnLoadCustom").on("click", function () {
        loadValue($("#customValue").val());
    });

    // Reloading via contentWindow.location.reload() - rather than re-assigning .src to
    // its own current value - guarantees an actual reload even when the URL hasn't
    // changed, which is exactly the case right after the iframe first loads
    $(".reload").on("click", function () {
        var frame = document.getElementById($(this).data("target"));
        frame.contentWindow.location.reload();
    });

    $("#btnDownloadEml").on("click", downloadDemoEmail);
});

// Set both iframes' query string to the same value at once - no cross-frame scripting
// needed, since both pages read their own myData independently from their own URL
function loadValue(value) {
    var encoded = encodeURIComponent(value);
    document.getElementById("frameVulnerable").src = "120-xss-vulnerable.html?myData=" + encoded;
    document.getElementById("frameFixed").src = "121-xss-fixed.html?myData=" + encoded;
}

// Builds and downloads a .eml demonstrating the email delivery vector: the link points at
// the vulnerable page with a payload that redirects (via window.location) to
// 126-xss-malicious.html - a much more realistic consequence than a plain alert, since a
// real attacker controls where the victim ends up, not just what pops up on screen.
//
// The link is resolved from this page's own document.baseURI (set by the navigator to this
// file's real, current address) rather than window.location - this page runs inside a
// srcdoc iframe when viewed through the navigator, where window.location doesn't reflect
// the lesson's actual URL, but document.baseURI does. That's what makes the generated link
// point at wherever the site is actually running right now, dev port or production
// hostname alike, instead of a hardcoded address that goes stale the moment anything changes.
function downloadDemoEmail() {
    var payload = "<script>window.location='126-xss-malicious.html';<\/script>";
    var targetUrl = new URL("120-xss-vulnerable.html?myData=" + encodeURIComponent(payload), document.baseURI).href;

    var body =
        "<html><body>" +
        "<p><a href=\"" + targetUrl + "\">Unsafe Link</a></p>" +
        "<p>Sincerely,<br>Scott McLean<br>Manager - Development Team</p>" +
        "</body></html>";

    var eml =
        "From: Scott McLean <smclean@databankimx.com>\r\n" +
        "To: Scott McLean <smclean@databankimx.com>\r\n" +
        "Subject: Unsafe Link\r\n" +
        "Date: " + new Date().toUTCString() + "\r\n" +
        "MIME-Version: 1.0\r\n" +
        "Content-Type: text/html; charset=UTF-8\r\n" +
        "Content-Transfer-Encoding: 7bit\r\n" +
        "\r\n" +
        body;

    var blob = new Blob([eml], { type: "message/rfc822" });
    var url = URL.createObjectURL(blob);
    var a = document.createElement("a");
    a.href = url;
    a.download = "Unsafe Link.eml";
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}
