# Basics of jQuery

jQuery is a JavaScript library, not a separate language - everything from the Basics of JavaScript chapter still applies. What jQuery adds is a consistent, shorter way to do the things that come up constantly in form scripting: finding elements, reading and writing their values, handling events, and calling web services.

## Why It Exists

In its own words, jQuery is "a fast, small, and feature-rich JavaScript library. It makes things like HTML document traversal and manipulation, event handling, animation, and Ajax much simpler with an easy-to-use API that works across a multitude of browsers." In practice: less code, more consistent behavior across browser versions, and less rework needed when a browser changes something underneath you.

jQuery is especially good at reducing the code needed to consume web services and REST APIs - see the `Samples.AsmxWebService.WebClient`, `Samples.MvcWebApi.WebClient`, and `Samples.WcfService.WebClient` projects elsewhere in this solution for further examples beyond the AJAX lesson later in this chapter.

## `$` and the Core Pattern

`$` is shorthand for `jQuery` - `$.ajax` and `jQuery.ajax` are the same thing. Nearly all of jQuery's functionality follows one pattern:

```javascript
$(selector).method(args);
```

The selector works like a CSS selector (covered in depth in the Selectors lesson), and the method that follows either reads or changes something about whatever the selector matched.

## Which Version, and OnBase Compatibility

These lessons use jQuery 3.6.0. OnBase forms have rendered using a Chromium-based engine since v22 (2022) - modern jQuery works without issue there. Older OnBase documentation floating around may mention jQuery 2.x compatibility concerns; that only applied when OnBase rendered forms as Internet Explorer 9, prior to v22, and no longer applies on a current installation.

## What's Ahead

1. **Selectors** - finding the elements you want to work with
2. **Getters and Setters** - the get-vs-set pattern that runs through almost every jQuery method
3. **Events** - responding to what the user does, including `$(document).ready()` vs. `window.onload`
4. **Traversal and Manipulation** - moving around the DOM and changing it
5. **AJAX** - calling a web service from a form, using a real example service
