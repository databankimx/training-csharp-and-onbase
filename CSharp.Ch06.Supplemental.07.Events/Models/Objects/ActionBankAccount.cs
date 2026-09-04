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

#pragma warning disable S112 // For lesson purposes, we are allowing the use of ApplicationException, which is generally not recommended in production code.
#pragma warning disable S125 // Allow commented code in training projects for demonstration purposes.
namespace CSharp.Ch06.Supplemental._07.Events.Models.Objects
{
    /// <summary>
    /// Models a bank account and demonstrates a simple event and event handler
    /// </summary>
    public class ActionBankAccount
    {
        #region Events
        /// <summary>
        /// Event to be raised when account balance would drop below zero
        /// </summary>
        public event Action Overdrawn;
        // Here we are using the built-in delegate "Action" for the event, so we don't need to explicitly declare a separate delegate
        #endregion

        #region Properties
        /// <summary>
        /// Account balance
        /// </summary>
        public decimal Balance { get; private set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of the ActionBankAccount class
        /// </summary>
        /// <param name="initialBalance">Initial account balance</param>
        public ActionBankAccount(decimal initialBalance = 0)
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
            Overdrawn?.Invoke();

            // Note: The syntax above takes advantage of null propagation and is equivalent to:
            // if (Overdrawn != null) Overdrawn();
        }
        #endregion
    }
}
#pragma warning restore S125
#pragma warning restore S112

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
