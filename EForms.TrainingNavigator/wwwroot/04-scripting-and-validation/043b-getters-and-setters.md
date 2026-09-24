# Getters and Setters

Most jQuery methods that read or change something follow one consistent rule: call it with no arguments to **get** the current value, or with one argument to **set** it.

## The Pattern

```javascript
$("#myField").val();      // GET - same as document.getElementById("myField").value
$("#myField").val("x");   // SET - same as ...value = "x"
```

This same pattern runs through most of jQuery, not just `.val()`:

- **`.val()`** - an input's value
- **`.text()`** - an element's text content (escapes HTML automatically)
- **`.html()`** - an element's inner HTML content (does *not* escape HTML)
- **`.attr("name")`** / **`.attr("name", "value")`** - an HTML attribute
- **`.css("property")`** / **`.css("property", "value")`** - a style property

Once you know the pattern, you can usually guess how an unfamiliar jQuery method works: no arguments reads, one argument writes.

## A jQuery-Specific Quirk: `.html(false)`

`.html()` is worth a specific callout, because it behaves unexpectedly with a raw Boolean value. Passing `false` directly doesn't populate the element with the text "false" the way you might expect - the safer, reliable approach is converting the value to a string explicitly first:

```javascript
$("#result").html(false);              // unreliable
$("#result").html(false.toString());   // reliably shows "false"
```

**Takeaway:** when writing a Boolean result with `.html()`, convert it to a string first with `.toString()` rather than passing the raw Boolean directly. This is exactly why every truthy/falsy demonstration throughout this training set does that conversion explicitly before calling `.html()` - it isn't unnecessary caution, it's working around this specific behavior.
