"use strict";

$(document).ready(function () {
    // Reloading via contentWindow.location.reload() - rather than re-assigning .src to its own
    // current value - guarantees an actual reload even when the URL hasn't changed
    $(".reload").on("click", function () {
        var frame = document.getElementById($(this).data("target"));
        frame.contentWindow.location.reload();
    });
});
