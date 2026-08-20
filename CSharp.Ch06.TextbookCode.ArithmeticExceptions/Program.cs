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
 * Four buttons, four arithmetic-exception scenarios: integer overflow (unchecked and
 *     checked), float overflow, and float divide-by-zero. This is the same contrast
 *     CSharp.Ch06.Supplemental.05.ExceptionHandling's ArithmeticExceptions() method
 *     covers, via console output there, via MessageBox clicks here. No bugs found.
 *
 * One cosmetic deviation from the raw download: the window's Text was left as the
 *     generic "Form1" default in the original (unlike every sibling TextbookCode.*
 *     project in this chapter, which all had a descriptive title already set), changed
 *     here to "ArithmeticExceptions" for consistency with the rest of this chapter's
 *     labs. No functional code was changed.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch06.TextbookCode.ArithmeticExceptions
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
