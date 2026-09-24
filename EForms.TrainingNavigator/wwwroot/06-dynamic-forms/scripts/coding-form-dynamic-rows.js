"use strict";

//#region Development/Testing Settings
// When true, trace logging is passed to the browser F12 console (not available when testing inside OnBase)
var debugMode = true;

// When true, log entries appear as pop-up alerts (use with caution and only when testing in OnBase)
var debugAlert = false;

// When true, the form will load with test data
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
var keywords = {
    poNumber: { id: "384", name: "kwPoNumber" },
    glCode: { id: "367", name: "kwGlCode" },
    glAmount: { id: "368", name: "kwGlAmount" },
    glDescription: { id: "324", name: "kwGlDescription" }
};

// Container for object representing the group of MIKG rows on the form at a given time
var mikgData;
//#endregion

//#region Test Data
var testData = {
    kwInvoiceNumber: "12345",
    kwInvoiceDate: new Date().toLocaleDateString(culture),
    kwInvoiceAmount: "$123.45",
    kwVendorName: "Global Worldwide International Corp, Inc.",
    kwVendorId: "54321",
    propUserRealName: "TEST USER",
    mikgData: [
        {
            rowId: 1,
            kwPoNumber: "111",
            kwGlCode: "1000",
            kwGlAmount: "$50.00",
            kwGlDescription: "Left-handed ginglepverschneppfler"
        },
        {
            rowId: 2,
            kwPoNumber: "112",
            kwGlCode: "2000",
            kwGlAmount: "$73.45",
            kwGlDescription: "Top-loading klumphdrix"
        }
    ]
};

// Add the test data values to the form
function fillTestData() {
    try {
        if (!testMode) return;

        traceLog("Filling test data...");

        var keys = Object.keys(testData);
        for (var k = 0; k < keys.length; k++) {
            if (keys[k] === "mikgData") {
                mikgData = testData.mikgData;
            } else {
                var id = keys[k];
                var val = testData[keys[k]];
                traceLog(id + " = " + val);
                $("#" + id).val(val);
            }
        }
    } catch (ex) {
        errorLog("Error in fillTestData() function!\n\n" + ex);
    }
}
//#endregion

