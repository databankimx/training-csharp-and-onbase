# Putting It All Together

A complete, realistic form (a user access request) combining everything covered in this chapter: text inputs, a `readonly` field driven by JavaScript, a `<datalist>`-backed department field, a grouped `<select>` for job title, checkboxes for equipment, radio buttons for access requests, and reset/submit buttons - organized into `<fieldset>`s with `<table>`s for field layout.

## The JavaScript Behaviors

A few small scripted behaviors tie the form together, beyond plain HTML:

- **Auto-generated username/email** - typing in First Name/Last Name computes a username (first initial + last name) and email address live, into two `readonly` fields the user can't edit directly.
- **Dependent job title options** - the Job Title `<select>`'s `<optgroup>`s are all disabled by default; choosing a Department enables only the matching group's options, and removes any already-selected option that no longer applies.
- **Reset handling** - the form's own native `reset` event re-disables the job title options and clears any selection, keeping the dependent dropdown in sync when the user clicks Reset (native reset alone wouldn't re-run that filtering logic on its own).

This is a good example of a form that's still almost entirely built from plain HTML, with a small amount of JavaScript layered on top to improve usability - not a JavaScript-driven form with HTML as an afterthought.
