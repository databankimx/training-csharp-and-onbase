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

using System.Runtime.InteropServices;

namespace CSharp.Ch04.TextbookCode.ShortPathNames
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern uint GetShortPathName(string lpszLongPath, char[] lpszShortPath, int cchBuffer);

        // Start with this program's executable file.
        private void Form1_Load(object sender, EventArgs e)
        {
            fileTextBox.Text = Application.ExecutablePath;
        }

        // Convert to a short name.
        private void goButton_Click(object sender, EventArgs e)
        {
            // Get the long file name.
            string longName = fileTextBox.Text;

            // Allocate a buffer to hold the result.
            char[] buffer = new char[1024];
            long length = GetShortPathName(
                longName, buffer,
                buffer.Length);

            // Get the short name.
            string shortName = new string(buffer);
            shortNameTextBox.Text = shortName.Substring(0, (int)length);
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
