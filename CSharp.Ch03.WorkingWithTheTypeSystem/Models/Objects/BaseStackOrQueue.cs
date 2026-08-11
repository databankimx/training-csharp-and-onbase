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
    /// Contains all common functionality to implement a stack or queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseStackOrQueue<T>
    {
        #region Private Members
        // List of data in queue
        protected readonly List<T> Values = [];
        #endregion

        #region Public Methods
        /// <summary>
        /// Add a value to the stack
        /// </summary>
        /// <param name="obj">Item to add</param>
        public void Add(T obj)
        {
            Values.Add(obj);
        }

        /// <summary>
        /// Obtain the next item in the stack or queue
        /// </summary>
        /// <returns></returns>
        public abstract T Next();

        /// <summary>
        /// Empty the stack
        /// </summary>
        public void Clear()
        {
            while (Values.Count > 0) Values.RemoveAt(0);
        }

        /// <summary>
        /// Count the number of items waiting in the stack
        /// </summary>
        /// <returns></returns>
        public bool Waiting()
        {
            return Values.Count > 0;
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
