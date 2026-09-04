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
    /// Models a bank account and demonstrates a simple event and event handler
    /// </summary>
    public class ImprovedBankAccount
    {
        #region Events
        /// <summary>
        /// Event to be raised when account balance would drop below zero
        /// </summary>
        public event EventHandler<OverdrawnEventArgs> Overdrawn;
        // Here we are explicitly declaring an event handler
        #endregion

        #region Properties
        /// <summary>
        /// Account balance
        /// </summary>
        public decimal Balance { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the ImprovedBankAccount class
        /// </summary>
        /// <param name="initialBalance">Initial account balance</param>
        public ImprovedBankAccount(decimal initialBalance = 0)
        {
            Balance = initialBalance >= 0 ? initialBalance : 0;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Add money to account balance
        /// </summary>
        /// <param name="amount">Amount to add to balance</param>
        public void Credit(decimal amount)
        {
            // Method should only accept a positive number
            if (amount < 0) throw new ApplicationException("Amount must be greater than zero!");

            Balance += amount;
        }

        /// <summary>
        /// Remove money from the account
        /// </summary>
        /// <param name="amount">Amount to remove from balance</param>
        public void Debit(decimal amount)
        {
            // Method should only accept a positive number
            if (amount < 0) throw new ApplicationException("Amount must be greater than zero!");

            // If balance is sufficient for the debit, remove the amount from the balance
            if (Balance >= amount)
            {
                Balance -= amount;
                return;
            }

            // Otherwise, raise the Overdrawn event (if there are subscribers, it will be non-null
            OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
        }
        #endregion

        #region Event Handlers
        protected virtual void OnOverdrawn(OverdrawnEventArgs args)
        {
            Overdrawn?.Invoke(this, args);
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
