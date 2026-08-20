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
 * A different take on the same org-chart tree as CSharp.Ch05.TextbookCode.IEnumerableTree.
 * This TreeNode doesn't implement IEnumerable<T> at all, it exposes a GetTraversal()
 *     method using a "yield return" iterator instead of a hand-rolled IEnumerator<T>
 *     class, worth comparing both approaches side by side.
 *
 * Two things worth knowing, deliberately left as originally downloaded:
 *   1. Every .cs file in this project declares "namespace TreeEnumerator", not
 *      "namespace CSharp.Ch05.TextbookCode.TreeEnumerator" like the rest of this
 *      chapter's downloads. This project is the one exception in the whole archive.
 *      Kept as-is per the "unedited code" policy for TextbookCode.* projects, the
 *      .csproj's RootNamespace/AssemblyName still follow this solution's naming
 *      convention, that setting doesn't force-rewrite the .cs files' own explicit
 *      namespace declarations, so there's no conflict either way.
 *   2. Form1.cs still contains the old manual-IEnumerator approach, commented out,
 *      referencing president.GetEnumerator(), a method this TreeNode no longer has.
 *      That commented block wouldn't even compile if uncommented as-is, it's a stale
 *      leftover from before this file was rewritten to use GetTraversal() instead.
 *
 * No bugs found otherwise, code is unchanged from the download aside from the
 *     project file format.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace TreeEnumerator
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
