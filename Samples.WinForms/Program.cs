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

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace Samples.WinForms
{
    /// <summary>
    /// Contains the application's entry point.
    /// </summary>
    internal static class Program
    {
        #region Methods
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <remarks>
        /// *Migration Note: ApplicationConfiguration.Initialize() (used in the original
        /// net10.0-windows version) is a modern .NET 6+-only WinForms SDK feature and does
        /// not exist on net48, replaced here with the classic, equivalent net48 pattern:
        /// Application.EnableVisualStyles() + SetCompatibleTextRenderingDefault(false).
        /// </remarks>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
