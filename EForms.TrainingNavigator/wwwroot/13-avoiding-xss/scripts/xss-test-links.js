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
        let frame = document.getElementById($(this).data("target"));
        frame.contentWindow.location.reload();
    });
});

// Set both iframes' query string to the same value at once - no cross-frame scripting
// needed, since both pages read their own myData independently from their own URL
function loadValue(value) {
    let encoded = encodeURIComponent(value);
    document.getElementById("frameVulnerable").src = "120-xss-vulnerable.html?myData=" + encoded;
    document.getElementById("frameFixed").src = "121-xss-fixed.html?myData=" + encoded;
}
