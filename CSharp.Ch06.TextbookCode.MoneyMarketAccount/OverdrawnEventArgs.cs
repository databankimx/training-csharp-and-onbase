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

namespace CSharp.Ch06.TextbookCode.MoneyMarketAccount
{
    class OverdrawnEventArgs : EventArgs
    {
        public decimal CurrentBalance, DebitAmount;

        public OverdrawnEventArgs(decimal currentBalance, decimal debitAmount)
        {
            CurrentBalance = currentBalance;
            DebitAmount = debitAmount;
        }
    }
}
