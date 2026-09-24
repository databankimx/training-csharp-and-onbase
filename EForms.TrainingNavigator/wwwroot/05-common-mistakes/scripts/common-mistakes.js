"use strict";

$(document).ready(function () {
    // --- Losing Track of "this" ---
    $("#btnThis").on("click", function () {
        try {
            var counter = {
                count: 0,
                increment: function () {
                    this.count++;
                    console.log("Count is now: " + this.count);
                }
            };

            counter.increment();

            // Simulate handing the method off as a bare callback, the way an event
            // handler assignment would -- "this" inside increment() is no longer "counter"
            var detached = counter.increment;
            try {
                detached();
            } catch (ex) {
                console.log("Calling the detached method failed: " + ex);
            }
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Expecting "Block-Level" Scope ---
    $("#btnScope").on("click", function () {
        try {
            for (var i = 0; i < 10; i++) {
                // Do something...
            }
            console.log("i = " + i);
            console.log("This happens because 'var' is hoisted to the top of the enclosing function, " +
                "not scoped to the for-loop's block. Using 'let' instead of 'var' would scope i to the loop.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Functions Created Inside a Loop ---
    $("#btnLoopFunctions").on("click", function () {
        try {
            $("#loopButtons").empty();
            for (var i = 0; i < 3; i++) {
                var btn = document.createElement("button");
                btn.textContent = "Loop Button " + i;
                btn.onclick = function () {
                    console.log("You clicked button " + i);
                };
                document.getElementById("loopButtons").appendChild(btn);
            }
            console.log("Buttons created. Click any of them -- notice they all log the same number.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Memory Leaks ---
    var leakInterval = null;
    $("#btnLeak").on("click", function () {
        try {
            if (leakInterval) {
                clearInterval(leakInterval);
                leakInterval = null;
                console.log("Stopped the leak demo.");
                return;
            }
            var theThing = null;
            var replaceThing = function () {
                var priorThing = theThing;  // hold on to the prior thing
                var unused = function () {
                    // 'unused' is the only place where 'priorThing' is referenced,
                    // but 'unused' never gets invoked
                    if (priorThing) {
                        console.log("hi");
                    }
                };
                theThing = {
                    longStr: new Array(1000000).join("*"),  // create a 1MB object
                    someMethod: function (someMessage) {
                        console.log(someMessage);
                    }
                };
                if (window.performance && window.performance.memory) console.log(window.performance.memory);
            };
            leakInterval = setInterval(replaceThing, 1000);
            console.log("Started the leak demo -- click again to stop it.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Circular Reference & Over-Subscribing ---
    $("#btnCircular").on("click", function () {
        try {
            addClickHandler($("#btnCircular"));
            console.log("Handler added. Click this button again -- each click adds ANOTHER handler on top.");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Inefficient DOM Updates ---
    $("#btnDomUpdates").on("click", function () {
        try {
            var slowTime = slowInsert();
            var fastTime = fastInsert();
            console.log("Direct DOM insertion (2,000 items): " + slowTime.toFixed(2) + "ms");
            console.log("DocumentFragment batch insertion (2,000 items): " + fastTime.toFixed(2) + "ms");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Misunderstanding Prototypal Inheritance ---
    $("#btnInheritance").on("click", function () {
        try {
            function Animal() {}
            Animal.prototype.sounds = [];

            var dog = new Animal();
            var cat = new Animal();
            dog.sounds.push("Woof");
            console.log("cat.sounds = " + JSON.stringify(cat.sounds) + "  (unexpectedly shares dog's array!)");

            function AnimalFixed() {
                this.sounds = [];
            }
            var dog2 = new AnimalFixed();
            var cat2 = new AnimalFixed();
            dog2.sounds.push("Woof");
            console.log("cat2.sounds = " + JSON.stringify(cat2.sounds) + "  (correctly independent)");
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Extracting a Method Loses Its "this" ---
    $("#btnFunctionReference").on("click", function () {
        try {
            var user = {
                name: "Alice",
                greet: function () {
                    console.log("Hello, I'm " + this.name);
                }
            };

            user.greet();

            var greetFn = user.greet;
            greetFn();

            var boundGreet = user.greet.bind(user);
            boundGreet();
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });

    // --- Calling a Function Instead of Referencing It ---
    $("#btnIntervalMistake").on("click", function () {
        try {
            function sayHi() {
                console.log("Hi!");
                return "some value";
            }

            console.log("Calling setInterval(sayHi(), 1000) -- logs once immediately, then never repeats:");
            setInterval(sayHi(), 1000);

            console.log("Calling setInterval(sayHi, 1000) -- logs every second, as intended:");
            var correctInterval = setInterval(sayHi, 1000);
            setTimeout(function () {
                clearInterval(correctInterval);
                console.log("Stopped the correct interval after 3 seconds.");
            }, 3000);
        } catch (ex) {
            console.log("Error!\n" + ex);
        }
    });
});

function addClickHandler(element) {
    element.on("click", function (e) {
        console.log("Clicked the " + element.prop("nodeName"));
    });
}

function slowInsert() {
    var list = document.getElementById("slowList");
    list.innerHTML = "";
    var start = performance.now();
    for (var i = 0; i < 2000; i++) {
        var li = document.createElement("li");
        li.textContent = "Item " + i;
        list.appendChild(li);
    }
    return performance.now() - start;
}

function fastInsert() {
    var list = document.getElementById("fastList");
    list.innerHTML = "";
    var start = performance.now();
    var fragment = document.createDocumentFragment();
    for (var i = 0; i < 2000; i++) {
        var li = document.createElement("li");
        li.textContent = "Item " + i;
        fragment.appendChild(li);
    }
    list.appendChild(fragment);
    return performance.now() - start;
}
