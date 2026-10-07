---
title: "IEnumerable - What foreach Actually Is"
chapter: 5
index: 10
dependencies: []
---

```csharp
using System;
using System.Collections;
using System.Collections.Generic;

public class TreeNode : IEnumerable<TreeNode>
{
    public int    Depth    { get; set; }
    public string Text     { get; set; }
    public List<TreeNode> Children { get; set; } = new();

    public TreeNode(string text) { Text = text; }

    public TreeNode AddChild(string text)
    {
        var child = new TreeNode(text) { Depth = Depth + 1 };
        Children.Add(child);
        return child;
    }

    public List<TreeNode> Preorder()
    {
        var nodes = new List<TreeNode>();
        TraversePreorder(nodes);
        return nodes;
    }

    private void TraversePreorder(List<TreeNode> nodes)
    {
        nodes.Add(this);
        foreach (var child in Children) child.TraversePreorder(nodes);
    }

    public IEnumerator<TreeNode> GetEnumerator() => new TreeEnumerator(this);
    IEnumerator IEnumerable.GetEnumerator() => new TreeEnumerator(this);
}

public class TreeEnumerator : IEnumerator<TreeNode>
{
    private List<TreeNode> _nodes;
    private int _index;

    public TreeNode Current => GetCurrent();
    object IEnumerator.Current => GetCurrent();

    public bool MoveNext() { _index++; return _index < _nodes.Count; }
    public void Reset()    { _index = -1; }

    public TreeEnumerator(TreeNode root) { _nodes = root.Preorder(); Reset(); }

    private TreeNode GetCurrent()
    {
        if (_index < 0 || _index >= _nodes.Count)
            throw new InvalidOperationException("Node index out of range!");
        return _nodes[_index];
    }

    ~TreeEnumerator() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool releaseManagedObjects)
    {
        if (!releaseManagedObjects) return;
        _nodes = null;
    }
}

internal static class Program
{
    private static void Main()
    {
        var ceo = new TreeNode("CEO");
        var vp1 = ceo.AddChild("VP of Engineering");
        var vp2 = ceo.AddChild("VP of Marketing");
        vp1.AddChild("Senior Engineer");
        vp1.AddChild("Engineer");
        vp2.AddChild("Marketing Manager");

        // foreach -- works because TreeNode implements IEnumerable<TreeNode>
        foreach (var node in ceo)
            Console.WriteLine(new string(' ', node.Depth * 2) + node.Text);

        Console.WriteLine();

        // The same iteration written out explicitly as what foreach compiles to:
        using var enumerator = ceo.GetEnumerator();
        while (enumerator.MoveNext())
            Console.WriteLine(new string(' ', enumerator.Current.Depth * 2) + enumerator.Current.Text);
    }
}
```
