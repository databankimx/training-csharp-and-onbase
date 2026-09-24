# Selectors

Every jQuery call starts with a selector - a string identifying which element(s) to work with, using the same syntax you already know from CSS.

## The Basics

| jQuery | Selects | Vanilla JS equivalent |
|---|---|---|
| `$("#myField")` | The element with that `id` | `document.getElementById("myField")` |
| `$(".myClass")` | Every element with that class | `document.getElementsByClassName("myClass")` |
| `$("input")` | Every element with that tag | `document.getElementsByTagName("input")` |
| `$("input[name='x']")` | Every `<input>` with that attribute value | `document.querySelectorAll("input[name='x']")` |

The practical advantage over vanilla JS isn't capability - `querySelectorAll` can do everything a jQuery selector can - it's consistency. One syntax covers id, class, tag, and attribute matching, rather than remembering a different method name for each.

## A Selector Always Returns a Collection

Even `$("#myField")`, matching a single unique `id`, technically returns a jQuery collection - it just happens to contain exactly one element. This matters most with `name` or class selectors, which commonly match more than one element at once (radio buttons and checkboxes sharing a `name`, for example, or several fields sharing a class). Use `.first()` to take just the first match, or `.each()` (covered in the Traversal and Manipulation lesson) to work with every match in the collection.

This lesson's own page demonstrates the same field found four different ways - by `id`, by `name`, by tag, and by class - to make the point concrete:

```javascript
switch (findType) {
    case "id":
        el = $("#kwDescription");
        break;
    case "name":
        el = $("input[name='OBKey__1_1']").first();
        break;
    case "tag":
        el = $("input").first();
        break;
    case "class":
        el = $(".keyword");
        break;
}
```

In practice, selecting by `id` is by far the most common approach - it's unambiguous, fast, and exactly why every field throughout this training set has an `id`, even on fields where it isn't otherwise required for anything else.

This is the primary reason that although OnBase only uses the `name` attribute to identify fields, we recommend that you always give your fields an `id` as well. It makes scripting easier, and it makes your scripts more robust to changes in the form design.