//#region DOM-load execution
$(document).ready(function () {
    try {
        // Hide the debug sections unless troubleshooting in test mode
        if (!debugMode) {
            $("#debugData").hide();
            $("#errorData").hide();
        }

        // Load the form data (from stored data in normal mode or test data in test mode)
        if (testMode) fillTestData();
        else loadMikgData();

        // Load the dynamic table rows from the loaded form data
        loadDynTable();

        // Populate the current MIKG data in JSON format to the test section
        debugJson();

        // You can apply your validation to the form's on-submit event (comment below)...
        //$("#glCodingForm").on("submit", function() {
        //    alert("Validation script here");
        //    return false;
        //});
        // ... But I prefer to drive this from a button click
        $("#btnSave").on("click", function () {
            if (validateRequiredFields() && validateDates() && validateCurrency() && validateMikgRows() && validateTotal()) {
                storeMikgData();
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

        $(":input").on("blur", function () {
            if ($(this).hasClass("error") && $(this).val() !== "") $(this).removeClass("error");
        });

        $(".currency").on("blur", function () {
            $(this).val(formatAsCurrency($(this).val()));
            calculateRunningTotal();
        });

        $("#btnAddRow").on("click", function () {
            addRow();
        });

        $("#glCodingForm").on("reset", function () {
            // Using setTimeout to ensure the code runs *after* the reset.
            setTimeout(function () {
                if (testMode) fillTestData();
                loadDynTable();
                $(":input").each(function () {
                    if ($(this).hasClass("error")) $(this).removeClass("error");
                });
                debugJson();
            }, 1);
        });

        traceLog("Initial script load complete...");
    } catch (ex) {
        errorLog("Error in window.onload function!\n\n" + ex);
    }
});
//#endregion

//#region Dynamic Table Functions
// Load all rows into the dynamic MIKG table
function loadDynTable() {
    try {
        rows = 0;

        // Remove all currently displayed rows
        $(".dyn-row").remove();

        // If no data is currently stored, set a single, blank row with row label 1 as the MIKG data
        if (mikgData.length === 0) {
            traceLog("No MIKG data exists. Loading default table.");
            mikgData = [{ rowId: "1", kwPoNumber: "", kwGlCode: "", kwGlAmount: "", kwGlDescription: "" }];
        }

        traceLog("Loading dynamic table with [" + mikgData.length + "] row" + (mikgData.length === 1 ? "" : "s") + "...");

        for (var r = 0; r < mikgData.length; r++) {
            rows++;
            displayRow(rows, mikgData[r]);
            traceLog("Showing row: " + rowPrefix + mikgData[r].rowId);
        }

        calculateRunningTotal();
    } catch (ex) {
        errorLog("Error in loadDynTable() function!\n\n" + ex);
    }
}

// Create and display a row, based on one instance of the MIKG data
function displayRow(rowNum, rowData) {
    try {
        $("#mikgBody").append(
            "<tr id=\"" + rowPrefix + rowNum + "\" class=\"dyn-row\">" +
            "<td class=\"right\">" +
            "<label for=\"kwPoNumber" + rowNum + "\">" + rowNum + "&nbsp;</label>" +
            "</td>" +
            "<td>" +
            "<input type=\"text\" name=\"OBKey__" + keywords.poNumber.id + "_" + rowNum + "\" id=\"" + keywords.poNumber.name + rowNum + "\" value=\"" + sanitizeText(rowData.kwPoNumber) + "\" />" +
            "</td>" +
            "<td>" +
            "<input type=\"text\" name=\"OBKey__" + keywords.glCode.id + "_" + rowNum + "\" id=\"" + keywords.glCode.name + rowNum + "\" value=\"" + sanitizeText(rowData.kwGlCode) + "\" />" +
            "</td>" +
            "<td>" +
            "<input class=\"currency line-item\" type=\"text\" name=\"OBKey__" + keywords.glAmount.id + "_" + rowNum + "\" id=\"" + keywords.glAmount.name + rowNum + "\" value=\"" + sanitizeText(rowData.kwGlAmount) + "\" />" +
            "</td>" +
            "<td>" +
            "<input class=\"wide\" type=\"text\" name=\"OBKey__" + keywords.glDescription.id + "_" + rowNum + "\" id=\"" + keywords.glDescription.name + rowNum + "\" value=\"" + sanitizeText(rowData.kwGlDescription) + "\" />" +
            "</td>" +
            "<td>" +
            (rowNum > 1 ? "<input class=\"buttons-small\" type=\"button\" name=\"btnDeleteRow" + rowNum + "\" id=\"btnDeleteRow" + rowNum + "\" data-row=\"" + rowNum + "\" value=\"X\" />" : "&nbsp;") +
            "</td>" +
            "</tr>"
        );

        if (rowNum > 1) $("#btnDeleteRow" + rowNum).on("click", function() {
            removeRow(rowNum);
        });

        $("#" + keywords.glAmount.name + rowNum).on("blur", function () {
            $(this).val(formatAsCurrency($(this).val()));
            calculateRunningTotal();
        });
    } catch (ex) {
        errorLog("Error in displayRow() function!\n\n" + ex);
    }
}

// Add a row to the dynamic table
function addRow() {
    try {
        if (rows === maxRows) {
            alert("Cannot add more than " + maxRows + " rows!");
            return;
        }
        rows++;
        displayRow(rows, { rowId: rows, kwPoNumber: "", kwGlCode: "", kwGlAmount: "", kwGlDescription: "" });
        traceLog("Added row " + rows);
        storeMikgData();
    } catch (ex) {
        errorLog("Error in addRow() function!\n\n" + ex);
    }
}

// Remove a row from the dynamic table
function removeRow(row) {
    try {
        traceLog(rows);
        if (rows === 1) return;
        $("#" + rowPrefix + row).remove();
        traceLog("Removed row " + row);
        rows--;
        storeMikgData();
        // Re-load the table to ensure rows re-number and avoid gaps/invalid instance numbers
        loadDynTable();
        calculateRunningTotal();
    } catch (ex) {
        errorLog("Error in removeRow() function!\n\n" + ex);
    }
}

// Save all current dynamic rows as MIKG data
function storeMikgData() {
    try {
        mikgData = [];
        var domRows = $("#mikgBody > tr");
        for (var r = 0; r < domRows.length; r++) {
            // Read the instance number directly from the row's own id (e.g. "gl3" -> "3"),
            // rather than parsing the displayed label text - more robust, and not dependent
            // on how the browser happens to normalize HTML entities like &nbsp; on read-back.
            var instance = domRows.eq(r).prop("id").replace(rowPrefix, "");
            mikgData.push({
                rowId: r + 1,
                kwPoNumber: $("#kwPoNumber" + instance).val(),
                kwGlCode: $("#kwGlCode" + instance).val(),
                kwGlAmount: $("#kwGlAmount" + instance).val(),
                kwGlDescription: $("#kwGlDescription" + instance).val()
            });
        }
        // Store the data as base64-encoded JSON in a hidden form field
        $("#mikgData").val(btoa(JSON.stringify(mikgData)));

        debugJson();
    } catch (ex) {
        errorLog("Error in storeMikgData() function!\n\n" + ex);
    }
}

// Load the MIKG data from the hidden form field
function loadMikgData() {
    try {
        mikgData = JSON.parse(atob($("#mikgData").val()));
    } catch (ex) {
        errorLog("Error in loadMikgData() function!\n\n" + ex);
    }
}
//#endregion

//#region Form Validation Functions
function validateRequiredFields() {
    try {
        traceLog("Checking required fields...");
        var valid = true;
        $(":input[data-required='true']").each(function () {
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
        $(".currency").each(function () {
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

// Sanitize values being added to the DOM to avoid cross-site scripting
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

// Calculate total of all GL amounts and compare to invoice total amount
function calculateRunningTotal() {
    try {
        setTimeout(function () {
            traceLog("Recalculating...");
            var subtotal = 0.0;
            $(".line-item").each(function () {
                traceLog($(this).prop("id") + " = " + $(this).val());
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
        }, 10);
    } catch (ex) {
        errorLog("Error in calculateRunningTotal() function!\n\n" + ex);
    }
}

// Populate the current MIKG data in JSON format to the test section
function debugJson() {
    try {
        if (debugMode) $("#debugInfo").html(JSON.stringify(mikgData, null, 2));
    } catch (ex) {
        errorLog("Error in debugJson() function!\n\n" + ex);
    }
}
//#endregion

//#region Debug Logging Functions
function traceLog(message) {
    if (!debugMode) return;
    log(message);
}

function errorLog(message) {
    message = "*** ERROR ***\n" + message;
    log(message);
    if (debugMode) $("#errorInfo").html($("#errorInfo").html() + ($("#errorInfo").html().length > 0 ? "\n\n" : "") + message);
}

function log(message) {
    if (typeof console !== "undefined") console.log(message);
    if (debugAlert) alert(message);
}
//#endregion
