# DataBank Branding Reference

The canonical source for DataBank's brand colors, icons, logos, and typography - the shared assets referenced throughout this training site (`EForms.01.HtmlLessons/styles/`) come from here.

## The Color Palette

Nine brand colors, each with three utility classes:

| Color | Hex | Text class | Background class | Border class |
|---|---|---|---|---|
| Navy | `#00263D` | `.navy` | `.bg-navy` | `.border-navy` |
| DataBank Blue | `#427DFF` | `.databank-blue` | `.bg-databank-blue` | `.border-databank-blue` |
| Light Blue | `#DBE7FF` | `.light-blue` | `.bg-light-blue` | `.border-light-blue` |
| Off White | `#EDF0F7` | `.off-white` | `.bg-off-white` | `.border-off-white` |
| White | `#FFFFFF` | `.white` | `.bg-white` | `.border-white` |
| Hot Coral | `#FF7669` | `.hot-coral` | `.bg-hot-coral` | `.border-hot-coral` |
| Gold Fusion | `#FFC864` | `.gold-fusion` | `.bg-gold-fusion` | `.border-gold-fusion` |
| Electric Green | `#46A56F` | `.electric-green` | `.bg-electric-green` | `.border-electric-green` |
| Electoral Teal | `#51C4BB` | `.electoral-teal` | `.bg-electoral-teal` | `.border-electoral-teal` |
| Spark Purple | `#8889F2` | `.spark-purple` | `.bg-spark-purple` | `.border-spark-purple` |

The first five are the primary palette; the remaining five are accent colors, used sparingly for icons, graphics, and supplemental visuals rather than main backgrounds or typography. Plus a `.db-font` utility applying DataBank's standard fallback font stack (`Arial, Helvetica, sans-serif`) - useful when you want the brand's plain body-text font without necessarily pulling in Inter.

### Color Use Ratios

For formal documents, keep the dominant primary background near 70%, with remaining supporting colors around 15% each. For everything else, keep the dominant background near 60%, with supporting color areas around 15%, 15%, and 10%.

## Icons and Logos

Every icon and logo is available as an inline background-image class (`.icon-*` and `.logo-*`), embedded as base64 SVG data so no separate image request is needed. Four brand families exist, each with primary/simplified/cloud/PageIQ variants as appropriate, and each color variant (dark background, light background, navy, white, and sometimes a plain brand-color version) covered separately.

### Stylesheet Bundles

Rather than one monolithic stylesheet, the assets are split into individual per-family files (`databank-colors.css`, `databank-fonts.css`, `databank-icons.css`, `databank-icons-cloud.css`, `databank-logos.css`, `databank-logos-simplified.css`, `databank-logos-cloud.css`, `databank-logos-pageiq.css`), plus four convenience bundles that `@import` the right subset for a given context:

| Bundle | Includes |
|---|---|
| `databank.css` | Colors, fonts, primary icons, primary + simplified logos |
| `databank-cloud.css` | Colors, fonts, Cloud icons and logos only |
| `databank-pageiq.css` | Colors, fonts, primary icons, PageIQ logos only |
| `databank-all.css` | Everything - every family, every variant |

This lesson's own page loads `databank-all.css` specifically so every variant can be shown side by side; a real project would typically load just the bundle matching its own product line, rather than pulling in logo families it'll never use.

The `Images/` folder alongside this lesson holds the same marks as standalone files too - SVG, PNG, and print-ready PDF, across every color variant and background context. Reach for the CSS classes for anything rendered directly in a web page; reach for the standalone files when you need an actual image file to hand off elsewhere (email signatures, print materials, non-web tools).

## Typography: Inter, TT Hoves Pro, and Poppins

DataBank's primary brand typeface is [Inter](https://rsms.me/inter/), distributed under the SIL Open Font License - free to use, including commercially, with the license terms in `Fonts/OFL.txt`.

A second typeface, [TT Hoves Pro](https://typetype.org/fonts/tt-hoves-pro), is also loaded and demonstrated on this page for comparison. **This one is not free for professional use** - the font files present here are trial files (their filenames literally include "Trial"), suitable for demonstration purposes only. Before using TT Hoves Pro in anything beyond this reference page, confirm proper licensing is in place.

Finally, I have included a third font, [Poppins](https://fonts.google.com/specimen/Poppins), which is a free Google font. It is not part of the standard DataBank branding system, but is used for modern marketing materials.

```css
@font-face {
    font-family: "Inter";
    src: url("../fonts/Inter-VariableFont_slnt,wght.ttf") format("truetype-variations");
    font-weight: 100 900;
    font-style: normal;
}
```

All fonts ship as **variable fonts** - one file covering every weight from Thin (100) through Black (900) via the `font-weight: 100 900` range syntax, rather than needing separate files per weight. For browsers that don't support variable fonts, `databank-fonts.css` also declares each static weight file individually, each one only loading when that specific weight is actually requested.

## Relationship to the jQuery Plugins Chapter

The `dbStylize` plugin covered earlier applies a self-contained subset of this same branding system (a `db-` prefixed naming scheme bundled into its own `db-stylize.css`) as a worked example for that lesson's own purposes. It's intentionally separate from this canonical reference rather than sharing these exact files - that lesson needed a complete, standalone example to demonstrate the plugin technique, not a dependency on this chapter.
