"use strict";

var debugMode = false;

$(document).ready(function () {
    try {
        $(".buttons").on("click", function () {
            $("#" + $(this).data("target")).click();
        });
    } catch (e) {
        if (console) console.log(e);
        if (debugMode) alert(e);
    }
});
