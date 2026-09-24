# Control Flow

Control flow is how a script makes decisions - running one block of code instead of another, based on whether a condition is true. Everything in this lesson builds directly on the comparison and logical operators from the previous lesson.

## `if` / `else if` / `else`

```javascript
if (amount <= 0) {
    result = "Amount must be greater than zero.";
} else if (amount < 500) {
    result = "No approval needed - amount is under $500.";
} else if (amount < 5000) {
    result = "Requires manager approval ($500-$4,999).";
} else {
    result = "Requires director approval ($5,000+).";
}
```

Each condition is checked in order, top to bottom. The moment one evaluates to `true`, that branch runs, and every remaining `else if`/`else` is skipped - even if a later condition would also have been `true`. This is why order matters: if the `amount < 5000` check came *before* `amount < 500` above, every amount under 500 would still correctly match it too (since anything under 500 is also under 5000), and the more specific "no approval needed" branch would never be reached.

`else if` isn't its own keyword - it's just an `else` immediately followed by another `if`. You can chain as many as you need, and the final `else` (with no condition) is optional - if you leave it off and none of the conditions match, nothing runs at all.

## `switch`

```javascript
switch (dept) {
    case "AP":
        result = "Routes to Accounts Payable.";
        break;
    case "AR":
        result = "Routes to Accounts Receivable.";
        break;
    case "HR":
    case "Benefits":
        result = "Routes to Human Resources.";
        break;
    default:
        result = "No matching department - routes to General Queue.";
        break;
}
```

`switch` compares one value against a list of possible exact matches. It's often easier to read than a long `if`/`else if` chain when every single branch is just checking the same variable against a different specific value - but it isn't a general replacement for `if`, since each `case` can only check for equality, not a range or a more complex condition.

A few things worth knowing:

- **Comparisons use `===`** (strict equality), with no type coercion - consistent with the general "prefer `===`" guidance from the Operators lesson.
- **`break` matters.** Without it, execution "falls through" into the next `case` below and keeps running - whether you meant it to or not. Forgetting a `break` is a genuinely common source of bugs, since the code will often still *run*, just do more than intended.
- **Falling through is sometimes intentional.** Stacking multiple `case` labels back to back with no code between them - like `"HR"` and `"Benefits"` above - lets several different values share the same result, since neither case has a `break` to stop at.
- **`default` is optional but usually worth including.** It runs when nothing else matched. Leaving it out isn't an error, but a value nobody anticipated will then just silently do nothing, which can be harder to notice than an explicit fallback.

## Logging vs. Alerting

Both `console.log()` and `alert()` show you what's happening while a script runs, but they're suited to different situations, and it's worth being deliberate about which one you reach for:

- **`console.log()`** writes to the browser's developer console, visible without interrupting the page at all. It's the right choice for routine, ongoing information while you're actively debugging - which branch of an `if` chain ran, what a variable's value is at a given point, confirming a function was even called.
- **`alert()`** pops up a modal dialog the user has to dismiss before anything else can happen. Use it sparingly - genuinely unexpected errors, or a message the user actually needs to see and acknowledge right now. Overusing `alert()` for routine debugging gets old fast, both for you during development and for anyone who ends up seeing a stray one you forgot to remove.

> **OnBase note:** there is no way to access the browser console from inside OnBase, under any circumstances. `console.log()` calls are completely silent there - genuinely useful while you're developing and testing a form in a plain browser tab, but invisible once that same script is actually running inside OnBase. Don't rely on console output as your only way of surfacing a problem in a form meant to run in OnBase; a meaningful `alert()` (or writing the information into a visible field on the form itself, as a few earlier lessons in this training set do) is what actually reaches you there.
