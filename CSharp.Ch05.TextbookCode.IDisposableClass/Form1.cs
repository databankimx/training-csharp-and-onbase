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

namespace CSharp.Ch05.TextbookCode.IDisposableClass
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Used to give objects different names.
        private int ObjectNumber = 1;

        // Create an object and dispose of it.
        private void createAndDisposeButton_Click(object sender, EventArgs e)
        {
            // Make an object.
            DisposableClass obj = new DisposableClass();
            obj.Name = "CreateAndDispose " + ObjectNumber.ToString();
            ObjectNumber++;

            // Dispose of the object.
            obj.Dispose();

            // Version that uses using.
            // Make an object.
            //using (DisposableClass obj = new DisposableClass())
            //{
            //    obj.Name = "CreateAndDispose " + ObjectNumber.ToString();
            //    ObjectNumber++;
            //}
        }

        // Create an object and do not dispose of it.
        private void createButton_Click(object sender, EventArgs e)
        {
            // Make an object.
            DisposableClass obj = new DisposableClass();
            obj.Name = "Create " + ObjectNumber.ToString();
            ObjectNumber++;
        }

        // Force garbage collection.
        private void collectGarbageButton_Click(object sender, EventArgs e)
        {
            GC.Collect();
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
