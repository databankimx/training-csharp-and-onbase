var debug = false;

$(document).ready(function () {
    try {
        if (!debug) $(".debug").hide();
    } catch (ex) {
        logError(ex, "document.ready");
    }
});

function log(message) {
    if (window.console) console.log(message);
    if (debug) $("#debugInfo").html($("#debugInfo").html() + "\n\n" + message);
}

function logError(ex, src) {
    message = "*** ERROR in " + src + " method ***\n" + ex;
    log(message);
}
