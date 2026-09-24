# Truthy/Falsy Values and Type Coercion

This may be the most important JavaScript lesson you'll learn, as it's the source of many issues that don't surface as errors, making them hard to debug.

## `==` vs. `===`, Revisited

JavaScript isn't strongly typed. The `==` operator is willing to convert ("coerce") one or both sides to a common type before comparing them, which can produce a result that doesn't match what the two values actually are. `===` checks type and value together, with no conversion at all, avoiding that entire class of surprise. This is the concrete reasoning behind the "prefer `===`" guidance from the Operators lesson - everything below is *why* that guidance exists.

## Truthy and Falsy Are About Conditionals, Not Equality

"Truthy" and "falsy" describe how a value behaves in a **conditional** context - inside an `if`, or anywhere else a value is used as a yes/no test:

- **Falsy** (acts like `false`): `false` itself, all forms of zero (`0`, `-0`, `0n`), empty strings (`""`, `''`), and also `null`, `undefined`, and `NaN`.
- **Truthy** (acts like `true`): everything else - including values you might not expect, like `{}`, `[]`, and the non-empty strings `"0"` and `"false"`.

That's a *different question* from whether a value literally `==`/`===` `true` or `false` directly - and, as this lesson's own page demonstrates with live, actually-evaluated comparisons (not just asserted claims), the two frequently disagree:

- `null == false` is `false`. `NaN == false` is `false`. `undefined == false` is `false`. All three are falsy in an `if`, yet none of them equal `false` directly.
- `"0" == true` is `false`. `{} == true` is `false`. Even `-1 == true` is `false`. All are truthy in an `if`, yet none equal `true` directly.
- Only `1 == true` (and `1n == true`) actually come out `true` among the truthy candidates tested on this lesson's page.

The one consistent thread: inside an actual conditional (`if (value)`, or a ternary `value ? x : y`), every one of these coerces exactly the way the truthy/falsy list above predicts, regardless of what a direct `==`/`===` comparison against `true`/`false` says. Equality comparison and conditional coercion are two different mechanisms in JavaScript, and they don't always agree with each other - a value can be truthy in an `if` and still fail an equality check against `true` outright.

## Other Coercion: Beyond True/False

Coercion isn't limited to Boolean contexts. `+` is the clearest everyday example, since it's overloaded for both addition and string concatenation, exactly as covered in the Operators lesson:

```javascript
"0" + 1;          // "01" - a string is involved, so + concatenates
1 + Number("1");  // 2    - explicitly converted to a number first, so + adds
```

**Takeaway:** when you need a value treated as a number rather than concatenated as text, convert it explicitly first - `Number(...)` or `parseInt(...)` - rather than relying on `+` to guess what you meant.

## A Different Kind of Gotcha: `document.write()`

Not every JavaScript surprise is about type coercion - this one earns its place here for a different reason. `document.write()` adds content to a page, but calling it on a page that's already finished loading does something most people don't expect: it clears the **entire** existing document first, because it implicitly calls `document.open()` before writing anything. This lesson's own page has a button that demonstrates this for real, wiping the page when clicked.

In practice, this means `document.write()` is only really safe to use while a page is actively still loading - which is how it was originally intended, decades ago - not as a general way to update content afterward. For updating a page once it's already loaded, use `innerHTML` or `appendChild()` instead, the same way every other demo throughout this chapter does.
