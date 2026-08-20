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
using CSharp.Ch06.Supplemental._07.Events.Models.Objects;
using CSharp.SharedLibrary.HelperClasses;
using CSharp.SharedLibrary.Models;
#endregion

namespace CSharp.Ch06.Supplemental._07.Events
{
    // Executable program
    internal static class Program
    {
        #region Chapter Notes
        /*
         * Definitions:
         *
         * Event:       An event is used to signal the program that something has occurred.
         * Publisher:   The object that raises the event
         * Subscriber:  The object that listens for and catches the event
         *
         * Syntax:      accessibility event delegate EventName;
         */
        #endregion

        #region Private Members
        // Used to set each "Account" to an initial $1000
        private const decimal InitialDeposit = 1000m;

        // Used in loop to debit the account until it overdrafts
        private static readonly decimal[] Withdrawals = [500m, 400m, 200m];

        // Used only to illustrate over-subscription
        private static int overdraftCount;
        #endregion

        #region Main Executable Method
        // Main method allows program to execute
        private static void Main()
        {
            try
            {
                #region Chapter Lessons
                // Demonstrate a simple example of handling an event
                SimpleBankAccountExample();
                GenericFunctions.Pause();

                // Demonstrate a simple example of handling an event (using a built-in delegate "Action")
                ActionBankAccountExample();
                GenericFunctions.Pause();

                // Demonstrate a better example of handling an event (using an event handler and custom arguments)
                ImprovedBankAccountExample();
                GenericFunctions.Pause();

                // Demonstrate an example of handling an event (using an event handler from the parent class)
                MoneyMarketAccountExample();
                GenericFunctions.Pause();

                // Demonstrate an example of oversubscribing an event and explicitly unsubscribing
                OversubscribingExample();
                GenericFunctions.Pause();
                #endregion
            }
            catch (Exception ex)
            {
                new DatabankException("Error Caught!", ex).Log();
                GenericFunctions.Pause();
            }
            finally
            {
                GenericFunctions.Pause(final: true);
            }
        }
        #endregion

        #region Chaper Lesson Methods
        // Demonstrate a simple example of handling an event
        private static void SimpleBankAccountExample()
        {
            Console.WriteLine($"Using SimpleBankAccount...{Environment.NewLine}");

            // Create a new bank account
            var account = new SimpleBankAccount(InitialDeposit);

            // Subscribe to the Overdrawn event
            account.Overdrawn += Account_Overdrawn;

            // Perform withdrawals until account is overdrawn
            foreach (decimal amount in Withdrawals)
            {
                Console.WriteLine($"Account balance: {account.Balance:C}");
                Console.WriteLine($"Debiting {amount:C}...");
                account.Debit(amount);
            }
        }

        // Demonstrate a simple example of handling an event (using a built-in delegate "Action")
        private static void ActionBankAccountExample()
        {
            Console.WriteLine($"Using ActionBankAccount...{Environment.NewLine}");

            // Create a new bank account
            var account = new ActionBankAccount(InitialDeposit);

            // Subscribe to the Overdrawn event
            account.Overdrawn += Account_Overdrawn;

            // Perform withdrawals until account is overdrawn
            foreach (decimal amount in Withdrawals)
            {
                Console.WriteLine($"Account balance: {account.Balance:C}");
                Console.WriteLine($"Debiting {amount:C}...");
                account.Debit(amount);
            }
        }

        // Demonstrate a better example of handling an event (using an event handler and custom arguments)
        private static void ImprovedBankAccountExample()
        {
            Console.WriteLine($"Using ImprovedBankAccount...{Environment.NewLine}");

            // Create a new bank account
            var account = new ImprovedBankAccount(InitialDeposit);

            // Subscribe to the Overdrawn event
            account.Overdrawn += OnAccountOverdrawn;

            // Perform withdrawals until account is overdrawn
            foreach (decimal amount in Withdrawals)
            {
                Console.WriteLine($"Account balance: {account.Balance:C}");
                Console.WriteLine($"Debiting {amount:C}...");
                account.Debit(amount);
            }
        }

        // Demonstrate an example of handling an event (using an event handler from the parent class)
        private static void MoneyMarketAccountExample()
        {
            Console.WriteLine($"Using MoneyMarketAccount...{Environment.NewLine}");

            // Create a new bank account
            var account = new MoneyMarketAccount(InitialDeposit);

            // Subscribe to the Overdrawn event
            account.Overdrawn += OnAccountOverdrawn;
            // This is equivalent to this older code example
            // `account.Overdrawn += new EventHandler<OverdrawnEventArgs>(OnAccountOverdrawn);`

            // Perform withdrawals until account is overdrawn
            foreach (decimal amount in Withdrawals)
            {
                Console.WriteLine($"Account balance: {account.Balance:C}");
                Console.WriteLine($"Debiting {amount:C}...");
                account.DebitFree(amount);
            }
        }

        // Demonstrate an example of oversubscribing an event and explicitly unsubscribing
        private static void OversubscribingExample()
        {
            Console.WriteLine($"Oversubscribing an event...{Environment.NewLine}");

            // Create a new bank account
            var account = new ImprovedBankAccount(InitialDeposit);

            // Subscribe to the Overdrawn event twice
            account.Overdrawn += OnAccountOverdrawnMulti;
            account.Overdrawn += OnAccountOverdrawnMulti;

            // Perform withdrawals until account is overdrawn
            foreach (decimal amount in Withdrawals)
            {
                Console.WriteLine($"Account balance: {account.Balance:C}");
                Console.WriteLine($"Debiting {amount:C}...");
                account.Debit(amount);
            }

            // Reset the account balance
            account.Credit(900m);

            Console.WriteLine();
            // Unsubscribe from the Overdrawn event once
            account.Overdrawn -= OnAccountOverdrawnMulti;

            // Perform withdrawals until account is overdrawn
            foreach (decimal amount in Withdrawals)
            {
                Console.WriteLine($"Account balance: {account.Balance:C}");
                Console.WriteLine($"Debiting {amount:C}...");
                account.Debit(amount);
            }
        }
        #endregion

        #region Event Handlers
        // Implements the delegate method for the Overdrawn event
        private static void Account_Overdrawn()
        {
            Console.WriteLine("Account overdrawn!");
        }

        // Implements the delegate method for the EventHandler<T> Overdrawn event
        private static void OnAccountOverdrawn(object sender, OverdrawnEventArgs e)
        {
            Console.WriteLine("Account overdrawn!");
            Console.WriteLine($"Balance [{e.CurrentBalance}] less than debit amount [{e.DebitAmount}]!");
        }

        // Implements the delegate method for the EventHandler<T> Overdrawn event
        private static void OnAccountOverdrawnMulti(object sender, OverdrawnEventArgs e)
        {
            overdraftCount++;
            Console.WriteLine("Account overdrawn!");
            Console.WriteLine($"Balance [{e.CurrentBalance}] less than debit amount [{e.DebitAmount}]!");
            Console.WriteLine($"Overdrawn Count: {overdraftCount}");
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
