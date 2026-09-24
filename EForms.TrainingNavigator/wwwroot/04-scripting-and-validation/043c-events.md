# Events

An event handler is a function that runs in response to something happening - a click, a field losing focus, the page finishing loading. This is where a form actually becomes interactive.

## `window.onload` vs. `$(document).ready()`

Both run a function once the page is "ready," but they mean different things by that word:

| | Fires when... |
|---|---|
| `$(document).ready()` | The HTML structure (the DOM) has been fully parsed - elements exist and can be found and manipulated |
| `window.onload` | Everything has finished loading, including every image, stylesheet, and iframe on the page |

`$(document).ready()` fires first, or at the same time as `window.onload` - never later, since finishing the DOM parse is always a prerequisite for the page finishing loading entirely. On a page with large images or slow embedded content, the gap between the two can be substantial; this lesson's own page measures and displays both timings live, using `performance.now()`, so the actual order is demonstrated rather than just asserted (on a simple page with no slow resources, the gap will usually be tiny, but the order is still real).

In practice: if all you need is to find elements and bind event handlers - which covers most form scripting - use `$(document).ready()`. Reach for `window.onload` only when you specifically need to know that every image or embedded resource has actually finished loading, not just the page structure itself.

> **Why this training set uses both:** the vanilla JavaScript lessons earlier in this chapter use `window.onload`, since jQuery hadn't been introduced yet at that point. Every jQuery lesson from here forward uses `$(document).ready()` instead - the more idiomatic choice once jQuery is available, for the reason explained above.

## Binding Events: `.on()` and Shorthand Methods

`.on()` is the general-purpose way to bind any event to an element:

```javascript
$("#btnOnDemo").on("click", function () {
    // runs when the button is clicked
});
```

Shorthand methods exist for the most common events - `.click(fn)` is equivalent to `.on("click", fn)`, just shorter to type. Both work identically. `.on()`'s advantage is consistency: it handles every event through one method, including events (or combinations of events) with no dedicated shorthand of their own.

`blur` is a particularly useful event for form work - it fires when a field loses focus, and is commonly used to validate or reformat a value right after the user finishes typing into it, rather than validating on every single keystroke.

## Over-Subscribing to Events

When we describe a an even as "bound," we mean that a function has been registered to run when the event occurs. Another name for this is "subscribed" - the function is subscribed to the event, and will be called whenever the event fires.

A common mistake is binding the same element multiple times, especialy if you bind events inside a function that itself runs multiple times. Each time the binding code runs, it adds another subscription to the event, so the handler will run multiple times for a single event. This is called "over-subscribing" to the event.

When over-subscribing happens, the user may see the same effect happen multiple times for a single action - for example, clicking a button may cause the same alert to pop up twice or three times. This can be confusing and frustrating for users.

For this reason, JQuery provides the `.off()` method, which removes all subscriptions to a given event on a given element. This is useful for "resetting" an element's event bindings before re-binding them, to avoid over-subscribing.

```javascript
$("#btnOnDemo").off("click");
```
