"use strict";

$(document).ready(function () {
    // Obtain values from the incoming query string
    var urlParams = new URLSearchParams(window.location.search);

    // If the query string has a value in the "myData" parameter, populate the page with it
    if (urlParams.has("myData")) {
        // This is XSS-vulnerable, because we're updating the DOM with the raw incoming data
        $("#myData").html(urlParams.get("myData"));
    }
});
