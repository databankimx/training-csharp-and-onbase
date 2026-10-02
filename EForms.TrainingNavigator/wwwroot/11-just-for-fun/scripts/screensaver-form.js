"use strict";

// When true - report trace logging to console
var debugMode = true;

// When true - report trace logging with user alert
var debugAlert = false;

// When true, don't perform screen-saver actions
var paused = false;

// Position and movement variables for screen-saver
var w, h, x, y, mh, mv, ci;

// Time since last mouse movement (milliseconds)
var lastMouse = 0;

// Interval time (in milliseconds) - how often the bounce/countdown loops tick
var int = 10;

// Wait time (in seconds) before the screen saver activates - adjustable via the slider,
// defaults to 30
var delay = 30;

// The last whole-second countdown value actually written to the page, so the DOM only
// gets touched once a second rather than on every 10ms tick
var lastDisplayedSeconds = null;

// Logo images to display as screen-saver, one per DataBank brand color
var logos = [
    { name: "navy", url: "images/icons/icon_databank_navy.svg", bg: "#FFFFFF", bc: "#00263D" },
    { name: "databank blue", url: "images/icons/icon_databank_blue.svg", bg: "#00263D", bc: "#00263D" },
    { name: "light blue", url: "images/icons/icon_databank_light_blue.svg", bg: "#00263D", bc: "#DBE7FF" },
    { name: "off white", url: "images/icons/icon_databank_off_white.svg", bg: "#00263D", bc: "#EDF0F7" },
    { name: "white", url: "images/icons/icon_databank_white.svg", bg: "#00263D", bc: "#00263D" },
    { name: "hot coral", url: "images/icons/icon_databank_hot_coral.svg", bg: "#FFFFFF", bc: "#FF7669" },
    { name: "gold fusion", url: "images/icons/icon_databank_gold_fusion.svg", bg: "#00263D", bc: "#00263D" },
    { name: "electric green", url: "images/icons/icon_databank_electric_green.svg", bg: "#FFFFFF", bc: "#00263D" },
    { name: "electoral teal", url: "images/icons/icon_databank_electoral_teal.svg", bg: "#00263D", bc: "#00263D" },
    { name: "spark purple", url: "images/icons/icon_databank_spark_purple.svg", bg: "#FFFFFF", bc: "#00263D" }
];

$(document).ready(function () {
    // Initialize global variables
    Initialize();

    // Hide the overlay layer used when the screen-saver is active
    $("#overlay").hide();

    // When the browser is resized, recompute the window X,Y size
    $(window).on("resize", function () {
        GetResolution();
    });

    // Wire up the delay slider - moving it updates `delay` immediately, live
    $("#delaySlider").on("input", function () {
        delay = parseInt($(this).val(), 10);
        $("#delayValue").text(delay);
        traceLog("Delay set to " + delay + " seconds");
    });
    $("#delayValue").text(delay);

    // Check every (interval) milliseconds to determine whether to move the screen-saver logo
    var logoLoop = setInterval(function () {
        if (paused) return;
        MoveLogo();
    }, int);

    // Check every (interval) milliseconds to see whether the mouse has been idle long
    // enough to activate the screen saver, and keep the on-page countdown current
    var mouseLoop = setInterval(function () {
        if (lastMouse > delay * 1000) {
            paused = false;
            if (!$("#logo").is(":visible")) {
                $("#logo").show();
                $("#overlay").show();
                $("#countdown").text("Screen saver active - move your mouse to stop it");
                lastDisplayedSeconds = null;
                traceLog("Displaying screen saver...");
            }
        } else {
            paused = true;
            if ($("#logo").is(":visible")) {
                $("#logo").hide();
                $("#overlay").hide();
                traceLog("Hiding screen saver...");
            }
            var remaining = Math.ceil((delay * 1000 - lastMouse) / 1000);
            // Only touch the DOM when the displayed number of seconds actually changes,
            // rather than re-writing it on every 10ms tick
            if (remaining !== lastDisplayedSeconds) {
                $("#countdown").text(remaining + " second" + (remaining === 1 ? "" : "s") + " until screen saver");
                lastDisplayedSeconds = remaining;
            }
        }
        lastMouse += int;
    }, int);

    // When the mouse is moved, reset the idle timer, which turns off the screen-saver
    $(window).on("mousemove", function () {
        lastMouse = 0;
    });
});

// Obtain a random integer
function getRandomInt(min, max) {
    try {
        if (isNaN(min) || isNaN(max)) {
            throw "Min and Max values must be numbers!";
        }
        return Math.floor(Math.random() * (max - min + 1) + min);
    } catch (ex) {
        errorLog("Error in getRandomInt() function!\n\n" + ex);
        return null;
    }
}

// Randomly switch the background icon
function swapIcon() {
    try {
        var i = ci;
        while (i === ci) i = getRandomInt(0, logos.length - 1);
        var logo = logos[i];
        $("#logo").css("background-color", logo.bg);
        $("#logo").css("border-color", logo.bc);
        $("#logo").css("background-image", "url(" + logo.url + ")");
        ci = i;
        traceLog("Set screen saver icon to " + logo.name);
    } catch (ex) {
        errorLog("Error in swapIcon() function!\n\n" + ex);
    }
}

// Initialize global variables
function Initialize() {
    try {
        mh = mv = 1;
        x = y = 10;
        ci = 0;
        GetResolution();
    } catch (ex) {
        errorLog("Error in Initialize() function!\n\n" + ex);
    }
}

// Obtain the X,Y dimensions of the browser window
function GetResolution() {
    try {
        w = $(window).width();
        h = $(window).height();
        $("#overlay").css("width", w + "px");
        $("#overlay").css("height", h + "px");
        traceLog("Window resolution detected: " + w + " x " + h);
    } catch (ex) {
        errorLog("Error in GetResolution() function!\n\n" + ex);
    }
}

// Move the logo image around the screen
function MoveLogo() {
    try {
        // Safety clamp: if the logo has somehow drifted out of bounds, reset it to its
        // starting position (10, 10 -- matching Initialize() and #logo's own CSS)
        if (x > w - 100 || x < 0) x = 10;
        if (y > h - 100 || y < 0) y = 10;
        if (x === 0 || x + 102 === w) {
            mh *= -1;
            swapIcon();
        }
        if (y === 0 || y + 102 === h) {
            mv *= -1;
            swapIcon();
        }
        x += mh;
        y += mv;
        $("#logo").css("top", y);
        $("#logo").css("left", x);
    } catch (ex) {
        errorLog("Error in MoveLogo() function!\n\n" + ex);
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
