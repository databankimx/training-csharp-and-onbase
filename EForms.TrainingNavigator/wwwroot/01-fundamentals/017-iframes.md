# Using iFrames

## `<iframe>...</iframe>`

An `<iframe>` ("inline frame") embeds another HTML document inside the current page, in its own independent browsing context.

```html
<iframe name="myFrame" id="myFrame" src="page.html" title="My IFrame"></iframe>
```

- `src` - the page to load inside the frame
- `name` - lets other elements target this frame specifically (see below)
- `title` - required for accessibility, describing the frame's purpose to screen readers

## Targeting an iFrame from a Link

A link's `target` attribute can reference an `<iframe>`'s `name`, loading the link's destination into that frame instead of navigating the whole page:

```html
<a href="page.html" target="myFrame">Load into the frame</a>
<iframe name="myFrame" ...></iframe>
```

This is a common pattern for a simple navigation sidebar next to a content frame - clicking a link updates only the frame, without a full page reload.

## Security Note

Content loaded in an `<iframe>` runs in its own isolated context - by default, JavaScript in the parent page and the framed page can't freely interact with each other, especially across different origins (domains). This is intentional, and a meaningful part of the browser's security model, not a limitation to work around casually.
