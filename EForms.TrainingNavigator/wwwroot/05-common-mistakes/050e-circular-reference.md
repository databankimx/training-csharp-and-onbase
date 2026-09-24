# Circular Reference & Over-Subscribing

An event handler that adds another handler to the same element every time it fires compounds without bound - a classic case of a handler that resubscribes itself instead of just running its logic directly.

## The Mistake

```javascript
function addHandler(element) {
    element.on("click", function () {
        // ... do the actual work ...
        addHandler(element);   // attaches ANOTHER handler, every time
    });
}
```

`.on()` doesn't replace an existing handler - it adds another one alongside whatever's already attached. So every time this handler fires, it does its work *and* attaches a brand new copy of itself, leaving the original one still attached too. The number of handlers actually attached to the element doubles with every single click: 1, then 2, then 4, then 8...

This lesson's own page tracks exactly how many times the handler logic actually runs, verified by simulation before writing this lesson: cumulative fires follow 1, 3, 7, 15, 31 after five clicks - each click running roughly double the number of times the one before it did, since every handler currently attached fires on every click, and each of those firings attaches one more. A few dozen clicks in, a single click would already be running the handler's logic many thousands of times.

## The Fix: Subscribe Once, Run Directly

```javascript
element.on("click", function () {
    // ... do the actual work, and nothing else ...
});
```

Subscribed exactly once, with nothing inside the handler re-subscribing it. Now the number of times the logic runs always matches the number of times the element was actually clicked - one for one, no matter how many times you click.

`.on()` is for attaching a handler once - when the page loads, or when an element is first created - not something to call again from inside the handler itself. If you ever find `.on()` being called from within an event handler for the same element it's already attached to, that's almost always a sign something has gone wrong, not a deliberate pattern.
