# Functions Created Inside a Loop

A direct consequence of the previous lesson's scoping issue, common enough to earn its own lesson: a closure created inside a `var`-based loop captures the *variable*, not the value it happened to hold at the moment the closure was created.

## The Mistake

```javascript
for (var i = 0; i < 3; i++) {
    var btn = document.createElement("button");
    btn.onclick = function () {
        console.log("This button logs: " + i);
    };
    container.appendChild(btn);
}
```

This lesson's own page builds three real, clickable buttons from this exact code. Click any of them, and every single one logs `3` - never `0`, `1`, or `2`, regardless of which button you actually click, even though only buttons labeled "Button 0" through "Button 2" were ever created. Since `var` creates only one `i` for the whole loop (as covered in the previous lesson), every closure created inside the loop shares that same variable. By the time any button is actually clicked - well after the loop has already finished running - `i` holds its final value: `3`, one step past the last value the loop body itself actually used, since the increment runs once more before the loop's condition finally fails.

## The Fix: `let`

```javascript
for (let j = 0; j < 3; j++) {
    var btn = document.createElement("button");
    btn.onclick = function () {
        console.log("This button logs: " + j);
    };
    container.appendChild(btn);
}
```

With `let` in place of `var`, each button correctly logs its own number instead. `let` creates a fresh, independent binding of `j` for *every single pass* through the loop - not just a scope confined to the loop as a whole, but a genuinely separate variable each time around. Each closure captures its own value rather than all of them sharing one, which is exactly why this lesson's fix demo's three buttons each report a different number.

This is one of the most common practical reasons to prefer `let` over `var` for loop counters - not the scoping quirk on its own from the previous lesson, but this specific, very common downstream consequence of it: any time a loop creates functions (event handlers, callbacks, anything stored to run later), `var` will silently produce this exact bug.
