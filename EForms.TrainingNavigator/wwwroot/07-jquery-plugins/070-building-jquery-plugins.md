# Building jQuery Plugins

A jQuery plugin is just a function attached to `$.fn` - the prototype every jQuery object inherits from. Once attached, it's callable on any jQuery selection exactly like a built-in method (`$(...).greenify()`). This lesson builds one up step by step, from the simplest possible version to a properly structured, reusable plugin.

## 1. Basic Structure

```javascript
$.fn.greenify = function () {
    this.css("color", "green");
};
```

Inside the function, `this` refers to the matched jQuery selection the plugin was called on - so `this.css(...)` applies to every element the caller selected.

## 2. Supporting Chaining

jQuery methods are chainable (`$(...).addClass(...).css(...)`) because each one returns `this`. A plugin that doesn't do the same breaks the chain for whoever calls it next:

```javascript
$.fn.greenify = function () {
    this.css("color", "green");
    return this;   // <- without this, .greenify().addClass(...) would fail
};
```

## 3. Protecting the `$` Alias

Not every page can assume `$` means jQuery - other libraries (Prototype, MooTools) use the same alias for themselves, and if more than one is loaded, whichever loads last "wins" the global `$`. Wrapping the plugin in an **Immediately-Invoked Function Expression (IIFE)** that takes jQuery as a parameter named `$` sidesteps the whole problem:

```javascript
(function ($) {
    $.fn.greenify = function () { ... };
}(jQuery));
```

Inside this function, `$` is guaranteed to mean jQuery, regardless of what it means anywhere else on the page - because it's a local parameter, not a reference to whatever the global `$` currently happens to be.

## 4. Private, Scoped Variables

The same IIFE wrapper doubles as a private scope. A variable declared inside it is invisible outside, and isolated from name collisions with anything else on the page:

```javascript
(function ($) {
    var shade = "#556b2f";
    $.fn.greenify = function () {
        this.css("color", shade);
        return this;
    };
}(jQuery));
```

## 5. Minimizing Footprint

Every plugin method you attach to `$.fn` is one more thing to document, maintain, and potentially collide with someone else's naming. Prefer one plugin that takes an argument to select its behavior over several separate, narrowly-named plugins:

```javascript
// Bad -- two plugins for two closely related behaviors
$.fn.openPopup = function () { ... };
$.fn.closePopup = function () { ... };

// Better -- one plugin, the argument picks the behavior
$.fn.popup = function (action) {
    if (action === "open") { ... }
    if (action === "close") { ... }
};
```

## 6. Using `.each()`

A caller might select more than one element - `this` inside the plugin is the whole set, not a single element. `this.each(...)` iterates the full selection while still returning `this` at the end, keeping the plugin chainable:

```javascript
$.fn.myNewPlugin = function () {
    return this.each(function () {
        // "this" here (not the outer "this") is one single element
    });
};
```

## 7. Accepting Options

`$.extend(defaults, options)` merges a caller-supplied options object over a set of defaults, so the caller only needs to specify what they actually want to change:

```javascript
$.fn.greenify = function (options) {
    var settings = $.extend({ color: "#556b2f", backgroundColor: "white" }, options);
    return this.css({ color: settings.color, backgroundColor: settings.backgroundColor });
};

$(...).greenify({ color: "orange" });  // backgroundColor still falls through to "white"
```

## 8 & 9. Putting It Together, Then Optimizing

The final two steps build a small, genuinely useful plugin - appending each matched link's own `href` after its visible text - first with an explicit `.each()` loop, then a shorter equivalent using `.append()`'s function-argument form, which is called once per matched element automatically:

```javascript
// Explicit .each()
this.filter("a").each(function () {
    var link = $(this);
    link.append(" (" + link.attr("href") + ")");
});

// Equivalent, using .append()'s own per-element function support
this.filter("a").append(function () {
    return " (" + this.href + ")";
});
```

Both produce the same result; the second is shorter once you know `.append()` supports this form, but the first is worth understanding on its own since the same "function called once per element" pattern shows up throughout jQuery's API.

See the next lesson for a complete, real-world plugin built on everything covered here.
