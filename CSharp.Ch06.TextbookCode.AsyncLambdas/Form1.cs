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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp.Ch06.TextbookCode.AsyncLambdas
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // The number of times we have run DoSomethingAsync.
        private int Trials = 0;

        // Create an event handler for the button.
        private void Form1_Load(object sender, EventArgs e)
        {
            runAsyncButton.Click += async (button, buttonArgs) =>
            {
                int trial = ++Trials;
                statusLabel.Text = "Running trial " + trial.ToString() + "...";
                await DoSomethingAsync();
                statusLabel.Text = "Done with trial " + trial.ToString();
            };
        }

        // Do something time consuming.
        async Task DoSomethingAsync()
        {
            // In this example, just waste some time.
            await Task.Delay(3000);
        }
    }
}
