"use strict";

$(document).ready(function () {
    // --- 1. Basic Plugin Structure ---
    $("#btnBasic").on("click", function () {
        try {
            $.fn.greenify = function () {
                this.css("color", "green");
            };
            $("#basicTarget a").greenify();
            console.log("Applied basic greenify() to #basicTarget links.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 2. Supporting Chaining ---
    $("#btnChaining").on("click", function () {
        try {
            $.fn.greenify = function () {
                this.css("color", "green");
                return this;
            };
            $("#chainingTarget a").greenify().addClass("greenified");
            console.log("Chained greenify().addClass('greenified') -- only possible because greenify() returns 'this'.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 3. Protecting the $ Alias ---
    $("#btnAlias").on("click", function () {
        try {
            (function ($) {
                $.fn.greenify = function () {
                    this.css("color", "green");
                    return this;
                };
            }(jQuery));
            console.log("Defined greenify() inside an IIFE. Inside that function, $ is guaranteed to mean jQuery, " +
                "regardless of what $ means anywhere else on the page.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 4. Private, Scoped Variables ---
    $("#btnPrivateVars").on("click", function () {
        try {
            (function ($) {
                var shade = "#556b2f";
                $.fn.greenify = function () {
                    this.css("color", shade);
                    return this;
                };
            }(jQuery));
            $("#privateVarsTarget a").greenify();
            console.log("Applied greenify() using a private 'shade' variable. " +
                "Try typing 'shade' in this console -- it doesn't exist out here.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 5. Minimizing Footprint ---
    $("#btnFootprint").on("click", function () {
        try {
            $.fn.popup = function (action) {
                if (action === "open") console.log("popup('open') called -- open logic runs here");
                if (action === "close") console.log("popup('close') called -- close logic runs here");
            };
            $(document).popup("open");
            $(document).popup("close");
            console.log("One plugin (popup) handled two related behaviors via an argument, " +
                "instead of two separate plugins (openPopup/closePopup).");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 6. Using .each() ---
    $("#btnEach").on("click", function () {
        try {
            $.fn.myNewPlugin = function () {
                return this.each(function () {
                    $(this).css("font-style", "italic");
                });
            };
            $("#eachTarget a").myNewPlugin();
            console.log("Applied myNewPlugin() to every matched link via .each().");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 7. Accepting Options ---
    $("#btnOptions").on("click", function () {
        try {
            (function ($) {
                $.fn.greenify = function (options) {
                    var settings = $.extend({
                        color: "#556b2f",
                        backgroundColor: "white"
                    }, options);
                    return this.css({
                        color: settings.color,
                        backgroundColor: settings.backgroundColor
                    });
                };
            }(jQuery));
            $("#optionsTarget div").greenify({ color: "orange" });
            console.log("Called greenify({ color: 'orange' }) -- only 'color' was overridden; " +
                "'backgroundColor' fell through to its default.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 8. Putting It Together ---
    $("#btnTogether").on("click", function () {
        try {
            (function ($) {
                $.fn.showLinkLocation = function () {
                    this.filter("a").each(function () {
                        var link = $(this);
                        link.append(" (" + link.attr("href") + ")");
                    });
                    return this;
                };
            }(jQuery));
            $("#togetherTarget a").showLinkLocation();
            console.log("Appended each link's own href after its text, using .each().");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- 9. An Optimized Variant ---
    $("#btnOptimized").on("click", function () {
        try {
            (function ($) {
                $.fn.showLinkLocation = function () {
                    this.filter("a").append(function () {
                        return " (" + this.href + ")";
                    });
                    return this;
                };
            }(jQuery));
            $("#optimizedTarget a").showLinkLocation();
            console.log("Same result as step 8, but .append() took a function directly instead of a manual .each() loop.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });
});
