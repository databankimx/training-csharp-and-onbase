"use strict";

$(document).ready(function () {
    // Obtain values from the incoming query string
    var urlParams = new URLSearchParams(window.location.search);

    // If the query string has a value in the "myData" parameter, populate the page with it
    if (urlParams.has("myData")) {
        var value = Sanitize(urlParams.get("myData"));
        // This is not XSS-vulnerable, because we've sanitized the incoming data before updating the DOM
        $("#myData").html(value);
    }
});

// Sanitize data to prevent the browser from executing it as HTML
function Sanitize(value) {
    // Escape the following characters (< > & ' ")
    value = value.replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/&/g, "&amp;")
        .replace(/'/g, "&#39;")
        .replace(/"/g, "&quot;");
    return value;
}
