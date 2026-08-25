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
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch09.WorkingWithDataCollections.Models.Collections
{
    /// <summary>
    /// A custom collection wrapping a List&lt;T&gt;, but enforcing a maximum capacity that no
    /// built-in collection type does on its own. This is the concrete "why would I build my
    /// own collection" answer: not because List&lt;T&gt; is missing storage capability, but
    /// because a specific business rule (a hard cap on how many items are allowed) needs to
    /// be enforced everywhere the collection is used, not just remembered by every caller.
    /// </summary>
    /// <typeparam name="T">Type of item the collection holds</typeparam>
    public class BoundedCollection<T> : ICollection<T>
    {
        #region Private Members
        // The actual backing storage. BoundedCollection doesn't reimplement storage/iteration
        //   logic itself, it wraps an existing, well-tested collection and adds one rule on top.
        private readonly List<T> items = [];
        #endregion

        #region Properties
        /// <summary>
        /// Maximum number of items this collection will hold
        /// </summary>
        public int MaxCapacity { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the BoundedCollection class
        /// </summary>
        /// <param name="maxCapacity">Maximum number of items this collection will hold</param>
        #pragma warning disable IDE0290 // Use primary constructor
        public BoundedCollection(int maxCapacity)
        #pragma warning restore IDE0290 // Use primary constructor
        {
            MaxCapacity = maxCapacity;
        }
        #endregion

        #region ICollection<T>
        /// <summary>
        /// Number of items currently in the collection
        /// </summary>
        public int Count => items.Count;

        /// <summary>
        /// Always false: this collection is never read-only, it can always be added to and
        /// removed from, up to MaxCapacity
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Add an item to the collection
        /// </summary>
        /// <param name="item">Item to add</param>
        public void Add(T item)
        {
            if (items.Count >= MaxCapacity)
                throw new DatabankException($"Cannot add item: collection already holds its maximum of {MaxCapacity} item(s).");

            items.Add(item);
        }

        /// <summary>
        /// Remove all items from the collection
        /// </summary>
        public void Clear()
        {
            items.Clear();
        }

        /// <summary>
        /// Determine whether the collection contains a specific item
        /// </summary>
        /// <param name="item">Item to look for</param>
        public bool Contains(T item)
        {
            return items.Contains(item);
        }

        /// <summary>
        /// Copy the collection's items into an array, starting at a specified array index
        /// </summary>
        /// <param name="array">Destination array</param>
        /// <param name="arrayIndex">Index in the destination array to start copying to</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Remove a specific item from the collection
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True if the item was found and removed</returns>
        public bool Remove(T item)
        {
            return items.Remove(item);
        }

        /// <summary>
        /// Get an enumerator over the collection's items
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        // Explicit non-generic IEnumerable implementation, required because ICollection<T>
        //   inherits from the non-generic IEnumerable as well as IEnumerable<T>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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
