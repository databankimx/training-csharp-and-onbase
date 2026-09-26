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
/// A single node in a singly-linked list - a value, and a reference to the next node (or
/// null, if this is the last one). Nothing links these nodes together in memory except
/// these references - unlike an array, a linked list's nodes don't need to sit next to
/// each other at all.
/// </summary>
internal class LinkedListNode
{
    #region Properties
    /// <summary>
    /// Node Value
    /// </summary>
    internal int Value { get; set; }

    /// <summary>
    /// Next Node
    /// </summary>
    internal LinkedListNode Next { get; set; }
    #endregion

    #region Constructors
    /// <summary>
    /// Initialize new LinkedListNode instance
    /// </summary>
    /// <param name="value"></param>
    internal LinkedListNode(int value)
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
