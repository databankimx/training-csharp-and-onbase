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

using System.Collections.Generic;

namespace TreeEnumerator
{
    class TreeNode
    {
        public int Depth = 0;
        public string Text = "";
        public List<TreeNode> Children = new List<TreeNode>();
        public TreeNode(string text)
        {
            Text = text;
        }

        // Add and create children.
        public TreeNode AddChild(string text)
        {
            TreeNode child = new TreeNode(text);
            child.Depth = Depth + 1;
            Children.Add(child);
            return child;
        }

        // Return the tree's nodes in an preorder traversal.
        public List<TreeNode> Preorder()
        {
            // Make the result list.
            List<TreeNode> nodes = new List<TreeNode>();

            // Traverse this node's subtree.
            TraversePreorder(nodes);

            // Return the result.
            return nodes;
        }
        private void TraversePreorder(List<TreeNode> nodes)
        {
            // Traverse this node.
            nodes.Add(this);

            // Traverse the children.
            foreach (TreeNode child in Children)
                child.TraversePreorder(nodes);
        }

        // Return an enumerator.
        public IEnumerable<TreeNode> GetTraversal()
        {
            // Get the preorder traversal.
            List<TreeNode> traversal = Preorder();

            // Yield the nodes in the traversal.
            foreach (TreeNode node in traversal) yield return node;
            yield break;
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
