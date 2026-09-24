$(document).ready(function () {
    var debug = false;

    var templateNames = ["databank", "cloud", "energy", "government", "gold", "coral"];

    try {
        var selTemplate = templateNames[Math.floor(Math.random() * templateNames.length)];
        $("#formatName").html(selTemplate);

        // call as dbStylize(<<template name - optional>>, <<options object - optional as {"selector": "css class(es)", ...}>>)
        // Current defined templates are [databank, cloud, energy, government, gold, coral]
        $(document).dbStylize(selTemplate, { "h2": "italic underline" });
    } catch (ex) {
        if (window.console) console.log(ex);
        if (debug) alert(ex);
    }
});
