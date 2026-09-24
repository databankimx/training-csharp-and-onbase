(function ($) {
    $.fn.dbStylize = function (styleName = "databank", options = {}) {
        var settings = $.extend({
            // put any default options here
        }, options);

        if (!templates.hasOwnProperty(styleName.toLowerCase())) styleName = "databank";

        var template = templates[styleName.toLowerCase()];
        for (var templateProp in template) {
            settings[templateProp] = settings[templateProp] ?? template[templateProp];
        }

        for (var settingsProp in settings) {
            var sel;
            switch (settingsProp) {
            case "text":
            case ".text":
                sel = "body, td, .text";
                settings[settingsProp] += " db-font";
                break;
            case "emph":
            case ".emph":
                sel = "h1, h2, h3, h4, h5, h6, .emph";
                break;
            case "icon":
            case ".icon":
                sel = ".icon";
                break;
            case "logo":
            case ".logo":
                sel = ".logo";
                break;
            case "area":
            case ".area":
                sel = "table, input, textarea, .area";
                break;
            default:
                sel = settingsProp;
                break;
            }

            $(sel).addClass(settings[settingsProp]);
        }

        return this;
    };
}(jQuery));
