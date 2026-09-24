# Variables and Data Types

A variable is a named container for a value - a way to store something (a number, some text, the result of a calculation) once and refer back to it by name, rather than writing the value out every time.

## Declaring a Variable: `var`, `let`, and `const`

```javascript
var kwDescription = "TEST DATA";
```

That's the pattern you'll use for the vast majority of OnBase form scripting. JavaScript has two newer ways to declare a variable, `let` and `const`, both introduced in ES6 (2015):

- `let` behaves like `var`, but is **block-scoped** rather than function-scoped (see below).
- `const` behaves like `let`, but the variable can never be reassigned after it's first set - useful for values that genuinely shouldn't change, like a fixed maximum or a configuration value.

> **OnBase note:** stick to `var` unless you're certain your form will only ever render on a modern, Chromium-based OnBase installation (v22, 2022, or later). Older installations render forms using Internet Explorer 9, which predates `let`/`const` entirely - a script using either one will simply fail on that engine. Since you often can't be sure in advance which engine a given deployment is actually running, `var` is the safe default for anything meant to work broadly.

### `var` Is Function-Scoped; `let`/`const` Are Block-Scoped

This is the real practical difference between them, not just which keyword you type. A variable declared with `var` is visible throughout the entire function it's declared in - even if it's declared inside an `if` block or a loop:

```javascript
function example() {
    if (true) {
        var x = 1;
    }
    console.log(x); // 1 - x "leaked" out of the if block
}
```

The same code with `let` would throw an error on that `console.log` line, since `x` only exists inside the `if` block's own `{ }`. This is rarely a problem in short, simple scripts, but it's worth knowing about - it's the kind of thing that can cause a confusing bug in a longer function where a variable name gets reused in more than one block.

## Primitive Data Types

JavaScript is **loosely typed**: a variable isn't declared as holding a particular type, and the same variable can hold a string at one point and a number later if you reassign it. The `typeof` operator tells you what type a value currently is:

```javascript
var kwDescription = "TEST DATA";
typeof kwDescription; // "string"
```

The primitive types you'll encounter:

| Type | Example | Notes |
|---|---|---|
| `string` | `"TEST DATA"` | Text, in single or double quotes (both work identically) |
| `number` | `1250.5` | Every number - there's no separate integer/float/decimal type like in C# |
| `boolean` | `false` | Just `true` or `false` |
| `null` | `null` | An explicit, intentional "no value," assigned deliberately |
| `undefined` | `undefined` | What a variable holds before anything has ever been assigned to it |
| `bigint` | `9007199254740993n` | For integers too large for `number` to represent precisely; rare in typical form work |

A couple of things worth knowing:

- `null` and `undefined` are both forms of "nothing," but they mean different things. `undefined` is what JavaScript gives you automatically; `null` is what *you* assign when you want to explicitly say "this has no value." A field's value being `undefined` usually means something went wrong (the field wasn't found); a field's value being `null` is a legitimate, intentional state.
- `typeof null` reports `"object"`, not `"null"`. This is a long-standing bug in the language itself, dating back to JavaScript's earliest implementation - it can't be fixed without breaking a huge amount of existing code on the web, so it's permanent. Just something to remember, not something to expect will ever change.
- There's no separate integer type. `10` and `1250.5` are both just `number` under the hood.

This lesson only covers *recognizing* a value's type via `typeof`. How JavaScript compares and converts values *between* types - where a lot of the language's trickier behavior lives - is covered in depth in the Truthy/Falsy Values and Type Coercion lesson later in this chapter.
