//#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
//#endregion

//#region Using Directives
"use strict";
//#endregion

//#region Constants
// URL to the web endpoint for the web service
var apiUrl = "https://localhost:44312/api/";

// When true, trace logging will alert the user in addition to logging to the browser console
var debugMode = false;
//#endregion

//#region Form Event Listeners
// Wait for the DOM to load before adding event handlers
$(document).ready(function () {
    // Only allow numbers to be typed in the zip code field
    $("#zipCode").forceNumeric();

    try {
        // Execute the Ping() method when clicked
        $("#btnPing").on("click", function () {
            callWebApi($(this).data("method"), null);
            writeLogEntry("Executed web API " + $(this).data("method") + "() method...");
        });
    } catch (pex) {
        writeLogEntry("Error executing web API " + $(this).data("method") + "() method!\n\n" + pex);
    }

    try {
        // Execute the Ping() method when clicked
        $("#btnPingRest").on("click", function () {
            var url = apiUrl + $(this).data("method");
            window.open(url, "_blank");
            writeLogEntry("Executed RESTful web API " + $(this).data("method") + "() method...");
        });
    } catch (prex) {
        writeLogEntry("Error executing web API " + $(this).data("method") + "() method!\n\n" + prex);
    }

    try {
        // Execute the Test() method when clicked
        $("#btnTest").on("click", function () {
            if (!validate($("#testData").val(), $(this).data("min-length"), "test data", $("#testData"))) return;
            var data = new Request($("#testData").val());
            callWebApi($(this).data("method"), JSON.stringify(data));
            writeLogEntry("Executed web API " + $(this).data("method") + "() method...");
        });
    } catch (tex) {
        writeLogEntry("Error executing web API " + $(this).data("method") + "() method!\n\n" + tex);
    }

    try {
        // Launch the Test() RESTful URL when clicked
        $("#btnTestRest").on("click", function () {
            if (!validate($("#testData").val(), $(this).data("min-length"), "test data", $("#testData"))) return;
            var guid = createGuid();
            var url = apiUrl + $(this).data("method") + "/" + guid + "/" + encodeURIComponent($("#testData").val());
            window.open(url, "_blank");
            writeLogEntry("Executed RESTful web API " + $(this).data("method") + "() method...");
        });
    } catch (trex) {
        writeLogEntry("Error executing RESTful web API " + $(this).data("method") + "() method!\n\n" + trex);
    }

    try {
        // Execute the LocationLookup() method when clicked
        $("#btnLookupLocation").on("click", function () {
            if (!validate($("#zipCode").val(), $(this).data("min-length"), "zip code", $("#zipCode"))) return;
            var data = new LocationRequest($("#zipCode").val());
            callWebApi($(this).data("method"), JSON.stringify(data));
            writeLogEntry("Executed web API " + $(this).data("method") + "() method...");
        });
    } catch (lex) {
        writeLogEntry("Error executing web API " + $(this).data("method") + "() method!\n\n" + lex);
    }

    try {
        // Launch the LocationLookup() RESTful URL when clicked
        $("#btnLookupLocationRest").on("click", function () {
            if (!validate($("#zipCode").val(), $(this).data("min-length"), "zip code", $("#zipCode"))) return;
            var guid = createGuid();
            var url = apiUrl + $(this).data("method") + "/" + guid + "/" + encodeURIComponent($("#zipCode").val());
            window.open(url, "_blank");
            writeLogEntry("Executed RESTful web API " + $(this).data("method") + "() method...");
        });
    } catch (lrex) {
        writeLogEntry("Error executing RESTful web API " + $(this).data("method") + "() method!\n\n" + lrex);
    }
});
//#endregion

//#region Helper Functions
// Ensure the field contains the necessary data
function validate(value, minLength, name, $target) {
    $("#requestJson").html("");
    $("#responseJson").html("");
    var min = parseInt(minLength);
    if (!value || value.length < min) {
        alert("You must include at least " + minLength + " character" + (minLength === 1 ? "" : "s") + " in the " + name + " field!");
        $target.focus();
        return false;
    }
    writeLogEntry("Validated field value [" + name + "] = [" + value + "] at minimum [" + minLength + "] character" + (minLength === 1 ? "" : "s") + "...");
    return true;
}

// Call the Web Service
function callWebApi(serviceMethod, payload) {
    try {
        writeLogEntry("Sending request " + (payload ? payload : "null") + " to method [" + serviceMethod + "]...");
        $.ajax({
            url: apiUrl + serviceMethod,
            method: "POST",
            crossDomain: true,
            async: false,
            contentType: "application/json",
            data: payload,
            success: function (result, status, request) {
                if ($("#responseJson").hasClass("error")) $("#responseJson").removeClass("error");
                // When we call JSON.stringify, we can pass an optional third argument (number of spaces to indent),
                //   which will automatically "pretty-print" the JSON if placed in a <pre> or <textarea> container.
                // The second argument, which we are setting null would be a delegate for a replacement function for stringify
                $("#requestJson").html(payload ? JSON.stringify(JSON.parse(payload), null, 2) : "null");
                $("#responseJson").html(JSON.stringify(result, null, 2));
                writeLogEntry("Received result " + JSON.stringify(result) + " from method [" + serviceMethod + "]...");
            },
            error: function (request, status, error) {
                if (!$("#responseJson").hasClass("error")) $("#responseJson").addClass("error");
                $("#requestJson").html(payload ? JSON.stringify(JSON.parse(payload), null, 2) : "null");
                $("#responseJson").html(error);
                writeLogEntry("Error calling web service!\n\n" + error);
            }
        });
    } catch (ex) {
        writeLogEntry("Error in callWebApi(" + serviceMethod + ") function!\n\n" + ex);
    } 
}

// Create a TestServiceRequest (function substitutes for "class" in non-compliant browsers such as OnBase)
function Request(testData) {
    this.Data = testData;
    this.Id = createGuid();
}

// Create a LocationLookupRequest (function substitutes for "class" in non-compliant browsers such as OnBase)
function LocationRequest(zipCode) {
    this.ZipCode = zipCode;
    this.Id = createGuid();
}

// Create a GUID (for request ID)
function createGuid() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

// Log trace, debug, and error data to the console and (optionally) display to user
function writeLogEntry(message) {
    console.log(message);
    if (debugMode) alert(message);
}

// forceNumeric() plug-in implementation
jQuery.fn.forceNumeric = function () {
    return this.each(function () {
        $(this).keydown(function (e) {
            var key = e.which || e.keyCode;

            if (!e.shiftKey && !e.altKey && !e.ctrlKey &&
                // numbers   
                key >= 48 && key <= 57 ||
                // Numeric keypad
                key >= 96 && key <= 105 ||
                // comma, period and minus, . on keypad
                key === 190 || key === 188 || key === 109 || key === 110 ||
                // Backspace and Tab and Enter
                key === 8 || key === 9 || key === 13 ||
                // Home and End
                key === 35 || key === 36 ||
                // left and right arrows
                key === 37 || key === 39 ||
                // Del and Ins
                key === 46 || key === 45)
                return true;

            return false;
        });
    });
}
//#endregion

//#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
//#endregion
