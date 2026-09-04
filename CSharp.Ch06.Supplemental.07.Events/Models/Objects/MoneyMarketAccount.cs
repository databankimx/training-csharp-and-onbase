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
    /// Creates a derived class from a parent that already exposes an event
    /// </summary>
    public class MoneyMarketAccount : ImprovedBankAccount
    {
        #region Constructors
        /// <summary>
        /// Create a new instance of the MoneyMarketAccount class
        /// </summary>
        /// <param name="initialBalance"></param>
        public MoneyMarketAccount(decimal initialBalance = 0) : base(initialBalance) {}
        #endregion

        #region Public Methods
        /// <summary>
        /// Deduct from account balance or invoke parent event
        /// </summary>
        /// <param name="amount">Amount to deduct</param>
        public void DebitFree(decimal amount)
        {
            // Method should only accept a positive number
            #pragma warning disable S112 // Allow throwing ApplicationException for lesson purposes, which is generally not recommended in production code.
            if (amount < 0) throw new ApplicationException("Amount must be greater than zero!");
            #pragma warning restore S112

            // If balance is sufficient for the debit, remove the amount from the balance
            if (Balance >= amount)
            {
                Balance -= amount;
                return;
            }

            OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
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
