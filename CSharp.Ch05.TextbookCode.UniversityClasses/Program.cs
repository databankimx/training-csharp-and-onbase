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
 * The best interface-implementation lesson in this chapter: four different
 *     TeachingAssistant classes (TeachingAssistant through TeachingAssistant4), each
 *     implementing IStudent a different way, direct, delegated to a private Student
 *     field, an implicit stub, and an EXPLICIT interface implementation. Form1_Load()
 *     specifically exercises the explicit-implementation gotcha the inline comment
 *     describes: ta.PrintGrades() would be a compile error for TeachingAssistant4
 *     (explicitly-implemented members aren't visible on the implementing type itself),
 *     while accessing the exact same method through an IStudent-typed reference works
 *     fine. This is a genuine, correct demonstration of the feature, not a bug.
 *
 * Same namespace note as CSharp.Ch05.TextbookCode.TreeEnumerator: every .cs file here
 *     declares "namespace UniversityClasses", not the "CSharp.Ch05.TextbookCode.*"
 *     convention used elsewhere in this chapter. Kept exactly as downloaded per the
 *     "unedited code" policy for TextbookCode.* projects, the .csproj's
 *     RootNamespace/AssemblyName still follow this solution's naming convention, which
 *     doesn't force-rewrite the .cs files' own explicit namespace declarations.
 *
 * No bugs found otherwise, code is unchanged from the download aside from the
 *     project file format.
 */
#endregion

#region Using Directives
using System;
using System.Windows.Forms;
#endregion

namespace UniversityClasses
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
