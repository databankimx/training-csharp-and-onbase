# Expecting Block-Level Scope

`var` is scoped to the enclosing *function*, not to the block (`{ }`) it happens to be declared inside. A loop written with `var` doesn't get its own private counter - it's reusing the exact same variable the rest of the function can see, and that variable keeps whatever value it last held once the loop finishes.

## The Mistake

```javascript
for (var i = 0; i < 10; i++) {
}
console.log("i after the loop = " + i);
```

This lesson's own page confirms the actual result: `i after the loop = 10`, not an error and not `undefined`. The loop's final increment (`i++` after the last pass) runs before the condition check fails, so `i` ends up one past the last value actually used inside the loop - and it stays there, fully accessible, for the rest of the function.

## The Fix: `let`

```javascript
for (let j = 0; j < 10; j++) {
}
console.log("j after the loop = " + j);   // throws - j doesn't exist out here
```

`let` creates a variable confined to the nearest `{ }` - here, the loop itself. Once the loop ends, `j` doesn't exist anymore at all, which is why this lesson's own fix demo actually throws a `ReferenceError` ("j is not defined") when it tries to reference `j` afterward, rather than returning some leftover value. That thrown error is the concrete proof the scoping actually changed, not just a claim about it.

Prefer `let` for loop counters - and for variables generally - in any code that doesn't need to support very old browsers or a pre-2022 OnBase installation still rendering forms with Internet Explorer 9. See the `var`-only compatibility caveat in the Variables and Data Types lesson for the full reasoning behind when that older constraint still applies.
