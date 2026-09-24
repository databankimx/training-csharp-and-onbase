# External Forms

Every lesson so far has assumed the form is opened and submitted from *within* OnBase itself. An **external form** is different: it's hosted on your own web server, outside OnBase entirely, and submitted through a dedicated OnBase web component - `LoginFormProc.aspx` - that processes the submission and creates the resulting document.

## Pointing the Form at LoginFormProc.aspx

The form's own `action` targets that component directly:

```html
<form method="post" action="https://my-host-server/AppNet/loginformproc.aspx?">
```

## Required and Optional Hidden Fields

A handful of hidden fields control how `LoginFormProc.aspx` processes the submission:

- **`OBDocumentType`** - the OnBase document type ID to store the submission as. **Required** on every external form.
- **`OBWeb_Redirect`** - the page to show after a successful submit. Only takes effect if `web.config` is set to prompt for a new form.
- **`OBWeb_FinalTargetPage`** - the page to show after Cancel, and also after Submit if `web.config` is set *not* to prompt for a new form.
- **`OBWeb_ReturnReadOnlyCopy`** - set to `"true"` to return a read-only copy of the submitted form instead of redirecting anywhere; overrides both `OBWeb_Redirect` and `OBWeb_FinalTargetPage` when set.
- **`LanguageParam`** - the language assembly to submit under; only needed in a multi-language OnBase system.

> **Historical note:** older OnBase versions had additional `LoginFormProc.aspx` settings for retrieving and updating an existing form through this same mechanism. Those have since been superseded by **FormPop** in later releases - if you're looking for that functionality today, FormPop is where to find it, not these settings.

## What Doesn't Work on an External Form

Because an external form isn't loaded from within an active OnBase session, anything requiring a live OnBase connection is unavailable, including:

- Keyword Datasets
- Auto-numbering keywords
- OnBase documents rendered as images (the `mz:` `alt` syntax from the Special Functionality lesson)

## A Security Note on Embedding

> **Hyland's own guidance:** by default, content embedded from the OnBase Web Server must be on the **same domain** as the page embedding it. If your solution needs to embed Web Server content from a different domain, the Web Server itself must be explicitly configured to allow it - see the `X-Frame-Options` section of the Web Server module's own reference guide before attempting this.
