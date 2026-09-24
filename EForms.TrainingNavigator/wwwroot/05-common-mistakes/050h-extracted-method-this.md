# Extracting a Method Loses Its "this"

Closely related to the "Losing Track of \"this\"" lesson earlier in this chapter, but worth its own lesson: pulling a method off an object into a standalone variable detaches it from that object entirely - not just when it's assigned as an event handler, but any time it's separated from the object it was originally called on.

## The Mistake

```javascript
var user = {
    name: "Alice",
    greet: function () {
        return "Hello, I'm " + this.name;
    }
};

user.greet();                  // "Hello, I'm Alice" - called ON the object, works fine

var greetFn = user.greet;      // just the function itself, no connection to "user"
greetFn();                     // "Hello, I'm " - this.name is now window.name, not "Alice"
```

`greetFn` looks like it should behave exactly like `user.greet` - it's the same function, after all. But once it's assigned to a plain variable, JavaScript has no way of knowing it was ever associated with `user` at all. Calling `greetFn()` is an ordinary function call, with `this` pointing at the global object instead.

Worth being precise about the exact result here: in a browser, the global object happens to have its own built-in `name` property (used for cross-window/cross-frame targeting), which defaults to an empty string rather than being `undefined` - so the result is a name that's silently blank, not an obviously broken value or a thrown error. That specific detail depends on this one particular built-in property; the general problem - `this` no longer pointing at `user` at all - is what actually matters, and it generalizes to any object, not just one with a property name that happens to collide with something built in.

## The Fix: Bind It Before Handing It Off

```javascript
var boundGreet = user.greet.bind(user);
boundGreet();                  // "Hello, I'm Alice"
```

`.bind(user)` locks `this` to `user` permanently, the moment it's called - `boundGreet` behaves correctly no matter how it's later called, passed around, or assigned elsewhere.

> **When this actually comes up:** passing a method as a callback - to `setTimeout`, to an array method like `.forEach()`, or storing it somewhere for later use - is functionally the same situation as extracting it into a variable first. Any time a method is handed off rather than called directly on its object, bind it first unless you're certain `this` genuinely won't be needed inside it.
