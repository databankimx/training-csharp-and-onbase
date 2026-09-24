# Arrays and Objects

Almost everything else in JavaScript is built from these two structures. An array is an ordered list; an object is a named collection of properties. A GL distribution table is naturally an *array* of *objects* - one object per row, each with its own PO number, GL code, and amount - which is exactly the shape used throughout this lesson's examples.

## Arrays

```javascript
var glRows = [
    { poNumber: "PO-4471", glCode: "6010", amount: 340.00 },
    { poNumber: "PO-4472", glCode: "6020", amount: 120.50 },
];

glRows[0].poNumber; // "PO-4471" - the first item is index 0, not 1
```

Arrays are indexed starting at zero - a common source of off-by-one mistakes if you forget that the first item is `[0]`, not `[1]`. `glRows.length` tells you how many items there are; the last valid index is always `length - 1`.

A few methods worth knowing well:

- **`.push(item)`** - adds an item to the end of the array, changing it in place.
- **`.forEach(fn)`** - runs a function once for every item. The array-focused counterpart to the `for...of` loop from the previous lesson - often more readable when all you need is "do this for each item," with no need to build up and return a new array.
- **`.map(fn)`** - builds and returns a **new** array, with each item transformed by the function you provide. The original array is left untouched.
- **`.filter(fn)`** - builds and returns a **new** array containing only the items for which your function returns `true`. Also leaves the original untouched.

`.map()` and `.filter()` both returning new arrays (rather than modifying the original) is worth internalizing - it means you can chain them together, and it means the original list is always still there afterward if you need it again.

## Objects

```javascript
var invoice = {
    invoiceNumber: "INV-1042",
    vendor: "Acme Supply",
    amount: 1035.75,
};

invoice.vendor;      // dot notation
invoice["vendor"];   // bracket notation - same result
```

The object literal (`{ }`) syntax above is by far the most common way to create a plain object - a single, one-off collection of related values, with no need for a reusable template.

Dot notation and bracket notation both access the same property, but they're not interchangeable in every situation: bracket notation accepts a variable, letting you look up a property whose name isn't known until the script actually runs:

```javascript
var fieldName = "amount";
invoice[fieldName]; // 1035.75 - fieldName's VALUE ("amount") is used as the property name
```

Dot notation can't do this - `invoice.fieldName` would look for a literal property named `fieldName`, not the property named by whatever `fieldName` currently holds. Adding a new property to an existing object is as simple as assigning to it directly (`invoice.status = "Pending";`) - no separate declaration step needed, unlike declaring a new variable.

## Comparing the Ways to Create an Object

By this point in the chapter, you've actually seen three different approaches to creating an object:

| Approach | When to use it |
|---|---|
| Object literal `{ }` | A single, one-off object - by far the most common case |
| Constructor function | A reusable "template" for creating many similar objects (covered in the Functions lesson) |
| `class` | A more modern, structured version of the constructor function pattern |

> **OnBase note:** as covered in the Functions lesson, `class` only works on a current, Chromium-based OnBase installation - not on an older one still rendering forms with Internet Explorer 9. Object literals and constructor functions both work everywhere, on every OnBase version, which is exactly why those are the two patterns actually used throughout this training set, rather than `class`.
