"use strict";

//#region Globals and Constants
var debugMode = true;
var debugAlert = false;
var culture = "en-us";
var storageKey = "xssDemoNotes_fixed";
//#endregion

//#region DOM-load execution
$(document).ready(function () {
    try {
        // Not XSS-vulnerable: whatever was last saved is written back with .val(), not .html(),
        // so even a stored payload that arrived completely unsanitized just displays as inert
        // text - the safety here comes from how this page reads and displays the data, not from
        // trusting that the data was already clean.
        var stored = localStorage.getItem(storageKey);
        if (stored !== null) $("#notes").val(stored);

        $("#note-modal").hide();
        $("#btnAddNote").on("click", function () {
            addNote();
        });
        $("#btnAddNoteText").on("click", function () {
            addNoteText();
        });
        $("#btnCancelNoteText").on("click", function () {
            $("#note-modal").hide();
        });
        $("#btnClearNotes").on("click", function () {
            clearNotes();
        });

        traceLog("Initial script load complete...");
    } catch (ex) {
        errorLog("Error in window.onload function!\n\n" + ex);
    }
});
//#endregion

//#region Helper Functions
function addNote() {
    try {
        $("#note-modal").show();
        $("#noteText").focus();
    } catch (ex) {
        errorLog("Error in addNote() function!\n\n" + ex);
    }
}

function addNoteText() {
    try {
        if ($("#noteText").val() === "") {
            alert("No note text was entered!");
            $("#noteText").focus();
            return;
        }
        var date = new Date();
        var updated = $("#notes").val() + ($("#notes").val() === "" ? "" : "\n\n") + date.toLocaleDateString(culture)
            + " - " + date.toLocaleTimeString(culture) + "\n" + sanitizeText($("#noteText").val());
        $("#notes").val(updated);
        localStorage.setItem(storageKey, updated);
        $("#note-modal").hide();
        $("#noteText").val("");
    } catch (ex) {
        errorLog("Error in addNoteText() function!\n\n" + ex);
    }
}

function clearNotes() {
    try {
        $("#notes").val("");
        localStorage.removeItem(storageKey);
    } catch (ex) {
        errorLog("Error in clearNotes() function!\n\n" + ex);
    }
}

function sanitizeText(text) {
    try {
        // Escape the following characters (< > & ' ")
        text = text.replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/&/g, "&amp;")
            .replace(/'/g, "&#39;")
            .replace(/"/g, "&quot;");
        return text;
    } catch (ex) {
        errorLog("Error in sanitizeText() function!\n\n" + ex);
        return "";
    }
}
//#endregion

//#region Debug Logging Functions
function traceLog(message) {
    if (!debugMode) return;
    log(message);
}

function errorLog(message) {
    log("*** ERROR ***\n" + message);
}

function log(message) {
    if (typeof console !== "undefined") console.log(message);
    if (debugAlert) alert(message);
}
//#endregion
