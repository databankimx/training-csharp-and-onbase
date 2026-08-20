/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Ch06.TextbookCode.Ch06RealWorldScenario01
{
    class OverdraftAccount : BankAccount
    {
        // The associated savings account.
        public BankAccount SavingsAccount { get; set; }

        // Remove money from the account.
        public new void Debit(decimal amount)
        {
            // See if there is this much money in the account.
            if (Balance + SavingsAccount.Balance < amount)
            {

                // Raise the Overdrawn event.
                OnOverdrawn(new OverdrawnEventArgs(Balance, amount));
            }
            else
            {
                // Remove the money we can from the overdraft account.
                if (Balance >= amount) Balance -= amount;
                else
                {
                    amount -= Balance;
                    Balance = 0m;

                    // If there's still an unpaid amount, take it from savings.
                    if (amount > 0m) SavingsAccount.Balance -= amount;
                }
            }
        }
    }
}
