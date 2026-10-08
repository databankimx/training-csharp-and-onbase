# GL Coding Form (Structure)

The actual form loaded inside `041-putting-it-together.html`'s `<iframe>` - see that lesson's own notes for what this structure represents.

The Invoice Information and GL Distribution sections are now populated with real keyword fields, including two rows of a Multi-Instance Keyword Group (the GL Distribution table - each row reuses the same keyword IDs with an incrementing instance number). This is still a **basic** version, though: no validation, computation, or dynamic row-adding yet - those come later, in the Scripting and Validation and Dynamic Forms chapters. Only the Accounting Notes section (a plain `<textarea>`) and a hidden-by-default Debug Information section round out the rest.

This file also pulls in DataBank's own branding stylesheets (`databank-colors.css`, `databank-icons.css`, `databank-logos.css`) alongside the form-specific `coding-form.css` - worth a look if you want to match DataBank's visual style in your own forms.
