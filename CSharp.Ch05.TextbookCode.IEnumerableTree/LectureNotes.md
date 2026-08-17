# Ch05 Textbook Code: IEnumerable Tree

## What This Is

The standalone WinForms original of the org-chart `IEnumerable`/`IEnumerator` demo also ported into `CodeLabOrgChart()` in `CSharp.Ch05.ImplementingClassHierarchies`, this time displayed in a read-only multiline textbox instead of printed to a console. No debugger required, the tree builds and displays automatically on load.

No bugs found.

---

## Worth Knowing: There's a Same-Named Sibling Project

`CSharp.Ch05.TextbookCode.TreeEnumerator` exists elsewhere in this chapter set. Despite the similar theme, its `TreeNode.cs` takes a noticeably different approach, no separate `TreeEnumerator.cs` file at all. Worth comparing the two directly once both are in place, seeing two different ways the publisher (or the underlying textbook) chose to demonstrate the same interface pair is more instructive than either one alone.

---

## Worth Noticing: Manual Enumeration, No `foreach`

```csharp
string text = "";
IEnumerator<TreeNode> enumerator = president.GetEnumerator();
while (enumerator.MoveNext())
    text += new string(' ', 4 * enumerator.Current.Depth) +
            enumerator.Current.Text +
            Environment.NewLine;
```

This calls `GetEnumerator()` and drives `MoveNext()`/`Current` by hand instead of using `foreach`. It's worth recognizing this as literally what `foreach` compiles down to, `foreach (TreeNode node in president)` would produce equivalent code. Seeing the manual version at least once makes it concrete that `foreach` isn't special syntax with its own separate mechanism, it's sugar over exactly this `IEnumerator` pattern.
