"use strict";

//#region Globals and Constants
var debugMode = true;
var debugAlert = false;
var culture = "en-us";
var storageKey = "xssDemoNotes_vulnerable";
//#endregion

//#region DOM-load execution
$(document).ready(function () {
    try {
        // Persistent XSS: whatever was last saved gets written straight back into the DOM
        // with .html() on every page load. If a malicious note was ever saved here, it
        // re-executes every single time this page loads - not just the one time it was typed.
        var stored = localStorage.getItem(storageKey);
        if (stored !== null) $("#notes").html(stored);

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
        // XSS-VULNERABLE: using .html() to insert raw, unsanitized user input directly into
        // the DOM, then persisting that exact same raw markup to localStorage - so the payload
        // survives and re-executes on every future page load, not just this one. Compare this
        // to the fixed version's addNoteText(), which uses .val() and passes the text through
        // sanitizeText() before it's ever written anywhere, including to storage.
        var updated = $("#notes").html() + ($("#notes").html() === "" ? "" : "\n\n") + date.toLocaleDateString(culture)
            + " - " + date.toLocaleTimeString(culture) + "\n" + $("#noteText").val();
        $("#notes").html(updated);
        localStorage.setItem(storageKey, updated);
        $("#note-modal").hide();
        $("#noteText").val("");
    } catch (ex) {
        errorLog("Error in addNoteText() function!\n\n" + ex);
    }
}

function clearNotes() {
    try {
        $("#notes").html("");
        localStorage.removeItem(storageKey);
    } catch (ex) {
        errorLog("Error in clearNotes() function!\n\n" + ex);
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
