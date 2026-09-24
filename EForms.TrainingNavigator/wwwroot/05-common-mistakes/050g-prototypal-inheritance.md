# Misunderstanding Prototypal Inheritance

JavaScript uses prototypal, not classical, inheritance. That distinction has a concrete practical consequence: a property assigned directly on a constructor's `prototype` is a single, shared value - not a per-instance copy the way a field in a classical language's class would be.

## The Mistake

```javascript
function Animal() {}
Animal.prototype.sounds = [];   // ONE array, shared by every instance

var dog = new Animal();
var cat = new Animal();
dog.sounds.push("Woof");
console.log(cat.sounds);   // ["Woof"] - unexpected!
```

`dog` and `cat` are two separate instances of `Animal`, but they both inherit the exact same `sounds` array from `Animal.prototype` - there's only ever one array in existence, and every instance's `.sounds` points at that same one. Pushing to `dog.sounds` is visible through `cat.sounds` too, because they were never two separate arrays to begin with, just one array being accessed from two different places.

This is the constructor-function equivalent of a mistake that's just as easy to make with the newer `class` syntax - a class field declared without `this` behaves the same shared-by-default way in some patterns, so the underlying concept matters regardless of which syntax you're using.

## The Fix: Assign Instance State Inside the Constructor

```javascript
function AnimalFixed() {
    this.sounds = [];   // a NEW array, created fresh for every instance
}

var dog2 = new AnimalFixed();
var cat2 = new AnimalFixed();
dog2.sounds.push("Woof");
console.log(cat2.sounds);   // [] - as expected
```

Because `this.sounds = []` runs fresh every single time `new AnimalFixed()` is called, each instance gets its own genuinely independent array right from the start. Pushing to one has no effect on the other at all.

As a rule of thumb: put shared *behavior* (methods, which don't hold mutable per-instance state on their own) on the prototype, and put per-instance *state* - anything that should be able to vary independently between different objects, like arrays, objects, or values that get modified over time - inside the constructor, assigned to `this`. Constant, truly shared values (like a fixed configuration value every instance should read but never modify) are a reasonable exception, but anything mutable belongs on `this`, not the prototype.
