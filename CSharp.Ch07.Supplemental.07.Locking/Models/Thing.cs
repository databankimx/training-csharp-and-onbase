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

#region Directives
using System.Threading;
#endregion

namespace CSharp.Ch07.Supplemental._07.Locking.Models
{
    /// <summary>
    /// Simple object for testing
    /// </summary>
    public class Thing
    {
        #region Properties
        /// <summary>
        /// Object ID
        /// </summary>
        public int Id { get; set; } = 0;

        /// <summary>
        /// Object Name
        /// </summary>
        public string Name { get; set; } = null;

        /// <summary>
        /// Mutex for locking
        /// </summary>
        public Mutex Mutex { get; set; } = null;

        /// <summary>
        /// Semaphore for locking
        /// </summary>
        public Semaphore SemaphorePool { get; set; } = null;
        #endregion

        ~Thing()
        {
            Mutex?.Dispose();
            SemaphorePool?.Dispose();
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
