# Basics of Bootstrap v5

The same "Murphy's Law Form" from the previous lesson, rebuilt in Bootstrap 5 - worth comparing side by side to see what changed between versions.

> **Version note:** this lesson pulls Bootstrap 5.3.8, the latest stable 5.x release at the time of writing.

## What Changed From v3

- **Icons**: Bootstrap 3's bundled `glyphicon` font is gone entirely in v5 - Bootstrap dropped its own icon font. This lesson uses [Bootstrap Icons](https://icons.getbootstrap.com/) (`bi bi-*`) for inline field icons, and [Font Awesome](https://fontawesome.com/) (`fa fa-*`) for the button icons - both loaded as separate libraries alongside Bootstrap itself, not bundled.
- **`.input-group-addon` → `.input-group-text`**: the class for a decorative addon inside an input group was renamed.
- **No more jQuery dependency in Bootstrap itself**: Bootstrap 5's own JavaScript (dropdowns, modals, etc.) no longer requires jQuery. This lesson still loads jQuery - but only for its own button-forwarding script, not because Bootstrap needs it.
- **Column proportions**: this lesson uses `col-sm-2` / `col-sm-10` rather than v3's `col-sm-3` / `col-sm-9` - a different proportion choice for this rebuild, not a v3-vs-v5 requirement. The grid system itself (rows summing to 12) is unchanged between versions.
- **`placeholder` attributes added**: a small addition on top of the direct v3 port, showing hint text in each empty field.

## What Stayed the Same

The core building blocks - `.container`, `.row`, `.col-<size>-<span>`, `.form-control`, `.input-group` - all carry over conceptually unchanged. The button-forwarding pattern (a visible `.btn` with `data-target` clicking a real, hidden `OBBtn_Save`/`OBBtn_Cancel` input) is identical to the v3 lesson too, right down to the JavaScript.

## Bootstrap Icons vs. Font Awesome

This lesson deliberately uses both libraries side by side - Bootstrap Icons for the input-group icons, Font Awesome for the button icons - as a way of showing that neither is exclusive to Bootstrap. Either can be dropped into any project regardless of which CSS framework (or none) it uses; picking one over the other is a matter of preference and icon selection, not a Bootstrap requirement.
