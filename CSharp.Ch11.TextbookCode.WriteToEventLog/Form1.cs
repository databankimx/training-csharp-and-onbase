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

using System.Diagnostics;

namespace WriteToEventLog
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Write an event log entry.
        private void writeButton_Click(object sender, EventArgs e)
        {
            string source = sourceTextBox.Text;
            string log = logTextBox.Text;
            string message = eventTextBox.Text;
            int id = int.Parse(idTextBox.Text);

            // Create the source if necessary. (Requires admin privileges.)
            if (!EventLog.SourceExists(source))
                EventLog.CreateEventSource(source, log);

            // Write the log entry.
            EventLog.WriteEntry(source, message,
                EventLogEntryType.Information, id);

            MessageBox.Show("OK");
        }
    }
}
