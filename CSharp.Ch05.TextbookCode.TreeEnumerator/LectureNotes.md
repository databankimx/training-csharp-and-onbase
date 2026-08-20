# Ch05 Textbook Code: Tree Enumerator

## What This Is

A different take on the same org-chart tree as `CSharp.Ch05.TextbookCode.IEnumerableTree`. This `TreeNode` doesn't implement `IEnumerable<T>` at all, no `GetEnumerator()`, no separate `TreeEnumerator` class. Instead it exposes a `GetTraversal()` method built with a `yield return` iterator:

```csharp
public IEnumerable<TreeNode> GetTraversal()
{
    List<TreeNode> traversal = Preorder();
    foreach (TreeNode node in traversal) yield return node;
    yield break;
}
```

No bugs affecting the demo. Two things worth knowing, both left exactly as downloaded.

---

## Worth Comparing: Two Ways to Make Something Enumerable

`IEnumerableTree` implements `IEnumerable<TreeNode>` on the type itself, so you can write `foreach (TreeNode node in president)` directly. This project instead exposes a plain method returning `IEnumerable<TreeNode>`, so the calling code has to say `foreach (TreeNode node in president.GetTraversal())` instead, note the `.GetTraversal()`, `president` itself isn't enumerable here.

`yield return` is doing real work in `GetTraversal()`: the compiler turns that method into a full state machine implementing `IEnumerable<T>`/`IEnumerator<T>` behind the scenes, which is exactly what `IEnumerableTree`'s `TreeEnumerator` class does by hand. Worth reading both projects back to back, `yield return` is the same mechanism, generated for you instead of written out.

---

## Worth Knowing: A Namespace Mismatch, Left As-Is

Every `.cs` file in this project declares `namespace TreeEnumerator`, not `namespace CSharp.Ch05.TextbookCode.TreeEnumerator` like every other download in this chapter. This is the one exception in the whole archive. Kept exactly as downloaded per the "unedited code" policy for `TextbookCode.*` projects, the `.csproj`'s `RootNamespace`/`AssemblyName` still follow this solution's naming convention (matching the project folder), that setting only affects the default namespace VS suggests for new files, it doesn't rewrite the `.cs` files' own explicit namespace declarations, so there's no build conflict either way.

---

## Worth Knowing: Dead Code, Left As-Is

`Form1.cs` still contains the old manual-enumerator approach, commented out:

```csharp
//IEnumerator<TreeNode> enumerator = president.GetEnumerator();
//while (enumerator.MoveNext())
//    text += new string(' ', 4 * enumerator.Current.Depth) +
//        enumerator.Current.Text +
//        Environment.NewLine;
```

`president.GetEnumerator()` wouldn't compile if this were uncommented, this `TreeNode` doesn't implement `IEnumerable` and has no such method. This is a stale leftover from before the file was rewritten to use `GetTraversal()` instead, worth noticing as its own small lesson: commented-out code doesn't get checked by the compiler, so it can silently rot into something that would no longer even build, and nothing catches that until someone tries to actually use it.
