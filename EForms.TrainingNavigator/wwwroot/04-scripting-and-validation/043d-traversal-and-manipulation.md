# Traversal and Manipulation

Beyond finding elements and reading/writing their values, jQuery provides a consistent set of tools for moving around the DOM relative to what you've already found, and for changing the structure of the page itself.

## `.each()`

The jQuery equivalent of the `.forEach()` array method from the JavaScript chapter - runs a function once for every element in a jQuery collection:

```javascript
$("#glTable .amount").each(function () {
    // "this" refers to the current element on this pass
});
```

Inside the callback, `this` refers to whichever element the current pass is on - the same role `this` plays inside a constructor function (covered in the Functions lesson), just applied once per item in a loop instead of once per call to `new`. Wrap it in `$(this)` to use jQuery methods on it.

## `.find()`

Searches **within** the elements you've already matched, rather than the whole page:

```javascript
$("#glTable").find(".amount");   // same as: $("#glTable .amount")
```

Both forms above do the same thing here, but `.find()` is often clearer (and sometimes the only option) when the element you're searching within is already stored in a variable, rather than something you'd write out as a single selector string.

## `.parent()`

Moves from an element up to its immediate containing element - useful when you've found something specific (a cell with a value you care about) but actually need to act on its container (the whole row):

```javascript
var row = $(this).parent();   // from a <td>, up to its containing <tr>
row.addClass("over-limit");
```

## `.addClass()` / `.removeClass()`

Add or remove a CSS class from an element. This is almost always a cleaner way to change how something looks than setting individual style properties directly with `.css()` - the visual details live in a stylesheet, and the script just toggles a class name on or off based on a condition. This lesson's own page uses this exact pattern to highlight GL distribution rows over a dollar threshold.

## `.append()`

Adds new content as the last child of the matched element:

```javascript
$("#glTable tbody").append(
    "<tr><td>PO-4474</td><td class='amount'>250</td></tr>"
);
```

This is how a form dynamically adds a new row to a table at runtime - exactly the mechanism a "add another distribution line" button would use on a real GL coding form. Because `.find()`-based logic searches the *live* DOM each time it runs, code written to process "every row in the table" will automatically pick up rows added this way afterward, with no extra work needed to keep track of them separately.
