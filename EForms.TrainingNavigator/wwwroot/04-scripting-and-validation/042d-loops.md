# Loops

A loop repeats a block of code - across every row of a GL distribution table, every item in a list of keywords, every property on an object. JavaScript has several loop styles, each suited to a slightly different situation.

## `for`

```javascript
for (var i = 0; i < glAmounts.length; i++) {
    lines.push("Row " + (i + 1) + ": $" + glAmounts[i].toFixed(2));
}
```

The classic loop: an initializer (`var i = 0`), a condition checked before every pass (`i < glAmounts.length`), and an increment that runs after every pass (`i++`), all declared together up front. This is the right choice when you need the index itself - to number rows, to compare adjacent items, or to only process every other item, for example.

## `while`

```javascript
while (total < 500 && i < glAmounts.length) {
    total += glAmounts[i];
    i++;
}
```

Repeats for as long as a condition stays true, with no built-in counter - you're responsible for managing that yourself (as `i` is here). The condition is checked **before** each pass, so if it's already false the very first time, the loop body never runs at all. This is the natural choice when you're looping until some condition is met, rather than a fixed number of times - like adding line items to a running total until it crosses a threshold.

## `do...while`

```javascript
do {
    lines.push("Ran with i = " + i);
    i++;
} while (i < glAmounts.length);
```

The same idea as `while`, but the condition is checked **after** each pass instead of before - so the body is guaranteed to run at least once, no matter what. This lesson's own page demonstrates that concretely by starting `i` at 99 (already past the end of the array) and showing the loop still runs exactly once before stopping. Genuinely useful any time you want to guarantee "do this at least once, then keep going only if needed" - but it's a less common need than the other loop styles, so reach for it specifically when that guarantee actually matters.

## `for...of`

```javascript
for (var amount of glAmounts) {
    lines.push("$" + amount.toFixed(2));
}
```

Iterates over the **values** in an array (or any other "iterable") directly - no index variable to declare or manage at all. Use this whenever you just need each value in turn and don't care about its position.

## `for...in`

```javascript
for (var key in invoiceRecord) {
    lines.push(key + ": " + invoiceRecord[key]);
}
```

Iterates over the **keys** (property names) of an object, not an array. Access each value with `invoiceRecord[key]`, using the key as a dynamic property name.

> **Don't mix these up:** `for...of` is for arrays and gives you each *value*. `for...in` is for objects and gives you each *key*. Using `for...in` on an array will technically run without an error, but gives you the array's numeric indexes as strings (`"0"`, `"1"`, `"2"`...) rather than the values themselves - almost never what you actually want, and an easy mistake to make since both names look so similar. Use `for...of` (or a plain `for` loop) for arrays, and `for...in` for objects.
