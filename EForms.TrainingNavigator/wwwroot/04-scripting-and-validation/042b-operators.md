# Operators

Operators combine, compare, or transform values. JavaScript's set will feel familiar if you've worked in almost any other language, with a few quirks worth calling out.

## Arithmetic Operators

`+`, `-`, `*`, `/`, `%` (remainder/modulo), and `**` (exponent) work the way you'd expect from basic math. `++`/`--` increment or decrement a variable by 1.

The one JavaScript-specific surprise: `+` is overloaded for both addition *and* string concatenation, and which one happens depends on the types involved:

```javascript
3 + 2;      // 5  (both numbers - addition)
"3" + 2;    // "32"  (a string is involved - concatenation, not addition)
```

This is one small piece of a much larger topic (how JavaScript converts values between types), covered fully in the Truthy/Falsy Values and Type Coercion lesson later in this chapter.

## Comparison Operators

`<`, `>`, `<=`, and `>=` work as expected. Equality is where JavaScript has its own behavior: `==`/`!=` compare after converting the two values to a common type if they differ, while `===`/`!==` compare both type and value with no conversion at all.

```javascript
0 == false;   // true  - coerced first
0 === false;  // false - different types, no coercion, so not equal
```

Default to `===`/`!==` unless you have a specific reason to want coercion - it avoids an entire category of subtle bugs. The full reasoning, and a list of which values coerce to `true`/`false`, is in the Truthy/Falsy Values and Type Coercion lesson.

## Logical Operators: `&&`, `||`, `!`

These combine or invert `boolean` values. The truth tables on this lesson's own page show every possible input/output combination for each - worth internalizing, since conditions built from these three operators come up constantly in validation logic:

- **`&&` (AND)** - `true` only when *both* sides are `true`
- **`||` (OR)** - `true` when *either* side is `true`
- **`!` (NOT)** - flips a single value: `!true` is `false`, `!false` is `true`

> **Watch out for:** `&` and `|` (a single character) look like typos of `&&` and `||`, but they're a completely different pair of operators - bitwise AND/OR, covered below - not logical AND/OR. They produce a *number*, not a `boolean`, and JavaScript won't throw an error if you use one by mistake, so the bug is silent rather than obvious. Double-check you have the right number of characters when typing `&&`/`||`.

## Bitwise Operators

These operate on the actual binary representation of a number, one bit at a time, rather than on the number as a whole - a different job entirely from the logical operators above, despite the similar-looking symbols. They're rarely needed for typical form-scripting work, but worth recognizing if you come across them:

| Operator | Meaning | Example |
|---|---|---|
| `&` | Bitwise AND | `5 & 3` -> `1` |
| `\|` | Bitwise OR | `5 \| 3` -> `7` |
| `^` | Bitwise XOR | `5 ^ 3` -> `6` |
| `~` | Bitwise NOT | `~5` -> `-6` |
| `<<` | Left shift | `5 << 1` -> `10` |
| `>>` | Right shift | `5 >> 1` -> `2` |

Every result above is a number, not `true`/`false` - concrete proof of why mixing these up with the logical operators causes a silent, confusing bug rather than an obvious error.

## Assignment Operators

`=` assigns a value. The others combine assignment with an operation, saving you from repeating the variable name:

```javascript
var total = 0;
total += 10;   // same as: total = total + 10;
total -= 3;    // same as: total = total - 3;
```

## Operator Precedence

When an expression mixes more than one operator, JavaScript evaluates them in a specific order - not simply left to right. This is exactly like regular math, where multiplication happens before addition even with no parentheses:

```javascript
2 + 3 * 4;    // 14, not 20 - * is evaluated before +
(2 + 3) * 4;  // 20 - parentheses always override the default order
```

The same idea applies to `&&` and `||`, and it's a genuinely common source of bugs, because it's easy to assume left-to-right evaluation when it isn't happening: `&&` binds tighter than `||`, so it's evaluated first even without any parentheses at all.

```javascript
true || false && false;
// evaluated as: true || (false && false)  -->  true
// NOT as:       (true || false) && false  -->  false
```

Both expressions use the exact same values and operators, but the ordering changes the outcome entirely. The page for this lesson has a full precedence table, from highest to lowest priority, but the practical takeaway matters more than memorizing the whole table: **when mixing `&&` and `||` in the same expression, add parentheses explicitly.** Don't rely on precedence rules being obvious to whoever reads the code next - including yourself, six months later.
