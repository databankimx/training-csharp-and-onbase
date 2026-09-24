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
    $("form").on("submit", function (e) {
        if (!useData) showPostData(e);
        return false;
    });
    $("#modal-close").on("click", function () {
        $("#data-modal").hide();
    });
    //showFormData();
});

function showPostData(e) {
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
    positionDataModal(e);
    $("#data-modal").show();
    return false;
}

// Positions #data-modal just above whichever button triggered the submit (a negative offset
// relative to the button - i.e. higher up the page, not lower), so it stays on screen even on
// long forms. e.originalEvent.submitter (standard on the native SubmitEvent) identifies exactly
// which submit button was clicked; if that's ever unavailable, falls back to the form's first
// submit-type button, which by convention in this project's forms is the Save-equivalent button.
// Mirrors the same offset formula 035-onbase-form-buttons.html already uses for its own buttons.
function positionDataModal(e) {
    var submitter = e && e.originalEvent && e.originalEvent.submitter;
    var anchor = submitter ? $(submitter) : $("form").find(":submit").first();
    var top = 20;
    if (anchor && anchor.length) {
        var buttonTop = anchor.offset().top;
        top = buttonTop > 120 ? buttonTop - 100 : 20;
    }
    $("#data-modal").css("top", top);
}

function showFormData() {
    try {
        var urlParams = new URLSearchParams(window.location.search);
        var count = 0;

        for (var i = 0; i < fields.length; i++) {
            var val = urlParams.get(fields[i]);
            if (!val) continue;
            count++;
            if (count === 1) log("Form data received...");
            log(fields[i] + ": " + val);
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
