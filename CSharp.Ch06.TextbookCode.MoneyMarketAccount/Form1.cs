/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * One correction from the original download: feeButton_Click called
 *     TheAccount.Debit(...) (identical to debitButton_Click) instead of
 *     TheAccount.DebitFee(...), fixed here so the "Fee" button actually exercises
 *     MoneyMarketAccount's own DebitFee() method instead of duplicating the Debit
 *     button. See CSharp.Ch06.TextbookCode.MoneyMarketAccount.csproj's Textbook
 *     Information header for details.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Globalization;

namespace CSharp.Ch06.TextbookCode.MoneyMarketAccount
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // The bank account.
        private MoneyMarketAccount TheAccount;

        // Create the account and subscribe the event handler.
        private void Form1_Load(object sender, EventArgs e)
        {
            // Create the account.
            TheAccount = new MoneyMarketAccount();
            TheAccount.Balance = 100m;

            // Subscribe to the Overdrawn event.
            TheAccount.Overdrawn += OverdrawnHandler;

            // Display the account balance.
            DisplayBalance();
        }

        // The event handler with event args.
        private void OverdrawnHandler(object sender, OverdrawnEventArgs args)
        {
            string message =
                "The account is overdrawn." + Environment.NewLine +
                "Current Balance: " + args.CurrentBalance.ToString("C") + Environment.NewLine +
                "Debit Amount: " + args.DebitAmount.ToString("C");
            MessageBox.Show(message);
        }

        // Add money to the account.
        private void creditButton_Click(object sender, EventArgs e)
        {
            TheAccount.Credit(decimal.Parse(amountTextBox.Text, NumberStyles.Currency));

            // Display the account balance.
            DisplayBalance();
        }

        // Remove money from the account.
        private void debitButton_Click(object sender, EventArgs e)
        {
            TheAccount.Debit(decimal.Parse(amountTextBox.Text, NumberStyles.Currency));

            // Display the account balance.
            DisplayBalance();
        }

        // Display the account balance.
        private void DisplayBalance()
        {
            balanceTextBox.Text = TheAccount.Balance.ToString("C");
        }

        // Remove a fee from the account.
        private void feeButton_Click(object sender, EventArgs e)
        {
            TheAccount.DebitFee(decimal.Parse(amountTextBox.Text, NumberStyles.Currency));

            // Display the account balance.
            DisplayBalance();
        }
    }
}
