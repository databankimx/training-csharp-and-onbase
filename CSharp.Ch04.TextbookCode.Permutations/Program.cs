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
 * This is the interactive WinForms original of the concatenation-vs-StringBuilder
 *     timing demo already ported into UsingStringBuilder() in CSharp.Ch04.UsingTypes.
 *     Type a letter count, click Go, and watch both approaches run and time themselves.
 *
 * The original download had the same Factorial() off-by-one bug already found and
 *     fixed in CSharp.Ch04.UsingTypes: "for (int i = 2; i < number; i++)" computed
 *     (number - 1)! instead of number!, so the "# Permutations" readout was always
 *     one factor short of correct. Fixed to "i <= number" here too.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch04.TextbookCode.Permutations
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
