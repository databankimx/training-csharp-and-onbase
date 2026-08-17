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
 * This project is a project-structure-only update of the "Shape Resources" real-world
 *     scenario from:
 *   MCSD Certification Toolkit (Exam 70-483)
 *   https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 *
 * This is the real-world example CSharp.Ch05.ImplementingClassHierarchies mentions and
 *     deliberately skips covering in lecture. The Ellipse and Circle classes carried into
 *     that project for reference come from here.
 *
 * Bug found and fixed: same pattern as CSharp.Ch04.TextbookCode.ShortPathNames, the
 *     original download's Form1.Designer.cs never wired Form1_Load to the form's Load
 *     event, even though Form1.cs defines that method and it's clearly meant to run the
 *     validation demo automatically when the form opens. The event handler existed but
 *     never fired, so the entire demo never ran at all. Added
 *     "this.Load += new System.EventHandler(this.Form1_Load);" to InitializeComponent()
 *     to actually wire it up.
 *
 * Separately, worth knowing (not a bug): all six test constructors in Form1_Load() run
 *     inside a single try, so the first invalid one (e1, negative width) throws
 *     immediately and every line after it never executes. This is meant to be stepped
 *     through in the debugger one line at a time (or with later lines commented out),
 *     the same pattern used by CastingArrays and CloneArray elsewhere in this training set.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch05.TextbookCode.Ch05RealWorldScenario01
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
