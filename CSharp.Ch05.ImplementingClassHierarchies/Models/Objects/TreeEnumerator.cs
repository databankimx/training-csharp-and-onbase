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
using System;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Creates an enumerator for the TreeNode class
    /// </summary>
    public class TreeEnumerator : IEnumerator<TreeNode>
    {
        #region Private Members
        // List of nodes in enumerated tree
        private List<TreeNode> nodes;

        // Index of current node
        private int currentIndex;
        #endregion

        #region IEnumerator
        /// <summary>
        /// Obtain the current node as a TreeNode
        /// </summary>
        public TreeNode Current => GetCurrent();

        /// <summary>
        /// Obtain the current node as an object
        /// </summary>
        object IEnumerator.Current => GetCurrent();

        /// <summary>
        /// Move the index to the next node
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            currentIndex++;
            return currentIndex < nodes.Count;
        }

        /// <summary>
        /// Reset the index
        /// </summary>
        public void Reset()
        {
            currentIndex = -1;
        }
        #endregion

        #region IDisposable
        // NOTE: IEnumerator inherits IDisposable, so we must implement it

        // Destructor
        ~TreeEnumerator()
        {
            Dispose(false);
        }

        /// <summary>
        /// Explicitly dispose of object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Dispose of object and optionally managed members
        protected virtual void Dispose(bool releaseManagedObjects)
        {
            if (!releaseManagedObjects) return;
            nodes = null;
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the TreeEnumerator class
        /// </summary>
        /// <param name="root"></param>
        public TreeEnumerator(TreeNode root)
        {
            nodes = root.Preorder();
            Reset();
        }
        #endregion

        #region Private Methods
        // Obtain the current node
        private TreeNode GetCurrent()
        {
            if (currentIndex < 0 || currentIndex >= nodes.Count)
                throw new InvalidOperationException("Node index of range!");

            return nodes[currentIndex];
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
