# Basic HTML Document Structure

> Always validate your HTML at the [W3C Validator](http://validator.w3.org/). If there are any errors or warnings, the form is not ready for delivery.

## `<!DOCTYPE>`

The document type declaration tells the browser what version of HTML to render. With the exception of HTML5+, these are defined as specific variants of SGML (Standard Generalized Markup Language).

Common DTDs include:

- **HTML 5+** *(Note: we only develop using this DTD)*
  ```
  <!DOCTYPE html>
  ```
- **HTML 4**
  - Strict: `<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01//EN" "http://www.w3.org/TR/html4/strict.dtd">`
  - Transitional *(you will encounter legacy forms using this DTD)*: `<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Transitional//EN" "http://www.w3.org/TR/html4/loose.dtd">`
  - Frameset: `<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.01 Frameset//EN" "http://www.w3.org/TR/html4/frameset.dtd">`
- **XHTML 1.0**
  - Strict: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">`
  - Transitional *(you will encounter many legacy forms using this DTD)*: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">`
  - Frameset: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Frameset//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd">`
- **XHTML 1.1**
  - Standard *(you will encounter some legacy forms using this DTD)*: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN" "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">`
  - Basic: `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML Basic 1.1//EN" "http://www.w3.org/TR/xhtml-basic/xhtml-basic11.dtd">`
- **Historical DTDs**
  - HTML 2.0: `<!DOCTYPE html PUBLIC "-//IETF//DTD HTML 2.0//EN">`
  - HTML 3.2: `<!DOCTYPE html PUBLIC "-//W3C//DTD HTML 3.2 Final//EN">`
  - XHTML 1.0 (Basic): `<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML Basic 1.0//EN" "http://www.w3.org/TR/xhtml-basic/xhtml-basic10.dtd">`

## `<html lang="en-us"> ... </html>`

These tags indicate the start and end of the HTML document. Nothing except comments (and the leading `<!DOCTYPE>` tag) should ever appear outside these tags.

It's recommended to specify the language (culture) attribute in this tag (typically `en-us`).

## `<head>...</head>`

The head section should contain all scripts, styles, and metadata:

- `<meta charset="utf-8">` - required for all HTML5 e-forms (we typically use UTF-8)
- `<title>...</title>` - required (with a value) for all HTML5 e-forms

## `<body>...</body>`

The body section should contain all HTML elements that will be displayed to the user. It should **not** contain stylesheets, scripts, XML, etc.
