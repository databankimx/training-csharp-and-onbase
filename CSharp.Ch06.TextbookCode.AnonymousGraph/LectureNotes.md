# Ch06 Textbook Code: Anonymous Graph

## What This Is

The direct publisher source `GraphForm.cs` in `CSharp.Ch06.DelegatesEventsAndExceptions` was adapted from. A `ComboBox` picks between three functions, each defined with a different delegate syntax (statement lambda, anonymous method, multi-line statement lambda), all graphed identically regardless of which syntax defined them.

No bugs found. `Load` correctly wired.

---

## Worth Comparing: The Source and the Adaptation

`GraphForm.cs` in this chapter's main lesson renamed variables to match house convention (`GraphPictureBox` instead of `graphPictureBox`, `theFunction` instead of `TheFunction`) and folded the demo into the chapter's main teaching form, but the underlying logic, all three equation cases, the coordinate-transform math in `DrawGraph()`, is unchanged. Worth reading both side by side, it's a concrete example of what "porting to house conventions without changing behavior" actually looks like in practice, the same file, twice, with only cosmetic differences between them.
