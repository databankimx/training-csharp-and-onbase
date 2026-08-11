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
#endregion

namespace CSharp.Ch03.WorkingWithTheTypeSystem.Models.Objects
{
    /// <summary>
    /// Implements a queue (FIFO) supporting a specified data type
    /// </summary>
    /// <typeparam name="T">Data type of objects in queue</typeparam>
    public class GenericQueue<T> : BaseStackOrQueue<T>
    {
        #region Public Methods
        /// <summary>
        /// Obtain the next item in the queue
        /// </summary>
        /// <returns></returns>
        public override T Next()
        {
            if (Values.Count <= 0) throw new IndexOutOfRangeException("GenericQueue is empty!");
            // Alternative design: instead of throwing, you could `return default(T);` here,
            //     silently handing back null/0/false instead of failing loudly. Throwing is
            //     usually the better choice, a silent default can hide a bug for a long time.

            var ret = Values[0];
            Values.RemoveAt(0);
            return ret;
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
