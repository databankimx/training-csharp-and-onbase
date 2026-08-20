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

namespace CSharp.Ch06.TextbookCode.StaticAndInstanceDelegates
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Make some Persons.
            Person alice = new Person() { Name = "Alice" };
            Person bob = new Person() { Name = "Bob" };

            // Make Alice's InstanceMethod variable refer to her own GetName method.
            alice.InstanceMethod = alice.GetName;
            alice.StaticMethod = Person.StaticName;

            // Make Bob's InstanceMethod variable refer to Alice's GetName method.
            bob.InstanceMethod = alice.GetName;
            bob.StaticMethod = Person.StaticName;

            // Demonstrate the methods.
            string result = "";
            result += "Alice's InstanceMethod returns: " + alice.InstanceMethod() + Environment.NewLine;
            result += "Bob's InstanceMethod returns: " + bob.InstanceMethod() + Environment.NewLine;
            result += "Alice's StaticMethod returns: " + alice.StaticMethod() + Environment.NewLine;
            result += "Bob's StaticMethod returns: " + bob.StaticMethod();
            resultsTextBox.Text = result;
            resultsTextBox.Select(0, 0);
        }
    }
}
