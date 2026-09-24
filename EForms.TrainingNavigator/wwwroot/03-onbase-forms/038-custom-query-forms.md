# Custom Query Forms

A **Custom Query form** is a specialized OnBase form used to search for documents (rather than create/edit one), with its own set of special field names beyond the ones already covered.

## Date Search Fields

- **`OBDocumentDate`** - searches by a specific document date. Not the same as `OBProperty_DocumentDate` (which *displays* the date property) - this one is a search *criterion*.
- **`OBFromDate`** / **`OBToDate`** - together, search by a range of document dates.

> **Never combine them:** `OBDocumentDate` and `OBFromDate`/`OBToDate` are mutually exclusive - don't put both on the same form.

## Keyword Comparison Operators: `OBOperKey_`

Pairing a `<select>` (or any input) named `OBOperKey_[keyword name]_#` or `OBOperKey__[keyword id]_#` with a matching `OBKey_...` field lets the user choose how that keyword should be compared, not just what value to match:

```html
<select name="OBOperKey__440_1">
    <option>=</option>
    <option>&lt;</option>
    <option>&lt;=</option>
    <option>&gt;</option>
    <option>&gt;=</option>
    <option>&lt;&gt;</option>
    <option>&quot; &quot;</option> <!-- literal match; note the space between the quotes -->
    <option>IsNull</option>
</select>
<input type="text" name="OBKey__440_1">
```

- The field must have a matching `OBKey_...` field for the operator to attach to - an operator field with no corresponding keyword field does nothing.
- It doesn't have to be a `<select>` - a plain input with a single fixed value works too, if you only ever need one operator.
- You don't have to offer every operator - omit any you don't want the user choosing.
- It can even be a hidden field, if you want a fixed operator the user never sees or changes.

## Keyword Logical Operators: `OBOperator_`

Similarly, `OBOperator_[keyword name]_#` or `OBOperator__[keyword id]_#` adds a logical connector (`AND`/`OR`) linking that keyword's criterion to the next one:

```html
<select name="OBOperator__284_1">
    <option>AND</option>
    <option>OR</option>
</select>
<input type="text" name="OBKey__284_1">
```

Same flexibility as comparison operators - can be a simple input, can omit options, can be hidden - and the two can be combined on the same keyword (a comparison operator *and* a logical operator linking to the next criterion).

## Full-Text Search: `OBFullTextSearch`

A field named `OBFullTextSearch` lets the query include a full-text search term, searching document content rather than just keyword values.

This requires either the Hyland Full Text or Full Text for Autonomy Idol license, and only works against documents that have actually been indexed by one of those modules - it won't find matches in a document that hasn't been indexed, regardless of what the document actually contains.

## Convention: Submit/Cancel Naming

This lesson's own Submit/Cancel buttons use `OBBtn_Yes`/`OBBtn_No` rather than `OBBtn_Save`/`OBBtn_Cancel` - by convention, custom query forms use the Yes/No pair (matching how a query is really a yes/no decision to run the search), while e-forms use Save/Cancel.
