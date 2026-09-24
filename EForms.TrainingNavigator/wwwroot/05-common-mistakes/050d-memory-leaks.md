# Memory Leaks

JavaScript is garbage-collected, but that doesn't mean memory management is automatic in every sense - a reference held somewhere unexpected can keep an object alive indefinitely, even when nothing obviously visible is still using it.

## The Mistake

```javascript
function replaceThingLeaky() {
    var priorThing = leakyThing;
    var unused = function () {
        if (priorThing) console.log("this never runs");
    };
    leakyThing = {
        bigData: new Array(1000000).join("*"),   // a ~1MB string
        unused: unused,   // keeps the closure - and priorThing - reachable
    };
}
```

`unused` is never actually *called* anywhere in this function. But as long as it exists - and it does, since it's stored as a property on the new `leakyThing` object - it keeps `priorThing` reachable, purely because a closure keeps its **entire enclosing scope** alive for as long as the closure itself is reachable, even variables the closure never actually touches when it runs. Every previous "thing" this function has ever created stays pinned in memory, one after another, none of them ever eligible for collection.

## The Fix: Don't Capture What You Don't Need

```javascript
function replaceThingFixed() {
    var priorThing = leakyThing;
    leakyThing = {
        bigData: new Array(1000000).join("*"),   // a ~1MB string
    };
}
```

Same function, with the unnecessary closure removed entirely. With nothing capturing `priorThing`, nothing keeps the old object reachable once `leakyThing` is reassigned - it becomes eligible for garbage collection right away.

## Why This Demo Is Illustrative, Not a Precise Benchmark

This lesson's page runs each version 300 times in a row, using a larger (~1MB) leaked object per call than an earlier version of this demo used, and reports the change in `window.performance.memory.usedJSHeapSize` (a Chrome-only API - the demo will say so plainly in another browser). Both the "before" and "after" readings are taken only after a short settling delay with nothing else running, giving the browser a genuine opportunity to run garbage collection if it's going to - rather than measuring immediately, back to back, with no idle time at all for a GC pass to occur.

Even with that improvement, garbage collection timing is still not fully deterministic: the browser decides when to actually reclaim memory, not your code, so any single run's numbers can still occasionally be noisy. Click each "Run 300 Times" button a few times in a row to see a clearer trend rather than judging from one run - the *general pattern* (the fixed version growing the heap noticeably less than the leaky one, consistently across repeated runs) is the real takeaway, not any single exact number.

The broader lesson generalizes well beyond this one specific example: a closure keeps everything in its enclosing scope alive for as long as the closure itself is reachable - even variables it never actually uses. Watch for closures that capture more than they need, especially ones stored somewhere long-lived (an event handler that's never removed, an object property, a module-level variable).
