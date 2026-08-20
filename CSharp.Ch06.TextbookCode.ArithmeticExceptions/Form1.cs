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

namespace CSharp.Ch06.TextbookCode.ArithmeticExceptions
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void integerOverflowButton_Click(object sender, EventArgs e)
        {
            try
            {
                int a = 1000000000;
                int b = 1000000000;
                int c = a * b;
                MessageBox.Show("c = " + c.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void integersCheckedButton_Click(object sender, EventArgs e)
        {
            checked
            {
                try
                {
                    int a = 1000000000;
                    int b = 1000000000;
                    int c = a * b;
                    MessageBox.Show("c = " + c.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void floatOverflowButton_Click(object sender, EventArgs e)
        {
            float a = 1e30f;
            float b = 1e30f;
            float c = a * b;
            MessageBox.Show("c = " + c.ToString());
        }

        private void floatDivideBy0Button_Click(object sender, EventArgs e)
        {
            float a = 0f;
            float b = 0f;
            float c = a / b;
            MessageBox.Show("c = " + c.ToString());
        }
    }
}
