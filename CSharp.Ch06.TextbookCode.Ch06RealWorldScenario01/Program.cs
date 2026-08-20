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
 * This project is a project-structure-only update of the "Overdraft Account" real-world
 *     scenario from:
 *   MCSD Certification Toolkit (Exam 70-483)
 *   https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * Two linked accounts, an overdraft-protected checking account backed by a savings
 *     account, both fully interactive via Credit/Debit buttons. No bugs found, code is
 *     unchanged from the download aside from the project file format. Two design
 *     details worth reading closely are called out in LectureNotes.md: OverdraftAccount
 *     uses "new" rather than "override" on Debit() (required, since the base BankAccount
 *     never declares Debit virtual), and OverdraftAccount deliberately manipulates
 *     SavingsAccount.Balance directly rather than calling SavingsAccount.Debit(), to
 *     avoid incorrectly raising the savings account's own Overdrawn event when combined
 *     funds are actually sufficient.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch06.TextbookCode.Ch06RealWorldScenario01
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
