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
 * The direct source CSharp.Ch06.Supplemental.07.Events's MoneyMarketAccount was adapted
 *     from, event inheritance demonstrated via a third "Fee" button alongside the usual
 *     Credit/Debit.
 *
 * Bug found and fixed: feeButton_Click called TheAccount.Debit(...), byte-for-byte
 *     identical to debitButton_Click, instead of TheAccount.DebitFee(...). This meant
 *     MoneyMarketAccount.DebitFee() (the whole reason this project exists, showing a
 *     subclass's own method correctly raising the inherited Overdrawn event) was dead
 *     code, never actually called by anything. Fixed by pointing feeButton_Click at
 *     DebitFee() instead. DebitFee() and Debit() happen to contain identical logic, so
 *     this didn't change what any button visibly did, it just means the "Fee" button
 *     now actually exercises the method it was built to demonstrate.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch06.TextbookCode.MoneyMarketAccount
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
