# Combining Diacritical Marks

Unlike the accented letters in the previous lesson (which are single, pre-composed characters like `á`), a combining diacritic is applied *on top of* a separate base letter - e.g. `a` followed by the combining grave-accent character `&#768;` produces `à`, built from two characters rather than one.

## A Validation Caveat

The W3C validator flags text built this way as a warning: **"Text run is not in Unicode Normalization Form C."** Where a pre-composed equivalent character exists (as it does for all six examples on this page), prefer the single accented-letter entity from the Accented Letters lesson instead - combining diacritics are really meant for cases where no pre-composed character exists at all, not as a general-purpose way to add accents.
