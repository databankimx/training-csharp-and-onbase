"use strict";

//#region Globals and Constants
var debugMode = true;
var debugAlert = false;
var culture = "en-us";
var money = "USD";
var regexDate = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
var totalAmountField = "kwInvoiceAmount";
var mikgFields = ["kwPoNumber", "kwGlCode", "kwGlAmount", "kwGlDescription"];
var itemAmountField = mikgFields[2];
var rows = 5;
//#endregion

//#region DOM-load execution
$(document).ready(function() {
    try {
        // You can apply your validation to the form's on-submit event (comment below)...
        //$("#glCodingForm").on("submit", function() {
        //    alert("Validation script here");
        //    return false;
        //});
        // ... But I prefer to drive this from a button click
        $("#btnSave").on("click", function () {
            // Validate the form before automatically clicking the hidden submit button
            if (validateRequiredFields() && validateDates() && validateCurrency() && validateMikgRows() && validateTotal()) {
                populateApprovalDates();
                traceLog("All validations passed. Submitting form...");
                // Prevent navigation away from page after submit
                // Instead, show success message and reset form
                alert("Form saved successfully!");
                // Uncomment the line below to actually submit the form (currently disabled to prevent blank page)
                // $("#btnSaveAfterValidation").click();
                // Reset form for next entry
                $("#glCodingForm").trigger("reset");
                traceLog("Form reset after successful save.");
            }
        });

        $("#note-modal").hide();
        $("#btnAddNote").on("click", function() {
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
            // Use context-aware formatting for GL Amount fields
            if ($(this).hasClass("line-item")) {
                $(this).val(formatCurrencyWithContext($(this)));
            } else {
                $(this).val(formatAsCurrency($(this).val()));
            }
            var subtotal = 0.0;
            $(".line-item").each(function() {
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
        });

        // Log to browser console
        traceLog("Initial script load complete...");
    } catch (ex) {
        errorLog("Error in window.onload function!\n\n" + ex);
    }
});
//#endregion

//#region Form Validation Functions
// Verify that required fields have values
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

// Verify that date fields are formatted as dates
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

// Verify MIKG rows have either ALL or NO values
function validateMikgRows() {
    try {
        traceLog("Checking MIKG fields for " + rows + " row" + (rows === 1 ? "" : "s") + "...");
        for (var i = 1; i <= rows; i++) {
            traceLog("Checking row " + i + "...");
            var filledFields = 0;
            for (const element of mikgFields) {
                if ($("#" + element + i).val() !== "") filledFields++;
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

// Verify that currency fields are formatted as currency
function validateCurrency() {
    try {
        var isValid = true;
        $(".currency").each(function () {
            var value = currencyToFloat($(this).val());
            // Use context-aware formatting for GL Amount fields
            if ($(this).hasClass("line-item")) {
                if (value === 0.0) {
                    $(this).val(formatCurrencyWithContext($(this)));
                } else {
                    $(this).val(formatAsCurrency($(this).val()));
                }
            } else if (value === 0.0) {
                $(this).val("");
            } else {
                $(this).val(formatAsCurrency($(this).val()));
            }
        });
        return isValid;
    } catch (ex) {
        errorLog("Error in validateCurrency() function!\n\n" + ex);
        return false;
    }
}

// Verify that MIKG amounts add up to invoice amount
function validateTotal() {
    try {
        traceLog("Validating invoice amount...");
        var total = 0.0;
        for (var i = 1; i <= rows; i++) {
            var fieldName = "#" + itemAmountField + i;
            var fieldValue = currencyToFloat($(fieldName).val());
            if (Number.isNaN(fieldValue)) fieldValue = 0.0;
            total += fieldValue * 1.0;
            traceLog(fieldName + " = " + fieldValue + "\nRunning total: " + total);
        }
        var amount = currencyToFloat($("#" + totalAmountField).val());
        traceLog("Comparing total [" + total + "] to invoice amount [" + amount + "]...");
        var status = total === amount;
        traceLog("Validation Result: " + status);
        if (status === false) {
            // Highlight invoice amount field
            var $invoiceField = $("#" + totalAmountField);
            if (!$invoiceField.hasClass("error")) $invoiceField.addClass("error");
            $invoiceField.focus();
            
            // Also highlight all GL Amount fields for reference
            $(".line-item").each(function () {
                if (!$(this).hasClass("error")) $(this).addClass("error");
            });
            
            alert("Total for distribution lines does not equal invoice amount!");
        } else {
            // Clear error highlighting on success
            $("#" + totalAmountField).removeClass("error");
            $(".line-item").removeClass("error");
        }
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
        // Escape the following characters (< > & ' ")
        text = text.replaceAll('<', "&lt;")
            .replaceAll('>', "&gt;")
            .replaceAll('&', "&amp;")
            .replaceAll('\'', "&#39;")
            .replaceAll('"', "&quot;");
        return text;
    } catch (ex) {
        errorLog("Error in sanitizeText() function!\n\n" + ex);
        return "";
    }
}

// Format currency with context awareness for GL Amount fields
function formatCurrencyWithContext($field) {
    try {
        var value = $field.val();
        var currencyValue = currencyToFloat(value);
        
        // If the value is 0 or empty, check if it's a GL Amount field in a row
        if (currencyValue === 0.0 && $field.hasClass("line-item")) {
            // Get the row number from the field ID (e.g., kwGlAmount2 -> 2)
            var fieldId = $field.attr("id");
            // Extract trailing digits from field ID
            var rowNum = "";
            for (var j = fieldId.length - 1; j >= 0; j--) {
                var char = fieldId.charAt(j);
                if (char >= "0" && char <= "9") {
                    rowNum = char + rowNum;
                } else {
                    break;
                }
            }
            if (rowNum !== "") {
                // Check if all other fields in this MIKG row are empty
                var otherFieldsEmpty = true;
                for (const fieldName of mikgFields) {
                    if (fieldName !== itemAmountField) { // Skip the amount field itself
                        var $otherField = $("#" + fieldName + rowNum);
                        if ($otherField.val() !== "") {
                            otherFieldsEmpty = false;
                            break;
                        }
                    }
                }
                // If all other fields in the row are empty, leave the amount field blank
                if (otherFieldsEmpty) {
                    return "";
                }
            }
        }
        
        // If value is zero and was originally empty, return blank
        if (currencyValue === 0.0 && value === "") {
            return "";
        }
        
        // Otherwise format as currency
        return formatAsCurrency(value);
    } catch (ex) {
        errorLog("Error in formatCurrencyWithContext() function!\n\n" + ex);
        return "$0.00";
    }
}

// Format a value as the specified culture currency
function formatAsCurrency(number) {
    try {
        if (Intl.NumberFormat !== undefined) {
            var formatter = new Intl.NumberFormat(culture, { style: "currency", currency: money });
            return formatter.format(currencyToFloat(number));
        }
        return formatMoney(currencyToFloat(number));
    } catch (ex) {
        errorLog("Error in currencyToFloat() function!\n\n" + ex);
        return "$0.00";
    } 
}

// Format a value as the specified culture currency (old way)
function formatMoney(amount, decimalCount = 2, decimal = ".", thousands = ",") {
    try {
        decimalCount = Math.abs(decimalCount);
        decimalCount = Number.isNaN(decimalCount) ? 2 : decimalCount;

        var negativeSign = amount < 0 ? "-" : "";

        amount = Math.abs(Number(amount) || 0).toFixed(decimalCount);
        var i = Number.parseInt(amount).toString();
        var j = (i.length > 3) ? i.length % 3 : 0;

        return negativeSign + (j ? i.substring(0, j) + thousands : '') + i.substring(j).replace(/(\d{3})(?=\d)/g, "$1" + thousands) +
            (decimalCount ? decimal + Math.abs(amount - i).toFixed(decimalCount).slice(2) : "");
    } catch (ex) {
        errorLog("Error in formatMoney() function!\n\n" + ex);
        return "$0.00";
    }
};

// Convert formatted currency to floating-point value
function currencyToFloat(curr) {
    try {
        var number = Number.parseFloat(stripCurrency(curr));
        return Number.isNaN(number) ? 0.0 : number;
    } catch (ex) {
        errorLog("Error in currencyToFloat() function!\n\n" + ex);
        return 0.0;
    }
}

// String currency symbols and commas
function stripCurrency(str) {
    try {
        // Convert to string and strip all currency symbols and commas
        return String(str).replaceAll('$', "").replaceAll(',', "");
    } catch (ex) {
        errorLog("Error in stripCurrency() function!\n\n" + ex);
        return "0.0";
    }
}

// Pad specified string
function padLeft(str, len, padChar = "0") {
    try {
        while (str.length < len) str = padChar + str;
        return str;
    } catch (ex) {
        errorLog("Error in padLeft() function!\n\n" + ex);
        return "";
    } 
}
//#endregion

//#region Debug Logging Functions
// Log trace messages for debugging
function traceLog(message) {
    if (!debugMode) return;
    log(message);
}

// Log errors
function errorLog(message) {
    log("*** ERROR ***\n" + message);
}

// Write to browser console and/or alert
function log(message) {
    if (typeof console !== "undefined") console.log(message);
    if (debugAlert) alert(message);
}
//#endregion
