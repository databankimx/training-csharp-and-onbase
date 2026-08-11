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
using System.Collections.Generic;
#endregion

namespace CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects
{
    /// <summary>
    /// Implements a queue (FIFO) with prioritization supporting a specified data type
    /// </summary>
    /// <typeparam name="T">Data type of objects in queue</typeparam>
    public class PriorityQueue<T>
    {
        #region Private Members
        // List of values in the queue
        private readonly List<T> values = [];

        // Priority list for queue
        private readonly List<int> priorities = [];
        #endregion

        #region Properties
        /// <summary>
        /// Return the number of items in the queue.
        /// </summary>
        public int NumItems => values.Count;
        #endregion

        #region Public Methods
        /// <summary>
        /// Add an item to the queue.
        /// </summary>
        /// <param name="newValue">Item to add to queue</param>
        /// <param name="newPriority">Priority of added item</param>
        public void Enqueue(T newValue, int newPriority)
        {
            values.Add(newValue);
            priorities.Add(newPriority);
        }

        /// <summary>
        /// Remove the item with the highest priority from the queue.
        /// </summary>
        /// <param name="topValue">Value to remove from queue</param>
        /// <param name="topPriority">Priority of item to be removed</param>
        public void Dequeue(out T topValue, out int topPriority)
        {
            // Find the highest priority.
            int bestIndex = 0;
            int bestPriority = priorities[0];
            for (int i = 1; i < priorities.Count; i++)
            {
                if (bestPriority >= priorities[i]) continue;

                bestPriority = priorities[i];
                bestIndex = i;
            }

            // Return the corresponding item.
            topValue = values[bestIndex];
            topPriority = bestPriority;

            // Remove the item from the lists.
            values.RemoveAt(bestIndex);
            priorities.RemoveAt(bestIndex);
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
