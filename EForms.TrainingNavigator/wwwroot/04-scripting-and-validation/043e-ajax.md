# AJAX

AJAX lets a script call a web service from the browser without reloading the page or navigating anywhere - the mechanism behind looking up data from an external system while a form is still open and being filled out.

## The Name Is Historical

"AJAX" originally stood for "Asynchronous JavaScript And XML." In practice today, a modern AJAX call almost always sends and receives JSON rather than XML - the name stuck around long after the "X" stopped being accurate for most real usage.

## A Real Example: Location Lookup by ZIP Code

This lesson's own page calls a real, working service - not a simulation - to look up a location by ZIP code. It only works from within DataBank's own network, since the service isn't publicly reachable, but the code is genuine and unmodified from a working example:

```javascript
$.ajax({
    url: wsUrl + "LookupLocation",
    type: "POST",
    contentType: "application/json",
    crossDomain: true,
    data: JSON.stringify({ ZipCode: zipCode, RequestId: requestId }),
    success: function (result) {
        // runs once the service responds successfully
    },
    error: function (jqXHR, textStatus, errorThrown) {
        // runs if the request fails instead
    },
});
```

A few things worth understanding about this call:

- **The request body is a JSON string, not a plain object.** `JSON.stringify()` converts the `{ ZipCode, RequestId }` object into the actual text sent to the server; `contentType: "application/json"` tells the server how to interpret that text.
- **This request has no keyword mapping.** Unlike an OnBase form field (`OBKey_...`), this is a plain HTTP call made by the page's own script - the service has no idea it's being called from an OnBase form, or from a form at all.
- **`crossDomain: true`** tells jQuery the request is going to a different host than the page itself, which needs to be explicitly allowed for the request to actually work.
- **Two separate callbacks handle the two possible outcomes** - `success` if the service responds normally, `error` if the request fails outright (the server is unreachable, returns an error status, or similar).

## AJAX Is Asynchronous

The "A" is the important part in practice: the call doesn't pause the script and wait for a response. `$.ajax()` sends the request and returns immediately - whatever code comes right after it keeps running before any response has actually come back. The `success`/`error` callbacks run later, whenever the response actually arrives, which could be milliseconds or seconds later, on its own separate timeline from the rest of the script.

> **Why this matters for error handling:** a plain `try`/`catch` wrapped around an AJAX call will *not* catch a failure coming back from the service - by the time the response arrives (success or failure), the `try` block has already finished running and moved on. This is exactly why AJAX calls need their own dedicated `error` callback, rather than relying on the same `try`/`catch` pattern used everywhere else in this training set for handling errors.
