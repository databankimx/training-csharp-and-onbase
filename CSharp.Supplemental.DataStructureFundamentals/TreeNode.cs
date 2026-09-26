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

namespace CSharp.Supplemental.DataStructureFundamentals;

/// <summary>
/// A single node in a binary search tree - a value, plus references to up to two child
/// nodes (Left holds smaller values, Right holds larger ones). This is the same idea
/// CSharp.Supplemental.TrieExamples builds on, just with up to 26 children (one per letter)
/// instead of 2, and no ordering rule between them.
/// </summary>
internal class TreeNode
{
    #region Properties
    /// <summary>
    /// Gets or sets the integer value.
    /// </summary>
    internal int Value { get; set; }

    /// <summary>
    /// Left child node
    /// </summary>
    internal TreeNode Left { get; set; }

    /// <summary>
    /// Right child node
    /// </summary>
    internal TreeNode Right { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="TreeNode"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value assigned to the node.</param>
    internal TreeNode(int value)
    {
        Value = value;
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
