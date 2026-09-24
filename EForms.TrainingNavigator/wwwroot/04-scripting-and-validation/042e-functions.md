# Functions

A function packages up a piece of logic so it can be reused, called with different inputs, and triggered by events - a button click, a field losing focus, the page finishing loading. Nearly everything a form actually *does* lives inside a function somewhere.

## Declarations vs. Expressions

```javascript
// Declaration
function calculateTax(amount, rate) {
    return amount * rate;
}

// Expression - the same thing, assigned to a variable instead
var calculateTaxExpr = function (amount, rate) {
    return amount * rate;
};
```

Both define a function that behaves identically. The practical difference is *when* each becomes available: a named function declaration is "hoisted" - available throughout the whole script, even on lines written before its own declaration - while a function expression only exists from the line it's assigned, the same as any other variable. In practice, either works fine for typical form scripting; declarations are usually the more readable default, since you don't have to think about where in the file something is defined relative to where it's used.

## Arrow Functions

```javascript
var calculateTaxArrow = (amount, rate) => amount * rate;
```

A more compact syntax, especially handy for short, one-off functions. For a simple calculation like this, it behaves identically to the declaration and expression versions above.

One real behavioral difference is worth knowing, though: arrow functions don't have their own `this` - they use whatever `this` refers to in the surrounding code instead. This rarely matters for a short calculation, but it matters a great deal for the constructor pattern below, which is exactly why that example uses a regular function rather than an arrow.

## Parameters, Defaults, and Return Values

```javascript
function greetVendor(name, greeting = "Hello") {
    return greeting + ", " + name + "!";
}
```

A parameter can have a default value, used only when the caller leaves that argument out of the call entirely (not when an explicit value like `""` or `null` is passed - only when it's actually missing). A function that doesn't explicitly `return` anything returns `undefined` automatically - worth remembering when a function seems to silently "not work," since a missing `return` is a common, easy-to-overlook cause.

## The Old-Style "Constructor Function" Pattern

Before the `class` keyword existed (added in ES6, 2015), a plain function was the way to create class-like objects - each with its own set of properties and methods. You'll see this pattern throughout existing OnBase forms, and it still works everywhere, which is exactly why it's worth understanding well:

```javascript
function Vendor(name, id) {
    this.name = name;
    this.id = id;
    this.describe = function () {
        return this.name + " (Vendor ID: " + this.id + ")";
    };
}

var vendor = new Vendor("Acme Supply", "V-1042");
```

Calling a function with `new` in front of it does something different from an ordinary function call: JavaScript creates a brand-new, empty object first, then runs the function with `this` pointing at that new object. Whatever you assign to `this` inside the function - simple properties like `name` and `id`, or even entire functions, like `describe` above - becomes part of the resulting object. Call `new Vendor(...)` again with different arguments, and you get a completely separate, independent object with its own `name` and `id` - the function is really a *template* for creating objects, not the object itself.

> **OnBase note:** the modern `class` keyword works fine on a current, Chromium-based OnBase installation, but not on an older one still rendering forms with Internet Explorer 9 - the same underlying compatibility issue as `let`/`const`, covered in the Variables and Data Types lesson earlier in this chapter. The constructor-function pattern above works everywhere, on every OnBase version, which is why it's still the safer pattern to actually *write* - even though recognizing `class` when you encounter it in newer code is still worth knowing. This comes up again, in a bit more depth, in the next lesson.
