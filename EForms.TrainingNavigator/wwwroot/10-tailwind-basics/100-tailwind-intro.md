# Introduction to Tailwind CSS

Tailwind CSS is a utility-first CSS framework. Instead of shipping pre-built components like Bootstrap's `.btn` or `.card`, Tailwind gives you a large set of small, single-purpose utility classes (`flex`, `pt-4`, `text-center`, `bg-blue-500`) that you compose directly in your markup to build any design.

## Utility-First vs. Component-First

Bootstrap is component-first: you drop in ready-made components and override them when you need something custom. Tailwind is utility-first: you build components out of low-level utilities, so you are never fighting the framework's opinions about how a button or form should look.

| Concept | Bootstrap | Tailwind |
| --- | --- | --- |
| Grid | `.container`, `.row`, `.col-sm-6` | `grid grid-cols-12`, `flex`, `gap-*` |
| Spacing | utility helpers plus component padding | `p-4`, `mt-2`, `gap-3` utilities everywhere |
| Buttons | `.btn .btn-primary` component | compose `px-4 py-2 rounded bg-blue-600 text-white` |
| Customization | override Sass variables / CSS | configure `tailwind.config.js` design tokens |

## How Tailwind Is Delivered

This is the most important point for OnBase e-forms: Tailwind is a build-time tool. There is no server component and no runtime JavaScript requirement. The normal workflow runs the Tailwind CLI, which scans your markup, finds the utility classes you actually used, and generates a single static `.css` file. You then reference that file with an ordinary `<link>` tag, exactly like any other stylesheet.

- **Production (recommended):** compile a static `tailwind.css` with the CLI and deploy that single file alongside your form. Only the classes you used are included, so the file stays small.
- **Demo / prototyping only:** the Tailwind Play CDN (`<script src="https://cdn.tailwindcss.com"></script>`) compiles utilities in the browser on the fly. It is convenient for quick demos like the next lesson, but it is not meant for production because it ships the full engine and runs at runtime.

## Why This Fits OnBase

Because the production output is just a plain compiled stylesheet, Tailwind is as safe for OnBase e-forms as hand-written CSS. There is nothing to install on the server, nothing that runs at request time, and no external runtime dependency. You compile once, commit the resulting CSS, and reference it from the form.

## What's Next

The next lesson rebuilds the familiar Murphy's Law Form in Tailwind so you can compare it directly to the Bootstrap 5 version from the previous chapter. For convenience the demo uses the Play CDN, but everything it shows compiles down to the same static CSS you would deploy in production.
