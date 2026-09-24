"use strict";

var debugMode = true;
var debugAlert = false;
var testMode = true;
var culture = "en-us";
var money = "USD";
var regexDate = /^\d{1,2}\/\d{1,2}\/\d{4}$/;
var totalAmountField = "kwInvoiceAmount";
var mikgFields = ["kwPoNumber", "kwGlCode", "kwGlAmount", "kwGlDescription"];
var itemAmountField = mikgFields[2];
var rows = 5;
var glVue;
var glTableHtml;

// Sample data for the Invoice Header fields - the GL Distribution rows already have their own
// sample data pre-encoded in the #glLines textarea, loaded separately by loadDynamicRows().
var testData = {
    kwInvoiceNumber: "12345",
    kwInvoiceDate: new Date().toLocaleDateString(culture),
    kwInvoiceAmount: "$1,000.00",
    kwVendorName: "Global Worldwide International Corp, Inc.",
    kwVendorId: "54321",
    propUserRealName: "TEST USER"
};

function fillTestData() {
    try {
        if (!testMode) return;
        traceLog("Filling test data...");
        var keys = Object.keys(testData);
        for (var k = 0; k < keys.length; k++) {
            traceLog(keys[k] + " = " + testData[keys[k]]);
            $("#" + keys[k]).val(testData[keys[k]]);
        }
    } catch (ex) {
        errorLog("Error in fillTestData() function!\n\n" + ex);
    }
}

$(document).ready(function () {
    try {
        if (!debugMode) $("#glLinesSection").hide();

        Vue.component("my-currency-input", {
            props: ["value"],
            template: '<input type="text" v-model="displayValue" @blur="isInputActive = false" @focus="isInputActive = true"/>',
            data: function () {
                return {
                    isInputActive: false
                }
            },
            computed: {
                displayValue: {
                    get: function () {
                        if (this.isInputActive) {
                            // Cursor is inside the input field - un-format display value for user
                            return this.value.toString();
                        } else {
                            calculateRunningTotal();
                            return formatAsCurrency(this.value);
                        }
                    },
                    set: function (modifiedValue) {
                        // Recalculate value after ignoring "$" and "," in user input
                        var newValue = parseFloat(modifiedValue.replace(/[^\d.]/g, ""));
                        if (isNaN(newValue)) {
                            newValue = 0;
                        }
                        // Note: we cannot set this.value directly, as it is a "prop" -- it needs to be
                        // passed back up to the parent component, so we $emit the event instead
                        this.$emit('input', newValue);
                    }
                }
            }
        });

        loadDynamicRows();

        if (testMode) fillTestData();

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

        $(":input").on("blur", function () {
            if ($(this).hasClass("error") && $(this).val() !== "") $(this).removeClass("error");
        });

        $(".currency").on("blur", function () {
            $(this).val(formatAsCurrency($(this).val()));
            calculateRunningTotal();
        });

        // When the form is reset, reload the dynamic data
        $("#glCodingForm").on("reset", function () {
            // Using setTimeout to ensure the code runs *after* the reset.
            setTimeout(function () {
                loadDynamicRows();
                if (testMode) fillTestData();
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
        if (valid) saveDynamicRows();
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

//#region Dynamic Row Functions
// Loads any previously saved rows and initializes the data in Vue
function loadDynamicRows() {
    try {
        traceLog("Loading dynamic rows...");

        // If we've already stored it, re-load the HTML from the table and reset the Vue instance
        // Otherwise, just store the original HTML from the table for later re-use
        if (glTableHtml) {
            $("#glTable").html(glTableHtml);
            glVue = null;
        } else {
            glTableHtml = $("#glTable").html();
        }

        var lines;
        if ($("#glLines").val()) {
            lines = JSON.parse(atob($("#glLines").val()));
            traceLog("Loaded " + lines.length + " existing GL lines");
        } else {
            lines = [{ poNumber: "", glCode: "", glAmount: "", glDescription: "" }];
            traceLog("Initialized dynamic rows without existing GL lines");
        }

        if (!glVue) {
            traceLog("Setting up GL Vue object...");
            glVue = new Vue({
                el: "#glTable",
                data: {
                    lines: lines
                },
                methods: {
                    addGlLine: function () {
                        lines.push({ poNumber: "", glCode: "", glAmount: "", glDescription: "" });
                        rows = lines.length;
                    },
                    deleteGlLine: function (index) {
                        lines.splice(index, 1);
                        rows = lines.length;
                        setTimeout(calculateRunningTotal, 1);
                    }
                }
            });
        } else {
            glVue.lines = lines;
        }

        rows = lines.length;
        setTimeout(calculateRunningTotal, 1);
    } catch (ex) {
        errorLog("Error in loadDynamicRows function!\n\n" + ex);
    }
}

// Saves the Vue GL lines as serialized JSON in base64 to the form
function saveDynamicRows() {
    try {
        traceLog("Saving dynamic rows...");
        $("#glLines").val(btoa(JSON.stringify(glVue.lines)));
    } catch (ex) {
        errorLog("Error in saveDynamicRows function!\n\n" + ex);
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
