/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * One correction from the original download: the ArgumentOutOfRangeException message
 *     below read "calcualte" instead of "calculate", fixed here.
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

namespace CSharp.Ch06.TextbookCode.Ch06RealWorldScenario02
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Calculate the number's factorial.
        private void calculateButton_Click(object sender, EventArgs e)
        {
            try
            {
                resultTextBox.Clear();

                long n;
                if (!long.TryParse(nTextBox.Text, out n))
                {
                    MessageBox.Show("The number must be an integer.");
                    return;
                }
                resultTextBox.Text = Factorial(n).ToString();
            }
            catch (ArgumentOutOfRangeException)
            {
                MessageBox.Show("The number must be at least 0.");
            }
            catch (OverflowException)
            {
                MessageBox.Show("This number is too big to calculate its factorial.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        // Calculate a number's factorial.
        private long Factorial(long n)
        {
            // Make sure n >= 0.
            if (n < 0) throw new ArgumentOutOfRangeException(
                "n", "The number n must be at least 0 to calculate n!");

            checked
            {
                long result = 1;
                for (long i = 2; i <= n; i++) result *= i;
                return result;
            }
        }
    }
}
