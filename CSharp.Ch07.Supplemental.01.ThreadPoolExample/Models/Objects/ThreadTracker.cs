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

namespace CSharp.Ch07.Supplemental._01.ThreadPoolExample.Models.Objects
{
    /// <summary>
    /// Defines a tracking object to be used in conjunction with pooled threads
    /// </summary>
    public class ThreadTracker
    {
        #region Properties
        /// <summary>
        /// Thread ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Event flag to be raised when work is complete
        /// </summary>
        public EventWaitHandle Handle { get; set; }

        /// <summary>
        /// Length of time to sleep (simulating intensive asynchronous work)
        /// </summary>
        public int SleepTime { get; set; }
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
