"use strict";

//#region Development/Testing Settings
var debugMode = true;
var debugAlert = false;
var testMode = true;
//#endregion

//#region Globals and Constants
var culture = "en-us";
var money = "USD";
var regexDate = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
var totalAmountField = "kwInvoiceAmount";
var mikgFields = ["kwPoNumber", "kwGlCode", "kwGlAmount", "kwGlDescription"];
var itemAmountField = mikgFields[2];
var rows = 1;
var maxRows = 10;
var rowPrefix = "gl";
//#endregion

//#region DOM-load execution
$(document).ready(function() {
    try {
        if (testMode) fillTestData();
        loadDynTable();

        // You can apply your validation to the form's on-submit event (comment below)...
        //$("#glCodingForm").on("submit", function() {
        //    alert("Validation script here");
        //    return false;
        //});
        // ... But I prefer to drive this from a button click
        $("#btnSave").on("click", function () {
            if (validateRequiredFields() && validateDates() && validateCurrency() && validateMikgRows() && validateTotal()) {
                populateApprovalDates();
                $("#btnSaveAfterValidation").click();
            }
        });

        // This training demo has no real server to receive the form's POST - action="#" is a
        // placeholder, not a real endpoint. Intercepting the actual submit event here prevents a
        // real HTTP POST (which, depending on how this file happens to be hosted, may be rejected
        // outright - e.g. IIS's static file handler returns 405 Method Not Allowed for a POST to a
        // plain .html file) and shows a simple confirmation instead, standing in for what a real
        // OnBase form's actual save would do.
        $("#glCodingForm").on("submit", function () {
            traceLog("Form submission intercepted - simulating a successful save.");
            alert("Form saved successfully! (No real submission occurs in this training demo.)");
            return false;
        });

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

        // Use the jQuery UI date-picker on date fields
        $(".date").datepicker();

        if ($("#kwRequestedBy").val() === "") $("#kwRequestedBy").val($("#propUserRealName").val());

        // Clear the error color from any field after entering a value
        $(":input").on("blur", function() {
            if ($(this).hasClass("error") && $(this).val() !== "") $(this).removeClass("error");
        });

        // Automatically format currency fields
        $(".currency").on("blur", function() {
            $(this).val(formatAsCurrency($(this).val()));
            calculateRunningTotal();
        });

        $("#btnAddRow").on("click", function() {
            addRow();
        });

        $("#btnRemoveRow").on("click", function () {
            removeRow();
        });

        $("#glCodingForm").on("reset", function () {
            // Using setTimeout to ensure the code runs *after* the reset.
            setTimeout(function() {
                if (testMode) fillTestData();
                loadDynTable();
                $(":input").each(function () {
                    if ($(this).hasClass("error")) $(this).removeClass("error");
                });
            }, 1);
        });

        traceLog("Initial script load complete...");
    } catch (ex) {
        errorLog("Error in window.onload function!\n\n" + ex);
    }
});
//#endregion

//#region Fill Test Data
function fillTestData() {
    try {
        if (!testMode) return;

        $("#kwInvoiceNumber").val("12345");
        $("#kwInvoiceDate").val(new Date().toLocaleDateString(culture));
        $("#kwInvoiceAmount").val("$123.45");
        $("#kwVendorName").val("Global Worldwide International Corp, Inc.");
        $("#kwVendorId").val("54321");
        $("#kwPoNumber1").val("111");
        $("#kwGlCode1").val("1000");
        $("#kwGlAmount1").val("$50.00");
        $("#kwGlDescription1").val("Left-handed ginglepverschneppfler");
        $("#kwPoNumber2").val("112");
        $("#kwGlCode2").val("2000");
        $("#kwGlAmount2").val("$73.45");
        $("#kwGlDescription2").val("Top-loading klumphdrix");
        $("#propUserRealName").val("TEST USER");
    } catch (ex) {
        errorLog("Error in fillTestData() function!\n\n" + ex);
    }
}
//#endregion

//#region Pseudo-Dynamic Table Functions
// Show the appropriate rows on load, based on which already have data
function loadDynTable() {
    try {
        $(".dyn-row").hide();
        rows = 1;
        for (var r = 1; r <= maxRows; r++) {
            var rowName = "#" + rowPrefix + r;
            if (r === 1) {
                traceLog("Rows: " + rows + "\nShowing row: " + rowName);
                $(rowName).show();
                continue;
            }
            for (var c = 0; c < mikgFields.length; c++) {
                var cellName = "#" + mikgFields[c] + r;
                if ($(cellName).val() !== "") {
                    rows++;
                    traceLog("Rows: " + rows + "\nShowing row: " + rowName);
                    $(rowName).show();
                    break;
                }
            }
        }
        calculateRunningTotal();
    } catch (ex) {
        errorLog("Error in loadDynTable() function!\n\n" + ex);
    }
}

