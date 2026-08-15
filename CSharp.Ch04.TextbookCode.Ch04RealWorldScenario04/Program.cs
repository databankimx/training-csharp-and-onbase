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
 * This project is a project-structure-only update of the "Displaying Currency Values"
 *     real-world scenario from:
 *   MCSD Certification Toolkit (Exam 70-483)
 *   https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * This is the real-world example CSharp.Ch04.UsingTypes mentions and deliberately
 *     skips near StringFormat(). Builds on Scenario 2 (the "%" tax rate handling),
 *     adding .ToString("C") currency formatting to every dollar amount on the form.
 *
 * Same copy-paste namespace artifact as Scenario 2: the original download's
 *     Program.cs and Form1.cs both declared "namespace Ch04RealWorldScenario01" here
 *     too, not something introduced during this migration. Renamed to match this
 *     project's folder like every other TextbookCode.* lab.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch04.TextbookCode.Ch04RealWorldScenario04
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
