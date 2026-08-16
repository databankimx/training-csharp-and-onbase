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

#region Using Directives
using System.Collections;
using System.Collections.Generic;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Define an enumerable tree node class
    /// </summary>
    public class TreeNode : IEnumerable<TreeNode>
    {
        #region Properties
        /// <summary>
        /// Degree of depth within the tree
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Content of node
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Children of this node on the tree
        /// </summary>
        #pragma warning disable IDE0028 // For lesson, not simplifying (to `new()`) to avoid confusion for students
        public List<TreeNode> Children { get; set; } = new List<TreeNode>();
        #pragma warning restore IDE0028
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the TreeNode class
        /// </summary>
        /// <param name="text">Node content</param>
        #pragma warning disable IDE0290 // Avoiding primary constructor for lesson clarity
        public TreeNode(string text)
        #pragma warning restore IDE0290
        {
            Text = text;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a new child node to the tree
        /// </summary>
        /// <param name="text">Node content</param>
        /// <returns>Child node</returns>
        public TreeNode AddChild(string text)
        {
            var child = new TreeNode(text) {Depth = Depth + 1};
            Children.Add(child);
            return child;
        }

        /// <summary>
        /// Create the ordered tree for traversal
        /// </summary>
        /// <returns>Tree list</returns>
        public List<TreeNode> Preorder()
        {
            var nodes = new List<TreeNode>();
            TraversePreorder(nodes);
            return nodes;
        }
        #endregion

        #region Private Methods
        // Traverse the tree and all child trees
        private void TraversePreorder(List<TreeNode> nodes)
        {
            nodes.Add(this);
            foreach (var child in Children) child.TraversePreorder(nodes);
        }
        #endregion

        #region IEnumerable
        /// <summary>
        /// Obtain the enumerator for the tree
        /// </summary>
        /// <returns>Enumerator</returns>
        public IEnumerator<TreeNode> GetEnumerator()
        {
            return new TreeEnumerator(this);
        }

        /// <summary>
        /// Obtain the enumerator for the tree
        /// </summary>
        /// <returns>Enumerator</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TreeEnumerator(this);
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
