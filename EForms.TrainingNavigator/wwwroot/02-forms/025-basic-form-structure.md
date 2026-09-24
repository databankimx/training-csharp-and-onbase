# HTML Forms

The main mechanism available to an HTML designer for passing information back to the server is the HTML form. This chapter covers how to create forms, assign user inputs, and prepare for building OnBase e-forms.

These lessons assume you're already comfortable with basic HTML - see the HTML Fundamentals chapter first if you need a refresher.

Much of the content and lesson order in this chapter is adapted from the [W3Schools HTML Forms tutorial](https://www.w3schools.com/html/html_forms.asp), which is also a good place to look up the [full reference for all HTML tags](https://www.w3schools.com/tags/default.asp).

## Basic HTML Form Structure

The fundamental HTML element is the `<form>` tag.

The `method` attribute determines how form data is sent:

- `GET`: form data will appear in the URL as `?field_1=value_1&...field_n=value_n`
- `POST`: form data will be encapsulated as part of the HTTP request

> **Note:** These lessons use `POST`, matching the method OnBase forms will always use. Since POST data isn't visible in the URL the way GET data is, submitting a form here intercepts the submission and logs the field values to the console instead of actually navigating anywhere.

The `action` attribute contains the URL of the server-side form handler. These lessons omit the `action` attribute, so each form submits to the page itself.

> **Important:** Always include a unique `id` attribute value on every form element!
