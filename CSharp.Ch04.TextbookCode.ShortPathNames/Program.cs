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

#region Textbook Information
/*
 * This project is a project-structure-only update of the code lab from:
 *   MCSD Certification Toolkit (Exam 70-483)
 *   https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * This is the interactive WinForms original of the GetShortPathName P/Invoke demo
 *     already ported into ImportedComDll() in CSharp.Ch04.UsingTypes.
 *
 * Bug found and fixed: the original download's Form1.Designer.cs never wired
 *     Form1_Load to the form's Load event, even though Form1.cs defines that method
 *     and it's clearly meant to pre-populate fileTextBox with the current
 *     executable's path. The event handler existed but never fired, so the field
 *     just started blank. Added "this.Load += new System.EventHandler(this.Form1_Load);"
 *     to InitializeComponent() to actually wire it up.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch04.TextbookCode.ShortPathNames
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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
