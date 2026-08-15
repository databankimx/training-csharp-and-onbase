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
 * Same shape as CastingArrays: WinForms, blank window, no try/catch around the
 *     deliberate runtime failure at the end of Form1_Load. Meant to be run under
 *     the debugger, not read from console output, there isn't any.
 *
 * A "Microsoft.CSharp" reference was added, this file uses the "dynamic" keyword,
 *     which needs it explicitly on net48 SDK-style projects (see CSharp.Ch04.UsingTypes
 *     LectureNotes.md for the full explanation of why).
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch04.TextbookCode.CloneArray
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
