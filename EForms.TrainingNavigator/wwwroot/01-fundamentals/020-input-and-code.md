# Displaying Inputs and Code

A handful of tags exist specifically for displaying technical content - code, keyboard input, program output, and variables - each rendered in a monospace font by default and each carrying its own semantic meaning.

## `<code>...</code>`

Represents a snippet of computer code. Combine with `<pre>` (see the Pre-Formatting Text lesson) to preserve a whole block's original line breaks and indentation - or, as in this lesson's own code sample, apply `white-space: pre-wrap` via CSS for the same effect without an actual `<pre>` tag.

## `<kbd>...</kbd>`

Represents keyboard input - text a user should type or a key combination they should press:

```html
Press <kbd>Ctrl + Alt + Del</kbd>
```

## `<samp>...</samp>`

Represents sample output from a program or system, as opposed to `<kbd>`'s user input:

```html
The command printed <samp>Hello, World!</samp>
```

## `<var>...</var>`

Represents a variable, in a mathematical expression or in code:

```html
The area is &frac12; &times; <var>b</var> &times; <var>h</var>
```
