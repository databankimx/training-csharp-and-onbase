# Losing Track of "this"

A method called *on* its object has `this` set correctly, pointing at that object. Hand the same function off as a bare reference, though, and that connection is lost entirely - `this` becomes whatever actually calls the function, not the object it was originally attached to.

## The Mistake

```javascript
var detached = counter.increment;
detached();   // "this" inside increment() is no longer "counter"
```

This lesson's own page demonstrates the concrete, tested result: called as a plain function like this, `this` becomes the global object, which has no `count` property - so `this.count++` silently produces `NaN` rather than throwing an error. That silence is exactly what makes this mistake easy to miss; nothing crashes, it just quietly does the wrong thing.

The far more common real-world trigger is assigning a method directly as an event handler:

```javascript
document.getElementById("btn").onclick = someObject.someMethod;
```

This looks completely correct - it reads like "call `someMethod` when the button is clicked" - but `this` inside `someMethod` becomes the button element that was clicked, not `someObject`. Any code inside that method expecting `this` to refer to the original object will silently operate on the wrong thing, or fail trying to read a property the button doesn't have.

## The Fix: `.bind()`

`Function.prototype.bind()` returns a new function with `this` permanently locked to whatever object you pass it, regardless of how that new function is later called:

```javascript
var bound = counter.increment.bind(counter);
bound();   // "this" is "counter", no matter how "bound" is called
```

Applied to the event-handler case above:

```javascript
document.getElementById("btn").onclick = someObject.someMethod.bind(someObject);
```

Now `this` inside `someMethod` is always `someObject`, regardless of what actually triggers the call. This is the standard fix whenever a method genuinely needs to be handed off elsewhere as a callback - bind it to the object it belongs to at the moment you hand it off, rather than assuming `this` will still make sense by the time it's actually called.
