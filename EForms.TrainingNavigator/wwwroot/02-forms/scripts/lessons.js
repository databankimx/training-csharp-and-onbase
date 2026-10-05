// Initialize global variables if not already defined
if (typeof useData === 'undefined') {
    var useData = false;
}
if (typeof pageName === 'undefined') {
    var pageName = "basic";
}

var fields = [];

$(document).ready(function () {
    $("#data-modal").hide();

    $(":input").each(function () {
        // Note: Query string data will contain field names not IDs
        if ($(this).prop("name").substring(0, 3) !== "btn") fields.push($(this).prop("name"));
    });
    $("textarea").on("focus", function () {
        if ($(this).html() === "Default Text") $(this).html("");
    });
    $("#btnCancel").on("click", function () {
        alert("Form cancelled...");
        $("#btnReset").click();
        return false;
    });
    $("form").on("submit", function () {
        if (!useData) showPostData();
        return false;
    });
    $("#modal-close").on("click", function () {
        $("#data-modal").hide();
    });
    //showFormData();
});

function showPostData() {
    log("Post data submitted...");
    var action;
    switch (pageName) {
        case "cq":
            action = "searched";
            break;
        case "uf":
            action = "used";
            break;
        default:
            action = "saved";
            break;
    }
    if (pageName === "cq") action = "searched";
    var data = "The following POST data will be " + action + ":<br><br>";
    var first = true;
    $(":input").each(function () {
        // jQuery's .val() on a checkbox/radio always returns its static value attribute,
        // regardless of whether it's actually checked - so unchecked ones need to be
        // explicitly excluded here, matching real form submission behavior.
        var type = $(this).prop("type");
        if ((type === "checkbox" || type === "radio") && !$(this).prop("checked")) return;
        if ($(this).val()) {
            log($(this).prop("name") + " = " + $(this).val());
            if (!first) data += "&amp;";
            data += encodeURIComponent($(this).prop("name")) + "=" + encodeURIComponent($(this).val());
            first = false;
        }
    });
    $("#data-content").html(data);
    $("#data-modal").show();
    return false;
}

function showFormData() {
    try {
        var urlParams = new URLSearchParams(window.location.search);
        var count = 0;

        for (const element of fields) {
            var val = urlParams.get(element);
            if (!val) continue;
            count++;
            if (count === 1) log("Form data received...");
            log(element + ": " + val);
        }

        if (count === 0) {
            log("No data submitted...");
        }
    } catch (ex) {
        log(ex, true);
    }
}

function log(message, isError = false) {
    if (isError) message = "***** ERROR *****\n" + message;
    if (window.console) console.log(message);
    else alert(message);
}