// Show one of the hidden table rows
function addRow() {
    try {
        if (rows === maxRows) return;
        rows++;
        var rowName = "#" + rowPrefix + rows;
        traceLog("Added row " + rows);
        $(rowName).show();
    } catch (ex) {
        errorLog("Error in addRow() function!\n\n" + ex);
    }
}

// Hide one of the displayed table rows
function removeRow() {
    try {
        if (rows === 1) return;
        var rowName = "#" + rowPrefix + rows;
        for (var c = 0; c < mikgFields.length; c++) {
            var cellName = "#" + mikgFields[c] + rows;
            $(cellName).val("");
        }
        traceLog("Removed row " + rows);
        rows--;
        $(rowName).hide();
        calculateRunningTotal();
    } catch (ex) {
        errorLog("Error in removeRow() function!\n\n" + ex);
    }
}
//#endregion

//#region Form Validation Functions
function validateRequiredFields() {
    try {
        traceLog("Checking required fields...");
        var valid = true;
        $(":input[data-required='true']").each(function() {
            traceLog($(this).prop("id") + " = " + $(this).val());
            if ($(this).val() === "") {
                valid = false;
                if (!$(this).hasClass("error")) $(this).addClass("error");
                alert("You must enter a value for [" + $(this).prop("id") + "]!");
                $(this).select();
                return false;
            }
            if ($(this).hasClass("error")) $(this).removeClass("error");
            return true;
        });
        return valid;
    } catch (ex) {
        errorLog("Error in validateRequiredFields() function!\n\n" + ex);
        return false;
    } 
}

function validateDates() {
    try {
        traceLog("Checking date fields...");
        var ok = true;
        $(".date").each(function () {
            traceLog($(this).prop("id") + " = " + $(this).val());
            if ($(this).val() === "") return true;
            if (!regexDate.test($(this).val())) {
                ok = false;
                $(this).val("");
                $(this).select();
                if (!$(this).hasClass("error")) $(this).addClass("error");
                alert("Value for field [" + $(this).prop("id") + "] must be a date in format MM/DD/YYYY!");
                return false;
            }
            return true;
        });
        return ok;
    } catch (ex) {
        errorLog("Error in validateDates() function!\n\n" + ex);
        return false;
    } 
}

function validateMikgRows() {
    try {
        traceLog("Checking MIKG fields for " + rows + " row" + (rows === 1 ? "" : "s") + "...");
        for (var i = 1; i <= rows; i++) {
            traceLog("Checking row " + i + "...");
            var filledFields = 0;
            for (var j = 0; j < mikgFields.length; j++) {
                if ($("#" + mikgFields[j] + i).val() !== "") filledFields++;
            }
            if (filledFields > 0 && filledFields < mikgFields.length) {
                alert("Distribution row " + i + " must either have no fields or all fields entered!");
                for (var k = 0; k < mikgFields.length; k++) {
                    var $field = $("#" + mikgFields[k] + i);
                    if (!$field.hasClass("error")) $field.addClass("error");
                    if (k === 0) $field.focus();
                }
                return false;
            }
        }
        return true;
    } catch (ex) {
        errorLog("Error in validateMikgRows() function!\n\n" + ex);
        return false;
    }
}

function validateCurrency() {
    try {
        $(".currency").each(function() {
            if (currencyToFloat($(this).val()) === 0.0) $(this).val("");
            else $(this).val(formatAsCurrency($(this).val()));
        });
        return true;
    } catch (ex) {
        errorLog("Error in validateCurrency() function!\n\n" + ex);
        return false;
    } 
}

function validateTotal() {
    try {
        traceLog("Validating invoice amount...");
        var total = 0.0;
        for (var i = 1; i <= rows; i++) {
            var fieldName = "#" + itemAmountField + i;
            var fieldValue = currencyToFloat($(fieldName).val());
            if (isNaN(fieldValue)) fieldValue = 0.0;
            total += fieldValue * 1.0;
            traceLog(fieldName + " = " + fieldValue + "\nRunning total: " + total);
        }
        var amount = currencyToFloat($("#" + totalAmountField).val());
        traceLog("Comparing total [" + total + "] to invoice amount [" + amount + "]...");
        var status = total === amount;
        traceLog("Validation Result: " + status);
        if (status === false) alert("Total for distribution lines does not equal invoice amount!");
        return status;
    } catch (ex) {
        errorLog("Error in validateTotal() function!\n\n" + ex);
        return false;
    }
}

