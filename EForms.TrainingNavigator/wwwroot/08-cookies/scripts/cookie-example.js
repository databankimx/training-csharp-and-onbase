"use strict";

var debugMode = true;
var debugAlert = false;

var nameCookieInfo = {
    name: "user",
    ask: "Please enter your name...",
    say: "Welcome back"
};

window.onload = function () {
    try {
        refreshCookieStatus();

        document.getElementById("btnSetCookie").onclick = function () {
            var name = document.getElementById("nameInput").value;
            if (name === "") {
                alert("Please enter a name first!");
                return;
            }
            setCookie(nameCookieInfo.name, name, 10);
            traceLog("Cookie set: " + nameCookieInfo.name + "=" + name);
            refreshCookieStatus();
        };

        document.getElementById("btnClearCookie").onclick = function () {
            clearCookie(nameCookieInfo.name);
            traceLog("Cookie cleared: " + nameCookieInfo.name);
            document.getElementById("nameInput").value = "";
            refreshCookieStatus();
        };
    } catch (ex) {
        errorLog(ex);
    }
};

// Update the visible status area and input to reflect whatever cookie currently exists
function refreshCookieStatus() {
    try {
        var value = getCookie(nameCookieInfo.name);
        var status = document.getElementById("cookieStatus");
        if (value === "") {
            status.innerHTML = "No saved name found.";
        } else {
            status.innerHTML = nameCookieInfo.say + ", <strong>" + value + "</strong>!";
            document.getElementById("nameInput").value = value;
        }
    } catch (ex) {
        errorLog(ex);
    }
}

// Sets a cookie (name/value pair with expiration [minutes])
function setCookie(name, value, expiration) {
    try {
        var d = new Date();
        d.setTime(d.getTime() + expiration * 60 * 1000);
        document.cookie = name + "=" + value + ";expires=" + d.toUTCString() + ";path=/";
    } catch (ex) {
        errorLog(ex);
    }
}

// Reads a cookie (name/value pair)
function getCookie(name) {
    try {
        var cookieData = decodeURIComponent(document.cookie);
        var cookies = cookieData.split(";");
        for (var c = 0; c < cookies.length; c++) {
            var cookie = cookies[c].trim();
            if (cookie.toLowerCase().indexOf(name.toLowerCase() + "=") === 0) return cookie.substring(name.length + 1, cookie.length);
        }
    } catch (ex) {
        errorLog(ex);
    }
    return "";
}

// Clears a cookie by re-setting it with an expiration in the past, which is the standard
// way to delete a cookie - there's no dedicated "delete" API for document.cookie.
function clearCookie(name) {
    try {
        setCookie(name, "", -1);
    } catch (ex) {
        errorLog(ex);
    }
}

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
