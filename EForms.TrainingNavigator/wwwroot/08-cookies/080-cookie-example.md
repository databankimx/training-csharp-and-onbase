# Cookies

A minimal, complete example of reading and writing browser cookies with plain JavaScript - no framework needed for this one.

## Setting a Cookie

```javascript
function setCookie(name, value, expiration) {
    var d = new Date();
    d.setTime(d.getTime() + expiration * 60 * 1000);
    document.cookie = name + "=" + value + ";expires=" + d.toUTCString() + ";path=/";
}
```

`expiration` is given in minutes here, converted to milliseconds for `Date`. `path=/` makes the cookie available across the entire site rather than just the page that set it - worth remembering, since a cookie set without a `path` is scoped only to the current page's own directory and everything below it.

## Reading a Cookie

`document.cookie` returns **every** cookie for the current page as one semicolon-delimited string (`name1=value1; name2=value2; ...`), not just the one you want - so reading a specific cookie means splitting that string apart and searching it:

```javascript
function getCookie(name) {
    var cookieData = decodeURIComponent(document.cookie);
    var cookies = cookieData.split(";");
    for (var c = 0; c < cookies.length; c++) {
        var cookie = cookies[c].trim();
        if (cookie.toLowerCase().indexOf(name.toLowerCase() + "=") === 0)
            return cookie.substring(name.length + 1, cookie.length);
    }
    return "";
}
```

`decodeURIComponent` reverses the URL-encoding cookie values are stored with, since cookie values can't contain certain characters (like `;` or `=`) unescaped.

## Clearing a Cookie

There's no dedicated "delete" API for `document.cookie` - clearing one just means re-setting it with an expiration already in the past, which the browser then discards on its own:

```javascript
function clearCookie(name) {
    setCookie(name, "", -1);
}
```

Reusing `setCookie()` with a negative `expiration` (here, -1 minute) is enough - the resulting date is already behind "now," so the cookie is gone the moment this runs.

## Putting Them Together

This lesson's page ties all three together into a small, real interface: a status line showing whether a saved name currently exists, a text field and "Save Name" button that calls `setCookie()`, and a "Clear Cookie" button that calls `clearCookie()` - both followed by `refreshCookieStatus()`, which re-reads the cookie and updates the page to match. Try saving a name, then reloading the page (the value persists); then clear it, and reload again (it's gone).
