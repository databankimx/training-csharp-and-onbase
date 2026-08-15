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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSharp.Ch04.TextbookCode.Ch04RealWorldScenario03
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Make a StringBuilder holding the ABCs.
            StringBuilder letters =
                new StringBuilder("ABCDEFGHIJKLMNOPQRSTUVWXYZ");

            // This one holds the next line of letters.
            StringBuilder line = new StringBuilder();

            // Create the result StringBuilder.
            StringBuilder result = new StringBuilder();

            // Loop over the letters.
            for (int i = 0; i < 26; i++)
            {
                // Add the next letter to line.
                line.Append(letters[i]);

                // Add line to the result.
                result.AppendLine(line.ToString());
            }

            // Display the result.
            stringBuilderTextBox.Text = result.ToString();
            stringBuilderTextBox.Select(0, 0);
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
