# Workflow User Forms

A **Workflow user form** is presented to a user as part of a Workflow task - it can use everything covered in earlier lessons (keywords, properties, special buttons), plus one addition unique to Workflow: **Workflow properties**.

## Setting Workflow Properties

```
name="OB_WFPROPERTY_[property name]_#"
```

Where `#` is the instance number. **The property name is case-sensitive.** This uses OnBase's **Session Property Bag** - a set of named values that persist across steps in a Workflow session, letting one task pass data forward to a later one.

- On load, the field displays the property's current value (if one is already set from an earlier task).
- On submit, the field's value is written back to the property bag under that name, available to subsequent tasks in the same Workflow.

## Submit/Cancel Convention, With a Workflow-Specific Caveat

This lesson's buttons follow the same `OBBtn_Yes`/`OBBtn_No` convention as custom query forms - but on a Workflow user form specifically, remember that `OBBtn_No` **does not abort the task** (it just refreshes the form without saving, per the OnBase Form Buttons lesson). If you actually want Cancel to abort the Workflow task, use `OBBtn_Cancel` instead of `OBBtn_No`.
