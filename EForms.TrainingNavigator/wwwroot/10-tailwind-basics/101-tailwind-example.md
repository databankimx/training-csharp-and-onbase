# Basics of Tailwind CSS

The same "Murphy's Law Form" from the Bootstrap chapter, rebuilt with Tailwind CSS utility classes - worth comparing side by side to see how a utility-first framework differs from Bootstrap's component-first approach.

> **Delivery note:** this lesson loads Tailwind from the Play CDN (`https://cdn.tailwindcss.com`) so the demo needs no build step. In production you would compile a static `tailwind.css` with the Tailwind CLI and reference that single file instead - see the introduction lesson for why that matters for OnBase.

## How This Differs From Bootstrap

- **No component classes**: there is no `.form-control`, `.btn`, or `.input-group`. Every element is styled by composing utilities directly in the markup, e.g. `px-3 py-2 border border-slate-300 rounded` for a text input.
- **Grid**: instead of `.container`/`.row`/`.col-sm-*`, this form uses `grid grid-cols-12 gap-2` with `col-span-*` on each cell. The responsive prefix `sm:` switches from stacked (`col-span-12`) to side-by-side (`sm:col-span-2` / `sm:col-span-10`).
- **Input groups**: the decorative icon addon is just a flex container (`flex`) with a styled `<span>` and an input that grows (`flex-1 min-w-0`). Rounded corners are applied per side (`rounded-l` / `rounded-r`) to mimic Bootstrap's `.input-group`.
- **Buttons**: the Save/Cancel buttons compose color, padding, radius, and hover states as utilities (`px-4 py-2 rounded bg-green-600 hover:bg-green-700 text-white`) rather than relying on `.btn .btn-success`.

## What Stayed the Same

The button-forwarding pattern is identical to the Bootstrap lesson: a visible styled button with `data-target` clicks a real, hidden `OBBtn_Save`/`OBBtn_Cancel` submit input, and the same small jQuery script wires it up. Tailwind only changes how the form is styled, not how it talks to OnBase. Font Awesome is still used for the icons, exactly as in the Bootstrap 5 lesson, since icon libraries are independent of your CSS framework.

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

3. **Use the same Tailwind utilities** for any new UI elements (buttons, error messages, etc.) so they integrate seamlessly with the form's existing design.

As a stretch goal, compile a static `tailwind.css` with the Tailwind CLI, swap the Play CDN `<script>` for a `<link>` to your compiled file, and confirm the form looks identical - that is the deployment model you would actually ship to OnBase.