// Stamp a Name/Date pair's date field with today, but only once that pair actually has a
// name recorded - and only if the date isn't already set, so re-saving never overwrites an
// earlier-recorded approval date.
function populateApprovalDates() {
    try {
        if ($("#kwRequestedBy").val() !== "" && $("#kwRequestDate").val() === "") {
            $("#kwRequestDate").val(new Date().toLocaleDateString(culture));
            traceLog("Requester date stamped.");
        }
        if ($("#kwApprovedBy").val() !== "" && $("#kwApprovedDate").val() === "") {
            $("#kwApprovedDate").val(new Date().toLocaleDateString(culture));
            traceLog("Approver date stamped.");
        }
    } catch (ex) {
        errorLog("Error in populateApprovalDates() function!\n\n" + ex);
    }
}
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
        $("#notes").val($("#notes").val() + ($("#notes").val() === "" ? "" : "\n\n") + date.toLocaleDateString(culture)
            + " - " + date.toLocaleTimeString(culture) + "\n" + sanitizeText($("#noteText").val()));
        $("#note-modal").hide();
        $("#noteText").val("");
    } catch (ex) {
        errorLog("Error in addNoteText() function!\n\n" + ex);
    }
}

function sanitizeText(text) {
    try {
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

function formatAsCurrency(number) {
    try {
        if (typeof Intl.NumberFormat !== "undefined") {
            var formatter = new Intl.NumberFormat(culture, { style: "currency", currency: money });
            return formatter.format(currencyToFloat(number));
        }
        return formatMoney(currencyToFloat(number));
    } catch (ex) {
        errorLog("Error in currencyToFloat() function!\n\n" + ex);
        return "$0.00";
    } 
}

function formatMoney(amount, decimalCount = 2, decimal = ".", thousands = ",") {
    try {
        decimalCount = Math.abs(decimalCount);
        decimalCount = isNaN(decimalCount) ? 2 : decimalCount;
        var negativeSign = amount < 0 ? "-" : "";
        var i = parseInt(amount = Math.abs(Number(amount) || 0).toFixed(decimalCount)).toString();
        var j = (i.length > 3) ? i.length % 3 : 0;
        return negativeSign + (j ? i.substr(0, j) + thousands : '') + i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousands) +
            (decimalCount ? decimal + Math.abs(amount - i).toFixed(decimalCount).slice(2) : "");
    } catch (ex) {
        errorLog("Error in formatMoney() function!\n\n" + ex);
        return "$0.00";
    }
};

function currencyToFloat(curr) {
    try {
        var number = parseFloat(stripCurrency(curr));
        return isNaN(number) ? 0.0 : number;
    } catch (ex) {
        errorLog("Error in currencyToFloat() function!\n\n" + ex);
        return 0.0;
    }
}

function stripCurrency(str) {
    try {
        if (isNaN(str)) return str.replace(/\$/g, "").replace(/,/g, "");
        return str;
    } catch (ex) {
        errorLog("Error in currencyToFloat() function!\n\n" + ex);
        return "0.0";
    }
}

function padLeft(str, len, padChar = "0") {
    try {
        while (str.length < len) str = padChar + str;
        return str;
    } catch (ex) {
        errorLog("Error in padLeft() function!\n\n" + ex);
        return "";
    } 
}

function calculateRunningTotal() {
    try {
        var subtotal = 0.0;
        $(".line-item").each(function () {
            if ($(this).val() !== "") subtotal += currencyToFloat($(this).val());
        });
        $("#subTotal").val(formatAsCurrency(subtotal));
        if ($("#subTotal").val() === $("#kwInvoiceAmount").val()) {
            if ($("#subTotal").hasClass("attention")) $("#subTotal").removeClass("attention");
            if (!$("#subTotal").hasClass("ok")) $("#subTotal").addClass("ok");
        } else {
            if ($("#subTotal").hasClass("ok")) $("#subTotal").removeClass("ok");
            if (!$("#subTotal").hasClass("attention")) $("#subTotal").addClass("attention");
        }
    } catch (ex) {
        errorLog("Error in calculateRunningTotal() function!\n\n" + ex);
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
