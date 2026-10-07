# Basics of Bootstrap v3

Bootstrap is DataBank's preferred framework for building responsive (adaptive) forms - predefined styles and layout classes that automatically adjust to different window sizes, without writing your own media queries for every layout. Both this lesson and the next build the exact same form ("Murphy's Law Form") twice, once per major Bootstrap version, so you can compare them directly.

## The Grid System

- **`.container`** - a centered, max-width wrapper for the whole form.
- **`.row`** - groups a set of columns together as one horizontal row.
- **`.col-<size>-<span>`** - sizes a column within its row. `<size>` is a breakpoint (`sm`, `md`, `lg`, etc. - the size of screen this sizing applies at); `<span>` is how many of the row's 12 grid units that column occupies. **Every row's spans must total exactly 12.**

This lesson uses `col-sm-3` / `col-sm-9` for its label/field pairs - a 3:9 split, giving labels a quarter of the row's width.

## Form Controls

- **`.form-control`** - Bootstrap's standard input styling, applied based on the input's type.
- **`.input-group`** / **`.input-group-addon`** - groups an input with a decorative addon (here, a `glyphicon` icon) so they render as one visually joined control.
- **`glyphicon glyphicon-*`** - Bootstrap 3's own icon font, bundled with the framework itself.

## The Button-Forwarding Pattern

The visible Save/Cancel buttons aren't the real OnBase submit buttons - they're plain `.btn`s with a `data-target` attribute. A small script does the actual forwarding:

```javascript
$(".buttons").on("click", function () {
    $("#" + $(this).data("target")).click();
});
```

Clicking the styled button programmatically clicks the real (hidden, via the `.invisible` class) `OBBtn_Save`/`OBBtn_Cancel` input. This lets the form use Bootstrap's button styling directly on the visible control while keeping the actual OnBase-recognized submit button exactly as OnBase expects it - unstyled and untouched.

See the next lesson for the same form rebuilt in Bootstrap 5, and what changed between the two versions.

## Recommended Practice Exercise

**Create a web service for Murphy's Laws autofill:**

This form is designed to look up Murphy's Law records by ID and autofill the Law Name and Law Text fields. The database already contains a `MurphysLaws` table with law records (id, name, text).

1. **Create a REST API endpoint** in your ASP.NET Core application (following the AJAX lesson pattern):
   - Route: `POST /api/murphyslaw/lookup`
   - Input: JSON with a `lawId` property
   - Output: JSON with `name` and `text` properties from the matching law record
   - Error handling: return a 404 if the law ID is not found

2. **Add AJAX autofill logic** to the Murphy's Law Form:
   - When the user enters a Law ID and presses Tab or clicks a button, call your new endpoint
   - On success, autofill the Law Name and Law Text fields with the response data
   - On error, display a message to the user (e.g., "Law ID not found")
   - Handle the same asynchronous behavior and error patterns covered in the AJAX lesson

3. **Use the same Bootstrap styling** for any new UI elements (buttons, error messages, etc.) so it integrates seamlessly with the form's existing design.

This exercise combines three key concepts: Bootstrap form layout and styling, creating a real REST API endpoint, and AJAX-driven form interaction with error handling.


