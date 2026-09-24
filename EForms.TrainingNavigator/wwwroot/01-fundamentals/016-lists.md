# Using Lists

HTML has three distinct kinds of list, each with a different purpose.

## Unordered Lists: `<ul>`

A bulleted list, for items with no meaningful sequence:

```html
<ul>
    <li>Coffee</li>
    <li>Tea</li>
</ul>
```

## Ordered Lists: `<ol>`

A numbered list, for items where the sequence/order matters:

```html
<ol>
    <li>Coffee</li>
    <li>Tea</li>
</ol>
```

## Nesting Lists

A `<ul>` or `<ol>` can be nested inside an `<li>` of another list, creating a sub-list under that item. The two types can be mixed - an ordered sub-list inside an unordered parent list, or vice versa - whatever best reflects the actual structure of the content.

## Description Lists: `<dl>`

A different kind of list entirely - pairs of terms and their descriptions, rather than a sequence of items:

```html
<dl>
    <dt>Coffee</dt>
    <dd>Regular, Decaf, or Dark Roast</dd>
    <dt>Tea</dt>
    <dd>Variety of bagged and loose-leaf teas</dd>
</dl>
```

- `<dt>` - the term being defined
- `<dd>` - its description

A `<dt>` can be followed by more than one `<dd>` if a term has multiple descriptions.

## Removing the Default Bullet/Number

Setting `list-style-type: none` in CSS removes the default bullet or number from a list item - useful for a heading-like `<li>` that shouldn't visually look like a list item (as in this lesson's own "Beverages" header rows), or any list you're styling entirely yourself.
