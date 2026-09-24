# Calling a Function Instead of Referencing It

`setInterval` and `setTimeout` both expect a **function reference** as their first argument - something they can call themselves, later, on their own schedule. Adding `()` calls the function **immediately** instead, and passes whatever it *returns* to `setInterval` in its place.

## The Mistake

```javascript
function sayHi() {
    console.log("Hi!");
}

setInterval(sayHi(), 1000);   // sayHi() runs RIGHT NOW - its return value goes to setInterval
```

`sayHi()` - with the parentheses - runs the instant this line is evaluated, before `setInterval` even has a chance to schedule anything at all. Whatever `sayHi()` returns (nothing explicit here, so `undefined`) is what `setInterval` actually receives as its first argument - not a function it can call later, just a value. `setInterval` never got a function reference to work with, so nothing genuinely repeats, no matter how long you wait afterward.

This lesson's page counts how many times the logic inside `sayHi` actually runs over a fixed three-second window, rather than asserting a specific claim about what `setInterval` does internally with a non-function value (which can vary - some environments accept a *string* as this argument and evaluate it later via an implicit `eval`, which is a different, messier situation than simply doing nothing). What's reliably, observably true either way: the counter only ever reaches `1` - the single immediate call - and never climbs further.

## The Fix: Pass the Function Itself

```javascript
setInterval(sayHi, 1000);   // correct - sayHi itself is scheduled to run every second
```

No `()` this time - `sayHi` by itself refers to the function, without calling it. `setInterval` now holds an actual reference it can call on its own schedule, once every 1000ms. This lesson's fix demo shows the counter climbing to roughly `3` over the same three-second window, confirming the function is genuinely being called repeatedly rather than just once.

If a repeating interval "only fires once" or "doesn't seem to be repeating at all," a stray pair of parentheses on the function being scheduled is the very first thing worth checking.
