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

namespace CSharp.Supplemental.TrieExamples;

internal class TrieNode
{
    #region Properties
    /// <summary>
    /// Contains all child nodes
    /// </summary>
    internal TrieNode[] Children { get; set; }

    /// <summary>
    /// Number of strings between root and this node
    /// </summary>
    internal uint WordCount { get; set; } = 0;
    #endregion

    #region Constructors
    /// <summary>
    /// Create and initialize instance of TrieNode
    /// </summary>
    /// <param name="size">Number of child nodes to generate</param>
    internal TrieNode(int size = 26)
    {
        Children = new TrieNode[size];
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
