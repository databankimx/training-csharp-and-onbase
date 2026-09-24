# OnBase Form Requirements

An HTML e-form has a few hard requirements OnBase imposes, beyond ordinary valid HTML.

## OnBase File Formats

These lessons apply to the following file formats:

- Electronic Form
- Virtual Electronic Form

## Form Method

The form's `method` must be `post`. OnBase does not accept e-form submissions via `GET`.

## Mapping Fields to Keywords

OnBase reads a form field's **`name`** attribute to determine which keyword it maps to - not its `id`. The name must take one of two formats:

### By Keyword Name

```
name="OBKey_Keyword_Name_#"
```

- `#` is the instance number on the form (`1`, `2`, `3`, etc.) - this applies both to a standalone keyword that appears more than once on the form (as in this lesson's own example) and to a keyword that's part of a Multi-Instance keyword group
- Any spaces in the keyword type's own name must be replaced with underscores
- This format **cannot** be used if the keyword type's name itself contains an underscore, or any HTML-special character (`<`, `>`, `#`, `"`, etc.) - use the by-ID format instead in those cases

### By Keyword ID

```
name="OBKey__###_#"
```

- `###` is the keyword's numeric ID in OnBase configuration
- `#` is the instance number, same as above
- Note the **two** underscores immediately after `OBKey` when using an ID instead of a name - this is what distinguishes the two formats

Either way, it's recommended to use meaningful `id` attributes (since `id` isn't used for keyword mapping, it's free for your own readability) and include a comment noting which keyword each field maps to, as this lesson's own markup does - something like `<!-- KW: Description: ID = (prod: 1), (test: 1) -->`, if the keyword's ID differs between environments.

## The Submit Button

An OnBase form's submit button must be named exactly `OBBtn_Save` or `OBBtn_Yes` - OnBase looks for one of these two specific names to know which button actually saves the form.

## What Does OnBase Store

- Keyword data is stored in the database, the same as with any other document type.
- The POST data itself (as a query string, e.g. `OBKey_Description_1=Annual+Report&nonKeywordField=extra+data`) is stored to a file in the disk group, and includes non-keyword fields.
  - **Note:** No disk group file is stored for a Virtual Electronic Form - so don't use non-keyword fields on one, since there's nowhere for that data to actually be saved.

## How is the HTML File Used

When OnBase displays an e-form, it loads the HTML file (stored as a SYS HTML Forms document type). The stored keywords (and POST data file, if applicable) are used to populate the form's fields. When the user submits the form, OnBase reads the field values and updates the keywords and POST data file accordingly.

## Tutorial Functionality Warning

The forms in this tutorial include JavaScript to simulate submitting the forms rather than actually submitting them to OnBase. For this reason, you should not copy/paste the script into your own forms, since it will prevent them from actually submitting to OnBase. The script is only included here to allow you to see the form's behavior in a browser without needing an OnBase server.
