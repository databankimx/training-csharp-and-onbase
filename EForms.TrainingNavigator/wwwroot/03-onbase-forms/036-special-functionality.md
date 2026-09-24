# Special Functionality

> **Note:** This lesson intentionally uses OnBase-only syntax (URL placeholders, backslash paths) that won't pass standard HTML validation. That's expected here - these are real OnBase conventions, not a mistake to fix.
>
> This page will also fail to pass validation because of the incomplete `<img>` tags. This is expected, since these use OnBase-specific attributes that aren't part of the HTML standard.

## Displaying an OnBase Document as an Image

An `<img>`'s `alt` attribute (not `src`!) can be used to render a stored OnBase document as an image, via a special `mz:` syntax:

```
alt="mz:DocTypeNum;KeyTypeNum1:KeyValue1; ... ;KeyTypeNumN:KeyValueN;"
```

Two non-keyword type numbers have special meaning within this syntax:

- `-10` - the page number to render from the retrieved image
- `-6` - the document date (formatted as `yyyymmdd`)

**Limitation:** the image cannot be reloaded after the form has opened - it's fetched once, at load time.

## URL Placeholders

Since a form doesn't know in advance what server it'll be deployed to, OnBase provides placeholder tokens that get substituted with the real URL at render time:

- `~[APPLICATION_SERVER_URL]~` - the AppServer virtual directory (e.g. `https://my-host-server/AppServer`)
- `~[APPLICATION_SERVER_BASE_URL]~` - the AppServer website's root (e.g. `https://my-host-server`)

These are commonly used in an `<img src="...">` to reference a logo or other static asset without hardcoding a specific server.

## Signature Pads

In the OnBase thick client, a connected Topaz signature pad can capture a signature directly onto the form:

```
alt="signature[:altLabel]"
```

For more than one signature field on the same form, prefix each additional one with an ascending number starting at `2`:

```
alt="2signature[:altLabel]"
alt="3signature[:altLabel]"
```

The `src` path provided must point to a folder the client can actually read from and write to.

**Limitation:** not supported in the Web Client or Unity Client - thick client only.

## Preventing an Unsaved Form From Being Closed

Adding a hidden field named `QueryStopUnload` forces the user to explicitly Submit or Cancel - they can no longer close the form via the window's `[X]` button. This is useful for ensuring any submit-time scripts actually get a chance to run, rather than being skipped by an abrupt close.

```html
<input type="hidden" name="QueryStopUnload">
```

**Limitation:** not supported in the Web Client.

## A Legacy Mechanism to Recognize, Never Use

In older forms you may come across a field like this:

```html
<input name="OBKeyProperty__1" id="kwPropKeyName" value="DYNAMIC">
```

The special value `DYNAMIC` (note there's no instance number in the field's own name) was a mechanism for adding fields via JavaScript, but **only in the thick client** - it breaks functionality everywhere else. Don't use this in any new form; it's documented here only so you recognize it if you run across it in a legacy one, and know it needs to be replaced rather than extended.
