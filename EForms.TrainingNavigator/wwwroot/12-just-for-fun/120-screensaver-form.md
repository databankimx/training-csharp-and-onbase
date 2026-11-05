# Screen Saver

While this is mostly just a silly demo, there are a few worthwhile lessons to be found in here. I originally created this just to show how `setInterval` works - the DVD-logo-style bouncing DataBank icon that takes over after the page has sat idle is really just an excuse to put two independent `setInterval` loops, and the `mousemove` event, to work.

## `setInterval`, Twice Over

Two separate `setInterval` loops drive the whole page, both ticking every `int` (10) milliseconds:

- **`logoLoop`** calls `MoveLogo()`, which nudges the bouncing icon's `x`/`y` position by its current direction (`mh`/`mv`, each `1` or `-1`), and flips that direction - plus swaps to a random new icon color - whenever it reaches an edge.
- **`mouseLoop`** tracks how long it's been since the last mouse movement (`lastMouse`), incrementing it by `int` on every tick. Once that exceeds `delay * 1000` milliseconds, the screen saver's `#logo` and `#overlay` become visible; the on-page countdown text is also kept current here, throttled to update only once a second rather than on every 10ms tick (see below).

`setInterval` doesn't guarantee its callback fires at exactly the interval you give it - if the browser is busy, ticks can run a little late - but for something like this, where the loop is just re-checking elapsed time on every pass rather than counting ticks, that's fine: `lastMouse` still ends up accurate regardless of exactly when each tick actually ran.

## `mousemove`: Resetting the Idle Timer

```javascript
$(window).on("mousemove", function () {
    lastMouse = 0;
});
```

Every mouse movement anywhere on the page resets `lastMouse` back to zero, which is what keeps the screen saver from ever activating while you're actively using the page. Attaching this to `window` rather than some more specific element means it fires regardless of what's actually under the cursor.

## Setting the Delay: A Slider, Not a Field

The delay before the screen saver activates is controlled by a range slider (5-second steps, from 5 to 60 seconds, defaulting to 30) rather than hardcoded:

```javascript
$("#delaySlider").on("input", function () {
    delay = parseInt($(this).val(), 10);
    $("#delayValue").text(delay);
});
```

Moving the slider updates the `delay` variable immediately, live - the very next `mouseLoop` tick picks up the new value, since it's read directly from the shared variable rather than cached anywhere.

## The Countdown, Throttled

The page shows either a live countdown ("12 seconds until screen saver") or, once it's active, a simple "Screen saver active" message. Since `mouseLoop` ticks every 10ms but the countdown only needs whole-second precision, updating the DOM on every tick would mean touching it 100 times a second for text that visibly changes once a second at most. A small guard fixes that:

```javascript
var remaining = Math.ceil((delay * 1000 - lastMouse) / 1000);
if (remaining !== lastDisplayedSeconds) {
    $("#countdown").text(remaining + " second" + (remaining === 1 ? "" : "s") + " until screen saver");
    lastDisplayedSeconds = remaining;
}
```

Only writing to the page when the displayed number actually changes keeps this cheap regardless of how fast the underlying loop ticks.

## A Real Bug, Fixed

The original version of `MoveLogo()` had a genuine bug:

```javascript
if (x > w - 100 || x < 0) x = b;
if (y > h - 100 || y < 0) y = b;
```

`b` was never declared anywhere in the script. Under `"use strict"`, referencing it throws a `ReferenceError` the moment the logo drifts out of bounds - silently breaking the animation loop from that point on (the surrounding `try`/`catch` swallows the error, so nothing visibly crashes, but the logo simply stops moving correctly). This version uses `10` instead - matching both `Initialize()`'s own starting position (`x = y = 10`) and `#logo`'s CSS (`left: 10px; top: 10px;`), which is clearly what this fallback was meant to reset to.

A second, smaller bug turned up while cleaning this lesson up further: `MoveLogo()` was also updating a `#logoBg` element that doesn't exist anywhere in the HTML - harmless (jQuery quietly does nothing when a selector matches zero elements), but dead code left over from some earlier version. Removed.

A third: the icon shown before the very first edge-bounce (before `swapIcon()` ever runs) comes straight from `#logo`'s own CSS, not from the `logos` array - and that CSS had `background-color: #00263D` (navy) paired with a navy-colored icon, making it invisible against its own background the first time the screen saver activates. `logos[0]` (the entry `ci` starts on) actually pairs the navy icon with a white background for contrast; the CSS default now matches that.

## Random Color Swapping, Without Repeats

```javascript
function swapIcon() {
    var i = ci;
    while (i === ci) i = getRandomInt(0, logos.length - 1);
    ...
    ci = i;
}
```

The `while` loop guarantees the newly-picked icon is never the same as the current one (`ci`) - a simple, effective way to avoid the mildly unsatisfying case of a "random" color swap that happens to pick the same color twice in a row. The `logos` array now covers all ten DataBank brand colors from the branding reference chapter (`images/icons/`), not just the original seven.
