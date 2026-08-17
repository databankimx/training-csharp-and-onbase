#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharp.Ch05.TextbookCode.IEnumerableTree
{
    class TreeEnumerator : IEnumerator<TreeNode>
    {
        // The tree's nodes in their proper order.
        private List<TreeNode> Nodes;

        // The index of the current node.
        private int CurrentIndex;

        // Constructor.
        public TreeEnumerator(TreeNode root)
        {
            Nodes = root.Preorder();
            Reset();
        }

        public TreeNode Current
        {
            get { return GetCurrent(); }
        }
        object IEnumerator.Current
        {
            get { return GetCurrent(); }
        }
        private TreeNode GetCurrent()
        {
            if (CurrentIndex < 0)
                throw new InvalidOperationException();
            if (CurrentIndex >= Nodes.Count)
                throw new InvalidOperationException();
            return Nodes[CurrentIndex];
        }

        public bool MoveNext()
        {
            CurrentIndex++;
            return (CurrentIndex < Nodes.Count);
        }

        public void Reset()
        {
            CurrentIndex = -1;
        }

        public void Dispose()
        {
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
