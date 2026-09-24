# Inefficient DOM Updates

Every time you touch the live DOM - appending a node, changing text, adding a class - the browser has to consider whether it needs to recalculate layout. That check has a real, measurable cost, and it adds up fast when it happens thousands of times in a tight loop.

## The Mistake

```javascript
var list = document.getElementById("myList");
for (var i = 0; i < 2000; i++) {
    var li = document.createElement("li");
    li.textContent = "Item " + i;
    list.appendChild(li);   // DOM touched 2,000 times
}
```

Each `appendChild()` call here touches the actual, rendered document - 2,000 separate opportunities for the browser to reconsider layout, even though only the very last one actually matters for what the user sees once the loop finishes.

## The Fix: Build Off-DOM First, Attach Once

```javascript
var list = document.getElementById("myList");
var fragment = document.createDocumentFragment();
for (var i = 0; i < 2000; i++) {
    var li = document.createElement("li");
    li.textContent = "Item " + i;
    fragment.appendChild(li);
}
list.appendChild(fragment);   // DOM touched once
```

A `DocumentFragment` is a lightweight container that isn't part of the rendered page at all - appending to it costs nothing in layout terms, since the browser has nothing to actually reconsider. Build the entire structure there first, then attach the whole thing to the real document in a single `appendChild()` call at the end. The browser only has to do its layout work once, for the final result, instead of 2,000 times along the way.

## A More Reliable Demo Than the Memory Leak Lesson

This lesson's page times both approaches directly, inserting the same 2,000 items each way and reporting the actual milliseconds elapsed. Unlike the memory-leak comparison earlier in this chapter, this timing doesn't depend on unpredictable garbage collection - it's simply measuring how long a loop takes to run, which is consistent and repeatable. Run both versions a few times and compare the numbers directly; the fragment version should come out consistently ahead, and the gap tends to widen as the item count grows larger.

This same principle applies to jQuery, too - `.append()` on a large batch of new content benefits from being built up as a single string or `DocumentFragment` first and attached once, rather than appending piece by piece inside a loop, for exactly the same reason.
