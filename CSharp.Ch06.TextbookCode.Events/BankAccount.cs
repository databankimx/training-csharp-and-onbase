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

namespace CSharp.Ch06.TextbookCode.Events
{
    class BankAccount
    {
        // EventHandler<OverdrawnArgs>.
        public event EventHandler<OverdrawnEventArgs> Overdrawn;

        // The account balance.
        public decimal Balance { get; set; }

        // Add money to the account.
        public void Credit(decimal amount)
        {
            Balance += amount;
        }

        // Remove money from the account.
        public void Debit(decimal amount)
        {
            // See if there is this much money in the account.
            if (Balance >= amount)
            {
                // Remove the money.
                Balance -= amount;
            }
            else
            {
                // Raise the Overdrawn event.
                if (Overdrawn != null)
                    Overdrawn(this, new OverdrawnEventArgs(Balance, amount));
            }
        }
    }
}
