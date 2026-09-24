# OnBase Form Buttons

OnBase recognizes a set of special button `name` values, each triggering different behavior beyond a plain form submission. Every button below is still an ordinary `<input type="submit">` (or `type="reset"`, for one) - the special behavior comes entirely from the `name` OnBase looks for.

## Autofill Buttons

- **`OBBtn_KSnnn`** (`nnn` = the autofill ID) - expands autofill, but **only on a new form**.
- **`OBBtn_ExpandKSnnn`** - expands autofill on either a new or an existing form.

Both share the same limitations:
- Won't work in the web client if the autofill's primary keyword is a date.
- Unless the autofill uses Multi-Instance Keyword Groups (MIKGs), only the first matching keyset is returned.
- Fills read-only keywords regardless of permission - but won't submit the form afterward without the **"Access Restricted Keywords"** permission.
- In the web client, can only autofill keyword types that are actually present on the HTML form.
- Not supported in Workflow.

## Cross-Reference and Retrieval Buttons

- **`OBBtn_CrossReference`** - triggers the keyword-based cross reference configured on the document type. The cross-reference keyword must be present on the form.
- **`OBBtn_CQ###`** (`###` = the custom query's ID) - executes that custom query directly from a button on the form.
- **`OBBtn_xRefItemnum`** - retrieves the document identified by the `xRefItemnum` field's value (a document handle). Requires a matching `<input name="xRefItemnum" value="nnn">` elsewhere on the form; only one instance is supported per form.

## Save Buttons

- **`OBBtn_Save`** / **`OBBtn_Yes`** - both submit the form; they're functionally identical. By convention, use `OBBtn_Save` on e-forms and `OBBtn_Yes` on user forms and custom queries.
- **`OBBtn_AutoSave`** - submits the form and stores the field values to the autofill table(s), for later use by the autofill buttons above.
- **`OBBtn_SaveNoClose`** - submits the form but keeps it open afterward.
- **`OBBtn_SaveAndClose`** - submits the form and closes it.

## Cancel Buttons

- **`OBBtn_Cancel`** / **`OBBtn_No`** - both close the form without saving; outside of Workflow, they're functionally equivalent. Inside a Workflow user form, they differ:
  - `OBBtn_Cancel` sets "last execution result" to `false` **and aborts the task**.
  - `OBBtn_No` sets "last execution result" to `false` but **does not** abort the task - it just refreshes the form without saving.
- Any `type="submit"` button whose name doesn't match one of OnBase's own special names is treated as `OBBtn_Cancel` by default.

## Reset

A plain `<input type="reset">` needs no special OnBase name at all - resetting the form back to its loaded values is a native HTML behavior, not something OnBase intercepts.
