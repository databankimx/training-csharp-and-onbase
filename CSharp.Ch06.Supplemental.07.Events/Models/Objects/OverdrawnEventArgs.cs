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

namespace CSharp.Ch06.Supplemental._07.Events.Models.Objects
{
    /// <summary>
    /// Defines event arguments for an Overdrawn event handler
    /// </summary>
    public class OverdrawnEventArgs : EventArgs
    {
        #region Properties
        /// <summary>
        /// Account balance
        /// </summary>
        public decimal CurrentBalance { get; set; }

        /// <summary>
        /// Amount to debit
        /// </summary>
        public decimal DebitAmount { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the OverdrawnEventArgs class
        /// </summary>
        /// <param name="currentBalance">Account balance</param>
        /// <param name="debitAmount">Amount to debit</param>
        public OverdrawnEventArgs(decimal currentBalance, decimal debitAmount)
        {
            CurrentBalance = currentBalance;
            DebitAmount = debitAmount;
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
