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

namespace CSharp.Ch08.Supplemental._04.ReflectionPerformance.Models
{
    /// <summary>
    /// A deliberately trivial class, its Value property and Increment() method do as
    /// little work as possible, so that timing comparisons measure the overhead of HOW
    /// the property/method is accessed (directly vs. via reflection), not the cost of
    /// whatever work it happens to do.
    /// </summary>
    public class Counter
    {
        #region Properties
        /// <summary>
        /// Current count
        /// </summary>
        public int Value { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Increment the count by one
        /// </summary>
        public void Increment()
        {
            Value++;
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
